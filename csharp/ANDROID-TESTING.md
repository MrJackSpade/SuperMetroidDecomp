# Android diagnostic handoff

## Build and update the private APK

The verified Windows build environment uses .NET SDK 10.0.400 and Android workload
36.1.69. An Android SDK and supported JDK must be installed/configured for the .NET
Android workload. The project targets `net10.0-android`, `android-arm64`; it does not
require Linux, Docker, an emulator, or the Windows Forms executable on the handheld.

From the repository root, with the private ROM and extracted audio assets present:

```powershell
dotnet build csharp/src/SuperMetroid.Android/SuperMetroid.Android.csproj -c Release
adb install -r csharp/src/SuperMetroid.Android/bin/Release/net10.0-android/android-arm64/org.supermetroid.csharp.testing-Signed.apk
```

Use the existing wireless-debugging connection or USB. If several devices are
connected, supply `adb -s DEVICE_SERIAL`. `install -r` updates the existing package;
do not uninstall or clear app data as an update workaround, because that deletes
private saves/configuration/recordings. Export a diagnostic bundle before changing
build machines or signing setup.

This is a private debuggable testing APK, not a store-release package. The project
currently uses the .NET Android SDK's local default debug keystore, even in Release
configuration; no custom signing secrets are embedded in the project. Consecutive
updates from this workspace have preserved the installed app and save hashes.
Keep that same local signing identity when updating this installation. A newly
generated key on another machine will not be interchangeable: preserve the keystore
in a secure private backup, outside Git, or explicitly configure the same identity
there. Never commit keystores/passwords or regenerate the key to solve an install
error. `*.keystore` and `*.jks` are ignored as an additional guard.

The APK contains private game assets and must not be distributed publicly.

## Capture on the handheld

Open **Testing tools** with Back/Mode or a long press on the game surface. Choose
one of the ten state slots and use **Save state** before exporting if you want to
capture the current position. **Export private diagnostics** exports the already
saved selected slot; it does not overwrite that slot with a new capture.

Choose local storage in Android's file picker, then transfer the ZIP to the private
Windows workspace. It contains saves, configuration, logs, and input recordings
with their preserved starting states. Debugger graphs can contain cartridge data:
do not publish the ZIP. Installed ROM/audio directories are not separately included.

## Replay an exported recording on Windows

Run from the repository root, with the existing private `Super Metroid.smc` and
`standalone-assets/audio` available. Inspect `input-recordings` inside the ZIP and
choose the `.smrec` basename corresponding to the reported session:

```powershell
dotnet run --no-launch-profile --project csharp/src/SuperMetroid.DiagnosticsVerification -c Release -- --replay-android-bundle "path/to/private-export.zip" "SuperMetroid-input-YYYYMMDD-HHMMSS-fff.smrec"
```

The sidecar JSON determines whether the journal starts at reset with recorded SRAM
and options, or from an exact preserved `.seed.smstate`. Do not substitute the
currently selected slot for the preserved recording seed: they can represent
different points in time.

The command validates the ROM digest and metadata, simulates every recorded input
through the shared frontend and managed audio engine, and prints final game/room
state and full-stream video/audio SHA-256 hashes. Run twice to compare deterministic
results. These hashes prove repeatability on the replay host, not equality with a
cartridge emulator or Android output that was not separately captured.

Replay never loads or writes the desktop's ordinary save files or debugger slots.
Only a uniquely created temporary directory is used for a preserved seed, and it is
removed afterward. A changed-build warning remains a warning; incompatible or
corrupt data fails visibly in the console.

## Other testing controls

- **INI settings** edits cheat/debug settings for the next app launch and shows
  pending values when they differ from the active session.
- **Controller bindings** changes host-key-to-SNES mappings immediately and saves
  them in `controller-bindings.json`. Restore defaults returns the Retroid mapping.
  Back/Mode and native menu navigation are independent of gameplay remapping.

## Import a save or debugger state

Use **Import debugger state** to choose a trusted `.smstate` file for the selected
slot. The full graph and ROM identity are checked before replacing the slot. Build
differences remain warnings. Import does not load the state automatically; use
**Load state** afterward. Invalid files leave the existing slot untouched.

Use **Import regular save (next launch)** for a JSON save. The file is validated and
staged separately from the current session, then activated on the next app launch.
Continuing the current game cannot overwrite the pending import. These actions
accept individual files, not the diagnostic ZIP itself; extract the wanted file
from a bundle first. Raw emulator `.srm` import is not exposed by this menu.

