# Crocomire projectile drop callback (#572)

Affected reported build: 0.2.1.0, room $A98D / state $A99F.

`DebugRunner --crocomire-projectile-drop "Super Metroid.smc"` loads the retail
room and seeds Crocomire's volley entry. The real spawned projectile is hit by
a constructed Power Beam through production projectile collision. Its five
four-frame impact records advance to $86:901B. Before the fix, frame 20 throws
the exact reported unhandled $9270 instruction exception.

Pinned `upstream-sm/src/sm_86.c` (`EprojInstr_9270`) and disassembly bank $86
agree: spawn drops at the projectile origin using Crocomire header $DDBF, with
no operands. The following goto/delete sequence must still execute. The missing
dispatcher case now calls the existing shared header-based pickup allocator
before advancing two bytes. It does not suppress an exception or skip a drop.

Regression assertions cover each impact hold, deletion on frame 20, and the
allocated pickup's header and origin (Samus starts at one energy to make this
deterministic). The focused test and complete Crocomire audit pass. This is a
cartridge-source-backed reproduction, not an original-CPU trace comparison.

The stalled impact actor is a lead for #573's lingering rings, but that issue's
rendered cleanup and lingering damage assertions remain outstanding. Do not
close #573 based solely on this crash regression.

## Rendered lifetime and contact follow-up (#573)

`--crocomire-impact-lifetime` adds the missing rendered/contact coverage using
actual room VRAM/CGRAM and the software OBJ renderer. It covers Power Beam,
missile, and Super Missile, each hitting after 0/1/4/7 flight frames. Every
case renders the impact actor for all twenty authored frames; frame 20 deletes
it. Ten subsequent frames contribute no ring pixels, and moving Samus to the
former impact origin with invincibility cleared causes no health loss. Pixel
contribution is measured against a same-frame render excluding only the target
actor, so the spawned pickup is not mistaken for a lingering ring. Captures
stay in memory; no copyrighted artwork is published.

The complete Crocomire audit also passes its independent live-projectile
contact/damage control. These twelve lifetime cases pass with the #572 callback
fix; no additional production change is needed. The short-lived ring is not
made harmless: pinned `HandleEprojCollWithProj` masks properties with $0FFF,
retaining damage and enabling Samus contact until cleanup. The reported
indefinitely lingering ring is consistent with the missing callback preventing
that cleanup; the player's exact recording has not been replayed here.

#573 is ready for player confirmation on the shared fix, not closed as a
separate speculative collision change.
