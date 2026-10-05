# Snkoz — installation (V1 / A01, public prerelease)

This is a public prototype prerelease. Test a short take before important work.
The Windows application records and edits video/audio with MIDI-triggered effects. The Android companion records a phone camera and transfers the recording over USB.

## 1. Windows

Use a 64-bit Windows PC with .NET Framework 4.x (4.8 recommended), Windows Media Player support for the embedded preview, and enough writable disk space for recordings and temporary renders. On a Windows N edition, the Windows media components may need to be enabled. Camera/microphone permissions must allow desktop applications.

Install the external dependencies first; they are not bundled in this public ZIP. See [DEPENDENCIES](../../DEPENDENCIES.md). After installation, keep this folder layout together:

```text
Snkoz/
  Windows/Snkoz-A01.exe
  Windows/English.tsv
  Dependencies/ffmpeg.exe
  Dependencies/ffprobe.exe
  Dependencies/Nvenc/...
  Dependencies/platform-tools/...
  Dependencies/BodyMask/...
  Android/Prise-USB-A01.apk
  Documentation/Installation/README.md
  Documentation/Usage/README.md
```

Extract the Windows ZIP to a writable folder and install its external dependencies. Run `Windows/Snkoz-A01.exe`. You can create a shortcut to that executable, with `Windows` as its working directory. Do not move just the executable away from its dependencies. Do not install the prototype under a read-only directory: preview/render caches are created beside `Windows`.

Choose **Editing → Edit → Language → English**, then restart Snkoz. French remains available. The English catalogue covers the recording/editing controls; some older diagnostic messages and the personal Urban Kaoz interface may still be French. The Android companion in this release has an English interface.

New installations default to `Videos/Snkoz/Recordings`. Existing saved preferences retain their chosen folders. Select a writable recording folder in **Recording → Choose folder**. Configurations default to `Documents/Snkoz/Config`; preferences are stored under `%LOCALAPPDATA%/Snkoz`.

The **Urban Kaoz** tab is an optional hardware sequencer control interface. It is not required for recording or editing with Snkoz. Ordinary users can ignore it, **Connect Teensy**, and **Controller**. A standard MIDI/USB-MIDI device can be used from the Recording tab.

## 2. Android camera companion

The app requires Android 8.0/API 26 or later and a supported camera. Device compatibility is not guaranteed merely by Android version; test a short recording on each phone.

1. Copy `Android/Prise-USB-A01.apk` to the phone. Open it and allow installation from that source if Android asks. Alternatively, from the package folder use:

   ```powershell
   .\Dependencies\platform-tools\adb.exe install -r .\Android\Prise-USB-A01.apk
   ```

2. Open **USB Recording / Prise USB**. Allow camera and microphone access, and notification permission if requested.
3. Choose the front/rear camera and quality. Start with **720p · smaller files · 3 Mbit/s** if transfer time and storage matter. Preview rotation changes the preview only.
4. Choose a recording folder. Recordings in the app's private folder can disappear when the app is uninstalled; a separately selected folder is easier to keep and access.
5. Enable Android developer options and **USB debugging**, connect a USB **data** cable, and approve this computer's USB debugging prompt. Windows may require the phone manufacturer's USB driver. A charging-only cable cannot work.
6. Keep the app open initially and wait for **READY FOR PC**. In Snkoz, use **Recording → Refresh sources**, then choose the Android device as a video source. Pairing is automatic through USB; no manual code is needed.

This is a **USB remote camera**, not a Wi-Fi streaming camera. Video is recorded locally on the phone; after Stop it is transferred and checked on the PC. Preparation may therefore continue after recording stops. Wi-Fi and Bluetooth are not required. Recording can continue with the screen off, but test your phone's background/battery restrictions before a long take.

For USB diagnosis, run `Dependencies/platform-tools/adb.exe devices -l`. The phone must appear as `device`, not `unauthorized` or `offline`. Approve the phone prompt, reopen the app, then refresh sources in Snkoz. Do not uninstall a previous app merely to resolve an installation error while it still contains recordings.

The APK is compiled and signature-verified, but this revision has not been installed or camera-tested on the phone during this delivery. It retains package ID `fr.roy.obsusb` and the previous development signing identity. Its visible version is V1, with versionCode 100001 (higher than R5 version 94205). It has not been installed in this delivery.

## 3. MIDI and optional GPU

Connect a MIDI interface or USB-MIDI instrument before refreshing MIDI ports. Configure MIDI input/output in **Recording**, independently of the Teensy serial connection. MIDI carries commands/events, not the instrument's audio: select a microphone or audio-interface input separately.

NVIDIA encoding is optional. **Automatic** tries NVIDIA and falls back to software. **NVIDIA only** reports an error instead of falling back. Effects, body-mask analysis, decoding and preparation can still use the CPU, even when compression uses the GPU. No firmware or GPU driver is installed by Snkoz.

## 4. Files and compatibility

Projects reference their media folder: a `.snkozproject` file alone does not contain all video/audio assets. Keep that folder with the project. Do not delete the source recording folders after saving a project.

A01 retains R5 project/montage format **9450** for explicit MIDI blocks. The application reads the older supported formats and migrates their MIDI timeline when opened. A01 and R5 use the same format; versions lacking 9450 support cannot read it. Existing media files are not rewritten by that migration. Older independently shifted camera edits are consolidated into the MIDI events visible in the old montage before creating the MIDI block.

This public distribution contains Windows/Android software; runtime dependencies are obtained separately from their upstreams. **No Arduino/Teensy/Pico sequencer firmware, signing keys, personal recordings or user configuration files are included.** Keep dependency licences/notices with any future distribution. The Snkoz license does not restrict rights granted by third-party dependency licenses.

Continue with [the usage guide](../Usage/README.md).

Windows displays V1 (A01); its technical file/assembly version is 10.1.0.1, after R5 version 9.42.5.0. See [version mapping](../../VERSION.md). Recovery files are stored under %LOCALAPPDATA%/Snkoz/Recovery and require the original media. [Guide français](README_FR.md).
