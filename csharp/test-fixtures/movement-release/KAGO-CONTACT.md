# Kagoing: ordinary enemy contact — partial #455

Uses the revision-zero ROM and native/disassembly pins in
[QUICK-DROP-CEILING.md](QUICK-DROP-CEILING.md). Original CPU contact callback
`$A0:8023/$A0:A477` reads Zoomer `$DCFF` damage and publishes invincibility and
knockback request. `$90:DDE9` decides whether the current animation admits it.
Turning suppression at `$90:DEE2` does not undo damage.

## Fixture

90 cases, 60 frames each: both facings; ground turn, morph, and unmorph input
patterns; one contact sampled at frames 6 through 20. Terrain is a flat row-16
floor in a 144-by-80 synthetic room. Starting X 1024, zero subpositions, Y 235
standing or 249 morphed. Morph Ball only, health 99, no beams/liquids/cheats.
Initial pose and previous pose/movement match; normal input and animation state.

- Turn: opposite direction held frames 8–29.
- Morph: Down on frames 8 and 12, released between taps.
- Unmorph: start morphed, Up on frame 8.

The native fixture invokes normal Zoomer touch at the selected frame before alpha,
with source X eight pixels toward the starting facing and matching Y. The port
uses its actual radius-based `ResolveOrdinarySamusContact` in the accepted-NMI
callback, at the same boundary. It prepares the real Zoomer definition/population,
enables instruction processing, and executes one no-player actor tick to establish
the sprite/interactive list, then restores the prescribed overlap before contact.
That preparation does not advance Samus. Each sampled actor is removed immediately
after contact in both fixtures so movement/AI/repeated overlap cannot contaminate
the isolated hurt-request timing. This is not a natural enemy-approach trajectory.

After contact, execute the normal input/movement/animation/pose/projectile/palette
stages documented in QUICK-DROP-TIMELINE. Port uses the complete runtime frame.
All 5,400 frames compare X/Y subpixels, pose/movement, animation frame/timer,
horizontal speed/mode/facing, vertical speed/direction, charge, health,
invincibility timer, hurt timer and knockback direction. Input/order/dimensions
and capture hash are checked. No RNG or player saves are involved.

## Reproduction and correction

Successful turn Kago: contact on frame 9 reduces health from 99 to 94, keeps
knockback direction zero, and finishes the turn at Y `00EB.FFFF` on frame 14.
Contact on frame 8 instead initiates normal knockback. Both facings agree.
Explicit assertions reject either blanket invulnerability or a disabled hurt system.

The matrix found a separate failure when a crouch/unmorph animation admits a hurt
request: the port installed its taller hurt pose without the shared pose-expansion
collision check, leaving Y `00F0.FFFF`, five pixels too deep in the floor. Native
UpdateSamusPose goes through `$91:F404` and keeps Y `00EB.FFFF`. Example: morph
pattern, contact frame 9, first divergence frame 12. Before correction: 1,254
mismatched frames. Passing the room/NMI/PLM context into normal hit admission and
reusing the existing expansion check yields zero mismatches. Terrain-less scripted
callers retain their explicit positioning; this fixture does not validate them.

Early test preparation omitted the enemy instruction-processing property and
therefore had no live spritemap: the fixture correctly rejected that missing
contact. No production collision bypass was introduced to make the fixture pass.

## Run

Apply `native-kago-contact-entrypoint.patch` in `upstream-sm` with
`git apply --unidiff-zero`, build Release x64, then:

```text
sm.exe --diagnostic-kago-contact "Super Metroid.smc" capture.csv
SuperMetroid.DebugRunner --kago-contact-audit "Super Metroid.smc" capture.csv
```

Headless error dialogs are suppressed; CPU execution is bounded. Remove native
host hooks after capture. Two independent captures match SHA256
`911A10033BE19C62F8FB76ADA49666022B38BA8B652945C252C1D1DB53223DF3`.
Accepted CSV: `kago-contact-native-capture.zip`.

## Remaining #455 scope

Kamer and Kzan platform traversal/solidity and their timed morph/unmorph/turn
sequences remain. This fixture establishes only ordinary enemy hurt interruption,
not platform pass-through, all enemy callbacks, or PAL behavior. Leave the ticket
open without awaiting-player-validation until those required cases are complete.
