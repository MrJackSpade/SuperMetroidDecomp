# Issue #376: repeated wall-grab failure after missed jump

New player slot 0 preserved separately on 2026-09-08. Private ROM/SRAM graph;
do not distribute publicly. Live slot and normal save are untouched.

SHA256: `968832EDC5E3E181D704154559779273F67A952BD277A9AF1428AD022978D3A0`.
Room `$04/$31`, Samus `(177,535)`, B2, anchor `(199,488)`, rope length 9,
length delta 0, angle `$9524`, angular velocity -59. This is not the prior
length-12 fixture: retraction is already stopped, with Up held in the capture.

## Reproduction and findings

```powershell
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --grapple-retry-audit
```

Holding Up, even with Jump kicks and left/right pumping, leaves length 9/delta 0
for 180 frames. Releasing/re-pressing Up during those otherwise identical inputs
reaches the wall-grab pose. Pinned bank-$9B code at `$BB64` reads **new** Up input.
Pinned bank-$94 `$AC41-$AC50` clears the length delta before checking collision
when a shortening step would cross below 8; a blocked last pixel can therefore
leave length 9 with no active retraction. The translated ordering matches it.

At wall grab, pressing Jump on the same frame Shoot is released opens the grace
window but does not jump. Holding Jump through the window then drops Samus.
Pinned `$9B:C814` dispatches only the release setup; `$C832` checks a **new** Jump
on subsequent frames. The tests reproduce this missed-edge sequence as well.

Without resetting the loaded game, the test then reattaches, retracts into the
ready pose again, releases Shoot with Jump also released, and presses Jump on the
following frame. It verifies the actual wall jump and upward/away displacement.

Coverage now includes: captured retry stall, re-pressed Up recovery, visible
ready-pose spritemaps, simultaneous-release missed Jump, full grace expiry,
reattachment, second ready pose, and successful later Jump. These pass on current
production code. No additional production fix was made: no divergence from the
pinned cartridge routines was established. This is a source comparison, not a
claim of an independent emulator playthrough of the new state. Issue remains open.
