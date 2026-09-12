# PipeBug formation and death lifecycle (#582)

## Reproduction and source

The original `--pipe-bug-audit` fails on clean baseline 1647ba14 and the linear
table migration 421b514a: it expects a killed Norfair formation member's header
to become zero, but the actual header is $DAFF. The audit also restores the dead
member's original properties and invents a Deleted flag after the death routine.

Pinned `upstream-sm/src/sm_a0.c`, `EnemyDeathAnimation` ($A0:A3AF), saves property
bit $4000, emits the death effect, clears the 64-byte record, and installs
$DAFF/bank $A3 when respawning. The killed actor's original header, position and
respawn-qualified physical index belong to the death projectile. No Deleted flag
is retained. Power Bomb's outer dispatcher subsequently adds ProcessOffScreen.

Pinned `upstream-disassembly/src/bank_B3.asm`, $B3:8BFF-$8CA5, and `sm_b3.c`
agree that the leader writes instruction/function/formation fields through all
five physical slots, even a cleared slot or respawn placeholder. Only the leader's
instruction timer and loop counter are reset at $8C19-$8C1F.

## Runtime defect reproduced before repair

After correcting the death fixture, an exact routine-boundary assertion fails:
`slot 1: instruction=1/22, loop=0/42`. The production formation loop called the
generic instruction installer for every member, which incorrectly reset each
follower's timers. The fix resets the leader once and uses pointer-only writes
for all five records, matching the cartridge stores. Other uses of the generic
instruction installer are unchanged.

## Verification

`PipeBugAudit.Formation.cs` checks all 16 combinations of four follower slots,
left/right facing, and respawn/no-respawn death. Each case performs real lethal
projectile collision, verifies the immediate clear/placeholder and death-effect
publication, then invokes the real formation routine at its native boundary.
Distinct survivor timer sentinels and the dead member's zero timers remain intact;
the leader alone resets to 1/0. All five native function, stagger-delay and
post-rise-function writes are checked, including the dead physical alias.

Beam and Power Bomb death assertions now check exact cleared/placeholder state,
kill count and a single effect carrying the pre-death header/position/native slot
and respawn bit. The Power Bomb post-callback property is checked separately.

The complete audit passes all 77 records in 23 populations, all four headers,
normal/strong emergence, Norfair stagger/flight, both yellow flight/arc directions,
sprite drawing, contact, beam death, freeze and Power Bomb death. Full Release
Verification and Windows Release build are also required for this runtime change.

No public ROM/state/image/audio fixture is added. This is a developer-discovered
diagnostic/runtime defect, not a claim that an unrelated player report is fixed.
