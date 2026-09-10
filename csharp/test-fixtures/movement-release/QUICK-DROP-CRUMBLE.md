# Quick Drop: actual crumble lifecycle — partial #456

Uses the ROM/host/disassembly pins from [QUICK-DROP-CEILING.md](QUICK-DROP-CEILING.md).
This extends the ceiling-only tests through actual respawning crumble PLM setup,
instruction execution, floor contact, and input-driven falling turn completion.

## Deterministic setup

60 cases, 60 frames each: both facings, opposite-direction input delayed 0–4
frames, and crumble actors aged 0–5 handler passes before the falling sequence.
Input remains held until frame 29. No jump button or cheats.

Synthetic 144-by-80 room; solid safety floor row 32. Five adjacent 1x1 crumble
blocks occupy row 16, columns 62–66. The cartridge spawn routine `$84:84E7` receives
header `$D044` with downward collision direction; its actual `$84:CE37` setup
changes `$B000` to `$80BC` and starts its four-pass timer. The fixture seeds a
previous trigger by running that setup and aging the real `$84:85B4` PLM handler.
It does not manually delete terrain. The port runs `TrySpawnSamusContactCrumbleBlock`
and the real PLM handler for the same warmup passes.

After warmup, initialize Samus at `(1032,233)`, zero subpixels, falling pose
`$29/$2A`, speed `3.0000`, direction down, Morph Ball only, no beams/liquid,
99 health. Previous pose and direction/movement match this seeded falling state.
This is a constructed in-flight start, not a controller-generated prior jump.

Then run normal alpha/interactive-enemy/movement/animation/projectile/pose/draw/
palette stages as in [QUICK-DROP-TIMELINE.md](QUICK-DROP-TIMELINE.md), followed by
the original CPU PLM handler each frame. The C# side runs complete `StepFrame`.
Compare exact X/Y fixed position, pose/movement, animation frame/timer, horizontal
speed/mode, vertical speed/direction, charge counter, and the central block word.
Input, dimensions, ordering and capture hash are checked.

## Successful technique and adjacent failure

Both facings agree. For age 2 / delay 0, frame 1 contacts the still-solid floor
during a falling turn: Y `00ED.FFFF`, speed `3.3800`. The real PLM subsequently
draws air `$0053` in that same frame. Samus continues falling with accumulated
velocity; turn completion on frame 6 reaches Y `00FF.2FFF`, speed `3.C400`.

Delay 1 is too late: contact on frame 1 lands Samus and clears speed. The block
still disappears in that frame. Native frame 2 switches landing art back to fall
at Y `00EC.FFFF`, zero speed, then begins building fall speed again.

## Reproduced production defect

The port omitted landing poses from both missing-floor transition admission
lists, although they execute movement type zero (standing). It therefore retained
landing/ground-turn art after support disappeared. Before the fix, 804 of 3,600
frames differed. Admitting landing poses to the existing shared walk-off path
produces zero mismatches, without changing successful Quick Drop velocity.

Early exploratory captures are not accepted: v1 seeded the wrong native previous
movement byte; v2 corrected it but contacted blocks only after they had become air.
The final v3 matrix deliberately overlaps solid contact with destruction and
includes explicit success, turn-completion, and late-failure assertions.

## Reproduction

Apply `native-quick-drop-crumble-entrypoint.patch` within `upstream-sm` using
`git apply --unidiff-zero`, build Release x64, then:

```text
sm.exe --diagnostic-quick-drop-crumble "Super Metroid.smc" capture.csv
SuperMetroid.DebugRunner --quick-drop-crumble-audit "Super Metroid.smc" capture.csv
```

Native entrypoint suppresses SDL dialogs and CPU execution is bounded. The shared
bounded helper now optionally accepts A/X/Y register arguments for authentic PLM
spawn; existing callers retain their zero-register behavior. Remove temporary
native host hooks after capture.

Two independent accepted captures have SHA256
`D790D2F30DCBCB6D8FC945B4F253594483B1DD681EEF8765F5ED044A3AC5CF24`.
CSV is archived in `quick-drop-crumble-native-capture.zip`.

## Remaining #456 scope

Follow-up: [QUICK-DROP-BOMB.md](QUICK-DROP-BOMB.md) supplies the remaining
bomb-family comparisons and the combined completion audit. The limits below
describe this fixture alone, not the final ticket status.

Actual bomb/Power Bomb destruction paths remain to be compared. These tests cover
one respawning 1x1 crumble actor family, not every size or regional timing variant.
The ticket remains open without awaiting-player-validation until the remaining
required paths are handled.
