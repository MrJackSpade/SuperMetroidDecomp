# Retroid Pocket Classic testing build — issue #355

This is an in-progress **private** Android port of the same C# game, not an emulator
or streaming client. The APK includes no cartridge or extracted audio. On first launch,
choose your own supported ROM with Android's document picker; see [ROM setup](ROM-SETUP.md).

## Current milestone

The separate Android host boots the cartridge frontend, runs the shared managed
audio renderer through Android AudioTrack, accepts controller key events, and uses
the software reference renderer. This is not yet the completed testing deliverable.
State-slot UI, portable recording/export, remapping, audio focus, complete lifecycle
recovery, and sustained gameplay performance verification remain work in progress.

## Build and install from Windows

The .NET 10 Android workload, Android SDK and Java tooling are required. They are
already installed on the development workstation. From the repository root:

```powershell
dotnet build csharp/src/SuperMetroid.Android -c Release
adb install -r csharp/src/SuperMetroid.Android/bin/Release/net10.0-android/android-arm64/org.supermetroid.csharp.testing-Signed.apk
adb shell am start -W -n org.supermetroid.csharp.testing/crc641619a672f3a517b4.MainActivity
```

Use Release for performance measurements. Both configurations embed the assemblies;
the APK does not require Visual Studio fast-deployment files. Current builds use the
workstation's local Android debug signing key. Keep that key outside source control;
maintaining a stable explicitly configured testing signing identity is a remaining
delivery task. Installing with `-r` preserves app data; do not uninstall or clear data
to update a build containing a useful save.

The private testing manifest permits `adb run-as` and disables Android cloud backup.
The SDK's LLVM JNI marshal-method optimization caused a reproduced `onCreate`
`UnsatisfiedLinkError` when used with the debuggable Release manifest. The project
explicitly uses dynamic JNI registration instead; C# Release optimization remains on.
Trimming/AOT are initially disabled so reflection-based state graphs are not removed.

## Storage and diagnostics

Package: `org.supermetroid.csharp.testing`. App-private `files/` contains:

- `game/`: cartridge copy and audio extracted from the user-selected ROM, verified on launch.
- `SuperMetroid.ini`: shared gameplay/audio options, created once with normal defaults.
- `SuperMetroid.save.json`: normal battery-backed save data, written atomically.
- `timing.log`: per-second emulation/presentation rates and average step/render/mix costs.
- `input-events.log`: initial controller-event diagnostics, not a replay recording.
- `last-error.txt`: game-worker failure details when a failure occurs.

```powershell
adb shell run-as org.supermetroid.csharp.testing tail -n 20 files/timing.log
adb shell run-as org.supermetroid.csharp.testing cat files/last-error.txt
```

The game worker pauses and releases queued host audio when focus/activity is lost.
It retains the in-process game and APU state for resume. Android process-death recovery
and a portable debugger-state handoff are not implemented by that pause alone.

## Display and input

Integer square-pixel scaling is the default. `DisplayViewport.IntegerPixels` uses the
largest whole equal scale fitting the available view, with centered black borders and
nearest-neighbor filtering. It deliberately does not apply the Windows 4:3 TV correction.
Diagnostics are overlays and cannot move the canvas when their text changes.

The paired Classic reports a physical framebuffer of 1080x1240 but its current display
rotation exposes 1240x1080. The host follows that configuration rather than forcing a
phone-style orientation. With the action bar hidden, the measured view was 1240x984,
giving a 1024x896 image at 4x. Immersive mode now hides status/navigation bars and
uses the full 1240x1080 surface, retaining centered 4x scaling. Diagnostics overlay
the top edge without reserving layout space. System bars can be revealed transiently
by an edge swipe; focus/resume restores immersive mode.

Back, controller Mode, or a long press opens testing tools, pausing the simulation.
Ten debugger slots are available. Empty slots show a message, not a fatal error.
The same menu includes INI settings for invincibility, unlocked infinite ammo, map
reveal, cinematic skip, audio and volume. Changes are saved atomically for the next
app launch; they do not reset the current run. A differing active value is shown
beside the saved value. Desktop-only renderer/GitHub-CLI options are preserved but
not offered as working Android features. D-pad and accept navigate these dialogs.
Input journals live in private `files/input-recordings`; state-load journals preserve
their exact seed separately from the overwritable slot. Regular saves remain in
`files/SuperMetroid.save.json`. Native file import/export is still pending.

`dotnet run --project csharp/src/SuperMetroid.DiagnosticsVerification -c Release`
tests the production session data class locally: empty/corrupt slots, 30 identical
video/audio continuation frames, separate journals, and seed survival after overwrite.
On-device state saving and the testing menu have been exercised; cross-runtime
replay and on-device restore verification are still pending.

Default Retroid mapping: reported A=accept/jump, B=back/dash, X=fire, Y=item cancel.
Player testing confirmed that the Classic reports Nintendo button names rather than
the Xbox positional naming initially assumed by the host. L1/R1=aim and Start/Select
retain their SNES functions. D-pad key events are
supported. A shared synchronized latch preserves taps completed between simulation
samples. Motion-axis handling and configurable mapping are still pending.

## Verification so far

- `dotnet run --no-restore --no-launch-profile --project csharp/src/SuperMetroid.Verification -c Release -- --android-host`
  verifies exact integer viewports (both orientations, insets, small surfaces) and tap,
  hold, multiple-producer, and focus-clear input behavior.
- Full Core verification and the Windows Release build passed after the shared audio
  extraction. Existing Windows code uses that same adapter.
- Actual-device APK install, private asset extraction, title/file/options/intro startup,
  Start-event delivery, and a Home/resume interval have been exercised. The timing log
  stopped advancing in the background, and the same activity resumed afterward.
- Initial Debug demo gameplay ran around 23 FPS with ~33 ms software rendering: unsuitable.
- Early optimized intro samples ran around 60 simulation FPS, ~4 ms rendering and
  ~0.3–0.4 ms mixing. Presentation varied below 60; startup underruns were observed.
  These are preliminary measurements, **not** proof of acceptable sustained gameplay.

Next: finish host functionality, then benchmark demanding real gameplay and improve
presentation/backend performance as required. Ticket #355 remains open and is not yet
awaiting player validation.
