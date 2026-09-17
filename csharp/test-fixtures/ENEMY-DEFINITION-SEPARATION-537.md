# Enemy definition separation (#537)

This file records incremental evidence for compiling application-owned enemy headers while
leaving editable presentation payloads independent. It is not a completion certificate for
all enemy definitions, room populations, vulnerability tables, drops, or presentation binding.

## Mama Turtle family

The contiguous 64-byte headers for Mama Turtle (`$A0:CF3F`) and Baby Turtle (`$A0:CF7F`)
are compiled in `MamaTurtleEnemyDefinitionCatalog`. The catalog owns their native gameplay
statistics, radii, AI/collision callback identities, and pointers that bind the family to its
current presentation data. Runtime dispatch uses the same named definition identities instead
of private literals in the functional enemy system.

Verification independently parses every field from the pinned cartridge and compares both
complete records. It then loads the real `$8F:D055` room and initializes its one parent plus
four children through an address-space guard that rejects all 128 original header bytes.
Population records, graphics, palette data, instruction programs, vulnerabilities, drops, and
name data remain cartridge-backed and are still outstanding under #537/#538/#549.
