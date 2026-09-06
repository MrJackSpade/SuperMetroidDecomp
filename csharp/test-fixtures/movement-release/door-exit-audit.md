# Isolated door-exit momentum diagnostic (#312)

Run from the repository root:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --door-exit-momentum-audit "Super Metroid.smc" CF80 CEFB 0008 018B
```

Arguments after the ROM are hexadecimal source room, destination room, source
Samus X and source Samus Y. The diagnostic selects the destination through the
source room's real door blocks and runs the production door coroutine, followed
by twenty neutral-input gameplay frames. It does not replay a player recording.

Four constructed cases combine an intact/broken Maridia tube with zero/2.75
pixels-per-frame initial base speed. No extra run speed is seeded. The broken
tube event matters: without it CEFB selects dry physics, which cannot reproduce
the reported underwater coast. Each line includes position, displacement,
base/extra speed, movement medium, FX surface, pose and input lock.

Observed with the current implementation: carried base speed survives the door
coroutine. After unlock it produces a 3.75-pixel first neutral-input displacement
(base speed plus the running fallback probe). Subsequent water deceleration is
slower than the dry case. After leaving the ledge, the water case alternates
1 and 1.75 pixels of horizontal travel. These are diagnostic observations, not
accepted cartridge expectations or proof of a fix. Compare the corresponding
native speed/pose sequence before changing production movement code.

This fixture contains no player SRAM, controller history or debugger snapshot.
