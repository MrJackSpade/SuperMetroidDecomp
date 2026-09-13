# Room callback capture compatibility (#606)

Reported host: 0.3.1.0. Captured graph: 0.2.1.0.

The preserved production state at `shaktool-release-021/slot-0.smstate` reproduces
the unavailable `SuperMetroidRuntime+<>c__DisplayClass461_0` exception. Inspection
of its decompressed callback metadata identifies `LoadCartridgeRoom` methods and
the captured runtime/room fields. Comparing the 0.2.1 and current room-loader
source shows the callback expressions unchanged; unrelated runtime member changes
renumbered the compiler-generated containing class.

The loader already supported the older ordinal 443 from #391. It now recognizes
the verified ordinal 461 as well, while retaining unique capture-layout selection,
individual field validation, and exact resolved method signature matching.
Unknown legacy closure names are not guessed.

New graph writes use `SuperMetroid.DebugState.RoomLoadCallbacks.v1` in all three
places: object type, field declaring type, and delegate declaring type. This
removes the compiler ordinal from this callback's serialized identity. Changes to
the captures or callback semantics require a reviewed identity version change;
this is not blanket compatibility for arbitrary compiler-generated closures.

Verification:

- Exact preserved-state load failed before the fix; afterward the existing dig
  replay completes 6,000 neutral frames, clears all 216 sand blocks, and traverses
  the passage to X=849.
- Constructed graph tests round-trip an actual room-loader callback under the
  stable name and both known legacy names, retaining a nondefault room and Samus
  state, and invoke the restored delegate. The serialized graph must not contain
  the current compiler-generated type name.
- Unknown legacy closure identity remains unresolved. Existing delegate signature
  mismatch rejection and all other state-layout migrations remain enforced.
- Windows Release build and full core regression verification are required.

This does not address #605's separate room re-entry persistence report.
