# Damage-boost slopespark parity — #436

## Cartridge evidence

The private source recording is `slopespark.smv`, an 804-frame, one-controller
SMV v4 movie with SHA-256
`B273CC5A2873EDD8CC92BC3C951D19BF30655884F881D4EF5A35CA2EB96ABE70`.
Its embedded snapshot names `Super Metroid (JU) [!].smc`; replay used the pinned
Japan/USA retail ROM with SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
The ROM, movie, screenshots, WRAM checkpoints, and emulator states remain private.

The recording demonstrates an air damage-boost slopespark in room header
`$8F:92FD` (Parlor and Alcatraz). Native pre-controller checkpoints establish the
critical sequence beginning at movie frame 220:

| Input frame | Held input | Native result before the next sample |
|---:|---|---|
| 220 | Left + Jump | damage-boost pose `$50`, knockback timer 3 |
| 221 | Left + Jump | slope approach at `$028E.ABFF,$00BB.FFFF` |
| 222 | none | slope landing pose `$A5` |
| 223 | Jump | neutral-jump transition `$4C` while knockback has one tick left |
| 224 | Left | windup art `$C8`, normal movement handler `$A337`, palette owner 6 |
| 225 | none | retained 8.0000 extra speed and 58 palette ticks |

That final combination is the technique's Shinespark Suit state: the normal
movement handler owns Samus, while windup art, palette time, stage-four boost, and
8.0000 extra speed remain available for a later launch. It is distinct from an
ordinary windup, whose movement handler is `$90:D068`.

## Original-CPU comparison fixture

`native-damageboost-slopespark-probe.h` loads the private frame-220 WRAM
checkpoint, restores unpatched retail ROM bytes, and executes the original 65816
input, movement, collision, animation, transition, hurt, and palette routines.
It records four paths over the authored retail slope:

- the movie's successful dry inputs;
- a dry adjacent failure with landing Jump delayed one frame;
- the same successful handoff while fully submerged without Gravity Suit; and
- its one-frame-late underwater failure.

The underwater setup retains the movie's room, slope, position, subpixels, pose
history, and stored shine. It substitutes the cartridge's suitless-water
post-contact vertical speed (2.0000), liquid state, and seven-tick knockback
window. The successful path needs four held-Left transition frames because the
underwater neutral-jump animation is slower. The delayed path reaches `$C8` with
handler `$90:D068`, proving that it is an ordinary windup rather than retained
Shinespark Suit state.

The accepted 36-state numeric trace is
`damageboost-slopespark-436-v1.csv`, normalized SHA-256
`70CFEDB97C179652EFE0C0F58EA0CD06591F3D86B1398322870CCE167700FAA6`.
No cartridge bytes are present in it.

To regenerate locally, apply
`native-damageboost-slopespark-entrypoint.patch` to the pinned native source,
build it, then run:

```text
sm.exe --diagnostic-damageboost-slopespark ROM FRAME-220.wram NEW.csv
```

## Port audit

Run the retained comparison with:

```text
SuperMetroid.DebugRunner --damage-boost-slopespark-audit ROM damageboost-slopespark-436-v1.csv
```

The audit loads the real Parlor/Alcatraz room through production room loading and
starts at the same post-contact boundary. It compares every captured state field:
fixed X/Y, pose, movement type, base/extra/vertical speed, knockback timer and
direction, stored-shine timer and palette owner, boost counter, and semantic
movement-handler ownership. It also asserts the successful retained-suit end
states and the two distinct adjacent failures.

Result: **36 original-CPU states, zero mismatches** in air and suitless water.
No production gameplay change was required.
