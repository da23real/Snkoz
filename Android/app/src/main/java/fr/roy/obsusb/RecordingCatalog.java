package fr.roy.obsusb;

import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.os.Environment;
import android.os.ParcelFileDescriptor;
import android.provider.DocumentsContract;
import android.provider.OpenableColumns;
import android.util.AtomicFile;
import org.json.JSONArray;
import org.json.JSONObject;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.text.SimpleDateFormat;
import java.util.*;

/** Durable metadata, independent of the activity and of the chosen storage provider. */
public final class RecordingCatalog {
    private final Context context;
    private final AtomicFile file;
    private JSONArray entries = new JSONArray();
    public RecordingCatalog(Context context) {
        this.context = context.getApplicationContext();
        file = new AtomicFile(new File(context.getFilesDir(), "recordings-v2.json"));
        if (file.getBaseFile().exists()) {
            try (InputStream in = file.openRead()) { entries = new JSONArray(new String(readAll(in), StandardCharsets.UTF_8)); }
            catch (Exception e) { throw new IllegalStateException("Index des vidéos illisible. Les vidéos sont conservées.", e); }
        }
        // A terminated process cannot leave an entry claiming it is still recording.
        boolean changed=false;
        for(int i=0;i<entries.length();i++) {
            JSONObject entry=entries.optJSONObject(i);
            if(entry!=null && entry.optString("state").equals("recording")) { put(entry,"state","incomplete"); changed=true; }
        }
        if(changed) save();
    }
    private static byte[] readAll(InputStream in) throws IOException {
        ByteArrayOutputStream out=new ByteArrayOutputStream(); byte[] buffer=new byte[8192]; int n;
        while((n=in.read(buffer))!=-1)out.write(buffer,0,n); return out.toByteArray();
    }
    private static void put(JSONObject obj,String key,Object value) { try {obj.put(key,value);}catch(Exception e){throw new IllegalStateException(e);} }
    private synchronized void save() {
        FileOutputStream out=null;
        try {out=file.startWrite();out.write(entries.toString().getBytes(StandardCharsets.UTF_8));file.finishWrite(out);}
        catch(Exception e) {if(out!=null)file.failWrite(out);throw new IllegalStateException("Impossible de mémoriser les prises",e);}
    }
    public synchronized void begin(String id,String name,Uri uri,String obsLocal) {
        if(find(id)!=null)throw new IllegalStateException("Identifiant de prise déjà utilisé");
        JSONObject entry=new JSONObject();put(entry,"id",id);put(entry,"name",name);put(entry,"uri",uri.toString());
        String local=obsLocal;
        boolean pc=local!=null && local.matches("\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}(\\.\\d{1,7})?");
        if(!pc)local=new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS",Locale.US).format(new Date());
        put(entry,"startedLocal",local);put(entry,"timeSource",pc?"pc":"phone");put(entry,"state","recording");
        entries.put(entry);save();
    }
    public synchronized void finish(String id,boolean valid) {
        JSONObject entry=find(id);if(entry==null)return;put(entry,"state",valid?"complete":"incomplete");save();
    }
    public synchronized void metadata(String id,JSONObject timing) {
        JSONObject entry=find(id);if(entry==null)return;put(entry,"timing",timing);save();
    }
    private JSONObject find(String id) {
        for(int i=0;i<entries.length();i++) {JSONObject j=entries.optJSONObject(i);if(j!=null && j.optString("id").equals(id))return j;}return null;
    }
    private boolean knownUri(String uri) {for(int i=0;i<entries.length();i++)if(entries.optJSONObject(i).optString("uri").equals(uri))return true;return false;}
    private void importOld(String name,Uri uri) {
        if(!name.matches("OBS_\\d{8}_\\d{6}_\\d{3}\\.mp4") || knownUri(uri.toString()))return;
        try {
            Date date=new SimpleDateFormat("yyyyMMdd_HHmmss_SSS",Locale.US).parse(name.substring(4,name.length()-4));
            JSONObject j=new JSONObject();put(j,"id",UUID.randomUUID().toString());put(j,"name",name);put(j,"uri",uri.toString());
            put(j,"startedLocal",new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS",Locale.US).format(date));
            put(j,"timeSource","phone-legacy");put(j,"state","complete");entries.put(j);
        }catch(Exception ignored){}
    }
    private void importLegacy() {
        int before=entries.length();
        File dir=context.getExternalFilesDir(Environment.DIRECTORY_MOVIES);
        File[] files=dir==null?null:dir.listFiles();
        if(files!=null)for(File f:files)if(f.isFile())importOld(f.getName(),Uri.fromFile(f));
        String folder=context.getSharedPreferences("config",Context.MODE_PRIVATE).getString("folder","");
        if(!folder.isEmpty()) {
            Uri tree=Uri.parse(folder);
            try (Cursor c=context.getContentResolver().query(DocumentsContract.buildChildDocumentsUriUsingTree(tree,DocumentsContract.getTreeDocumentId(tree)),new String[]{DocumentsContract.Document.COLUMN_DOCUMENT_ID,DocumentsContract.Document.COLUMN_DISPLAY_NAME},null,null,null)) {
                if(c!=null)while(c.moveToNext())importOld(c.getString(1),DocumentsContract.buildDocumentUriUsingTree(tree,c.getString(0)));
            }catch(Exception ignored){}
        }
        if(entries.length()!=before)save();
    }
    public synchronized String list() throws Exception {
        importLegacy();JSONArray result=new JSONArray();
        for(int i=0;i<entries.length();i++) {
            JSONObject entry=entries.getJSONObject(i),copy=new JSONObject(entry.toString());copy.remove("uri");
            long size=-1;boolean available=false;
            Uri uri=Uri.parse(entry.getString("uri"));
            try(ParcelFileDescriptor fd=context.getContentResolver().openFileDescriptor(uri,"r")) {
                if(fd!=null){size=fd.getStatSize();available=true;}
            }catch(Exception ignored){}
            copy.put("size",size);copy.put("available",available);result.put(copy);
        }
        return new JSONObject().put("recordings",result).toString();
    }
    public synchronized InputStream open(String id) throws Exception {
        JSONObject entry=find(id);
        if(entry==null || !(entry.optString("state").equals("complete") || entry.optString("state").equals("incomplete")))throw new FileNotFoundException("Prise terminée introuvable");
        InputStream in=context.getContentResolver().openInputStream(Uri.parse(entry.getString("uri")));
        if(in==null)throw new FileNotFoundException("Vidéo inaccessible");return in;
    }
    public String digest(String id) throws Exception {
        MessageDigest sha=MessageDigest.getInstance("SHA-256");long size=0;
        try(InputStream in=open(id)){byte[] buffer=new byte[128*1024];int n;while((n=in.read(buffer))!=-1){sha.update(buffer,0,n);size+=n;}}
        StringBuilder hash=new StringBuilder();for(byte b:sha.digest())hash.append(String.format(Locale.US,"%02x",b));
        return new JSONObject().put("sha256",hash.toString()).put("size",size).toString();
    }
}
