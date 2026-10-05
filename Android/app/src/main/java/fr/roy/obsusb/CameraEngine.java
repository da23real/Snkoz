package fr.roy.obsusb;

import android.content.ContentValues;
import android.content.Context;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.*;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.media.MediaRecorder;
import android.net.Uri;
import android.os.*;
import android.provider.DocumentsContract;
import android.provider.MediaStore;
import android.util.Range;
import android.util.Size;
import android.view.Surface;
import org.json.JSONObject;
import java.io.File;
import java.text.SimpleDateFormat;
import java.util.*;
import java.util.concurrent.CompletableFuture;

/** All mutable camera/recorder state belongs to one HandlerThread. */
public final class CameraEngine {
    public interface PreviewSource { CompletableFuture<byte[]> capture(); }
    public volatile PreviewSource previewSource;
    public CompletableFuture<byte[]> previewJpeg() {
        PreviewSource source=previewSource;
        try {
            if(source==null || !new JSONObject(snapshot).optString("state").equals("stopped"))throw new IllegalStateException("Aperçu indisponible : ouvrir l'application à l'arrêt");
            return source.capture();
        }catch(Exception e){CompletableFuture<byte[]> failed=new CompletableFuture<>();failed.completeExceptionally(e);return failed;}
    }
    private final Context context;
    public final RecordingCatalog catalog;
    private final HandlerThread thread = new HandlerThread("CameraUsb");
    private final Handler handler;
    private CameraDevice camera;
    private CameraCaptureSession capture;
    private CameraCharacteristics characteristics;
    private SurfaceTexture texture;
    private Surface preview;
    private Surface recordingSurface;
    private boolean previewVisible=true;
    private MediaRecorder recorder;
    private ParcelFileDescriptor outputFd;
    private Uri outputUri;
    private boolean mediaPending;
    private boolean active, ready, opening;
    private int generation, maxHeight = 1080, rotation;
    private String cameraId, folder = "", state = "stopped", sessionId = "";
    private String filename = "", lastFile = "", error = "";
    private long started, pausedAt, pauseTotal, lastContact;
    private long recordStartNs, firstSensorNs, firstCallbackNs, recordStopNs;
    private boolean sensorRealtime;
    private JSONObject musicMarker = new JSONObject(), stopMarker = new JSONObject();
    private final CameraCaptureSession.CaptureCallback timingCallback = new CameraCaptureSession.CaptureCallback() {
        @Override public void onCaptureStarted(CameraCaptureSession session, CaptureRequest request, long timestamp, long frameNumber) {
            if (recordStartNs == 0 || firstCallbackNs != 0 || !state.equals("recording")) return;
            long received = SystemClock.elapsedRealtimeNanos();
            if (sensorRealtime && timestamp < recordStartNs) return;
            firstSensorNs = timestamp; firstCallbackNs = received; publish();
        }
    };
    public CompletableFuture<String> marker(JSONObject value) {
        CompletableFuture<String> result = new CompletableFuture<>();
        handler.post(() -> {
            try {
                if (!sessionId.equals(value.optString("session"))) throw new IllegalArgumentException("Prise différente");
                String kind = value.getString("kind");
                if (!kind.equals("music") && !kind.equals("stop")) throw new IllegalArgumentException("Repère inconnu");
                JSONObject mark = new JSONObject();
                mark.put("teeUs",value.getLong("teeUs")); mark.put("tick",value.getLong("tick"));
                mark.put("pcEpoch",value.getDouble("pcEpoch")); mark.put("phoneMonoNs",value.getLong("phoneMonoNs"));
                mark.put("receivedMonoNs",SystemClock.elapsedRealtimeNanos());
                if (kind.equals("music")) musicMarker=mark; else stopMarker=mark;
                result.complete("{\"ok\":true}");
            } catch(Exception e) {result.completeExceptionally(e);}
        });return result;
    }
    private JSONObject timing() throws Exception {
        JSONObject t = new JSONObject();
        t.put("recordStartNs",recordStartNs);t.put("firstSensorNs",firstSensorNs);t.put("firstCallbackNs",firstCallbackNs);
        t.put("recordStopNs",recordStopNs);t.put("sensorRealtime",sensorRealtime);
        t.put("originQuality","estimated-first-camera-frame; encoded/audio offset not calibrated");
        t.put("musicMarker",musicMarker);t.put("stopMarker",stopMarker);return t;
    }
    private Size videoSize = new Size(1280, 720);
    private Range<Integer> fps;
    private volatile String snapshot = "{\"state\":\"stopped\",\"ready\":false}";

