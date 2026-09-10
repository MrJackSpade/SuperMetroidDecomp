# #523 Magdollite Tunnel attack investigation

Affected player version: 0.1.1. Room $02/$23 is $8F:AEB4, state AEC1,
population $A1:AC53. Its nine Magdollite records represent three composites.
The head's parameter two is 3A60; each arm/overlay parameter is zero.

## Reproduced root cause

`--magdollite-runtime-audit ROM` loads the unmodified retail room through the
full runtime, places Samus at X192 on the floor below row eight (Y139), gives
999 health and Varia, and runs 640 frames of neutral input. Cheats are off;
normal contact/knockback can move Samus. No enemy state is patched.

Before correction, the head entered its attack and waited for landing, while
the body remained BodyRising indefinitely. No lava was emitted. The translated
arm used its own zero speed parameter. Pinned bank A8, B21A/B225 and B29C/B2A7,
reads Magdollite velocity at X-$40: the preceding head owns movement speed.
The original initialization still fills each slot's own speed fields; they
must not all be overwritten with head values.

Rising/falling now read the head's speed fields at the point of use. The same
runtime sequence produces 126 frames with lava and projectile OBJ submissions;
123 consecutive flight frames assert X changes by -3/+3 according to facing
and Y remains fixed, matching bank 86 E049-E09B. Initial head up speed is
FFFC:6000 while body/overlay remain zero, preserving the reproduced setup.
The existing untouched-room enemy-system audit also passes.

This is a verified mechanical checkpoint, not final closure. Exact cadence,
proximity controls, rendered tile/palette inspection and player validation
remain before marking the whole report ready. No native CPU capture of this
encounter is claimed; the fix is cross-checked against pinned disassembly.
