# Crocomire Power Bomb reaction (#528)

Affected player version: **0.1.1**. The forward charge is cartridge behavior; no
gameplay correction is needed for the reported expectation of backward knockback.

Run the headless regression:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --crocomire-power-bomb-audit "Super Metroid.smc"
```

## Stimulus

Load the retail room/enemy population. Let the ordinary wake animation reach each
of the three mouth shapes. Submit a Power Bomb explosion through
`ResolveOrdinaryPowerBombHits` at the body position with radius 255. Do not invoke
the private reaction directly or force an instruction list/fight function.

Both body and tongue are admitted by the retail vulnerability record. The body
selects its special reaction without losing HP; the tongue receives its normal
damage callback. Existing `--crocomire-audit` covers those collision details.

## Independent schedule and result

Pinned `upstream-disassembly/src/bank_A4.asm` and `upstream-sm/src/sm_a4.c` agree:
$A4:B992 initializes the charge counter from $869E (three). The selected program
is $BDAE, $BDB2, or $BDB6 according to the current mouth component. Those programs
close the mouth, animate the claws, then run two repetitions of the $BE06 loop.
Each repetition contains eleven **move left four pixels** callbacks, toward Samus
on Crocomire's left. This is not the rightward mouth-shot knockback program.

The test separately walks the ROM's timed spritemap records, motion-neutral
sound/dust callbacks, four-pixel moves, goto, and charge-counter handoff. Unknown
instructions fail. It compares production X/Y on every call and every timed
spritemap before handoff, rather than deriving expectations from the production
AI's outputs.

| Mouth at impact | First move call | Handoff call | Total displacement |
| --- | ---: | ---: | ---: |
| Fully open ($BDAE) | 83 | 175 | -88 pixels |
| Partly open ($BDB2) | 81 | 173 | -88 pixels |
| Closed ($BDB6) | 79 | 171 | -88 pixels |

Calls are zero-based, starting with the first enemy update after collision. Every
movement is -4 pixels; Y stays constant. The next ordinary forward-step program
may immediately choose a projectile attack before its next timed frame.

Identical no-Power-Bomb controls remain in the initial encounter sequence without
moving forward or entering the charge. This distinguishes the explosion reaction
from an unrelated attack already in progress.

All three schedules and controls pass on the project's ROM. No gameplay code was
changed. This is a collision/AI test with an independent ROM-list schedule, not a
new full emulator video comparison, and it does not model the entire expanding
Power Bomb visual effect. Keep the investigation open awaiting player confirmation
of the documented behavior rather than reversing an authentic reaction.
