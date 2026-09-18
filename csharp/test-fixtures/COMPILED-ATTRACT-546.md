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

The standard verifier now derives the exact source-byte set for every scene
table, equipment/setup record, input-object header and reachable input command.
It runs all 23 production scenes through their complete displayed duration with
those bytes blocked. Any fallback to the cartridge definitions fails at the
first read. Together with the field-by-field and 552,000-step interpreter
comparisons above, this covers scene setup, selection, duration and every
controller record exercised by normal playback.

Attract runtimes receive the same current map, projectile, trail, flare, Grapple
and beam presentation catalogs as ordinary gameplay runtimes; the bindings are
retained across each scene replacement. Their independently extracted resources
are verified by their owning asset suites. Room loading, animation and other
presentation still depend on the wider #530/#549 migrations, but those are not
attract-definition reads and do not keep #546 open. User recordings remain
separate tool data, not editable engine programs.
