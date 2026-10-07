# Issue #1229: Wave/Plasma passes immune enemies

Affected build carried forward: 0.4.14-smoke1228 (v0.4.15 tags the same gameplay commit). Evidence: player-supplied `beam issue.mp4`, 36.667 seconds, original on the right, port on the left. Frames were inspected across the recording and at 30 fps around the diagonal shots. The right side shows dud effects at the Ripper; the left side lacks those effects. The player additionally reports easier Mother Brain combat, without a phase or more specific sequence.

## Immune enemy hit

The common port handler calculated zero damage but omitted the native no-damage branch. The collision prelude correctly preserves ordinary Plasma penetration, so the missing subsequent rejection left the shot alive indefinitely through immune enemies.

Native $A0:A75B branches on zero calculated damage to $A0:A7A8. That branch sets projectile direction bit $10, creates sprite object 6 at the projectile coordinates, and queues library-1 sound $3D, regardless of Plasma penetration. The fix calls the existing equivalent dud helper on zero damage, retaining enemy-specific post-shot callbacks.

A focused fixture loads Red Tower $A253's actual Ripper, initializes a Wave/Plasma projectile through its production initializer, and runs the ordinary collision handler. Before: one hit, health 200 unchanged, direction 0002, no dud sound. After: one hit, health unchanged, direction 0012, dud sprite at the impact position, and sound 003D. The next real projectile update clears the collision payload. This confirms gameplay removal as well as impact presentation.

## Mother Brain follow-up

Source inspection of the reported encounter found a separate omission in the later-form head callback's common no-death damage tail: it never applied Plasma invincibility. The cartridge tail at $A0:A854-$A0:A85F tests bit 8 and writes 16 frames before subtracting health. The ordinary port handler already does this; Mother Brain's private copy did not.

Restored that timer using the existing EnemyShotTiming definition. A focused callback fixture uses the actual loaded Mother Brain head, form 2, and an initialized charged Wave/Plasma shot. Health changes 18000 -> 17250 (the initialized 750 damage), with timer 0 before and 16 after. This confirms the omitted cartridge side effect. It does not reproduce the player's unspecified full battle or establish that this is the only cause of the reported difficulty difference. Her body already creates duds, and her later-form zero-damage head branch already rejects shots; they did not share the Ripper omission. Phase-one beam rejection and normal damaging Plasma penetration were not rewritten.

Native references: pinned InsaneFirebat NTSC J/U 1.0 disassembly revision 362be646929cf8e483f692b73a6561cfc2dc1d0d, bank_A0.asm (normal shot damage, no-damage branch, CreateADudShot), bank_A9.asm ($B503 body and $B507 head callbacks), bank_B4.asm (Ripper and Mother Brain vulnerability definitions).

## Confirmation

Release DebugRunner build and both focused assertions pass:

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0-windows/SuperMetroid.DebugRunner.dll --immune-plasma-hit-audit 'C:/Users/Service Account/AppData/Local/SuperMetroid'
```

Player confirmation remains pending for the supplied clip and Mother Brain report. No broad playthrough or weapon sweep was used.
