# #485 native bomb animation observation

`native-bomb-radius-probe.h` uses the existing unpatched-ROM CPU loader. Include
it after `native-release-probe.h` in sm_rtl.c and call DiagnosticMetroidBomb with
the ROM path before SDL initialization. Suppress explicit SDL error dialogs
during the probe; restore hooks and rebuild afterwards. No SRAM is loaded or
written. The experiment places a normal bomb through $90:BF9D, then executes
$90:AECE repeatedly with Samus fixed. It does not simulate enemy or Samus AI.

Observed with the project's retail ROM:

| Update after placement | Fuse | X/Y radius | Next list | Sprite |
| --- | --- | --- | --- | --- |
| 51 | 8 | 4/4 | 9FFB | AD53 |
| 58 | 1 | 4/4 | 9FF3 | AD4C |
| 59–60 | 0 | 8/8 | A073 | A83E |
| 61–62 | 0 | 12/12 | A07B | A854 |
| 63–64 | 0 | 16/16 | A083 | A86A |
| 65–66 | 0 | 16/16 | A08B | A880 |
| 67–68 | 0 | 16/16 | A093 | A896 |
| 69 | 0 | 0/0 | 0000 | 0000 |

Coordinates remain 128/153 until deletion; type changes from 0500 to 0501 at
update 59, then zero on deletion. Explosion instruction timers alternate 2/1.
These radii agree with the failing port trajectory after accounting for its
initial attachment frame before bomb placement. Do not enlarge the explosion
radius as a workaround. Native enemy attachment and Samus movement still need
a joint comparison before declaring the missed detachment a diagnosed defect.
