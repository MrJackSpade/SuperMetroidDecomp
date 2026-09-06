# Quicksand cartridge comparison — #316

The native probe executes the private ROM's original 65816 instructions, including
inside-block detection, movement, pose transitions, and the one-shot PLM deletion
handler. It does not use the translated C quicksand callbacks as an oracle.

There are 16 cases of 180 frames each: shallow versus submerging sand, Gravity Suit
off/on, jump held 8/60 frames, and takeoff after 8/80 sinking frames. Both sides use
constructed 64-by-16-block geometry in Maridia, submerged beneath water. The upper
sand row uses BTS 82; the lower row is either 82 or submerging BTS 83. Solid terrain
begins at row 14. No enemies, recorded controller data, or private save data are
included in this fixture.

All 2,880 frames match Y including subpixels, pose, vertical speed including
subpixels, vertical direction, and extra sand displacement. This covers standing,
takeoff, rising, jump release, landing, sinking, and contact with the underlying
solid floor. It does not claim exhaustive input-handler or full-animation parity.

To reproduce, apply `quicksand-integration.patch` to `upstream-sm`, build Release
x64 with the installed toolset, and run from the repository root:

```powershell
./upstream-sm/build/bin-x64-Release/sm.exe --quicksand-probe 'Super Metroid.smc' > csharp/test-temp/native-quicksand.log
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-restore -- --quicksand-comparison-audit 'Super Metroid.smc' csharp/test-temp/native-quicksand.log
```

The native CLI dispatches before SDL and suppresses Windows fault dialogs. Reverse
only this integration patch after use and rebuild the ordinary executable. Remove
the generated trace; retain these reproducible fixture sources.

The separate `--sand-physics-audit` verifies the actual room 04/1A sinking path.
The normal Verification suite tests each sand callback's direction/suit branches,
horizontal submerging side effects, and pose-probe carry versus grounding.
