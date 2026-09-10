# #461: downback and collision-selected posture

Source: https://wiki.supermetroid.run/Hitbox_Manipulation (revision 10438).
This is a room-local original-CPU comparison, not a controller route across rooms.

## Pinned evidence

- ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
- Native host: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
- Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
- Decompressed CSV SHA256: `BAACB116E1DE5A03CDBE43DB2C2BA7DA419ABAF538F412DF31401EF1DE682801`.
- Two separate native captures produced identical CSV bytes.

`native-downback-entrypoint.patch` installs only an explicit headless diagnostic
entry point. Apply it to the pinned native tree, build, and run:

```text
sm.exe --diagnostic-downback "Super Metroid.smc" downback-461-v2.csv
SuperMetroid.DebugRunner --downback-audit "Super Metroid.smc" downback-461-v2.csv
```

The CSV is archived in `downback-461-v2.zip`. Remove the temporary native hooks
after capture. The probe restores original ROM bytes and executes bounded original
CPU routines; it does not call translated movement to generate expectations.

## Matrix and observed properties

1,360 cases, 112 frames each: both facings; ordinary falling, descending normal
jump, descending wall jump, actual unmorph input, and actual ordinary Zoomer contact;
empty space or a close 32-pixel opening; forward, Down, Down then back, Down then
forward; 17 Down-start timings. All inputs release at frame 40.

The authored opening is between rows 29 and 32 at column 66 (right) or 61 (left).
The lower safety floor is row 48. It tests entering the mouth and contacting its
lower edge, not traversing an entire retail tunnel. An earlier capture placed this
geometry too far away and was discarded as insufficient collision coverage.

All frames compare fixed-point position, pose, movement type, animation frame/timer,
base/extra horizontal velocity, acceleration mode, facing, vertical speed/direction,
flare counter, health, invincibility, knockback timer/direction and active movement
radii. Radius is sampled after native alpha / before managed movement: the native
end-of-frame scratch radius can retain the old pose while managed radius is eager.

Named assertions independently establish:

- Falling, unmorph and damage entry retain the radius-ten down-aim pose and original
  facing while holding Down+back. Normal jump and wall jump turn instead.
- Damage entry actually loses five health and has active invincibility; it is not a
  manually selected hurt pose. Unmorph likewise executes the real Up transition.
- At frame 20, ordinary falling downback enters the opening while forward-only and
  Down+forward controls remain blocked against its edge, in both facings.
- A right-facing falling, geometry-one, forward-aim case (delay 11, frame 28)
  substitutes crouch while preserving downward speed `$0000:C400` and direction two.
- Wall-jump entry, geometry one, Down+back, delay 11, frame 20 substitutes crouch
  without starting aerial reversal acceleration, in both facings.

## Reproduced defects and correction

The original managed implementation diverged on 7,192 frames. `$91:F404` first
resolves pose expansion and then returns whether the final pose differs from the
requested pose. `$91:EB88` skips the requested momentum/landing command when it does.
Managed ordinary and compact landings incorrectly ran command five even after
collision substituted crouch or rejected the requested pose. Gating that command
removed all but five mismatches.

Those five were aerial turns that folded momentum before resolving expansion.
Native resolves collision before `$91:F433` dispatches the final movement type;
a crouch substitution never reaches `$91:F952/$91:F98A`. Moving the fold after
accepted collision removed the remaining mismatches. No downback-specific movement
rule was added. The held-Down priority in tables `$91:AD94/$91:ADD2` already works.

The old cramped-landing unit test asserted the same incorrect unconditional
cleanup. Its geometry/position assertions remain, but velocity expectations now
reflect the original-CPU result. A separate assertion preserves suit-palette restoration,
which belongs before the landing-command gate. Player confirmation remains separate from these
deterministic tests.
