# Issue 415: aerial Bomb Spread

Technique source: https://wiki.supermetroid.run/Bomb_Spread (down-aim turnaround
and alternate charged-walljump routes). This is a technique audit, not a new
player-version report. The walljump route remains outstanding.

## Down-aim turnaround: original CPU comparison

The C# fixture uses the real runtime in a constructed local Landing Site clearing,
air above a flat floor at Y=512. Starting pose is normal left/right at (512,490),
zero subpixels, Morph Ball/Bombs/Charge, no liquids or host cheats. Charge and all
later poses are earned through input, not forced. The native room uses the same
local terrain with a 64-block stride; C# retains the loaded room's stride.

Zero-based sequence, 125 frames per case:

- Hold Shoot throughout. Add held Jump from frame 70.
- Hold Down from frame 76 through 104, except one release frame.
- Hold the opposite facing direction from frame 78 onward.
- Sweep the single Down release across frames 78 through 87. Repress it the next
  frame. Release Down for good on frame 105.

Only release on 84 / repress on 85 succeeds, in both directions. The successful
case enters airborne morph on 85, finishes on 91, retains charge 80 while holding
Down, advances the spread hold counter on 92 through 104, then creates five bombs
and consumes charge on 105. Every neighboring timing fails to create a spread.

The headless probe runs original $90:E695 alpha, $A0:9785 overlap, $90:A337
movement, $90:8000 animation, $91:E8B6 collision transition, and $91:EB88 pose
transition routines each frame. The initializer restores retail ROM bytes after
the upstream harness's normal patching. Pins: upstream C
`578f90b3cc49557bb70060ad033bb90b8cf8ac50`, disassembly
`362be646929cf8e483f692b73a6561cfc2dc1d0d`; ROM SHA256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

All 2,500 rows match exactly: inputs, pose, X/Y including subpixels, charge,
spread hold counter and bomb count. Native and managed CSV SHA256:
`A8A1D43A6AE93043E785633C099ED92B3AE8123F339A66F831CD44CF06849F00`.
No production mismatch was found or gameplay change made.

## Reproduction

Verification commands:

```text
--aerial-spread-transition
--aerial-spread-transition NATIVE_CSV
--aerial-spread-trace NEW_MANAGED_CSV
```

The no-argument variant is in the full suite and asserts the success/failure
window, retained/consumed charge, hold timing and synchronized charge mirrors.
The CSV variant additionally compares every recorded field, header and EOF.
Output creation refuses to overwrite earlier evidence.

Apply `movement-release/native-aerial-spread-entrypoint.patch` inside the pinned
upstream worktree, build Release x64, then run:

```text
sm.exe --diagnostic-aerial-spread ROM NEW_NATIVE_CSV
```

This path returns before SDL and disables native error dialogs. Reverse the exact
entrypoint patch and rebuild the ordinary binary afterwards. Those temporary
hooks were removed after this comparison. Traces remain private under test-temp.

## Limits / remaining acceptance

This is a room-local gameplay-logic comparison, not emulator video or full-game
timing. Room AI, gamma, PPU and audio mixing are omitted from the native slice.
The trace does not yet compare individual aerial bomb origins/trajectories or
the separate charged-walljump route. Those remain required before #415 is ready
for player validation. Existing grounded trajectory coverage is not substituted
for those acceptance checks.