Replaced files are retained under the app-private `import-backups` directory with
unique suffixes. They can be retrieved with the existing private-device diagnostic
access; a recovery browser is not yet exposed in the menu.

The remaining lifecycle/performance acceptance work is tracked in issue #355.
Import/export/replay support is not completion of that issue.

## Opt-in audio-focus device probe

Normal APKs exclude this diagnostic. To exercise real Android focus callbacks
without another app taking window focus, build with:

```powershell
dotnet build csharp/src/SuperMetroid.Android/SuperMetroid.Android.csproj -c Release -p:AndroidHostProbes=true
```

Install that APK, launch the game, and leave the testing menu closed. Deliver an
intent to the already running Activity:

```powershell
adb shell am start --activity-single-top -n org.supermetroid.csharp.testing/crc641619a672f3a517b4.MainActivity --es audio-focus-probe transient
adb shell run-as org.supermetroid.csharp.testing cat files/audio-focus-probe.log
```

Wait at least five seconds before repeating with `duck` instead of `transient`.
The probe waits for intent-delivery resume, then creates a second AudioManager
client for three seconds. It does not play or record audio. The log must show
`LossTransient`/`LossTransientCanDuck`, `canRun=False`, unchanged timing while held,
and `Gain` with `canRun=True`, with `focused=True resumed=True` throughout the
audio interruption. This checks actual framework callbacks from a second client
in the same application, not another application's UID or a phone call.

Both cases passed on the Retroid Pocket Classic on 2026-09-08. Save JSON, its backup,
and debugger slot 0 retained their pre-test SHA-256 values. Rebuild **without**
`AndroidHostProbes=true` and reinstall afterward; do not leave a probe APK as the
player's testing build. Broader sustained-performance acceptance remains separate.

## Presenter cadence regression

The Android View maintains its own animation invalidation chain while the session
is active. Publication still copies into the single pending mailbox, and paint FPS
counts only newly consumed frames, not redraws of the previous bitmap. Simulation
and PCM production remain on their independent fixed-step worker.

On 2026-09-08, the same autonomous demo window (recorded host frames 2600–4400,
30 timing windows per run) painted at 49.26 FPS with producer-only invalidation
and 59.58 FPS with display-paced invalidation. Both runs simulated at 60.01 FPS
(rounded); minimum one-second paint rates were 40.3 and 57.1 respectively.
This comparison excludes startup/loading and is not a claim of flawless audio.

For a lifecycle regression, open the testing menu, allow its animation to settle,
then compare `adb shell dumpsys gfxinfo org.supermetroid.csharp.testing` twice
several seconds apart. `Total frames rendered` must stop increasing while the
native menu is idle. The device check stayed at 6150 over four seconds. Resume
must restart consumption without requiring fresh input or recreating the Activity.

## Experimental AOT build (not the default)

`-p:AndroidExperimentalAot=true` enables non-profiled Mono AOT and explicitly roots
all declarations in the three application assemblies. Android requires linking
for this build; JSON reflection is explicitly retained. A generic `TrimMode=copy`
experiment did not preserve application declarations with this SDK and is not a
substitute for these roots.

Before installing an experimental APK, compare each application assembly with its
`obj/Release/net10.0-android/android-arm64/linked` counterpart:

```powershell
dotnet run --no-launch-profile --project csharp/src/SuperMetroid.DiagnosticsVerification -c Release -- --compare-assembly-metadata INPUT.dll LINKED.dll
```

This read-only tool checks named types, fields, properties, events, and method
overload counts including private declarations without executing either assembly.
It does **not** establish signature/body equivalence, framework reflection support,
or debugger-state compatibility. A successful device state load/continuation test
and performance comparison remain required before making AOT the default.

The initial rooted experiment on 2026-09-08 built with zero warnings/errors and retained
34,904 Core, 248 Diagnostics, and 469 Android declarations. Existing debugger slot
0 loaded with the normal changed-build warning and continued rendering/audio.
The short opening/demo run recorded zero underruns, but some presentation windows
still fell into the 40s. This is not full performance or state-portability acceptance.
The normal APK and exact pre-test save were restored after that initial experiment.

After the raster-allocation reductions through `6688264`, the rooted AOT build
also passed a full gameplay debugger-graph round trip on the handheld: imported
the controlled Landing Site fixture into previously unused slot 9, loaded frame
720 with the expected build warning, continued to frame 1931, saved, and reloaded
frame 1931 without a build warning. Gameplay continued after reloading. The test
slot was removed and original regular save/backup and slot 0 hashes were verified
after restoration. This extends the earlier cinematic-only state check; it does
not prove portability of every possible runtime graph.

