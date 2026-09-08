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
