# A01 / V1

## 1. Components

| Component | User-facing version | Technical version |
|---|---|---|
| Windows | Snkoz V1 | Assembly/File 10.1.0.1 |
| Android | V1 | versionCode 100001; package fr.roy.obsusb |

A01 is the folder, filename and release-tag designation. V1 is the visible
product version. Sources descend from Snkoz V9.42 R5 (Windows 9.42.5.0), with
separate recovery, menu/timeline work and the final track-deletion adjustment.
Historical source filenames are retained to preserve provenance.

Android retains the previous signing identity and uses a monotonically higher
versionCode than 94205. A self-built APK signed with another key cannot replace
the official APK in place. Never uninstall just to bypass a signing error while
recordings remain in private app storage.

## 2. Format and validation

Project/montage format 9450 is retained. Recovery uses a distinct envelope
version 1 and .snkozrecovery extension. A01 projects were opened by the R5
reader in synthetic tests. R5 may recreate an empty MIDI lane after opening
a project whose MIDI track was deleted in A01.

Validation performed: 46 workspace checks, 24 track-deletion/layout checks,
18 MIDI-editing checks, 1,280 musical navigation positions and English UI checks.
Windows compilation passes with a pre-existing CS0649 warning. Android compilation
and v2/v3 signatures pass. Layout screenshots were inspected at several sizes.

Real camera/audio/MIDI capture, physical controller operation, Android installation
and timing on representative user projects remain to be validated. This is a
prerelease, not a claim of universal device compatibility.

## 3. Distribution scope

Only Snkoz Windows/Android sources and applications are distributed here.
Third-party dependencies must be installed separately from their official
upstreams. No sequencer firmware or device image is included.
