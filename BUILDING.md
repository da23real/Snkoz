# Building Snkoz A01

## 1. Windows

The `Windows` directory contains the exact C# sources of the published Windows
10.1.0.1 application. On Windows with .NET Framework 4.x, run `Compiler.ps1`
from PowerShell. It invokes Microsoft's Framework C# compiler and produces
`Windows/Snkoz-A01.exe`. No Arduino toolchain is used.

Keep `English.tsv` alongside the executable and install runtime dependencies
as described in [DEPENDENCIES](DEPENDENCIES.md). The compiled Windows program
contains an optional hardware-control interface, but no device firmware.
Some historical source/test names and namespace names still use UrbanKaoz or
Synkoz; the published application name is Snkoz.

## 2. Android

The `Android/app/src/main` tree contains the original Java, manifest and icon.
The app uses Java 8 language level, SDK 35 and minimum API 26.

`Tools/build_android_unsigned.ps1` is the existing release build procedure
with the private signing step removed and the JDK path supplied by JAVA_HOME
or the `-Jdk` parameter. Supply the installed SDK with `-Sdk` if necessary.
It requires Build Tools 35.0.0 and the android-35 platform; output is
`BuildAndroid/aligned.apk`. Sign that file with your own keystore using
Android's apksigner before installing a development build. See the official
[app-signing documentation](https://developer.android.com/studio/publish/app-signing).

No official signing key or password is provided. A differently signed build
cannot replace the official app while retaining the same installed identity.
Do not uninstall an app containing recordings just to bypass a signing error.

The app/build.gradle file records the same application ID and version metadata;
the standalone PowerShell build does not require Gradle or a downloaded wrapper.

## 3. Contribution scope

Original sources and public documentation are covered by LICENSE; external
SDKs and dependencies retain their own licenses. Only Windows/Android changes
belong in this repository. Do not contribute sequencer firmware, SDK binaries,
personal media or signing material.
