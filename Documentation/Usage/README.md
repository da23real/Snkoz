# Snkoz — recording and editing guide

This guide uses the English interface labels in V1 (A01). Select **Edit → Language → English** in Editing and restart if necessary. See [installation](../Installation/README.md) for Windows and Android setup.

## 1. Choose recording sources

Open **Recording**. The **Urban Kaoz** tab is a separate personal hardware project; it is not needed for normal recording/editing.

1. Connect cameras, microphones/audio interfaces and any Android phone.
2. Click **Refresh sources** after connecting or removing a device.
3. Add a track with **+ Track**, then open **Configure**. Select video, audio, or both. Select **No video** for audio-only, or **No audio** for a silent PC camera. The Android companion records its phone microphone with its video.
4. Choose the available resolution/frame rate and give each track a short, recognisable name. Settings offered depend on the source. Avoid recording the same microphone on several tracks unless intended.
5. Check each source's preview/readiness before a long take. A source failure is reported independently; it does not prove that the other sources failed.
6. Use **Choose folder** for the take destination. **Save setup / Load setup** store the source configuration for later reuse.

## 2. Record as MIDI master

1. In Recording, choose **PC MIDI master**. A Teensy connection is unnecessary in this mode.
2. Click **Refresh MIDI**, choose the MIDI input receiving notes/CC/program changes and the output connected to the equipment that should follow Snkoz.
3. Click **Connect MIDI**. Set the tempo in BPM. On the instrument/sequencer, enable reception of external MIDI clock and Start/Stop as appropriate for that device. Avoid routing Snkoz's output straight back to its input as a MIDI loop.
4. Optionally enable **Calibration note one beat before Start**. This sends C4/MIDI note 60 to all 16 channels before Start, providing an alignment reference. It is not a substitute for routing the instrument's audio to a recording input.
5. Click **Record**. Snkoz starts its configured sources and sends MIDI Start/clock. Play the instruments or let the connected sequence run. A MIDI input is required to capture the musical events used by effects; output clock alone supplies timing, not your notes.
6. Click **Stop**. Allow file finalisation, Android transfer and preparation to finish. Stop keeps the final partial bar; Pause uses the application's complete-bar segmentation. Start with a short test take to check each source and MIDI events.

**MIDI slave** can receive external clock, but Snkoz is optimized for master operation. In slave mode, external Start does not automatically trim the beginning of the recording. Keep the lead-in for latency alignment, then trim it in Editing.

After recording, open the take folder/project in **Editing** as needed. Stopping a new take does not mean that an existing edit should be discarded. **File → Open → Takes** opens the storage folder; **File → Open → Folder…** loads a take into the editor.

## 3. Preview audio and latency

Use **Edit → Project settings → Preview audio…** in Editing. This is a global preview setting, not one setting per video track:

- Automatic uses that track's audio, or the chosen reference when the track has no sound.
- Off makes the preview silent.
- Custom selection mixes the checked audio tracks, with individual gains.

The preview audio choice does not choose the final export's mix. The final montage mixes the active audio tracks. Avoid doubling the same performance by mixing both an external microphone and an unwanted camera microphone.

Before extensive editing, open **Edit → Project settings → Latencies…**. Select the video, audio and MIDI channel used as references. Compare a visible action and the audio waveform with Start, the calibration note, or a known musical event. Adjust **Video offset** and **Audio offset** independently: positive delays, negative advances. Units can be milliseconds, frames or musical values. The MIDI timing grid remains fixed. Use the short playback to check alignment, then **Apply** to save offsets for previews and exports.

If you edit in another video application later, use the corrected exported audio. Raw audio in the original recording folder does not include Snkoz's latency correction.

## 4. Select, cut, delete and close a gap

The timeline contains audio/video blocks and an explicit **MIDI block lane**, above the compact note/CC/program display. The MIDI filters only change what is displayed.

- Left-click a block to select it without moving the playhead. Ctrl+click adds/removes blocks. There is no permanent Select button.
- Click the ruler or blank space between tracks to place the playhead without clearing the selection.
- Use the three numeric position fields: bar (from 1), beat (1–4), subdivision (0–3). Changes move the playhead; a value beyond the project ends at its exact final time. Enter commits typed input. The measure ruler stays fixed above vertically scrolling tracks.
- **◀ / ▶** move the playhead by the chosen musical step. The bottom scrollbar pans the view; it does not move blocks.
- **Ctrl+A** selects every block, including MIDI. **C** selects the column intersected by the playhead. **Ctrl+B** or **Edit → Split at playhead** cuts selected blocks at that position, including MIDI.
- **Edit → Delete → Block** keeps the **Delete** key, removes the selected blocks and leaves a gap. The context menu also offers **Delete — close gap** (Shift+Delete), for a complete selected section across the edit.
- To close an existing gap: select the blocks to its right on all relevant tracks, including MIDI, then right-click **Close gap**. The same displacement is applied to the selection. If gaps differ between tracks, the smallest common gap is closed so that relative alignment is preserved.
- Drag selected blocks horizontally to move them manually. Blocks remain on their original tracks. Select all relevant audio/video/MIDI blocks before moving a passage. Overlap with unselected blocks on the same track is refused.

**Edit → Delete → Track → name** removes the entire track, including empty tracks, without a shortcut. Undo restores the deletion within the existing two levels. Source media are kept. Deleting MIDI removes its events but keeps the musical grid. The compact **▶** button on each track opens its preview and has a tooltip.

Right-click does not replace a selection. With a selection, commands target that selection; without one, they target the clicked block. **Block FX** is unavailable for multiple blocks. If a text entry field still has focus, click the block lane before using editing shortcuts.

