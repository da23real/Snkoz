# Installer Snkoz Windows R21

## 1. Extraire

Télécharge `Snkoz-VA01.15-R21-Windows.zip` depuis la [release R21](https://github.com/da23real/Snkoz/releases/tag/windows-VA01.15-R21), puis extrais entièrement l’archive. Ne lance pas l’application depuis le ZIP.

Conserve les dossiers voisins `Windows`, `Dependencies` et `Documentation`. L’exécutable est `Windows/Snkoz-VA01.15-R21.exe`. Aucun installateur ne déplace tes projets.

## 2. Dépendances

Suis [DEPENDENCIES.md](../../DEPENDENCIES.md). Le téléchargement ne contient pas les binaires externes :

- `Dependencies/ffmpeg.exe` et `Dependencies/ffprobe.exe` pour capture, miniatures et rendus.
- `Dependencies/platform-tools/adb.exe` et le reste de Platform-Tools pour les caméras Android USB.
- Python et modèle du masque corporel seulement pour cette option.

Windows avec .NET Framework 4.x est nécessaire. Le paquet n’est pas présenté comme testé sur tout PC neuf.

## 3. Lancer

Lance manuellement l’exécutable. Choisis tes dossiers et périphériques. Les presets génériques demandent tes propres images. Aucun pack commercial ni vidéo personnelle n’est fourni.

Android s’installe séparément avec débogage USB autorisé : [APK VA01.16 R25](https://github.com/da23real/Snkoz/releases/tag/android-VA01.16-R25). Ce paquet Windows ne le remplace pas.

Le catalogue anglais historique est partiel ; le français est la référence pour les nouveautés.

## 4. Compatibilité

R21 lit 100218 à 100232 et écrit 100232 ; R20 ne lit pas les nouveaux fichiers. Aucune conversion automatique. Le convertisseur privé du séquenceur est exclu ; sa commande peut signaler son absence. Aucun firmware ni code Arduino/Teensy/Pico n’est fourni.

[Guide](../Usage/README_FR.md) · [Limites](../../VERSION.md) · [Vidéo](https://youtu.be/prQQBhpKYqU)
