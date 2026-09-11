# Map-station automatic map display (#561)

Affected player version: 0.1.1. Ready for player confirmation after the fix below.

## Cause and native evidence

`upstream-disassembly/src/bank_85.asm`, routine
`MaybeTriggerPauseScreen_or_ReturnSaveConfirmationSelection` at $85:80FA,
compares MessageBoxIndex with $0014 and writes GameState=$000C at $85:8107.
The pinned C translation's `DisplayMessageBox_Async` has the same terminal branch.
This is an automatic transition after the acquisition message, not admission of
manual Start input during station ownership. The frontend implemented only the
latter, so closing the map message resumed ordinary gameplay indefinitely.

The frontend now recognizes completion of that specific message and enters the
existing pause fade/setup/map path. Other messages still cannot admit manual
pause on their closing frame. No new map renderer or station-specific menu exists.

## Reproduction and verification

Run from the repository root with the local matching retail ROM:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --map-station-pause-gate-audit "Super Metroid.smc"
```

The fixture seeds a fresh Crateria save, enters through the frontend, loads the
retail map room via door $83:8BDA, and places Samus beside its access block.
From there Left input must activate the station through real collision; the test
no longer directly calls `TryNotifyStationTouch`. A pulses acknowledge the message,
with no Start input before the automatic map display.

Before the correction, message close at fixture frame 160 fails: state remains
MainGameplay instead of PausingDarkening. Afterward it passes through the visible
map screen, dismisses using held Start (native delayed-held menu input), returns
to gameplay, and continues pushing into the acquired station for 180 frames
without a repeated prompt, pause or input lock. Acquired-area state is asserted.
The map capture under ignored `csharp/test-temp/map-station-561/automatic-map.png`
was visually inspected: Crateria's acquired map is visible inside the map frame.
No ROM, player state or screenshot is published.

The old audit incorrectly required the game to remain in gameplay after map
message completion; that assertion has been replaced, not preserved as cartridge
evidence. Full managed verification and the Windows desktop build pass (zero
build warnings/errors). Native instruction ordering was cross-checked in pinned
sources; this is not a claim of cycle-exact native-emulator comparison of the
complete station animation. Issue remains open awaiting player confirmation.
