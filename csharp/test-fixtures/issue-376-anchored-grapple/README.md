# Issue #376: grapple wall jump, Halfie Climb

Private player slot 0 preserved on 2026-09-08 as `slot-0-named.smstate`.
Contains private ROM/SRAM data; do not distribute publicly.

SHA256: `AAFF652718BA7101FA6C45A0B96D3501E6D05F1E81DB24B3DABF1D13DF83AA8A`.
Room `$04/$31`, header `$8F:D913` (Halfie Climb / Maridia grapple wall shaft).
Starting position `(142,651)`, grapple inactive, Power Bomb selected.
The live slot and normal save are not modified by this test.

## Run

```powershell
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --anchored-grapple-audit
```

The test uses the real state loader and frontend with only controller inputs:
select Grapple, approach the right wall, jump, fire horizontally after gaining
height, and hold Up + toward-wall while firing to retract into the wall grab.
Holding Shoot+Jump retains the grab at `(175,552)`. Releasing Shoot opens the
30-frame window; a new Jump press the following frame queues the wall jump.
The next frame detaches and selects pose `$84`; subsequent frames move Samus
upward/left to `(158,489)`. Assertions cover each of these stages. Screenshots
are generated under `csharp/test-temp/issue-376-anchored-grapple` and were visually
inspected for the wall grab and upward/away movement.

This establishes the mechanic in the reported room, not an entire-room traversal.
No production change was needed for this sequence.

## Cartridge evidence

Pinned `upstream-sm/src/sm_9b.c` and the independent local disassembly
`upstream-disassembly/src/bank_9b.asm` agree:

- `$9B:C814` wallgrab: holding Shoot retains the anchor. Releasing Shoot (or losing
  the anchor) installs the wallgrab-release handler and writes timer 30.
- `$9B:C832` wallgrab release: decrements the timer, probes toward the wall, and
  accepts a fresh Jump edge through `$90:9CAC`.
- `$9B:C9CE` wall jumping: tears down grapple and selects the opposite-facing
  wall-jump pose. The beam does not remain anchored during the resulting jump.

Player instructions: hold Shoot to anchor; hold Up to pull into the wall-grab
pose; release Shoot; then promptly press Jump (within roughly half a second).
Holding away from the wall with Jump reproduces the tested departure.

Issue remains open for player confirmation of this input sequence. This is an
investigation result and regression test, not a claimed gameplay fix.

## Follow-up: missing visual wall-grab cue

Player reports continued dangling instead of a recognizable ready sprite. The
existing fixture starts standing, not in the reported dangling condition. This
controlled sequence does visibly change from B2 frame 20 (hanging) to B8 frame 0
(bent-knee wall grab). `graphics-50.png` and `graphics-57.png` capture both states.

Expanded assertions independently pin the disassembly's B8 top/bottom spritemap
indices `$032F/$05B5` and tile definitions `$92:CD22/$92:D254`. They compare every
byte of both split DMA uploads in live VRAM against the ROM, throughout the held
wall grab and release window. All checks pass; no production change was made.

This establishes correct wall-grab art in the tested route, not that the player's
different dangling attempt enters the same state. The issue is active (validation
label removed); a capture while stuck dangling would distinguish failure to enter
wall grab from incorrect rendering after entry.

Follow-up: the player supplied that capture. See `../issue-376-stuck-grapple/README.md`.
It reproduces a wrong Jump/Run binding in the swing kick, which this earlier route
never needed. The binding is now corrected and both fixtures remain regression tests.
