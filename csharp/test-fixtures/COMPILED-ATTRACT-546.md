# #546 compiled attract-demo mechanics

## Scene records migrated

StockAttractDemoScenes owns all 23 application scene definitions across sets
6/6/6/5. Each immutable record contains room/door identity, placement, duration,
room/Samus setup identities, inventory and input-object identity. These are
compiled mechanics, not an editable JSON schema or a hidden ROM reconstruction.
Setup callbacks use the existing named domain catalog. Presentation remains
owned by the separate audiovisual migration work.

SuperMetroidGame's scene admission and next-scene check now use this catalog,
which accepts no address space. The original AttractDemoScene.Read remains an
independent development oracle for synthetic and retail verification; normal
frontend scene selection no longer calls it. Out-of-range sets/indices fail
explicitly; the exact one-past-last index is the end-of-set sentinel.

Verification `--stock-attract-scenes` compares every field of every record and
all four sentinels against the pinned NTSC ROM, including signed X offsets and
the completion-only fourth set. The check is also in the standard suite.
DebugRunner `--attract-demo-frontend-audit ROM` passes the actual title timeout,
18 ordinary scene durations, 90-frame frozen holds, cycling and cancellation
with unchanged SRAM using the compiled frontend selection path.

## Controller programs migrated

StockAttractInputPrograms contains 23 object headers and 823 reachable decoded
records, including the shared delete list and the pre-instruction continuation.
The shipped control flow uses only timed input, goto and delete. Held and edge
buttons are independent typed values; addresses identify native control-flow
labels, not an emulated memory blob. The catalog is split into bounded files.

Production runtime loading and stepping use the compiled path without an address
space. The original ROM reader/interpreter remains available for intro scenes and
as the development comparison path. No serialized instance fields were added.

The standard scene test now compares all twelve observable script-state values
on 552,000 handler calls: 23 scenes, four cancellation schedules, 6,000 calls each.
Both sides of the native movement-type-$1A pre-instruction are exercised. The
normal frontend audit also passes with compiled controller playback, not merely
compiled scene selection. These are ROM-backed interpreter comparisons, not a
new independent native CPU replay of all movement outcomes.

## Remaining scope

Room loading, animation and other
presentation still depend on the wider #530/#549 migrations; this is not yet a
ROM-free game or a completed #546. Preserve user recordings as separate tool
data, not editable engine programs. Replacement audiovisual asset tests and
desktop/Android ROM-absent execution remain required before completion.
