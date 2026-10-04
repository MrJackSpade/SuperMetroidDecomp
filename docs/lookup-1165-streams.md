# Issue 1165: five agent streams

Exclusive partition of **523 files / 1128 named table definitions** from [the inventory](lookup-remaining-1165.md), snapshot 2026-10-04T20:43:25.3086366Z. Each inventoried file belongs to exactly one stream. This partitions the existing inventory; it does not recertify its semantic completeness or change its counting unit.

| Agent | Checklist | Files | Definitions | Largest assigned families |
| --- | --- | ---: | ---: | --- |
| 1 | [Stream 1](lookup-1165-stream-1.md) / [#1238](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1238) | 82 | 226 | Samus, Escape, Powamp, Cacatac |
| 2 | [Stream 2](lookup-1165-stream-2.md) / [#1239](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1239) | 111 | 226 | Torizo, Kraid, Projectile, Pause |
| 3 | [Stream 3](lookup-1165-stream-3.md) / [#1240](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1240) | 105 | 226 | Mother Brain, Enemy, Intro, Grapple |
| 4 | [Stream 4](lookup-1165-stream-4.md) / [#1241](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1241) | 112 | 225 | Ridley, Gameplay, Room, Draygon |
| 5 | [Stream 5](lookup-1165-stream-5.md) / [#1242](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1242) | 113 | 225 | Ceres, Map, Phantoon, Dead |
| **Total** | | **523** | **1128** | |

Families stay together, including Torizo/Bomb Torizo/Golden Torizo, Ridley/Ceres Ridley/Norfair Ridley, and Mother Brain/Shitroid/Baby Metroid. Largest families are assigned first to the least-loaded stream. Definition counts balance the lists, not expected effort: a large installed resource family may require more work than several small literal tables.

## Assignment contract

- Give each agent its stream checklist. All five streams implement parent issue #1165 through child tickets #1238–#1242, explicitly authorized by the user. Keep in-progress on the parent; workers do not change labels.
- The file lists are exclusive write ownership. Agents may read any source, but may edit only their assigned files and their own stream checklist/report. Do not edit another stream to complete a dependency.
- The coordinator owns the master inventories, stream manifest/index, shared review JSON, existing shared verification entry points and project files. Agents propose patches to these in their reports; the coordinator applies them serially. Git staging, commits, pushes and publication are also serialized by the coordinator.
- Before editing a dependency or creating a new source/test file absent from the manifest, have the coordinator assign it to one stream and update the manifest. Never have two agents claim the same path. Prefer separate verification partial files for independent stream changes; shared wiring stays with the coordinator.
- Agents record completed conversions, concrete retention justifications, focused confirmation and dependencies in their own checklist/report. The coordinator reconciles those records into the master inventory and review ledger. A dependency handoff must name the required contract and owning stream.
- Recheck current source and review records before work: the implementation thread may have completed entries after the inventory snapshot. Remove completed work from the stream report with its evidence; do not redo it.
- Retention is permitted only for impossibility or semantic nonsense, supported by the actual data and consumer. Performance, size, complexity and missing provenance do not qualify. Meaningful case dispatch is permitted. Tools/tests remain evidence and focused confirmation, not conversion targets or discovery sweeps.

## Validation

The machine-readable [ownership manifest](lookup-1165-streams.json) covers every inventoried file exactly once. All entry names are copied into their owning stream with source locations and notes. File and definition totals reconcile with the input inventory; no source files or ticket state were changed to create this split.


## Execution coordination

The current session supports the coordinator plus three concurrent workers. Streams 1–3 start first; streams 4–5 are queued and receive worker slots at completed batch boundaries. This is a capacity constraint, not a deferral of their scope. Workers stop mutations for a completed batch until the coordinator acknowledges integration.

Each stream exclusively owns `csharp/src/SuperMetroid.Verification/Program.LookupStreamN.cs` for its corresponding N=1..5, as recorded under additionalOwnership in the manifest. The coordinator wires methods into existing entry points and runs shared builds and focused checks serially. Additional paths require explicit ownership assignment before editing.
