package fr.roy.obsusb;

import android.Manifest;
import android.app.*;
import android.content.*;
import android.content.pm.PackageManager;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.*;
import android.net.Uri;
import android.os.*;
import android.view.*;
import android.widget.*;
import org.json.JSONObject;
import java.security.SecureRandom;
import java.util.*;

public final class MainActivity extends Activity implements TextureView.SurfaceTextureListener {
    private CameraEngine engine;
    private CameraService service;
    private boolean bound;
    private TextureView texture;
    private TextView status, detail, folderLabel, link;
    private Button folderButton, defaultButton, stopButton;
    private Spinner cameras, quality, previewRotation;
    private final ArrayList<String> cameraIds = new ArrayList<>();
    private final Handler ui = new Handler(Looper.getMainLooper());
    private SharedPreferences prefs;
    private boolean resumed, changingUi;
    private String token, serverError = "";
    private int previousWidth,previousHeight,previousRotation=-1,previousRawWidth,previousRawHeight;
    private static void changedText(TextView view,String value){if(!value.contentEquals(view.getText()))view.setText(value);}
    private final ServiceConnection connection=new ServiceConnection(){
        @Override public void onServiceConnected(ComponentName name,IBinder binder){
            service=((CameraService.LocalBinder)binder).getService();engine=service.engine;serverError=service.error;
            if(engine==null){status.setText("SERVICE NOT READY");detail.setText(serverError);return;}
            if(service.retainedTexture!=null && texture.getSurfaceTexture()!=service.retainedTexture)texture.setSurfaceTexture(service.retainedTexture);
            configure();activate();
            engine.previewSource=thisPreview();
            if(Build.VERSION.SDK_INT>=33 && checkSelfPermission(Manifest.permission.POST_NOTIFICATIONS)!=PackageManager.PERMISSION_GRANTED && !prefs.getBoolean("notificationAsked",false)){
                prefs.edit().putBoolean("notificationAsked",true).apply();requestPermissions(new String[]{Manifest.permission.POST_NOTIFICATIONS},3);
            }
        }
        @Override public void onServiceDisconnected(ComponentName name){engine=null;service=null;bound=false;}
    };
    private final Runnable refresh = new Runnable() {
        @Override public void run() { if (engine != null) { engine.tick(); render(); } ui.postDelayed(this, 500); }
    };
    @Override public void onCreate(Bundle saved) {
        super.onCreate(saved);
        prefs = getSharedPreferences("config", MODE_PRIVATE);
        token = prefs.getString("token", "");
        if (token.isEmpty()) {
            byte[] bytes = new byte[6]; new SecureRandom().nextBytes(bytes); StringBuilder b = new StringBuilder();
            for (byte value : bytes) b.append(String.format(Locale.US, "%02X", value));
            token = b.toString(); prefs.edit().putString("token", token).apply();
        }
        buildUi();
        ui.post(refresh);
    }
    private int dp(int n) { return (int)(n*getResources().getDisplayMetrics().density+.5f); }
    private CameraEngine.PreviewSource thisPreview() {
        return () -> {
            java.util.concurrent.CompletableFuture<byte[]> result=new java.util.concurrent.CompletableFuture<>();
            ui.post(() -> {
                android.graphics.Bitmap bitmap=null;
                try {
                    if(!resumed || engine==null || !texture.isAvailable() || !new JSONObject(engine.status()).optString("state").equals("stopped"))throw new IllegalStateException("Open the phone app with recording stopped");
                    bitmap=texture.getBitmap(480,270);
                    if(bitmap==null)throw new IllegalStateException("Preview not ready");
                    java.io.ByteArrayOutputStream bytes=new java.io.ByteArrayOutputStream();bitmap.compress(android.graphics.Bitmap.CompressFormat.JPEG,65,bytes);
                    if(!new JSONObject(engine.status()).optString("state").equals("stopped"))throw new IllegalStateException("Preview disabled during recording");
                    result.complete(bytes.toByteArray());
                }catch(Exception e){result.completeExceptionally(e);}finally{if(bitmap!=null)bitmap.recycle();}
            });
            return result;
        };
    }
    private TextView text(String value, int size, int color) {
        TextView t = new TextView(this); t.setText(value); t.setTextSize(size); t.setTextColor(color); t.setPadding(0, dp(3), 0, dp(3)); return t;
    }
    private void buildUi() {
        LinearLayout root = new LinearLayout(this); root.setOrientation(LinearLayout.HORIZONTAL); root.setBackgroundColor(Color.rgb(15, 23, 34)); root.setPadding(dp(12),dp(12),dp(12),dp(12));
        root.setOnApplyWindowInsetsListener((view, insets) -> {
            view.setPadding(dp(12)+insets.getSystemWindowInsetLeft(), dp(12)+insets.getSystemWindowInsetTop(), dp(12)+insets.getSystemWindowInsetRight(), dp(12)+insets.getSystemWindowInsetBottom());
            return insets;
        });
        FrameLayout frame = new FrameLayout(this); frame.setBackgroundColor(Color.BLACK);
        texture = new TextureView(this); texture.setSurfaceTextureListener(this);
        texture.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
        frame.addView(texture, new FrameLayout.LayoutParams(-1,-1));
        TextView badge = text("  LOCAL RECORDING · OPTIONAL USB PC PREVIEW  ", 11, Color.WHITE); badge.setBackgroundColor(0xAA000000);
        FrameLayout.LayoutParams badgeParams = new FrameLayout.LayoutParams(-1,-2,Gravity.BOTTOM); frame.addView(badge,badgeParams);
        root.addView(frame,new LinearLayout.LayoutParams(0,-1,1));
        ScrollView scroll = new ScrollView(this); LinearLayout panel = new LinearLayout(this); panel.setOrientation(LinearLayout.VERTICAL); panel.setPadding(dp(14),0,0,0); scroll.addView(panel);
        root.addView(scroll,new LinearLayout.LayoutParams(dp(310),-1));
        panel.addView(text("USB RECORDING", 22, Color.WHITE));
        panel.addView(text("USB: automatic pairing with Snkoz", 18, Color.rgb(84,214,188)));
        status = text("PREPARING",24,Color.WHITE); panel.addView(status);
        detail = text("",13,Color.LTGRAY); panel.addView(detail);
        link = text("Waiting for the Windows application",13,Color.LTGRAY); panel.addView(link);
        panel.addView(text("Camera",13,Color.LTGRAY)); cameras = new Spinner(this); panel.addView(cameras);
        ArrayList<String> labels = new ArrayList<>();
        try {
            CameraManager manager = (CameraManager)getSystemService(CAMERA_SERVICE);
            for (String id : manager.getCameraIdList()) {
                Integer facing = manager.getCameraCharacteristics(id).get(CameraCharacteristics.LENS_FACING);
                cameraIds.add(id); labels.add((facing != null && facing == CameraCharacteristics.LENS_FACING_FRONT ? "Front" : "Rear") + " · " + id);
            }
        } catch (Exception e) { labels.add("Camera unavailable"); }
        cameras.setAdapter(new ArrayAdapter<>(this,android.R.layout.simple_spinner_dropdown_item,labels));
        int selected = cameraIds.indexOf(prefs.getString("camera",""));
        if (selected < 0) { selected = 0; for(int i=0;i<labels.size();i++) if(labels.get(i).startsWith("Rear")) { selected=i; break; } }
        cameras.setSelection(selected);
        panel.addView(text("Target quality · H.264 + AAC audio",13,Color.LTGRAY));
        quality = new Spinner(this); quality.setAdapter(new ArrayAdapter<>(this,android.R.layout.simple_spinner_dropdown_item,new String[]{"720p · high quality · 6 Mbit/s", "720p · smaller files · 3 Mbit/s", "480p · 1,5 Mbit/s", "360p · 0,8 Mbit/s"}));
        quality.setSelection(Math.max(0,Math.min(3,prefs.getInt("quality939",0)))); panel.addView(quality);
        panel.addView(text("Preview rotation only",13,Color.LTGRAY));
        previewRotation=new Spinner(this);previewRotation.setAdapter(new ArrayAdapter<>(this,android.R.layout.simple_spinner_dropdown_item,new String[]{"0°","90°","180°","270°"}));
        previewRotation.setSelection(((prefs.getInt("previewRotation",0)+90)%360)/90);panel.addView(previewRotation);
        previewRotation.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener(){
            @Override public void onItemSelected(AdapterView<?> parent,View view,int position,long id){prefs.edit().putInt("previewRotation",(position*90+270)%360).apply();render();}
            @Override public void onNothingSelected(AdapterView<?> parent){}
        });
        folderLabel = text("",12,Color.LTGRAY); panel.addView(folderLabel); updateFolder();
        folderButton = new Button(this); folderButton.setText("Choose folder / SD card"); panel.addView(folderButton);
        panel.addView(text("Create a USB-Recordings folder in internal storage or on the SD card, then choose Use this folder. This affects new recordings; earlier recordings remain available for transfer.",12,Color.LTGRAY));
        folderButton.setOnClickListener(v -> {
            Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT_TREE);
            intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION | Intent.FLAG_GRANT_WRITE_URI_PERMISSION | Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION);
            startActivityForResult(intent,2);
        });
        defaultButton = new Button(this); defaultButton.setText("Restore default folder"); panel.addView(defaultButton);
        defaultButton.setOnClickListener(v -> { prefs.edit().remove("folder").apply(); updateFolder(); configure(); });
        stopButton = new Button(this); stopButton.setText("EMERGENCY LOCAL STOP"); panel.addView(stopButton);
        stopButton.setOnClickListener(v -> new AlertDialog.Builder(this).setTitle("Stop recording on this phone?")
            .setMessage("Other PC tracks keep recording. The local file is kept.")
            .setNegativeButton("Cancel",null).setPositiveButton("Stop",(dialog,which)-> {
                try { if(engine!=null)engine.command("stop",new JSONObject(engine.status()).optString("session")); } catch(Exception ignored) {}
            }).show());
        panel.addView(text("Recording continues in the background with the screen off. Use the notification to close the USB camera. Wi-Fi and Bluetooth can remain disabled.",12,Color.LTGRAY));
        Button close=new Button(this);close.setText("Close USB camera");panel.addView(close);
        close.setOnClickListener(v->new AlertDialog.Builder(this).setTitle("Close USB camera ?").setMessage("Any active recording will stop on this phone only.").setNegativeButton("Cancel",null).setPositiveButton("Close",(d,w)->{stopService(new Intent(this,CameraService.class));finish();}).show());
        AdapterView.OnItemSelectedListener listener = new AdapterView.OnItemSelectedListener() {
            @Override public void onItemSelected(AdapterView<?> parent, View view, int position, long id) { if (!changingUi) configure(); }
            @Override public void onNothingSelected(AdapterView<?> parent) {}
        };
        cameras.setOnItemSelectedListener(listener); quality.setOnItemSelectedListener(listener);
        setContentView(root); configure();
    }
    private int rotation() {
        int value = getWindowManager().getDefaultDisplay().getRotation();
        return value == Surface.ROTATION_90 ? 90 : value == Surface.ROTATION_180 ? 180 : value == Surface.ROTATION_270 ? 270 : 0;
    }
    private void configure() {
        if (cameraIds.isEmpty()) return;
        int pos = Math.max(0,cameras.getSelectedItemPosition()); String id = cameraIds.get(pos); int q=Math.max(0,quality.getSelectedItemPosition()); int height=new int[]{720,720,480,360}[q];int bitrate=new int[]{6000000,3000000,1500000,800000}[q];
        prefs.edit().putString("camera",id).putInt("height",height).putInt("quality939",q).apply();
        if(engine!=null)engine.configure(id,height,prefs.getString("folder",""),rotation(),bitrate);
    }
    private void updateFolder() {
        String folder = prefs.getString("folder","");
        folderLabel.setText(folder.isEmpty() ? (Build.VERSION.SDK_INT >= 29 ? "Destination: Films / Prise-USB" : "Destination: private app video folder (choose a folder to keep recordings after uninstalling)") : "Destination: " + Uri.decode(folder));
    }
    @Override protected void onActivityResult(int request,int result,Intent data) {
        super.onActivityResult(request,result,data);
        if(request==2 && result==RESULT_OK && data!=null && data.getData()!=null) {
            try {
                Uri uri=data.getData(); getContentResolver().takePersistableUriPermission(uri,data.getFlags() & (Intent.FLAG_GRANT_READ_URI_PERMISSION|Intent.FLAG_GRANT_WRITE_URI_PERMISSION));
                prefs.edit().putString("folder",uri.toString()).apply(); updateFolder(); configure();
            } catch(Exception e) { Toast.makeText(this,"Folder rejected: " + e.getMessage(),Toast.LENGTH_LONG).show(); }
        }
    }
    private void render() {
        if(engine==null)return;
        try {
            JSONObject j=new JSONObject(engine.status()); String state=j.optString("state"); boolean idle=state.equals("stopped");
            String label=state.equals("recording")?"RECORDING":state.equals("paused")?"PAUSED":state.equals("starting")?"STARTING":j.optBoolean("ready")?"READY FOR PC":"CAMERA NOT READY";
            changedText(status,label);int color=state.equals("recording")?Color.rgb(255,105,110):Color.rgb(84,214,188);if(status.getCurrentTextColor()!=color)status.setTextColor(color);
            long seconds=j.optLong("durationMs")/1000;
            String message=String.format(Locale.FRANCE,"%02d:%02d · %d × %d\n%s",seconds/60,seconds%60,j.optInt("width"),j.optInt("height"),j.optString("file"));
            if(!j.optString("error").isEmpty()) message+="\n"+j.optString("error");
            if(!serverError.isEmpty()) message+="\n"+serverError;
            JSONObject timing=j.optJSONObject("timing");JSONObject marker=timing==null?null:timing.optJSONObject("musicMarker");
            if(marker!=null && marker.has("pcEpoch"))message+="\nMusical start: "+new java.text.SimpleDateFormat("HH:mm:ss.SSS",Locale.FRANCE).format(new Date((long)(marker.optDouble("pcEpoch")*1000)));
            changedText(detail,message);
            int viewWidth=texture.getWidth(), viewHeight=texture.getHeight();
            if(viewWidth>0 && viewHeight>0 && j.optInt("height")>0 && (viewWidth!=previousWidth || viewHeight!=previousHeight || prefs.getInt("previewRotation",0)!=previousRotation || j.optInt("width")!=previousRawWidth || j.optInt("height")!=previousRawHeight)) {
                int degrees=prefs.getInt("previewRotation",0);boolean swapped=degrees%180!=0;
                float rawWidth=j.optInt("width"),rawHeight=j.optInt("height");
                float fit=Math.min(viewWidth/(swapped?rawHeight:rawWidth),viewHeight/(swapped?rawWidth:rawHeight));
                Matrix matrix=new Matrix();matrix.setScale(rawWidth*fit/viewWidth,rawHeight*fit/viewHeight,viewWidth/2f,viewHeight/2f);
                matrix.postRotate(degrees,viewWidth/2f,viewHeight/2f);texture.setTransform(matrix);
                previousWidth=viewWidth;previousHeight=viewHeight;previousRotation=degrees;previousRawWidth=j.optInt("width");previousRawHeight=j.optInt("height");
            }
            long age=j.optLong("linkAgeMs",-1);
            changedText(link,age<0?"Waiting for PC software":"USB contact received · recording stored locally");
            cameras.setEnabled(idle); quality.setEnabled(idle); folderButton.setEnabled(idle); defaultButton.setEnabled(idle); stopButton.setEnabled(!idle);
        } catch(Exception ignored) {}
    }
    private boolean permissions() { return checkSelfPermission(Manifest.permission.CAMERA)==PackageManager.PERMISSION_GRANTED && checkSelfPermission(Manifest.permission.RECORD_AUDIO)==PackageManager.PERMISSION_GRANTED; }
    private void ensureService(){
        if(!resumed || !permissions())return;
        if(service!=null && !service.running){if(bound)unbindService(connection);bound=false;service=null;engine=null;}
        if(!bound){Intent intent=new Intent(this,CameraService.class);startForegroundService(intent);bound=bindService(intent,connection,BIND_AUTO_CREATE);}
        else activate();
    }
    private void activate() {
        if(resumed && engine!=null && service!=null && texture.isAvailable() && permissions()) {
            engine.previewSource=thisPreview();
            if(service.retainedTexture==null)service.retainedTexture=texture.getSurfaceTexture();
            else if(texture.getSurfaceTexture()!=service.retainedTexture)texture.setSurfaceTexture(service.retainedTexture);
            engine.activate(service.retainedTexture,rotation());
        }
    }
    @Override protected void onResume() { super.onResume(); resumed=true; if(!permissions()) requestPermissions(new String[]{Manifest.permission.CAMERA,Manifest.permission.RECORD_AUDIO},1); else ensureService(); }
    @Override protected void onPause() { resumed=false; if(engine!=null){engine.previewSource=null;engine.setPreviewVisible(false);} super.onPause(); }
    @Override protected void onDestroy() { ui.removeCallbacks(refresh);if(bound)unbindService(connection);bound=false;super.onDestroy(); }
    @Override public void onRequestPermissionsResult(int request,String[] names,int[] grants) {
        super.onRequestPermissionsResult(request,names,grants);
        if(permissions())ensureService();else Toast.makeText(this,"Camera et microphone requis pour enregistrer avec le son",Toast.LENGTH_LONG).show();
    }
    @Override public void onSurfaceTextureAvailable(SurfaceTexture t,int w,int h) { activate(); }
    @Override public void onSurfaceTextureSizeChanged(SurfaceTexture t,int w,int h) {}
    @Override public boolean onSurfaceTextureDestroyed(SurfaceTexture t) { if(engine!=null)engine.setPreviewVisible(false); return service==null || service.retainedTexture!=t; }
    @Override public void onSurfaceTextureUpdated(SurfaceTexture t) {}
}
