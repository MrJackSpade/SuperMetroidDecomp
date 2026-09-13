# Metroid Power Bomb farming candidate (#441)

This is a connected managed capture, **not completed cartridge parity**.

Run DebugRunner with:

```text
--metroid-power-bomb-farming-capture ROM PRIVATE_OUTPUT_DIRECTORY
```

Eight cases use initial RNG words 1..4 and either two or three real Shoot presses.
Samus has one five-ammo Power Bomb pack, full 10/10 missiles and supers, 399 energy,
Morph Ball and Gravity, no Ice, and no cheats. The constructed flat-floor Landing
Site fixture loads one actual Metroid, initially near Samus, with normal AI and
the runtime's real RNG callbacks. No damage, freeze, projectile, cooldown or
enemy-position edits occur after the initial seed.

Shoot occurs on frames 5 and 350, plus 695 for the three-shot case. Each run lasts
1,050 frames. The current managed boundary is four 100-damage events after two
bombs (Metroid survives at 100 HP), versus death on the fifth event at frame 786
after three bombs. Hits occur at 96, 144, 441, 489, 786. These timings need native
confirmation; the 48-frame repeat is observed rather than assumed correct.

MOV1 exports geometry/movement; metadata records inventory, initial enemy and
scrolls. JSONL records every input, health/ammo, coordinates, explosion state,
enemy damage/invincibility/death, RNG, pickup collection and all enemy-projectile
slots including the pickup identity word. Outputs are private ROM-derived data,
not redistributable fixtures. No player save or save-state slot is touched.

Remaining #441 work: reproduce the connected sequence on the original CPU,
assert eligible drops and RNG consumption (including successful PB replenishment
and adjacent failure), and test actual save/reload timing variation. Merely
seeding four RNG values does not prove save/reload behavior. Existing tests that
set the Metroid to one HP do not establish this encounter. The old conditional
immunity commentary is not evidence of retail immunity: the pinned Metroid's
Power Bomb vulnerability entry is one, so the ordinary damage path is admitted.