    public CameraEngine(Context context) {
        this.context = context.getApplicationContext();
        catalog = new RecordingCatalog(this.context);
        thread.start(); handler = new Handler(thread.getLooper());
        handler.post(this::publish);
    }
    public String status() { return snapshot; }
    public void contact() { handler.post(() -> { lastContact = SystemClock.elapsedRealtime(); publish(); }); }
    private int videoBitrate939=6000000;
    public void configure(String id, int height, String tree, int rotationDegrees,int bitrate) {
        handler.post(() -> {
            if (!state.equals("stopped") || opening) return;
            if (Objects.equals(cameraId,id) && maxHeight==height && folder.equals(tree) && rotation==rotationDegrees && videoBitrate939==bitrate) return;
            videoBitrate939=bitrate; cameraId = id; maxHeight = height; folder = tree; rotation = rotationDegrees;
            if (active) { closeCamera(); openCamera(); }
        });
    }
    public void activate(SurfaceTexture surfaceTexture, int rotationDegrees) {
        handler.post(() -> { active = true; texture = surfaceTexture; rotation = rotationDegrees; previewVisible=true; openCamera(); updateTargets(); });
    }
    public void setPreviewVisible(boolean visible) { handler.post(() -> {previewVisible=visible;updateTargets();}); }
    private void updateTargets() {
        if(capture==null || camera==null || state.equals("starting"))return;
        try {
            if(recordingSurface!=null && (state.equals("recording")||state.equals("paused"))) {
                capture.setRepeatingRequest(previewVisible?request(CameraDevice.TEMPLATE_RECORD,preview,recordingSurface):request(CameraDevice.TEMPLATE_RECORD,recordingSurface),timingCallback,handler);
            } else if(previewVisible)capture.setRepeatingRequest(request(CameraDevice.TEMPLATE_PREVIEW,preview),null,handler);
            else capture.stopRepeating();
        }catch(Exception e){error="Aperçu : "+readable(e);publish();}
    }
    public void deactivate(String reason) {
        handler.post(() -> {
            active = false;
            if (!state.equals("stopped")) {
                stopRecording(); error = reason;
            }
            closeCamera(); texture = null; publish();
        });
    }
    public void shutdown() { handler.post(() -> { active = false; stopRecording(); closeCamera(); if(texture!=null){texture.release();texture=null;} thread.quitSafely(); }); }

