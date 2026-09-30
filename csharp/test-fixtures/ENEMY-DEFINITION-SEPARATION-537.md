# Enemy definition separation (#537)

This file records the original Mama Turtle conversion. The complete current enemy
definition/population acceptance is recorded in `ENEMY-DEFINITIONS-537.md`; the
incremental evidence below does not by itself establish that broader result.

## Mama Turtle family

The contiguous 64-byte headers for Mama Turtle (`$A0:CF3F`) and Baby Turtle (`$A0:CF7F`)
are compiled in `MamaTurtleEnemyDefinitionCatalog`. The catalog owns their native gameplay
statistics, radii, AI/collision callback identities, and pointers that bind the family to its
current presentation data. Runtime dispatch uses the same named definition identities instead
of private literals in the functional enemy system.

Verification independently parses every field from the pinned cartridge and compares both
complete records. It then loads the real `$8F:D055` room and initializes its one parent plus
four children through an address-space guard that rejects all 128 original header bytes.
That was the original focused fixture. Population, instruction, vulnerability, drop and
name definitions have since been compiled, with graphics and colors supplied by installed
catalogs. Core no longer has a cartridge reader; the current source/type boundary and
remaining shared acceptance are documented in `ROM-FREE-SOURCE-ACCESS-549.md`.
