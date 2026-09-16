# Frozen-enemy gate clipping parity (#407)

Affected version: Unknown (technique-parity audit)

## Pinned NTSC cartridge behavior

The bounded original-CPU probe in `native-frozen-gate-probe.h` loads the
Caterpillar room (`$8F:A322`) from the pinned Japan/USA ROM. It retains the
retail level data, downward gate at block `(38,53)`, and bank-$84 PLM handler.
The room's three retail Zero enemies are also asserted by the managed verifier.
Only the long player setup that moves and freezes a Zero beside the gate is
collapsed into its final collision body.

Samus uses the standing-right collision box at `(602,860)`, with zero
subpixels. The Zero uses its retail 8-by-8 collision radii at Y=860. The CSV
records four original-CPU calls through `$90:93B1` followed by `$84:85B4`:

- six pixels toward a frozen Zero at X=621 leaves the future hitboxes tangent;
  terrain wins, Samus stops at X=603, and the gate remains asleep;
- seven pixels creates strict frozen-enemy overlap first; enemy collision clips
  the move to X=608, block column 38, and the gate wakes;
- moving the frozen Zero one pixel right restores tangency and the failure; and
- unfreezing the Zero makes it non-solid, so terrain wins again.

This is the collision-priority mechanism described by the Gate Glitch article.
On the pinned NTSC revision, Speed Booster is the equipment needed by ordinary
gameplay to produce the successful motion. The focused probe injects only the
final 7.0000-pixel request so the collision/PLM boundary remains deterministic;
it does not grant a synthetic gate exception or alter ordinary movement.

Regenerate the fixture by applying `native-frozen-gate-entrypoint.patch` to the
pinned `upstream-sm` source, copying `native-frozen-gate-probe.h` beside
`sm_rtl.c`, rebuilding, and running:

```text
sm.exe --frozen-gate-probe "Super Metroid.smc" NEW.csv
```

## Managed behavior

The managed verifier loads the same retail room through the production room
loader, locates its real gate and Zero population, and runs the production
solid-enemy-first horizontal mover and PLM handler. It compares enemy-versus-
terrain collision ownership, final X, and gate wake state against every native
CSV row, including both adjacent failures.

The project supports the pinned NTSC Japan/USA cartridge only. The article's
PAL Caterpillar setup reportedly works without Speed Booster, but no PAL ROM was
loaded and no PAL-only physics were introduced. PAL coverage remains explicitly
unsupported rather than being guessed into the NTSC runtime.

## References

- Pinned local Super Metroid disassembly (`upstream-sm`), especially
  `$90:93B1`, `$A0:A8F0`, `$84:85B4`, and `$84:BB6B`.
- <https://wiki.supermetroid.run/Gate_Glitch>
