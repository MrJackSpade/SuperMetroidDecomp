# Zebes approach ship visibility (#513)

## Reproduction and cause

The production cinematic kept drawing its Mode 7 ship after the approach ended.
The native routine at $8B:CA85 writes $10 to the main-screen layer register at
$8B:CABF before entering the 64-frame hold. That enables only OBJ: the planet and
stars remain visible, but the ship's BG1 plane must disappear before the pan.
The translation preserved the phase/timer change but omitted the layer-mask write.

The regression runs the real scene and compares each frame with an OBJ-only
composition from the same immutable PPU snapshot. Before the fix, all 64 hold
frames and 73 slide frames retained the Mode 7 layer. At the first hold and slide
frames the ship contributed three visible pixels, not just an unused descriptor.
It also verifies that the ship is actually visible beforehand: 174 approach-C
frames contain a nonzero Mode 7 contribution (26 pixels on the first such frame).

## Fix

Store and apply the translated main-screen layer mask. At the native transition,
disable BG1 while leaving all four OBJ priorities enabled. Both the direct software
renderer and captured-layer renderer honor the same mask. No timing, actor deletion,
camera trajectory, or tilemap clearing workaround is involved.

## Verification

DebugRunner command:

```
--zebes-ship-visibility-audit "Super Metroid.smc" OUTPUT_DIRECTORY
```

Fails before the production fix; passes afterward with zero incorrect hold/slide
frames. Every hold/slide composite must equal the OBJ-only reference in both paths,
and no Mode 7 layer may remain in those snapshots. Before/after captures were
inspected locally and remain ignored; no ROM or images are published.

The full Ceres/Zebes audit passes (1678 cinematic calls, 641 landing frames,
automatic checkpoint reload and Landing Site left-travel checks). Windows Release
build passes. Player confirmation remains required; this is a pinned-source and
rendered-frame regression, not an original-emulator video comparison.
