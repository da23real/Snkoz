package fr.roy.obsusb;

import android.app.*;
import android.content.*;
import android.content.pm.ServiceInfo;
import android.graphics.SurfaceTexture;
import android.os.*;
import org.json.JSONObject;

/** Started from the visible activity; keeps camera/microphone alive with a visible notification. */
public final class CameraService extends Service {
    public final class LocalBinder extends Binder { public CameraService getService(){return CameraService.this;} }
    private final IBinder binder=new LocalBinder();
    public CameraEngine engine;
    public SurfaceTexture retainedTexture;
    public String error="";
    public boolean running;
    private UsbServer server;
    private PowerManager.WakeLock wakeLock;
    private final Handler main=new Handler(Looper.getMainLooper());
    private String lastState="";
    private final Runnable tick=new Runnable(){@Override public void run(){
        if(!running)return;
        if(engine!=null){engine.tick();try{String state=new JSONObject(engine.status()).optString("state");if(!state.equals(lastState)){lastState=state;((NotificationManager)getSystemService(NOTIFICATION_SERVICE)).notify(7,notification());}}catch(Exception ignored){}}
        main.postDelayed(this,1000);
    }};
    @Override public void onCreate(){
        super.onCreate();
        ((NotificationManager)getSystemService(NOTIFICATION_SERVICE)).createNotificationChannel(new NotificationChannel("camera-usb","Camera USB active",NotificationManager.IMPORTANCE_LOW));
    }
    private Notification notification(){
        int flags=PendingIntent.FLAG_UPDATE_CURRENT|PendingIntent.FLAG_IMMUTABLE;
        PendingIntent open=PendingIntent.getActivity(this,0,new Intent(this,MainActivity.class),flags);
        PendingIntent stop=PendingIntent.getService(this,1,new Intent(this,CameraService.class).setAction("STOP"),flags);
        String text=lastState.equals("recording")?"Enregistrement en cours · écran éteint autorisé":lastState.equals("paused")?"Prise en pause · commandes USB actives":"Prête pour le PC · commandes USB actives";
        return new Notification.Builder(this,"camera-usb").setSmallIcon(android.R.drawable.presence_video_online).setContentTitle("Prise USB")
            .setContentText(text).setContentIntent(open).setOngoing(true).setOnlyAlertOnce(true)
            .addAction(new Notification.Action.Builder(null,"Close USB camera",stop).build()).build();
    }
    @Override public int onStartCommand(Intent intent,int flags,int startId){
        if(intent!=null && "STOP".equals(intent.getAction())){shutdown();stopForeground(true);stopSelf();return START_NOT_STICKY;}
        if(running)return START_NOT_STICKY;
        try {
            if(Build.VERSION.SDK_INT>=29)startForeground(7,notification(),ServiceInfo.FOREGROUND_SERVICE_TYPE_CAMERA|ServiceInfo.FOREGROUND_SERVICE_TYPE_MICROPHONE);
            else startForeground(7,notification());
            wakeLock=((PowerManager)getSystemService(POWER_SERVICE)).newWakeLock(PowerManager.PARTIAL_WAKE_LOCK,"obsusb:camera");wakeLock.acquire();
            engine=new CameraEngine(this);
            server=new UsbServer(engine,getSharedPreferences("config",MODE_PRIVATE).getString("token",""));
            running=true;main.post(tick);
        }catch(Exception e){error="Service caméra : "+e.getMessage();shutdown();stopForeground(true);stopSelf();}
        return START_NOT_STICKY;
    }
    private void shutdown(){
        running=false;main.removeCallbacks(tick);
        if(server!=null){server.close();server=null;}
        if(engine!=null){engine.shutdown();engine=null;}
        retainedTexture=null;
        if(wakeLock!=null && wakeLock.isHeld())wakeLock.release();wakeLock=null;
    }
    @Override public IBinder onBind(Intent intent){return binder;}
    @Override public void onDestroy(){shutdown();super.onDestroy();}
}
