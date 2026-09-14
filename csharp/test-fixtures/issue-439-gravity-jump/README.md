# Gravity Jump pause/equipment parity (#439)

Result: **15 cases, 3,300 movement frames, zero native/port mismatches**.
The port already preserves the native technique. No gameplay fix was needed;
the new regression checks the actual pause/equipment integration rather than
assigning an equipment word to a jumping Samus. Awaiting player validation.

## Setup and exact inputs

- Constructed 144 by 80 block room, flat floor at block row 32 (Y=512).
- Standing right at X=1000, Y=491, initially zero subpixels and velocity.
- Gravity Suit collected, no other items or beams, 99 energy. No cheats/enemies.
- Water surface Y=8: the entire compared trajectory remains submerged.
- Frame 0: Start. Frames 1–30: the native gameplay fade-out interval.
- Jump is pressed at movement frame 27, 28, 29, 30 or 31 and then held.
- After frame 30, gameplay freezes for the menu. Frame 31 denotes the first
  resumed gameplay frame, not the first host frame spent in the menu.
- Three equipment scenarios: keep Gravity equipped; remove Gravity in the
  menu; or start and remain suitless. All three go through pause/unpause.

The managed fixture enters the real frontend from a disposable in-memory save,
requests pause through Start, and advances all 30 fade-out gameplay frames. It
waits for the interactive menu, holds R to reach equipment, releases input, and
presses A only in the remove-Gravity scenario. With this inventory the actual
initial selector chooses Gravity. Start then leaves the menu; held Jump is
established during the frozen unpause fade, after the menu no longer accepts
equipment toggles. The late-jump control presses Jump on the first resumed frame.

No position, velocity, pose or animation is assigned across the menu boundary.
The checker asserts they remain frozen, and checks that only the intended
equipped bit changes while collected inventory remains intact.

## Native comparison boundary

The headless native runner executes original unpatched 65816 instructions:

- Input, interaction, movement, animation, pose transitions and bookkeeping for
  every compared movement frame.
- `$90:EA45` for Start admission and its fade-counter initialization.
- `$80:8924` for the 30 fade-out updates, requiring darkness before freeze.
- `$82:AB47` for the real initial equipment selector and `$82:B0C2` for the
  real suit-page A response. It does not directly clear the Gravity bit.
- `$91:E633` for the movement-specific equipment reconciliation performed
  during native pause teardown.

Rendering-only native menu frames, NMI timing, DMA and audio are **not** emulated
by this CPU fixture. Their frozen movement interval is collapsed. The C# side
does run its full frontend menu/fade sequence. This compares the pause movement
boundary and equipment mechanics, not native menu pixels, real-time duration,
controller hardware polling or a full-machine emulator movie. RNG is irrelevant
to the constructed no-enemy scene. PAL is not covered.

## Assertions and measured result

Every movement frame compares X/Y including subpixels, pose, animation frame and
timer, both Y-speed words, vertical direction and equipped inventory.

On a last-fade-frame launch, the suited and Gravity-removal cases start with
`$0004.E000` Y speed. The menu preserves that pair. After resuming, keeping
Gravity decelerates it to `$0004.C400`, while removing Gravity decelerates it
only to `$0004.D800`. Ordinary suitless launch begins at `$0001.C000`.

The independent apex assertions (world Y, smaller is higher) are:

| Scenario | Jump 27 | Jump 28 | Jump 29 | Jump 30 | Jump 31 |
| --- | --- | --- | --- | --- | --- |
| Keep Gravity | 017C.E7FF | 017C.E7FF | 017C.E7FF | 017C.E7FF | 017C.E7FF |
| Remove Gravity | 0085.23FF | 0079.5BFF | 006D.4FFF | 006D.4FFF | 01BA.1FFF |
| Start suitless | 01BA.1FFF | 01BA.1FFF | 01BA.1FFF | 01BA.1FFF | 01BA.1FFF |

Thus the adjacent late input loses the height exploit, not just some animation
frames. Each case runs through frame 219, beyond its apex. The longer exploited
descent is not followed all the way back to the floor.

## Reproduce

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --gravity-jump-comparison-audit 'Super Metroid.smc' `
  csharp/test-fixtures/issue-439-gravity-jump/native.csv
```

Rebuild the original-CPU trace from an x64 Visual Studio developer prompt:

```bat
csharp\native\GravityJumpAudit\build.cmd
csharp\native\GravityJumpAudit\audit.exe "Super Metroid.smc" > new-native.csv
```

The shared console CPU adapter is in `csharp/native/Common`; it rejects unmapped
accesses and bounds every call. No SDL, dialogs or player files are involved.
Only source and numeric expectations are published, never ROM bytes or saves.
The ROM and LF-normalized trace SHA-256 identities are pinned in
`GravityJumpNativeExpectations.cs`. Rebuilding the shared adapter also reproduces
the existing #425 Slopekiller trace unchanged.

## Diagnostic references

- [14% / Gravity Jump](https://wiki.supermetroid.run/14%25#Gravity_Jump)
- Pinned `upstream-disassembly/src/bank_82.asm`: pause/unpause state handlers,
  initial equipment selection and suit button response.
- Pinned `upstream-disassembly/src/bank_90.asm`: pause admission and movement.

The written technique description was a lead; numeric expectations were obtained
by executing the pinned Japan/USA cartridge, not copied from the description.