    private void openCamera() {
        if (!active || camera != null || opening || texture == null) return;
        opening = true; ready = false; final int gen = ++generation;
        try {
            CameraManager manager = (CameraManager) context.getSystemService(Context.CAMERA_SERVICE);
            if (cameraId == null) {
                for (String id : manager.getCameraIdList()) {
                    cameraId = id;
                    Integer facing = manager.getCameraCharacteristics(id).get(CameraCharacteristics.LENS_FACING);
                    if (facing != null && facing == CameraCharacteristics.LENS_FACING_BACK) break;
                }
            }
            if (cameraId == null) throw new IllegalStateException("Aucune caméra disponible");
            characteristics = manager.getCameraCharacteristics(cameraId);
            Integer ts = characteristics.get(CameraCharacteristics.SENSOR_INFO_TIMESTAMP_SOURCE);
            sensorRealtime = ts != null && ts == CameraCharacteristics.SENSOR_INFO_TIMESTAMP_SOURCE_REALTIME;
            StreamConfigurationMap map = characteristics.get(CameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
            if (map == null) throw new IllegalStateException("Formats caméra indisponibles");
            videoSize = chooseSize(map.getOutputSizes(MediaRecorder.class), maxHeight);
            Size viewSize = choosePreview(map.getOutputSizes(SurfaceTexture.class), videoSize);
            fps = chooseFps(characteristics.get(CameraCharacteristics.CONTROL_AE_AVAILABLE_TARGET_FPS_RANGES));
            texture.setDefaultBufferSize(viewSize.getWidth(), viewSize.getHeight());
            preview = new Surface(texture);
            manager.openCamera(cameraId, new CameraDevice.StateCallback() {
                @Override public void onOpened(CameraDevice device) {
                    if (!active || gen != generation) { device.close(); return; }
                    opening = false; camera = device; error = ""; createPreview();
                }
                @Override public void onDisconnected(CameraDevice device) { cameraFailure(device, gen, "Camera déconnectée"); }
                @Override public void onError(CameraDevice device, int code) { cameraFailure(device, gen, "Erreur caméra " + code); }
            }, handler);
        } catch (Exception e) { opening = false; error = readable(e); closeCamera(); }
        publish();
    }
    private void cameraFailure(CameraDevice device, int gen, String message) {
        if (gen != generation) { device.close(); return; }
        active = false; stopRecording(); closeCamera(); error = message; publish();
    }
    private static Size chooseSize(Size[] choices, int height) {
        if (choices == null || choices.length == 0) throw new IllegalStateException("Aucun format vidéo");
        for (int h : new int[]{height, 720, 480,360,240}) {
            if(h>height)continue;
            int w = h == 480 ? 640 : h * 16 / 9;
            for (Size s : choices) if (s.getWidth() == w && s.getHeight() == h) return s;
        }
        Size best = null;
        for (Size s : choices) if (s.getHeight() <= height && (best == null || (long)s.getWidth()*s.getHeight() > (long)best.getWidth()*best.getHeight())) best = s;
        if (best != null) return best;
        return Collections.min(Arrays.asList(choices), Comparator.comparingLong(s -> (long)s.getWidth()*s.getHeight()));
    }
    private static Size choosePreview(Size[] choices, Size video) {
        Size best = null;
        for (Size s : choices) {
            if (s.getWidth()*video.getHeight() != s.getHeight()*video.getWidth() || s.getWidth() > 1920) continue;
            if (best == null || Math.abs(s.getWidth()-1280) < Math.abs(best.getWidth()-1280)) best = s;
        }
        return best == null ? choices[0] : best;
    }
    private static Range<Integer> chooseFps(Range<Integer>[] choices) {
        if (choices == null) return null;
        Range<Integer> best = null;
        for (Range<Integer> r : choices) if (r.contains(30) && (best == null || r.getUpper()-r.getLower() < best.getUpper()-best.getLower())) best = r;
        if (best == null) for (Range<Integer> r : choices) if (r.getUpper() <= 30 && (best == null || r.getUpper() > best.getUpper())) best = r;
        return best;
    }
    private CaptureRequest request(int template, Surface... surfaces) throws CameraAccessException {
        CaptureRequest.Builder b = camera.createCaptureRequest(template);
        for (Surface s : surfaces) b.addTarget(s);
        b.set(CaptureRequest.CONTROL_MODE, CaptureRequest.CONTROL_MODE_AUTO);
        int[] modes = characteristics.get(CameraCharacteristics.CONTROL_AF_AVAILABLE_MODES);
        if (modes != null) for (int mode : modes) if (mode == CaptureRequest.CONTROL_AF_MODE_CONTINUOUS_VIDEO) {
            b.set(CaptureRequest.CONTROL_AF_MODE, mode); break;
        }
        if (fps != null) b.set(CaptureRequest.CONTROL_AE_TARGET_FPS_RANGE, fps);
        return b.build();
    }
    private void closeSession() {
        ready = false;
        if (capture != null) {
            try { capture.stopRepeating(); capture.abortCaptures(); } catch (Exception ignored) { }
            capture.close(); capture = null;
        }
    }
    private void createPreview() {
        if (!active || camera == null || preview == null) { publish(); return; }
        closeSession(); final int gen = generation;
        try {
            camera.createCaptureSession(Collections.singletonList(preview), new CameraCaptureSession.StateCallback() {
                @Override public void onConfigured(CameraCaptureSession session) {
                    if (gen != generation || !active || !state.equals("stopped")) { session.close(); return; }
                    capture = session;
                    try { if(previewVisible)session.setRepeatingRequest(request(CameraDevice.TEMPLATE_PREVIEW, preview), null, handler); ready = true; }
                    catch (Exception e) { error = readable(e); ready = false; }
                    publish();
                }
                @Override public void onConfigureFailed(CameraCaptureSession session) { if (gen == generation) { error = "Configuration aperçu refusée"; publish(); } }
            }, handler);
        } catch (Exception e) { error = readable(e); publish(); }
    }

    public CompletableFuture<String> command(String action, String wantedSession) {
        return command(action,wantedSession,"");
    }
    public CompletableFuture<String> command(String action, String wantedSession, String obsLocal) {
        CompletableFuture<String> result = new CompletableFuture<>();
        handler.post(() -> {
            lastContact = SystemClock.elapsedRealtime();
            try {
                if (action.equals("start")) {
                    if (wantedSession == null || !wantedSession.matches("[a-zA-Z0-9_-]{1,80}")) throw new IllegalArgumentException("Identifiant de prise invalide");
                    if (!state.equals("stopped")) {
                        if (sessionId.equals(wantedSession)) { publish(); result.complete(snapshot); return; }
                        throw new IllegalStateException("Une autre prise est déjà active");
                    }
                    if (!active || !ready || camera == null) throw new IllegalStateException("Camera non prête : ouvrir l'application et vérifier les permissions");
                    startRecording(wantedSession, obsLocal, result); return;
                }
                if (action.equals("stop") && state.equals("stopped")) { publish(); result.complete(snapshot); return; }
                if (!sessionId.equals(wantedSession)) throw new IllegalStateException("Identifiant de prise différent : commande refusée");
                if (action.equals("pause")) {
                    if (state.equals("recording")) { recorder.pause(); pausedAt = SystemClock.elapsedRealtime(); state = "paused"; }
                    else if (!state.equals("paused")) throw new IllegalStateException("Aucune prise à mettre en pause");
                } else if (action.equals("resume")) {
                    if (state.equals("paused")) { recorder.resume(); pauseTotal += SystemClock.elapsedRealtime()-pausedAt; state = "recording"; }
                    else if (!state.equals("recording")) throw new IllegalStateException("Aucune prise à reprendre");
                } else if (action.equals("stop")) {
                    stopRecording();
                    if (!error.isEmpty()) throw new IllegalStateException(error);
                } else throw new IllegalArgumentException("Commande inconnue");
                publish(); result.complete(snapshot);
            } catch (Exception e) { publish(); result.completeExceptionally(e); }
        });
        return result;
    }
    private void startRecording(String wantedSession, String obsLocal, CompletableFuture<String> result) {
        final int gen = generation;
        try {
            state = "starting"; ready = false; error = ""; sessionId = wantedSession;
            recordStartNs=firstSensorNs=firstCallbackNs=recordStopNs=0;musicMarker=new JSONObject();stopMarker=new JSONObject();
            filename = "Prise_" + new SimpleDateFormat("yyyyMMdd_HHmmss_SSS", Locale.US).format(new Date()) + ".mp4";
            openOutput();
            catalog.begin(sessionId,filename,outputUri,obsLocal);
            recorder = new MediaRecorder();
            recorder.setAudioSource(MediaRecorder.AudioSource.CAMCORDER);
            recorder.setVideoSource(MediaRecorder.VideoSource.SURFACE);
            recorder.setOutputFormat(MediaRecorder.OutputFormat.MPEG_4);
            recorder.setOutputFile(outputFd.getFileDescriptor());
            recorder.setVideoEncodingBitRate(videoBitrate939);
            recorder.setVideoFrameRate(fps == null ? 30 : Math.min(30, fps.getUpper()));
            recorder.setVideoSize(videoSize.getWidth(), videoSize.getHeight());
            recorder.setVideoEncoder(MediaRecorder.VideoEncoder.H264);
            recorder.setAudioEncoder(MediaRecorder.AudioEncoder.AAC);
            recorder.setAudioEncodingBitRate(128000); recorder.setAudioSamplingRate(48000);
            Integer sensor = characteristics.get(CameraCharacteristics.SENSOR_ORIENTATION);
            Integer facing = characteristics.get(CameraCharacteristics.LENS_FACING);
            boolean front = facing != null && facing == CameraCharacteristics.LENS_FACING_FRONT;
            recorder.setOrientationHint(((sensor == null ? 0 : sensor) + (front ? rotation : -rotation) + 360) % 360);
            recorder.setOnErrorListener((mr, what, extra) -> { if (mr == recorder) { stopRecording(); error = "Erreur d'enregistrement " + what + "/" + extra; publish(); } });
            recorder.setOnInfoListener((mr, what, extra) -> {
                if (mr == recorder && (what == MediaRecorder.MEDIA_RECORDER_INFO_MAX_FILESIZE_REACHED || what == MediaRecorder.MEDIA_RECORDER_INFO_MAX_DURATION_REACHED)) {
                    stopRecording(); error = "Limite de fichier ou de durée atteinte"; publish();
                }
            });
            recorder.prepare();
            recordingSurface = recorder.getSurface();
            closeSession();
            camera.createCaptureSession(Arrays.asList(preview, recordingSurface), new CameraCaptureSession.StateCallback() {
                @Override public void onConfigured(CameraCaptureSession session) {
                    if (gen != generation || !active || !state.equals("starting")) { session.close(); result.completeExceptionally(new IllegalStateException("Démarrage annulé")); return; }
                    capture = session;
                    try {
                        session.setRepeatingRequest(previewVisible?request(CameraDevice.TEMPLATE_RECORD,preview,recordingSurface):request(CameraDevice.TEMPLATE_RECORD,recordingSurface), timingCallback, handler);
                        recordStartNs=SystemClock.elapsedRealtimeNanos();recorder.start(); started = SystemClock.elapsedRealtime(); pauseTotal = 0; pausedAt = 0;
                        state = "recording"; ready = true; publish(); result.complete(snapshot);
                    } catch (Exception e) { failStart(e, result); }
                }
                @Override public void onConfigureFailed(CameraCaptureSession session) {
                    if (gen == generation) failStart(new IllegalStateException("Format vidéo refusé. Essayer 720p."), result);
                    else result.completeExceptionally(new IllegalStateException("Camera fermée"));
                }
            }, handler);
            handler.postDelayed(() -> {
                if (!result.isDone() && gen == generation && state.equals("starting")) failStart(new IllegalStateException("Délai de démarrage caméra dépassé"), result);
            }, 8000);
        } catch (Exception e) { failStart(e, result); }
        publish();
    }
    private void failStart(Exception e, CompletableFuture<String> result) {
        closeSession(); releaseRecorder(); finishOutput(false); state = "stopped";
        try{catalog.finish(sessionId,false);}catch(Exception ignored){}
        error = readable(e); createPreview(); publish(); result.completeExceptionally(e);
    }
    private void stopRecording() {
        if (state.equals("stopped")) return;
        recordStopNs=SystemClock.elapsedRealtimeNanos();
        boolean wasStarted = state.equals("recording") || state.equals("paused");
        // Keep the capture session and its surfaces alive while the encoder drains.
        // abortCaptures/close before MediaRecorder.stop could discard pending frames.
        ready = false; String stopError = "";
        if(capture!=null)try{capture.stopRepeating();}catch(Exception ignored){}
        if (recorder != null && wasStarted) {
            try { recorder.stop(); } catch (Exception e) {
                stopError = "Finalisation échouée : " + filename + " à vérifier. " + readable(e);
                android.util.Log.e("ObsUsbCamera", "Finalisation " + filename + ", état=" + state + ", durée active ms=" + ((state.equals("paused")?pausedAt:SystemClock.elapsedRealtime())-started-pauseTotal), e);
            }
        }
        closeSession();
        releaseRecorder(); finishOutput(wasStarted); state = "stopped";
        if (!stopError.isEmpty()) error = stopError;
        try{catalog.metadata(sessionId,timing());catalog.finish(sessionId,wasStarted && stopError.isEmpty() && error.isEmpty());}catch(Exception e){error=readable(e);}
        createPreview(); publish();
    }
    private void releaseRecorder() {
        if (recorder != null) { try { recorder.reset(); } catch (Exception ignored) {} recorder.release(); recorder = null; }
        recordingSurface=null;
    }
    private void openOutput() throws Exception {
        mediaPending = false;
        if (!folder.isEmpty()) {
            Uri tree = Uri.parse(folder);
            Uri parent = DocumentsContract.buildDocumentUriUsingTree(tree, DocumentsContract.getTreeDocumentId(tree));
            outputUri = DocumentsContract.createDocument(context.getContentResolver(), parent, "video/mp4", filename);
        } else if (Build.VERSION.SDK_INT >= 29) {
            ContentValues values = new ContentValues();
            values.put(MediaStore.Video.Media.DISPLAY_NAME, filename); values.put(MediaStore.Video.Media.MIME_TYPE, "video/mp4");
            values.put(MediaStore.Video.Media.RELATIVE_PATH, "Movies/Prise-USB"); values.put(MediaStore.Video.Media.IS_PENDING, 1);
            outputUri = context.getContentResolver().insert(MediaStore.Video.Media.EXTERNAL_CONTENT_URI, values); mediaPending = true;
        } else {
            File dir = context.getExternalFilesDir(Environment.DIRECTORY_MOVIES);
            if (dir == null) throw new IllegalStateException("Stockage indisponible");
            if (!dir.exists() && !dir.mkdirs()) throw new IllegalStateException("Impossible de créer le dossier vidéo");
            outputUri = Uri.fromFile(new File(dir, filename));
        }
        if (outputUri == null) throw new IllegalStateException("Impossible de créer le fichier vidéo");
        outputFd = context.getContentResolver().openFileDescriptor(outputUri, "rw");
        if (outputFd == null) throw new IllegalStateException("Impossible d'ouvrir le fichier vidéo");
    }
    private void finishOutput(boolean keep) {
        if (outputFd != null) { try { outputFd.close(); } catch (Exception ignored) {} outputFd = null; }
        if (outputUri != null) {
            try {
                if (!keep) {
                    if ("file".equals(outputUri.getScheme())) new File(outputUri.getPath()).delete();
                    else if (!folder.isEmpty()) DocumentsContract.deleteDocument(context.getContentResolver(), outputUri);
                    else context.getContentResolver().delete(outputUri, null, null);
                } else {
                    if (mediaPending) { ContentValues v = new ContentValues(); v.put(MediaStore.Video.Media.IS_PENDING, 0); context.getContentResolver().update(outputUri, v, null, null); }
                    lastFile = outputUri.toString();
                }
            } catch (Exception e) { error = "Stockage : " + readable(e); }
        }
        outputUri = null; mediaPending = false;
    }
    private void closeCamera() {
        generation++; opening = false; closeSession();
        if (camera != null) { camera.close(); camera = null; }
        if (preview != null) { preview.release(); preview = null; }
    }
    private void publish() {
        try {
            JSONObject j = new JSONObject();
            j.put("protocol", 3); j.put("version","9.36.2");j.put("timing",timing()); j.put("state", state); j.put("ready", ready && active);
            j.put("session", sessionId); j.put("file", filename); j.put("lastFile", lastFile); j.put("error", error);
            j.put("videoBitrate",videoBitrate939);j.put("requestedHeight",maxHeight);j.put("width", videoSize.getWidth()); j.put("height", videoSize.getHeight());
            long now = SystemClock.elapsedRealtime();
            j.put("durationMs", state.equals("recording") || state.equals("paused") ? Math.max(0, (state.equals("paused") ? pausedAt : now)-started-pauseTotal) : 0);
            j.put("linkAgeMs", lastContact == 0 ? -1 : now-lastContact);
            snapshot = j.toString();
        } catch (Exception ignored) { }
    }
    public void tick() { handler.post(this::publish); }
    private static String readable(Exception e) { return e.getMessage() == null ? e.getClass().getSimpleName() : e.getMessage(); }
}
