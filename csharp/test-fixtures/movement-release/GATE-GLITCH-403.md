# Wrong-side gate investigation (#403)

Affected player version: 0.1.1. The reported room $02/$22 is Kronic Boost,
`RoomHeader_KronicBoost` at `$8F:AE74`, not an inferred substitute room.
Exact player weapon, trajectory and input sequence remain unspecified.

## Initial managed shot-origin sweep

`GateGlitchRoomAudit` loads the authored 32x48 room, whose downward gate is at
block `$0287`, pixel (112,320). Each trial clones unchanged loaded terrain/BTS
and reloads the resident PLM population, then advances its instructions twice.
The gate's initial spawn request is cleared from the observation queue.

Samus is stationary in normal-jump up-left aim pose `$6A`, with zero subpixels,
speed, equipped beams and items. X runs 128..152 inclusive; Y runs 304..384.
HUD selection is beam/missile/super (0/1/2), with ten actual missiles and supers,
no infinite-ammo setting. Shoot is pressed only on frame zero. Real projectile
production, collision and PLM stepping run for up to 24 frames per trial.
The positive observation is the resident gate's trigger timer becoming nonzero,
not Samus crossing the gate and not merely an impact animation.

| Weapon | Activating origins / 2,025 |
| --- | --- |
| Ordinary beam | 50 |
| Missile | 119 |
| Super Missile | 116 |

Examples: missile (128,347) activates on frame zero; ordinary beam (140,360)
also activates on frame zero. These are measured managed outcomes, **not native
expectations**. The initial matrix deliberately includes possibly unreachable
origins and overlaps: it cannot establish a player-executable exploit or prove
that the activation window is too wide. No production fix or issue closure is
justified by these counts alone.

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --gate-glitch-room-audit "Super Metroid.smc"
```

The command prints every successful launch and per-weapon totals, preserving
pixel boundaries for a native sweep. It never moves Samus after seeding, changes
player save slots, or claims to reproduce the unspecified 0.1.1 input sequence.
Next: original-CPU comparison of these same launch windows, followed by real
rising/spinning/falling input cases, gate animation/state assertions, and blue/
green weapon and orientation controls. Existing gate-filter tests are not a
substitute for those technique checks. Source: the Gate_Glitch wiki and pinned
bank-$84/$94 routines; no wiki claim has yet been promoted to an expected result.
