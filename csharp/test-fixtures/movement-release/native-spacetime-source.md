# SpaceTime bounded source ownership (#1262)

The existing `native-spacetime-beam-probe.h` and trace captured original cartridge
execution. Its second alpha pass reaches callback `$90:AD16` with X=0, Y=$912F,
and direct-page source pointer $000000. The initial shot instead inherits Y=$0038
and does not enter the copy loop. These inputs and both progression hashes remain
asserted by `--spacetime-beam`.

## Static dependency bound

The malformed entry increments X and Y twice before the signed comparison with
$0020. The second-pass copy therefore starts at source `$00:9131` and destination
`$7E:C1C2`. It reads a word, writes a word, and increments both 16-bit indices by
two. With the recorded zero pointer, the immutable byte range is exactly
`$00:9131..FFFF` (28,367 bytes, the low-bank mirror of `$80:9131..FFFF`). The high
byte of the word starting at $FFFF carries into `$01:0000`, a live WRAM mirror.
Y then wraps to $0001 and continues through $001F before exiting at $0021. These
last 33 bytes remain live WRAM reads. The 28,400-byte destination span ends at
`$7F:30B1`; it cannot overwrite the direct-page source pointer.

The projectile's four animation records at `$93:912F/9137/913F/9147` provide the
other inherited list values in this same source window; the adjacent goto returns
to $912F. The other callback's small inherited Y is confined to low WRAM.
This is a bound on the already-supported technique, not arbitrary corrupt CPU
execution or support for arbitrary edited source pointers.

`SpacetimeBeamCopyDefinitions` owns only those immutable instruction bytes as data.
It rejects addresses outside that range. Production keeps the existing copy order,
WRAM/SRAM reads, progression-mirror synchronization and save behavior. It does not
substitute captured progression bytes or a hardcoded reset. The verification
compares every definition byte with the supported ROM and runs the projectile
path through a bus exposing mutable/peripheral capabilities only.

## Equipment transition

Source inspection also identified the required selection-$0E graphics load on
pause teardown. Its native tile pointer is `$90:C3CD = C401`, giving a 256-byte
upload from `$9A:C401`. Its palette pointer is `$90:C3E5 = 19FF`: all 32 palette
bytes come from the live low-WRAM mirror, not immutable colors. The dedicated
graphics definition preserves this, and installation format 85/projectile format
15 installs the additional editable sheet. Existing asset enum ordinals are
unchanged. Both loader paths are checked byte-for-byte and with distinct mutable
palette contents; the twelve ordinary combinations and Chainsaw remain covered.

## Confirmation

Before: `--spacetime-beam` threw at source `$009131`.
After: the original hashes `$8E3EEC21` and `$2C169A4A`, saved dispatcher `$0880`,
live equipment `$100E`, saved progression, inventory retention and intro/Ceres
restart assertions pass. The frontend fixture now uses the existing installed
asset bindings needed by its cinematic path; assertions are unchanged.

The copy regression was introduced by `142ee315b`; the invalid-selection graphics
path was removed by `c26f1a567`. Supported ROM SHA256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