The subsequent `3c433bc` candidate retained all 34,905 Core, 248 Diagnostics, and
512 Android declarations checked by the metadata tool. Windows Release build,
the full portable rendering contract, Android host checks, and the diagnostic
state/import/export suite passed. These checks are prerequisites for the sustained
device run, not substitutes for its timing/audio results.

The subsequent sustained AOT run beginning 2026-09-08 10:04:47 UTC covered 153
approximately one-second windows through opening/title and multiple attract demos:
81 opening windows averaged 59.90 emulation / 59.68 presentation FPS; 70 gameplay
demo windows averaged 59.99 / 59.88 (minimum paint 58.0); two transition windows
averaged 60.35 / 59.35. AudioTrack reported zero underruns throughout. State labels
refer to the end of each window, not every frame within it. This initially favorable
sample was contradicted by continuing the same run: at 10:07:27 UTC, host frame
9608, render cost increased from about 5.6 ms to 22.6 ms. Subsequent PlayingDemo
windows simulated/painted at approximately 36-37 FPS, with cumulative underruns
reaching 499. Later opening frames returned to 60 FPS without restarting. AOT
therefore remains opt-in; the proposed default change was not adopted. Investigate
the exact demo/effect at this host-frame interval before changing scheduling again.
Thermal service still reports `HAL Ready: false` without temperatures; do not claim
measured thermal headroom or broad sustained-performance acceptance.

### Reproducing the late X-ray demo cost

The slow interval is X-ray in room `$01/$06`, using `XrayGameplayRenderLayer`.
Generate its exact autonomous frame without a three-minute device wait:

```powershell
dotnet run --no-launch-profile --project csharp/src/SuperMetroid.DiagnosticsVerification -c Release -- --export-autonomous-performance-state 9650 csharp/test-temp/xray-performance-new
```

The destination must not exist. It contains a private slot-9 state, recording,
and PNG; it does not touch desktop saves. The command advances the real frontend
and audio acknowledgements without input, then benchmarks the immutable packet.
Load the generated state into an unused handheld slot for device measurements;
preserve and restore the handheld's regular save because loading a state also
changes its SRAM. Exported states contain private cartridge data, not public assets.

On the AOT handheld, five seconds from this state reproduced 22.5-22.8 ms rendering
and approximately 37 FPS. Hoisting immutable register flags and backdrop lookup
outside the X-ray pixel loop reduced rendering to 20.4-20.7 ms, approximately
41 FPS. The before/after frame-9650 PNG SHA-256 was identical
(`E10BFC06ED2D822E9C6ABFE692A0C00429A1E947D622319A76CE59C21300389F`),
and X-ray activation/window/release checks passed. This is a partial optimization,
not a fix for the sustained-performance requirement: per-pixel background sampling
still needs profiling and improvement.

The next sampler revision caches each source tile row and precomputes its palette
colors once per immutable packet. 98,304 comparisons against the previous scalar
formula cover both bit depths, both map dimensions, flips, page/address wrapping,
discontinuous coordinates, priorities and transparent/opaque color zero. The same
frame-9650 image retains the hash above. Device rendering falls to 12.8-13.1 ms;
after the first resumed window simulation reaches 60 FPS with zero underruns.
Presentation still ranges 42-51 FPS in this short interval, with mailbox replacements.
This removes the measured simulation bottleneck but is not smooth-presentation
acceptance for #355.

A subsequent direct `Choreographer.IFrameCallback` experiment (invalidating before
traversal, with producer invalidations disabled while active) did not fix the AOT
presentation dips. At host frames 3867–3988 it measured 60 simulation FPS but
49–51 paint FPS, despite Android reporting only 4 janky frames out of 4080 and zero
audio underruns. The experiment was removed, not adopted. Next investigation must
correlate mailbox publication and consumption timestamps; another nearby change
to invalidation scheduling alone is not supported by these results.

`frame-handoff.log` now retains the last 2048 mailbox events at each stopped
lifecycle boundary and is included in diagnostic exports. Each row is monotonic
Stopwatch ticks, event kind, publication sequence, and generation-0/generation-2
collection counts: P=publish into empty mailbox,
R=replace pending frame, C=begin consuming, U=upload finished, E=draw found no frame.
Sequence IDs are mailbox-local, not cartridge frame numbers. Recording uses a
preallocated ring without per-event allocations; formatting/writing happens only
on pause or worker shutdown. Capture immediately after a low-paint window, because
a later healthy window can overwrite the relevant tail.
