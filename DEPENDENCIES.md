# Dependencies / Dépendances

## 1. Public package / Paquet public

The public Windows ZIP contains Snkoz, its guides, catalogue and original mask
helper. **It does not bundle FFmpeg, ADB, Python, Python packages or the Google
segmentation model.** Download those from their upstreams and keep their notices.
Snkoz's restrictive license does not apply to those independent components.

Le ZIP Windows public exige cette installation complémentaire. Les dépendances
ne sont pas incluses : le paquet local de développement était différent. Les
fonctions caméra/export nécessitent FFmpeg ; Android nécessite aussi ADB ; le
masque corporel exige sa configuration Python optionnelle. Aucun de ces fichiers
n'est un firmware de séquenceur.

## 2. FFmpeg — required / obligatoire

Download a Windows x64 **essentials** build from
[Gyan's FFmpeg builds](https://www.gyan.dev/ffmpeg/builds/). Extract the archive
and copy `bin/ffmpeg.exe` and `bin/ffprobe.exe` to Snkoz's `Dependencies` folder.
Keep the downloaded license and source notices with your local installation.
The validated local baseline used FFmpeg 9.0.2 essentials; newer builds are not
automatically qualified by Snkoz's existing tests.

Télécharger puis extraire le build Windows x64 « essentials ». Copier les deux
exécutables dans `Dependencies`, au même niveau que les dossiers `Windows` et
`Documentation`. Conserver les licences fournies par le distributeur.

NVIDIA encoding is optional. A separate NVENC-capable FFmpeg executable can be
placed at `Dependencies/Nvenc/ffmpeg.exe`; the local baseline used Gyan 7.1.1
essentials. Without this optional executable, choose software encoding or use
Automatic and its software fallback. NVIDIA drivers are supplied separately by
the hardware vendor.

These Gyan builds are GPLv3. Their licenses and source-access obligations are
independent of Snkoz's license. See [FFmpeg legal information](https://ffmpeg.org/legal.html).
We do not redistribute those binaries in this public release.

## 3. ADB — required for Android USB / requis pour Android USB

Download [Google SDK Platform-Tools for Windows](https://developer.android.com/tools/releases/platform-tools),
accept Google's terms and extract the entire `platform-tools` directory into
Snkoz's `Dependencies`. The result must contain
`Dependencies/platform-tools/adb.exe`, its DLLs and notices.

Extraire le dossier complet, activer le débogage USB sur le téléphone et autoriser
le PC. Un câble de données est nécessaire. ADB is a host tool, not phone firmware.

## 4. Optional body mask / Masque corporel optionnel

The tested runtime was Python 3.12.10 x64 with the versions listed in
`Dependencies/BodyMask/requirements.lock.txt`. The original `segment.py` helper
is included, but Python, its packages and the model are not redistributed here.

Install [Python](https://www.python.org/downloads/) into the custom folder
`Dependencies/BodyMask/python` using its full installer (include pip).
The required interpreter location is `Dependencies/BodyMask/python/python.exe`.
Use that interpreter's pip module to install the requirements from the lock
file. The Windows embeddable ZIP does not include pip and needs additional
setup; a normal installation into the custom directory is simpler.

Download Google's square 256×256
[SelfieSegmenter float16 model, version 1](https://storage.googleapis.com/mediapipe-models/image_segmenter/selfie_segmenter/float16/1/selfie_segmenter.tflite)
to `Dependencies/BodyMask/selfie_segmenter.tflite`. Read the
[MediaPipe image-segmentation guide](https://developers.google.com/edge/mediapipe/solutions/vision/image_segmenter)
and its linked model card and applicable terms.

Installer Python avec pip dans le dossier indiqué, installer les paquets du
fichier requirements.lock.txt avec ce Python, puis télécharger le modèle Google
au chemin exact. Tant que ces éléments manquent, laisser le masque corporel
désactivé. Un message ancien peut proposer de « réinstaller la livraison complète » :
pour ce paquet public, suivre la présente procédure. Le reste du montage et
les effets sans masque peuvent être utilisés avec FFmpeg seul.

## 5. Expected layout / Arborescence attendue

```text
Snkoz-VA01.15-R21/
  Windows/Snkoz-VA01.15-R21.exe
  Windows/English.tsv
  Documentation/
  Dependencies/
    ffmpeg.exe
    ffprobe.exe
    Nvenc/ffmpeg.exe                   (optional)
    platform-tools/adb.exe             (Android USB)
    BodyMask/segment.py
    BodyMask/requirements.lock.txt
    BodyMask/python/python.exe         (optional body mask)
    BodyMask/selfie_segmenter.tflite    (optional body mask)
```

The Android APK is a separate download. On the phone it does not require
FFmpeg or the Windows Python runtime. Keep your existing local working
installation if you already have these dependencies; the public package does
not remove them or change their licenses.
