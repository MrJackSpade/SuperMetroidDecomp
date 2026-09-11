# Map-station input ownership through pause (#570)

Affected player version: 0.2.0.

## Exact failure and why the first diagnostic missed it

The first station-controls audit held the opposite direction while attached and
through acknowledgement, then supplied **neutral input during the map fade**.
That did not exercise the player's reported download-to-display interval.

Extending the real Crateria station sequence to hold Right during that fade
reproduced the report: on fade frame 18, Samus changed from pose `$8A` (left-facing
wall contact) to `$26` (ground turn), with `InputLocked=False` while the frontend
was still `PausingDarkening`. This assertion failed before production changes.

## Cartridge ownership

Cross-checked pinned `upstream-sm` (`578f90b3cc49557bb70060ad033bb90b8cf8ac50`)
and `upstream-disassembly` (`362be646929cf8e483f692b73a6561cfc2dc1d0d`):

- `$84:B146` invokes Samus command six when station collision is admitted.
- `$90:F1AA` installs locked alpha and empty beta `$90:E8D6`.
- Map access lists `$84:AD86` / `$84:ADA4` finish their art holds and delete
  the access PLM. They do **not** invoke an unlock command.
- `$82:A2E3` calls command `$0C` during gameplay-resume setup.
- `$90:F29E` restores normal Samus handlers when the empty station beta is active.

The port's shared map/resource-station cleanup incorrectly unlocked Samus as
soon as the access animation ended. The correction separates completed access
art from the still-owned input interval and releases map-station input during
the frontend's `UnpausingB` setup, alongside the existing equipment reconciliation.
It does not add a bespoke movement clamp or change general pause fade timing.

## Regression

`SuperMetroid.DebugRunner --map-station-controls-audit <retail-ROM>` covers:

- Both attachment sides, through ordinary collision and controller input.
- Beam, missile, and super missile; held and alternating fresh input.
- Pose preservation through download, acknowledgement, **every fade frame until
  the map is displayed**, and the map-dismissal fade before unpause setup.
- Input release at unpause setup, then ordinary shooting and unchanged ammo.
- An in-memory debugger graph round trip while the access art has completed but
  Samus remains locked. The new semantic phase uses the existing serialized enum
  field; no serialized fields or existing enum values were changed.

This is a production-runtime repro with pinned source confirmation, not a native
emulator recording comparison. The unrelated station charge-cancellation and
automatic-versus-manual pause fade-rate questions are not claimed resolved here.
