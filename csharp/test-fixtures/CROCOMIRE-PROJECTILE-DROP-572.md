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
