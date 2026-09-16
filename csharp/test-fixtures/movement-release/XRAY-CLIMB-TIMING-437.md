# X-Ray climb timing audit (#437)

This fixture compares the port's X-Ray stand-up timing with the pinned
`Super Metroid (Japan, USA) (En,Ja)` cartridge (`SHA-256
12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`).
It is deliberately a room-local state audit rather than a complete out-of-bounds
route.

## Captured cartridge path

`native-xray-climb-probe.h` runs these original 65C816 routines in cartridge
frame order:

1. the active X-Ray HDMA pre-instruction from bank `$88`;
2. `Samus_SetRadius` at `$90:EC22`, as alpha does after HDMA;
3. the dedicated pose-input handler at `$91:FCAF` while X-Ray still owns it;
4. the X-Ray movement handler at `$90:E94F`;
5. `Samus_Animate` at `$90:8000`.

The capture covers both facings, air and fully submerged suitless-water
physics, and every release offset from eight frames before through twelve
frames after the turnaround input. Each case records the fixed-point position,
pose, radii, direction/movement metadata, animation frame/timer, HDMA phase,
and freeze word. The accepted CSV is stored in
`xray-climb-timing-437-v4.zip`; its uncompressed SHA-256 is
`C8F15D9C669E5559686E12D5E848013A76D79967DB3EDB8BA725E92E397A6DC5`.

Run the managed comparison with:

```text
dotnet run --project csharp/src/SuperMetroid.DebugRunner -- \
  --xray-climb-timing-audit "Super Metroid.smc" \
  csharp/test-fixtures/movement-release/xray-climb-timing-437-v4.zip
```

To recapture privately, temporarily apply
`native-xray-climb-entrypoint.patch` to the pinned `upstream-sm`, build its x64
Release target, and run:

```text
sm.exe --diagnostic-xray-timing ROM NEW.csv
```

The native hook is diagnostic-only and is not left applied to `upstream-sm`.

## Results

- 1,848 compared cartridge frames, zero field mismatches.
- Every successful teardown raises Samus exactly five pixels. The separate
  384-case displacement/depth audit in `XRAY-CLIMB-438.md` proves that primitive
  for both facings, all depths one through sixteen, air, water, and lava/acid.
- Air succeeds at release offsets `-2,-1,0,+1,+2`; the immediately adjacent
  `-3` and `+3` cases fail. This confirms the five-frame cartridge window.
- Suitless water succeeds at offsets `-2` through `+11`; `-3` and `+12` fail.
  The wider window is caused by the water-buffered crouched-turn animation, not
  by a special X-Ray allowance.
- Deactivation is ordered exactly like the cartridge: release changes phase
  two to three; the next two HDMA calls enter phases four and five; the fourth
  call performs teardown before alpha. The comparison includes each of those
  frames rather than asserting only the final position.
- Left and right cases are symmetric. Across the two media and both facings,
  38 of the 84 release cases climb.

## Setup, morph, and prolonged-scope boundaries

The ROM list at `$91:D223` performs eight explicit setup callbacks
`$91:CAF9`, `$CB1C`, `$CB57`, `$CB8E`, `$D0D3`, `$D173`, `$D1A0`, and `$D2BC`
before installing the ordinary `$88:86EF` beam pre-instruction. The production
`SetupStage` state uses the same order. `VerifyXraySetupBuffers` additionally
asserts the two separately timed BG1 reads, stage-four reveal construction,
and both VRAM page writes through the real runtime.

On hardware, expensive stage four spans three additional video refreshes. Those
are CPU-overrun lag frames, not three extra state-machine callbacks. This port
is an emulated-frame C# translation rather than a cycle-counted 65C816 emulator,
so it does not fabricate three global no-gameplay frames. The cartridge input
window above starts from the already installed full-beam handler and is exact;
claims about tapping entirely inside those three hardware-only lag refreshes
remain outside this architecture's timing guarantee.

X-Ray owns pose input while active, so Down aims the scope and cannot enter
Morph until teardown restores ordinary input. After teardown the normal morph
transition remains available even when Samus is embedded; no host-side safety
check prevents the cartridge's documented self-softlock hazard. Collision and
posture are exercised by the ordinary morph/door/gate/frozen-enemy regression
fixtures rather than by an X-Ray-specific escape rule.

Holding the scope does not rebuild its reveal map. `VerifyXraySetupBuffers`
keeps X-Ray active for ninety frames after deliberately destroying the live
source and proves that the frozen WRAM reveal snapshot remains unchanged. The
port models the gameplay-visible BG2 substitution, but it does not expose raw
SNES WRAM beyond modeled room buffers; arbitrary out-of-bounds memory-layout
corruption is therefore intentionally not promised as portable behavior.
