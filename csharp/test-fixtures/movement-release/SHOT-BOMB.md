# Shot suppression of bomb jumping (#458)

The accepted capture is `shot-bomb-native-capture.zip`, containing
`shot-bomb-458-v3.csv`. SHA-256 of the CSV:
`04951D7C788EF33D63D71BB64F0AA1129A32DACBB4963FB92189DD6624B32F66`.
Two independent native runs produced identical bytes.

ROM: Japan/USA rev 0, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native runner: upstream-sm `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly cross-check: `362be646929cf8e483f692b73a6561cfc2dc1d0d`, banks 90/A0.

## Reproduction

Include `native-release-probe.h` and then `native-shot-bomb-probe.h` after
StateRecorder in sm_rtl.c. Add a headless main dispatch calling
`DiagnosticShotBomb(argv[2], argv[3])` for `--shot-bomb ROM CSV`, before SDL.
Build the pinned native project and execute twice to different output files.
Remove these temporary hooks afterward. No player saves or emulator windows needed.

Each case starts in a constructed 16x32-block air room with floor at row 16,
walls, optional low tunnel, zero BTS, and correctly initialized room byte size.
Samus has Morph/Bombs, no beams, and no cheats. Real inputs place a bomb at
frame 0, unmorph using Up, optionally shoot at frame 35..60, and optionally
morph and move into the tunnel. Both facings, shot/no-shot controls, and both
geometries yield 208 cases of 100 frames. The native runner executes full
alpha, projectile interaction, beta movement/animation/pose processing, and
the gameplay timer tail. It does not hand-position the bomb or force a launch.

Run the production comparison after extracting the accepted archive:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --shot-bomb-comparison-audit "Super Metroid.smc" PATH/shot-bomb-458-v3.csv
```

It checks inputs and every frame's X/Y fixed-point position, pose, animation,
bomb-jump direction, vertical speed/direction, interaction timer, projectile
counts, bomb fuse and bomb type. Explicit assertions additionally require the
shot suppression window, neighboring successful launches, matched tunnel
controls, and timer expiry.

## Findings and changes

- A beam sets the projectile interaction timer to ten. A09785 rejects bomb
  overlap while it is nonzero; A09169 decrements it after the frame. Previously
  the C# producer wrote the word but neither the bomb consumer nor tail used it.
  Runtime now samples overlap after both weapon producers and decrements the timer.
- On frame 51, shots at 42..51 prevent the jump; shots at 41 or 52 do not.
  No-shot standing controls launch. Tunnel cases 42/43 demonstrate a real
  shot/no-shot difference rather than morph timing alone. This is not blanket
  immunity based on the firing sprite.
- Standing movement A383 resets normal left/right animation while Shoot is
  held (A3C1 timer 16, frame zero), independently of successful shot admission.
  The reset was absent; the production standing handlers now implement it.
- Hit interruption clears unsupported pending bomb jumps before pose selection.
  C# skipped the clear when an animation transition won, retaining a stale
  direction. Six tunnel cases exposed this (198 differing frame records).
  Rejection now runs before animation-selected pose installation as in the ROM.

Exploratory v1/v2 captures are not accepted: v1 failed to finish unmorphing;
both omitted the room-size word used by projectile block collision. The latter
caused a false beam-lifetime discrepancy. The comparator rejects their hashes.
No projectile-lifetime workaround was introduced.

## Verification

- Accepted comparison: 208 cases / 20,800 frames, zero mismatches.
- Existing original bomb-chain, repeated, triple, horizontal, ladder, ceiling,
  hurt/bomb, and live hurt/bomb captures: 468,160 frames, zero mismatches.
- Metroid post-alpha controller capture: 7,200 frames, zero mismatches.
- Full core Verification suite passed; DebugRunner build: zero warnings/errors.

These are deterministic native/production comparisons. Player confirmation is
still requested; this issue is not closed by synthetic testing alone.
