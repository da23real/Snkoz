package fr.roy.obsusb;

import org.json.JSONObject;
import java.io.*;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.Locale;
import java.util.concurrent.TimeUnit;

/** Reachable through adb forward only: IPv4 loopback, never 0.0.0.0. */
public final class UsbServer implements AutoCloseable {
    public static final int PORT = 18765;
    private final CameraEngine engine;
    private final String token;
    private final ServerSocket server;
    private volatile boolean closed;
    public UsbServer(CameraEngine engine, String token) throws IOException {
        this.engine = engine; this.token = token;
        server = new ServerSocket(); server.setReuseAddress(true);
        server.bind(new InetSocketAddress(InetAddress.getByName("127.0.0.1"), PORT), 4);
        Thread worker = new Thread(this::serve, "CommandesUsb"); worker.setDaemon(true); worker.start();
    }
    private void serve() {
        while (!closed) {
            try (Socket socket = server.accept()) { socket.setSoTimeout(3000); handle(socket); }
            catch (IOException ignored) { }
        }
    }
    private static String line(InputStream in) throws IOException {
        ByteArrayOutputStream b = new ByteArrayOutputStream();
        for (int n = 0; n < 2048; n++) {
            int c = in.read(); if (c == -1) throw new EOFException(); if (c == '\n') return b.toString("US-ASCII").replace("\r", ""); b.write(c);
        }
        throw new IOException("Entête trop longue");
    }
    private void handle(Socket socket) throws IOException {
        InputStream in = socket.getInputStream();
        try {
            String[] request = line(in).split(" ");
            if (request.length != 3) throw new IllegalArgumentException("Requête invalide");
            int length = 0; String auth = "";
            for (int n = 0; n < 32; n++) {
                String header = line(in); if (header.isEmpty()) break;
                int colon = header.indexOf(':'); if (colon < 0) throw new IllegalArgumentException("Entête invalide");
                String key = header.substring(0, colon).trim().toLowerCase(Locale.ROOT), value = header.substring(colon+1).trim();
                if (key.equals("content-length")) length = Integer.parseInt(value);
                if (key.equals("authorization")) auth = value;
                if (key.equals("transfer-encoding")) throw new IllegalArgumentException("Transfert non pris en charge");
                if (n == 31) throw new IllegalArgumentException("Trop d'entêtes");
            }
            if(request[0].equals("GET") && request[1].equals("/pair")){reply(socket,200,new JSONObject().put("token",token).toString());return;} // USB/ADB-authorized loopback only; no network listener.
            if (!MessageDigest.isEqual(auth.getBytes(StandardCharsets.UTF_8), ("Bearer " + token).getBytes(StandardCharsets.UTF_8))) {
                reply(socket, 401, "{\"error\":\"Code USB incorrect\"}"); return;
            }
            if (length < 0 || length > 4096) throw new IllegalArgumentException("Requête trop volumineuse");
            if(request[0].equals("GET") && request[1].equals("/preview")) {
                byte[] jpeg=engine.previewJpeg().get(1500,TimeUnit.MILLISECONDS);
                if(!new JSONObject(engine.status()).optString("state").equals("stopped"))throw new IllegalStateException("Aperçu interdit pendant la prise");
                reply(socket,200,new JSONObject().put("jpeg",android.util.Base64.encodeToString(jpeg,android.util.Base64.NO_WRAP)).toString());return;
            }
            if (request[0].equals("GET") && request[1].equals("/clock")) {
                reply(socket,200,new JSONObject().put("monoNs",android.os.SystemClock.elapsedRealtimeNanos()).put("wallMs",System.currentTimeMillis()).toString());return;
            }
            if (request[0].equals("GET") && request[1].equals("/status")) {
                engine.contact(); reply(socket, 200, engine.status()); return;
            }
            if(request[0].equals("GET") && (request[1].equals("/recordings") || request[1].startsWith("/recording/"))) {
                if(!new JSONObject(engine.status()).optString("state").equals("stopped"))throw new IllegalStateException("Stop la prise avant de transférer");
                if(request[1].equals("/recordings")){reply(socket,200,engine.catalog.list());return;}
                String path=request[1].substring("/recording/".length());boolean hash=path.endsWith("/sha256");
                String id=hash?path.substring(0,path.length()-7):path;
                if(!id.matches("[a-zA-Z0-9_-]{1,80}"))throw new IllegalArgumentException("Identifiant de vidéo invalide");
                if(hash){reply(socket,200,engine.catalog.digest(id));return;}
                try(InputStream video=engine.catalog.open(id)) {
                    OutputStream out=socket.getOutputStream();
                    out.write("HTTP/1.1 200 OK\r\nContent-Type: video/mp4\r\nConnection: close\r\nCache-Control: no-store\r\n\r\n".getBytes(StandardCharsets.US_ASCII));
                    byte[] buffer=new byte[128*1024];int n;while((n=video.read(buffer))!=-1)out.write(buffer,0,n);out.flush();
                }
                return;
            }
            if (!request[0].equals("POST") || !(request[1].equals("/command") || request[1].equals("/marker"))) { reply(socket, 404, "{\"error\":\"Adresse inconnue\"}"); return; }
            byte[] body = new byte[length]; int read = 0;
            while (read < length) { int count = in.read(body, read, length-read); if (count < 0) throw new EOFException(); read += count; }
            JSONObject command = new JSONObject(new String(body, StandardCharsets.UTF_8));
            if(request[1].equals("/marker")){reply(socket,200,engine.marker(command).get(3,TimeUnit.SECONDS));return;}
            String result = engine.command(command.getString("action"), command.optString("session", ""), command.optString("startedLocal", "")).get(12, TimeUnit.SECONDS);
            reply(socket, 200, result);
        } catch (Exception e) {
            Throwable cause = e.getCause() == null ? e : e.getCause();
            JSONObject error = new JSONObject();
            try { error.put("error", cause.getMessage() == null ? cause.getClass().getSimpleName() : cause.getMessage()); } catch (Exception ignored) {}
            reply(socket, 409, error.toString());
        }
    }
    private static void reply(Socket socket, int code, String body) throws IOException {
        byte[] data = body.getBytes(StandardCharsets.UTF_8);
        String headers = "HTTP/1.1 " + code + (code == 200 ? " OK" : " Error") + "\r\nContent-Type: application/json; charset=utf-8\r\nContent-Length: " + data.length + "\r\nConnection: close\r\nCache-Control: no-store\r\n\r\n";
        OutputStream out = socket.getOutputStream(); out.write(headers.getBytes(StandardCharsets.US_ASCII)); out.write(data); out.flush();
    }
    @Override public void close() { closed = true; try { server.close(); } catch (IOException ignored) {} }
}
