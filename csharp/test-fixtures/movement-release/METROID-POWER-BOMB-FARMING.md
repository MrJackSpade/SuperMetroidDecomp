# Metroid Power Bomb farming comparison (#441)

The connected damage/drop/collection sequence matches **8,400 original-CPU
frames** on the pinned NTSC ROM. Actual save/reload variation remains unfinished;
this does not complete #441's whole acceptance scope.

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
1,050 frames. The shared boundary is four 100-damage events after two
bombs (Metroid survives at 100 HP), versus death on the fifth event at frame 786
after three bombs. Hits occur at 96, 144, 441, 489, 786. Up on frame 850 and Jump
on 855..884 reach the drop without editing Samus or the pickup. Collection occurs
on frame 851: seed 1 yields a PB (ammo 2 to 3), seeds 2/3 large energy, seed 4 small
energy. The two-shot controls survive and collect nothing. These are assertions
in the capture routine as well as fields in the native comparison.

MOV1 exports geometry/movement; metadata records inventory, initial enemy and
scrolls. JSONL records every input, health/ammo, coordinates, explosion state,
enemy damage/invincibility/death, RNG, pickup collection and all enemy-projectile
slots including the pickup identity word. Outputs are private ROM-derived data,
not redistributable fixtures. No player save or save-state slot is touched.

## Original CPU reproduction

Include `native-release-probe.h` then `native-metroid-farming-probe.h` after the
StateRecorder declaration in pinned upstream `sm_rtl.c`. Temporarily dispatch
`--metroid-farming ROM MOV1 OUTPUT_CSV RNG_SEED SHOTS` before SDL startup to
`DiagnosticMetroidFarming`. Build Release x64 and run all eight corresponding
MOV1 seeds. Remove the temporary includes/entrypoint and restore the normal
executable afterward. No translated enemy/damage functions supply expected data.

Use DebugRunner `--metroid-farming-compare MANAGED_JSONL NATIVE_CSV` to compare.
It checks every frame's movement, radii, resources, explosion words, enemy death,
RNG, all 18 projectile IDs/lifetimes, and active projectile coordinates/lists/
timers/pickup identity/header. Samus and slot-17 pickup radii are also compared.
Inactive C# slots clear their object fields while native WRAM retains old words:
after both IDs are zero, those non-live fields are deliberately not compared.
This is not raw WRAM parity, pixel/audio parity, or a PAL claim.

The comparison exposed two defects: Power Bomb cleanup/allocation cleared the
radius-speed word prematurely (fixed by retaining it until native setup); and
unmorph published the target collision radius one frame early, collecting the
pickup early. Unmorph now retains the old live radius through beta, and ordinary
alpha refreshes it at the cartridge's next-frame boundary. A focused radius-speed
regression and the direct unmorph test cover these ownership rules. The full
connected capture checks collection frame and ammunition rather than no-crash.

Remaining #441 work: test actual save/reload timing variation. Merely
seeding four RNG values does not prove save/reload behavior. Existing tests that
set the Metroid to one HP do not establish this encounter. The old conditional
immunity commentary is not evidence of retail immunity: the pinned Metroid's
Power Bomb vulnerability entry is one, so the ordinary damage path is admitted.
