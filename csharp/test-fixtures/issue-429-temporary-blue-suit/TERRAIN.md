# Temporary boost: actual terrain breaking

Partial #429.32 cases combine both facings,60/140-frame run-ups (partial/full
retained boost), upward/downward contact, and two BTS variants each for Speed
Booster and bomb blocks. All cases retain R after crouching until399. No boost,
contact-damage index, pose, or position is injected after initialization.

Sample400 replaces only the contacted tile span and invokes the real vertical
collision/movement dispatcher for four pixels in the selected direction. Native
executes `$94:9763`; C# executes `SamusBlockCollision.MoveVertical` with an empty
PLM pool. This is a collision-phase sample, not a complete gameplay frame. It
does not call block mutation directly or tick the PLM animation handler.

Modes0/1 use special-solid BTS0E/0F downwards;2/3 repeat upwards. Modes4/5 use
bombable-solid BTS00/01 downwards;6/7 repeat upwards. Original tile words are
B123/F123. The tile span covers the actual earned position's left/right bounds.

## Native contract and result

- Partial retained0201 leaves tiles unchanged, collides, and allocates no actor.
- Full retained0401 changes speed-block words to00B6 and bomb-block words to0058,
  admits all four pixels, and allocates one PLM for each contacted tile.
- Contact-damage index remains zero in both cases. Native setup `$84:CDEA` and
  `$84:CE83` inspect the boost counter directly, independently of the contact
  damage index. This corrects no production code: the port already does the same.
- Upward rejected contact snaps the fraction to0000; downward rejected contact
  retainsFFFF. Exact position/subpixel comparison covers this, not only carry.

The fixture compares all prior movement/charge fields plus collision, both edge
tile words and active PLM count. Semantic assertions require exact mutation,
allocation count and admitted full-boost displacement.32 cases /12,832 observations
(12,800 full prefix frames plus32 collision calls) match without production changes.
Independent recapture normalized SHA-256:
`5E2E1203C611077418D168737BE24BE91CA3EF1A9336ACF6070BD159DB897D08`.

```powershell
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" terrain > csharp\test-temp\temporary-blue-terrain.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --temporary-blue-terrain-audit 'Super Metroid.smc' csharp/test-fixtures/issue-429-temporary-blue-suit/terrain.csv
```

Build per README.md. Only numeric observations are published. This establishes
immediate block-breaking admission/mutation, not subsequent crumble artwork or
respawn timing. Existing dedicated block lifecycle tests cover those separately.
Longer complete soft-unmorph chains, equipment-menu transitions and persistent
Blue Suit contrasts remain outstanding for429; it is not ready for closure.
