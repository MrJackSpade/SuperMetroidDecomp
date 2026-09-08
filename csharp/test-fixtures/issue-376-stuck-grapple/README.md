# Issue #376: actual stuck grapple state

Private slot 0 captured while Samus was stuck dangling in `$04/$31` (Halfie Climb),
preserved separately from the earlier standing fixture. Contains private ROM/SRAM
data; never distribute publicly. Live slot/SRAM are not modified by the test.

SHA256: `77BC9FD66CAFD0D834E4B8FAB5AF20FB2E528FEE8D7B02EF1262C11D7BE6E0A7`.
Samus `(179,540)`, pose B2; anchor `(199,488)`, rope length 12, angle `$9180`.

## Root cause and cartridge evidence

Retraction/body collision holds the rope at length 12 in this tight angle. A
collision-assisted swing kick lets it move away far enough to retract to length
8 and enter the wall-grab pose. The translated kick checked normalized **B (Run)**
instead of **A (Jump)**. The player could not supply the needed kick using Jump.

Both pinned `upstream-sm/src/sm_9b.c`, `GrappleBeamFunc_BD44`, and independent
`upstream-disassembly/src/bank_9b.asm`, `HandleGrappleKick` at `$9B:BD44`, explicitly
read new input and test **JumpBinding** at `$9B:BD4B`. Correcting this one binding
restores the native kick; there is no special room exception, forced collision
bypass, forced wall grab, or substituted sprite.

The older successful approach did not need this kick. The older synthetic kick
test incorrectly supplied B too, so it validated arithmetic but missed the binding.

## Reproduction and verification

```powershell
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --stuck-grapple-audit
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --anchored-grapple-audit
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --grapple-movement
```

The stuck-state test changes no Samus/room/rope data; it supplies controller input:
hold Shoot+Up, swing away for 45 frames then toward the wall, and pulse Jump every
eight frames. This failed before the fix (still B2, length 12 after 180 frames).
Substituting Run for Jump before the fix succeeded, isolating the wrong binding.

After the fix:

- The same Run-input control never produces a kick and remains at length 12.
- Jump-input replay produces a kick, retracts to length 8, and reaches B8 at
  `(175,504)`. Top/bottom spritemaps and actual VRAM upload bytes are checked
  against the cartridge wall-grab graphics.
- Releasing Shoot and pressing Jump+Left enters the release window, queues the
  wall jump, detaches into pose 84, and rises/moves away to `(157,441)`.
- Screenshots of the actual stuck, ready, and departed states were visually
  inspected in `csharp/test-temp/issue-376-stuck-grapple`.
- The original successful approach and the corrected synthetic movement, kick,
  collision, wall-grab, wall-jump, drawing and release tests still pass.

Player-facing sequence: while attached, hold Up and tap Jump to kick out of the
tight hanging angle (swing away as needed). Once the braced wall-grab pose appears,
release Shoot, then press Jump to perform the wall jump.
