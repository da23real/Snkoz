# Installer Snkoz Beta V02.00 pour Windows

## 1. Extraire et lancer

1. Télécharge [Snkoz-Beta-V02.00-Windows.zip](https://github.com/da23real/Snkoz/releases/download/beta-v02.00/Snkoz-Beta-V02.00-Windows.zip). Dans l’Explorateur Windows, fais un clic droit sur le ZIP et choisis **Extraire tout**. Choisis un emplacement où tu peux écrire, par exemple Documents.
2. Télécharge une version Windows 64 bits de FFmpeg, par exemple le paquet **release essentials**. Extrais-la, puis copie `ffmpeg.exe` et `ffprobe.exe` depuis son dossier `bin` vers le dossier `Dependencies` de Snkoz. [Téléchargements Windows de FFmpeg](https://ffmpeg.org/download.html#build-windows)
3. Dans le dossier extrait de Snkoz, ouvre `Windows` et double-clique sur `Snkoz-VA01.60-R99.exe`.

```text
Snkoz-Beta-V02.00/
  INSTALLATION.md
  LICENSE
  Windows/
    Snkoz-VA01.60-R99.exe
  Dependencies/
    ffmpeg.exe    à ajouter
    ffprobe.exe   à ajouter
```

## 2. Prise multicaméra

Dans **Prise**, ajoute tes sources vidéo et audio et vérifie l’aperçu avant de lancer l’enregistrement. Pour un téléphone Android utilisé en USB, extrais également les outils ADB dans `Dependencies/platform-tools`, active le débogage USB sur le téléphone et accepte l’autorisation de connexion du PC. [Outils Android pour Windows](https://developer.android.com/tools/releases/platform-tools) · [Compagnon caméra Android](https://github.com/da23real/Snkoz/releases/tag/android-VA01.16-R25)

## 3. Vérifier la version

Le titre de la fenêtre affiche **VA01.60 R99** et le build **10.1.60.99**. Cette version est distribuée sous le nom **Beta V02.00**. Le fichier `SHA256SUMS.txt` de la publication donne l’empreinte du ZIP et de son exécutable. [Empreintes sur GitHub](https://github.com/da23real/Snkoz/releases/tag/beta-v02.00)
