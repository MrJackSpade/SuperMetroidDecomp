# Android diagnostic handoff

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

User-facing import and the remaining lifecycle/performance acceptance work are
still tracked in issue #355. Export/replay support is not completion of that issue.
