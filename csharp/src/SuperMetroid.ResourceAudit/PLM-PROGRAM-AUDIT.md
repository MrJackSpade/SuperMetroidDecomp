# Static PLM program closure (#1161)

This development-only audit catches missing bank-$84 program words, byte operands,
draw definitions and control-flow targets before a player encounters the program.
It does not open a ROM, save, recording or runtime, and never calls a PLM handler.
Finite definition inventory is static data analysis, not a gameplay parameter sweep.

## Run and build gate

```powershell
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --plm-program-audit --root . --json out/plm-program-audit.json
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -c Release -- --plm-program-self-check
dotnet build csharp/src/SuperMetroid.ResourceAudit -c Release -p:RunPlmProgramAudit=true
```

Use a freshly built auditor from the same source checkout. Exit 0 means no findings
in the declared scope; 1 means missing/unresolved coverage; 2 is an auditor failure
with the full exception on stderr. Windows process policy prevents modal errors.

The `Static PLM program audit` workflow runs on relevant pushes/PRs and can also be
started manually. It retains JSON even on failure. Windows release packaging runs
the same build gate, so a missing record cannot be packaged silently.

## Independent requirements and actual providers

- All 70 compiled retail header initial lists are required independently of program
  exports. Named `RoomPlmInstructionLists` constants and constant instruction/link
  assignments in production source supply additional roots. Header constants used
  in a conditional expression are not mistaken for its returned list addresses.
- Every word position claimed by the production `RoomPlmProgramDefinitions` registry
  is inventoried. Disconnected components cover sleeping-list wake continuations,
  animation tails and linked records not named by a header. Byte spans distinguish
  packed operands from overlapping word reads; entering an operand is an error.
- A positive timer requires the following draw-pointer word and a provider that
  actually owns that draw. The draw providers are discovered from the production
  draw dispatcher, including its exact constant-pointer branches.
- Opcode layouts require every word and byte operand, including odd-byte sound IDs,
  unaligned VRAM-copy fields and four glass-shard parameters. Both conditional edges,
  loops and link targets are followed. Delete/sleep terminate the current path;
  other independently declared components still remain in the inventory.
- Scroll programs, collectible state machines, station animations and treadmills
  have explicit alternative owners, recorded in JSON. They are not suppressed
  missing generic instruction streams. Coloured/grey closing lists, gates, elevator
  platforms and other interpreter-owned actors remain in generic record coverage.
- Reviewed method-token fingerprints guard interpreter widths, helper advances,
  typed routes and every source method that reads program operands. Changed/removed
  consumers, newly introduced readers or stale contracts fail. Review the actual
  changed operand layout/routing before updating a fingerprint; never just copy a
  new hash to silence a finding. Comments and whitespace do not revoke a proof.

The production interpreter shares the exact definition resolver with the auditor.
Missing high-window operands now produce an explicit missing-definition exception,
not an attempted WRAM read. Only the actual low bank-$84 WRAM mirror can use live
memory. No ROM-read fallback was introduced.

## Findings corrected in this pass

There were 119 confirmed missing references: 107 draw-pointer operands and twelve
entire entry programs. Initial analyzer setup findings and the typed grey-door
pre-instruction format are not counted as runtime defects.

| Problem | Cause and correction | Confirmation |
| --- | --- | --- |
| Bomb block animation/restoration | Eight shared tails omitted 47 alternating draw words. Compile the dimension-specific forward/reverse draws and linked restore pointers. | Static closure; native-word comparison and existing focused block fixtures. |
| Contact crumble animation/restoration | Eight programs omitted 47 draw words. Compile the same native breakup sequence and the type-B linked restores. | Static closure; native-word comparison and focused crumble fixtures. |
| Breakable grapple blocks | Both programs omitted 13 draw words, including their initial solid grapple appearance. Compile the initial hold and complete break/reverse sequence. | Static closure; native-word comparison and focused grapple fixtures. |
| Super Missile, Power Bomb and enemy-breakable parents | Five live entry programs had no compiled records. Extend the shot-program catalog with their native queue forms and durations, including permanent Power Bomb's 3/2/1/1 timing. | Native control-word comparisons; explicit new-record assertions. |
| Bombed crumble/gated parent reveals | Six live reveal/delete programs were absent. Compile their timer, intact parent draw and delete; add only the missing draw definitions. | Static closure and native-record assertions. Existing editable linked-restoration domain remains unchanged. |
| Animal rescue wall | The source assignment selected an absent program. Compile the three-block vertical break frames, sound operand, escaped event and deletion. | Static closure; exact frame shape/word and event assertions. |

The corrected inventory has 4,068 defined word positions, 1,362 records, 773 timed
draws, 1,059 word operands and 163 byte operands, with zero findings. This includes
200 newly compiled word positions. Audit self-checks explicitly recreate the
reported `$84:CBBC` omission and confirm missing byte/draw, conditional/link paths,
unknown opcode, loops and source-guard invalidation.

## Boundaries

This proves finite record/operand/provider closure, not correct pixels, collision
trajectories, sound playback, owner-specific opcode eligibility or arbitrary
interprocedural points-to analysis. State-selected addresses outside the declared
roots/compiled components, dynamically authored WRAM, debugger fragments and memory
corruption are not certified. A completely new dynamic program family needs a
reviewed ownership adapter; it must not be declared covered merely by the presence
of one exported word. Whole-game validation remains the player's job.

Addresses and definitions were checked against the pinned bank-$84 source and,
for the existing block confirmations, the project's cartridge import fixture.
Reference labels alone are not substituted for revision-specific values (the
max-one sound lists use the cartridge's direct `$8C7C` entry).
