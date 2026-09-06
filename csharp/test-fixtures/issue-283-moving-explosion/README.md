# Travelling Super Missile explosion (#283)

Player clarification: the visible explosion continues travelling through the room.
This is not merely an explosion on the firing frame or a nearby muzzle collision.

Run from the repository root:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-restore -- --moving-missile-explosion-audit csharp/test-fixtures/issue-283-moving-explosion/player.smrec 'Super Metroid.smc'
```

Before a production fix, this fails at frame 31488 in room $8F:D461. Slot 1 remains
type $8800, spritemap $AA84, pre-instruction None, but moves in world space:

| Frame | X | Y |
| --- | --- | --- |
| 31487 | $0141 | $00D1 |
| 31488 | $014B | $00D1 |
| 31489 | $015E | $00D1 |
| 31490 | $0172 | $00D1 |

Before impact, slot 1 is slot 0's invisible SuperMissileLink. At frame 31487,
the main projectile crosses a thin solid block: row 13, column 19, word $8119,
BTS $00. Adjacent columns are special-air $3115/BTS $82. The main missile is
past the block while the helper samples it, becomes an explosion, and is moved
again by the owner's UpdateSuperMissileLinkAxis calls. The observer checks both
moving explosion-family slots and live missiles displaying ROM explosion art.
It excludes fresh firing allocations and room changes to avoid obvious false
positives. Slot reuse across other producers still warrants contextual review.

The fixture preserves the player's 20260906-105957-985 recording. It uses the
existing headless replay's synthetic audio acknowledgements, not Windows audio.
This is a reproduced failure, not a completed fix. Cartridge helper handling
must be checked before changing production code; merely freezing the sprite is
not sufficient evidence of correct missile collision behavior.
