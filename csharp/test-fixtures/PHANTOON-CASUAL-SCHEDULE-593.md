# Phantoon casual-flame boundary parity (#593 / #547)

## Reproduction and native evidence

Pinned bank $A7:CFCA decrements the timer, $CFCD BEQ expires at zero and $CFCF
BPL returns only for positive values. The port returned for zero as well, delaying
every expiry one frame.

At $CFFC the pattern counter decrements; $CFFF BEQ and $D001 BPL select the
terminal branch for zero/negative. $D003 stores FFFF and $D012 loads pointer+2,
the 180-frame inter-pattern delay. The port instead retained zero, later selected
the count/header word as a timer, and produced an extra mouth animation. The
pinned C equivalent `Phantoon_Func_7` independently shows both zero branches.

Initial migration-only checks preserved this port behavior. Assembly cross-check
disproved it; the corrected native-branch regression failed before production
changes: remaining count expected 65535, actual 0. The misleading header-as-timer
interpretation was discarded, not frozen into the compiled data contract.

## Fix and verification

- Both timer/count zero branches now match the cartridge.
- Exhaustion sets FFFF and selects word one, never the header.
- Four indirect schedules (30 words behind four native pointers) now use named
  compiled patterns. The native reverse indexing is preserved.
- All 65,536 RNG words exercise real selection with exactly one RNG call.
- Four 2,048-frame traces check timer, count and exact mouth instruction trigger
  on every frame using ROM-backed expected data and native branch semantics.
- These production fixtures have no bus; instruction-list/art loading remains
  separate and is not claimed ROM-free.
- Full Release Verification, complete 5,906-frame Phantoon audit and Windows
  Release build pass. Natural casual-flame contact now occurs at frame 865 rather
  than 866, consistent with correcting expiry at zero.

The schedule migration and boundary correction share the same production helper
and regression, so they are one scoped commit covering #593 and this #547 slice.
#593 awaits player confirmation; #547's wider work remains open.
