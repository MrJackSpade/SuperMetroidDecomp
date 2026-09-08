# Issue #371: Wrecked Ship Entrance upward shinespark

Private player fixture: `slot-0-named.smstate`, copied byte-for-byte from desktop
slot 0 before diagnostics on 2026-09-08. It contains private ROM/SRAM data and must
not be distributed publicly. The live slot and normal save were not modified.

SHA256: `3DB40A0C1B4E6E87E5E64BB7845177C95761F78A7DC04B93259B77680F55DA99`.

Room `$03/$00`, header `$8F:C98E`, Samus `(1513,635)`, health **1/699**.

## Reproduction

From the repository root:

```powershell
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --shinespark-shaft-audit
```

The production state reader loads a temporary copy. The test walks left along the
runway, boosts right, taps Down for one frame, waits for crouch, then holds Jump.
It stays within this room. Console failures do not open error dialogs.

- Original one-energy state: launch stops at Y=619, `EndedByLowEnergy=true`,
  `EndedByCollision=false`.
- Identical inputs with health replenished in memory: launch traverses the shaft
  to its actual ceiling at Y=67, `EndedByLowEnergy=false`. Final health is 659.
- Four separate storage replays accept a one-frame Down press with neither,
  either, or both Right and Run held. Each immediately stores charge, with 179
  ticks remaining after that frame's palette tick.

## Cartridge cross-check

Pinned `upstream-sm/src/sm_90.c`, `Samus_EndSuperJump` (`$90:D2BA`), and
`upstream-disassembly/src/bank_90.asm`,
`EndShinesparkIfCollisionDetectedOrLowEnergy`, both stop a spark below 30 energy,
independently of terrain collision. The current invincibility setting only
prevents health reaching zero; it does not override this cutoff.

Pinned `upstream-sm/src/sm_91.c`, `Samus_CrouchTrans` (`$91:F7B0`), and the matching
`InitializeSamusPose_CrouchingTransition` in `bank_91.asm` test boost stage four
and store 180 ticks for this ROM revision. They do not require a prolonged Down
hold. No storage timing discrepancy was reproduced by these four input tests.

No production change is justified by the collision diagnosis alone. Extending
invincibility to bypass the shinespark energy cutoff is a separate cheat-policy
choice, pending the player's answer. Issue remains open.
