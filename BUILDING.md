# Building Snkoz Windows R21

## 1. Windows

The 219 C# files in `Windows` match the local VA01.15 R21 delivery byte for byte. No application behavior was changed for publication. The packaging build script is adapted to the public directory layout.

On Windows with the .NET Framework 4.x compiler installed, run:

```powershell
& ./Windows/Compiler.ps1
```

Output: `Build/Windows/Snkoz-VA01.15-R21.exe`, version 10.1.15.20. Entry point: `UrbanKaoz.DisplayRecorder.Program`; historical namespace/file names remain. The CS0649 warning on `NotesView.Model` is known. No Arduino toolchain is used.

For execution, keep the executable and `English.tsv` under `Windows`, and runtime dependencies in the adjacent `Dependencies` directory. See [DEPENDENCIES.md](DEPENDENCIES.md). The historical English catalogue is partial; French is the reference interface for recent additions.

The release distributes the original validated R21 executable. A separate verification build checks these public sources. Compiler timestamps may make hashes differ even with identical sources.

## 2. Android remains historical

`Android` and `Tools/build_android_unsigned.ps1` remain unchanged A01 companion-camera sources, not the sources of the separate Android R25 APK. This Windows release does not rebuild or replace Android. The historical procedure uses Java 8, SDK 35 and API 26 minimum and produces an unsigned APK. No private signing material is present.

## 3. Publication boundary

Only desktop C# sources, desktop assets, public documentation and existing companion-camera sources belong here. No sequencer implementation, microcontroller source/firmware or private converter is included. Hardware-facing C# files implement PC communication only. The optional converter command requires a separate private tool and is unavailable in the standalone public package.