MIDI events are not quantized by movement. A moved MIDI block retains the same internal timing and note durations; notes, CC and program changes all receive the same offset. The clock grid stays in place. Notes crossing a cut boundary are clipped to the resulting block boundaries. Audio/video rendering is still sampled at its normal sample/frame rate.

There are **two undo levels for cuts/deletions**. Mute is not put into the undo history. Mute changes are lightweight; use **File → Save** to update the main project; recovery also includes mutes. Linking, moving, importing and effect adjustment do not create deep undo snapshots in this prototype.

## 5. Track effects and block effects

Right-click a single video block:

- **Track FX** edits the track's default effects, used by blocks without their own override.
- **Block FX** edits only that block. Splitting preserves the effects on each resulting block, and subsequent block edits are independent. A block override is not replaced by changing Track FX.

Add an effect with **+ Effect**, select its type and trigger, then set its MIDI channel. Note triggers can follow all notes or one detected note. CC triggers use the exact CC number (CC85 means 85, without a one-based offset). Choose the correct channel and use **Detect CC min / max**. Set the amount/multiplier and **CC hold after last event**; no matching CC data means no CC trigger.

Effects are processed from top to bottom. A MIDI-triggered Mute effect attenuates the effects below it; this is different from muting a timeline block. Use arrows to change effect order. Valid effect changes enter the working montage and its deferred recovery; only File → Save / Save as updates the main project. FX presets can save one effect or the effect list for reuse.

Select the effect whose trigger should determine the preview start, then **Preview FX**. The preview searches for a trigger inside the current block and starts shortly before it, bounded by that block. Previous blocks' triggers are ignored. Use low preview resolution and a short duration while adjusting; increase quality for final checks. Duration/quality are retained per track, while preview audio is common.

## 6. Animations and freely placed media

Use **+ Animation** inside FX to add a MIDI-triggered PNG/GIF or short MP4 animation. PNG sprite sheets use the supported 4×2 layout; GIFs provide their frames. Choose the start frame and the frame that should coincide with the trigger. Set frame duration, display duration, Gate and loop behaviour. Gate follows note release; CC can drive the animation position directly.

Use **Place on video** to move and resize the overlay. Note/image mapping can use absolute pitch around a centre or a chord/scale association. Unassigned notes can have no image or the configured fallback.

For media placed at a fixed timeline position instead of MIDI-triggered FX, use **Insert → Media…** (choose file, then musical position on a new track) or the context menu **Insert media here**. PNG/GIF can form an overlay on a transparent track. MP4 can be inserted into a free interval; it must not overlap another block on that same track. This is distinct from a short MP4 used as a triggered animation.

## 7. Body masks

In an effect, enable **Body mask — experimental**. The effect is restricted to the detected silhouette, while the background is retained. Adjust mask edge/offset settings and test a short, low-resolution preview first. Detection runs locally using the separately installed model/runtime; no detected body means no masked effect. It is segmentation, not skeletal tracking, and difficult lighting, occlusion or multiple people can give imperfect results. Body masks are not available in Live mode.

## 8. Final export

Save the project, then use **File → Export → Full montage…**. Choose resolution and encoder. 720p is useful for smaller/faster drafts; do not upscale pixel-art animations unnecessarily. NVIDIA only accelerates supported compression stages, not every effect or mask calculation.

The highest visible opaque video track covers lower tracks. Transparent graphic tracks can overlay it; muted blocks let lower video tracks show. Active audio tracks are mixed independently of video visibility.

The montage export ends at the end of the **last non-muted video block**, with audio cut to that same endpoint. Intermediate gaps remain. If all video is muted, no empty render is produced. **File → Export → Track → track name** can still cover that track's full edit duration and uses black/silence for muted parts.

Progress includes preparation and rendering. **Cancel rendering** abandons the job. **Stop and keep completed part** preserves the part that can be finalised; a montage may only retain completed segments, and an immediate stop may have nothing usable to keep.

## 9. Practical limits of this preview release

The Windows editor and Android app are prototypes undergoing prerelease validation. MIDI timing edits, multi-track cuts and gap closing have automated/synthetic tests; those do not replace a short real camera/audio/MIDI test on your equipment. Some legacy diagnostic strings remain French. The optional Urban Kaoz controls are outside the generic recording/editing workflow.

Keep the project and media folders together. A01 retains R5 project format 9450. R5 can read these projects; versions lacking 9450 support cannot. No Arduino/Teensy/Pico firmware is required or included in this software-only delivery.

## 10. Manual saving and recovery

**File → Save** updates the current project; **Save as…** chooses another file. With no named project, Save asks for a file. Automatic recovery never writes to that main file or the source take montage.

After edits, recovery waits for three seconds without changes, with at least fifteen seconds between automatic writes. It copies edit data once, then serializes and writes in the background. Moving and muting only mark the montage as changed; they do not copy all MIDI events or write files synchronously. Recovery is suspended during recording. Switching montages queues pending changes for recovery. The last few seconds may not yet have a recovery if the application closes or crashes: save explicitly when needed.

**File → Open → Recovery** lists the five most recent recovery files across projects for this Windows user, by project name and local date/time. Open one to inspect it. Opening does not overwrite the manual project; Save explicitly updates the original project, while Save as chooses a different destination. Media remain external. Recovery files have their own format and do not replace source recordings.

**Edit → Copy / Paste** (Ctrl+C / Ctrl+V) copies selected blocks, including linked A/V and MIDI, and pastes at the playhead on their original tracks. Internal event timing is preserved; overlap is refused. The clipboard is local to the current montage. Cut/delete retain two structural undo levels; move, paste and mute do not add undo levels.

The permanent editing toolbar contains controller activation, the joystick pictogram (tooltip), position, musical step, arrows and a zoom slider. The bottom scrollbar pans the view. Existing useful hardware commands and context menus remain available.

[Guide français](README_FR.md).
