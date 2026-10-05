# Snkoz — V1 / A01

Record and edit video/audio with musical timing and MIDI-triggered effects.
Snkoz includes a Windows application and an Android USB camera companion.

**Prototype / prerelease. Windows requires external dependencies: install them
before recording or exporting. They are not included in the public download.**

## 1. Download and install

[Official releases](https://github.com/da23real/Snkoz/releases) contain:

- **Snkoz-A01-Windows.zip** — Windows application, English catalogue and guides.
- **Prise-USB-A01.apk** — Android USB camera companion (Android 8 or later).
- **SHA256SUMS.txt** — checksums of the two downloads.

[Installation FR](Documentation/Installation/README_FR.md) ·
[Installation EN](Documentation/Installation/README.md) ·
[Dependencies / Dépendances](DEPENDENCIES.md).

Sources: [Windows](https://github.com/da23real/Snkoz/tree/main/Windows), [Android](https://github.com/da23real/Snkoz/tree/main/Android/app),
[building from source](BUILDING.md). No Arduino, Teensy or Pico firmware,
signing key, personal recording or user configuration is included.

## 2. Manuals / Manuels

[Manuel français](Documentation/Usage/README_FR.md) ·
[English guide](Documentation/Usage/README.md).

A01 includes File/Edit/Insert menus, five separate recovery files, numeric
musical position, a fixed ruler, Ctrl+B splitting including MIDI, two undo
levels, Delete → Block/Track, compact ▶ previews and MIDI port refresh.

Windows: **10.1.0.1**, displayed as **V1**. Android: **V1**, versionCode
**100001**. The Android APK is compiled and signature-verified; this revision
has not been installed or camera-tested as part of this release preparation.
See [version and validation notes](VERSION.md).

## 3. Licence et vidéos commerciales

Snkoz est gratuit à utiliser, y compris pour un travail professionnel.
Vous pouvez publier, monétiser et vendre les vidéos produites, sans redevance
Snkoz. Vous devez disposer des droits sur les médias que vous utilisez.

Les sources sont consultables et modifiables pour votre usage privé et pour
contribuer ici. Les forks GitHub nécessaires aux contributions sont autorisés.
Les distributions indépendantes, la revente et les miroirs du logiciel ne sont
pas autorisés sans accord écrit. Les dépendances conservent leurs licences.
Ce projet utilise une **licence spécifique source-available**, pas une licence
open source standard. Le texte contractuel est [LICENSE](LICENSE).

## 4. Contribute

Open an [issue](https://github.com/da23real/Snkoz/issues) to report a problem or
suggest an improvement. For code, submit a pull request to this official
repository. Contributions are reviewed before merging; users cannot directly
replace the official release. See [CONTRIBUTING](CONTRIBUTING.md).
