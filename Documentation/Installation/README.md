# Install Snkoz Windows R21

Fully extract `Snkoz-VA01.15-R21-Windows.zip` from the [R21 release](https://github.com/da23real/Snkoz/releases/tag/windows-VA01.15-R21). Keep `Windows`, `Dependencies` and `Documentation` together.

Follow [DEPENDENCIES.md](../../DEPENDENCIES.md): FFmpeg/ffprobe for media, Android Platform-Tools for USB phones, and optional Python/body-mask components. Third-party binaries are not bundled. Windows with .NET Framework 4.x is required.

Run `Windows/Snkoz-VA01.15-R21.exe` manually. Generic animation presets require your own images. The English catalogue is partial; French is the reference for new labels.

R21 reads project formats 100218-100232 and writes 100232. R20 cannot read new R21 files. No automatic migration. The private sequencer-project converter is excluded and its command is unavailable. No microcontroller code or firmware is included.

Android remains a [separate release](https://github.com/da23real/Snkoz/releases/tag/android-VA01.16-R25). See [usage](../Usage/README.md), [limits](../../VERSION.md), and [video](https://youtu.be/prQQBhpKYqU).
