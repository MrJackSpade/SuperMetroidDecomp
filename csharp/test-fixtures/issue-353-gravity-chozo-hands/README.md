# Issue #353: Gravity Suit route blocked by Chozo hand collision

Player debugger state 9 preserved September 7, 2026. The live slot is untouched.

Report: the Chozo statue hands have no collision, preventing the statue walking
animation from being triggered and blocking the Gravity Suit route.

- Fixture: `slot-9.smstate`, 2,287,474 bytes.
- SHA-256: `360CE656DE2CB141FC00BD79A801C6A1F5B3AD379400E9E45FEB244CDD3A15B5`.
- Source: `debug-states/SuperMetroid-debug-slot-9.smstate`.
- Repository HEAD at preservation: `2e7b255ea9f57e910a7c25794ca01afa4cd22600`.
  This does not establish which build the player's running process used.

Private cartridge-derived debugger fixture; do not distribute publicly.
## Reproduction

`slot-9-named.smstate` is a schema-3 copy converted by the known-compatible
`61edf37` core using the production load/save path, without advancing frame 13184.
SHA-256: `80DF0F0D6FBA1E088952988F3AC8D61918128D4A1C0BA9D7D21C3D25231B10D0`.
The original schema-2 fixture is retained unchanged. The named copy avoids legacy
compiler-token drift when adding production methods during this fix.

From the repository root, run the console-only diagnostic:

```powershell
dotnet run --no-restore --no-launch-profile --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --chozo-state-audit
```

The diagnostic loads this immutable fixture through the production state reader
in a temporary slot directory, advances one normal game frame, and asserts the
hand block's actual collision type and BTS. It never overwrites a live slot.

Before the fix, the command exits 1: room `$C98E`, hand block `(74,23)` is
`$00FF` / BTS `$00`, while the enemy's `$D6EE` and `$D6FC` requests remain pending.
This is not merely old saved terrain: the pending initialization requests still
have not been consumed after the normal frame.

Native evidence: `$AA:E725` publishes `$D6EE` at `(74,23)`. Its synchronous
`$84:D616` setup writes type/BTS `$B080`, preserving the visual tile. That hand
must be solid even before morph-pose admission through `$84:D620`. The runtime
currently transfers other enemy PLM requests but never the Chozo requests.

The completed diagnostic also constructs the approach immediately above the hand
without changing room terrain, enemy state, or boss flags. It invokes the real
vertical collision dispatcher and verifies that standing contact and a living
Phantoon cannot activate the hand. Admitted morph contact must both collide and
disable controls. It then runs the full room sequence through ordinary game frames:

- 1,938 frames to release in this fixture; Samus ends at `$0134,$027D`.
- Samus matches the native hand-offset tables on every carried frame.
- The two authored slopes open during the sequence and return to spikes afterward.
- 33 enemy-sound publication frames, instead of replaying the last sound every frame.

This is a real-room integration regression with a constructed approach, not a
controller-only climb from the original standing position. Player confirmation
of the Gravity Suit route remains required.
