# Recharge station input softlock

Affected version: v0.4.5, carried forward from the latest build handed to the
player. The player confirmed the report concerns energy recharge stations.

## Reproduction

The focused `--recharge-station-admission` fixture loads a station through the
production population loader and touches its access block through the real
collision entry point. At full health, the original code fails the assertion
`full resource must be rejected before station input lock`. Setup sets
InputLocked, then the station handler declines to start an operation, leaving
no active operation to release control. This also affects touching a station
again after a completed refill. The fixture is synthetic; no player save or
recording was supplied, so this confirms the identified trigger, not their
exact play sequence.

## Cartridge behavior and fix

The pinned InsaneFirebat disassembly revision
362be646929cf8e483f692b73a6561cfc2dc1d0d identifies these checks:

- Energy access setup $84:B285/B2B8 rejects equal current/maximum health before
  ActivateStationIfSamusArmCannonLinedUp can lock input.
- Missile access setup $84:B2E8/B31B has the same check for missiles. Its port
  shared the same omission; both recharge types are corrected together.
- The first access-list instructions $84:AE35/AEBF recheck fullness. If already
  full, they run Samus command one to restore movement and delete the access.
  The port previously skipped activation without performing that unlock.

Restored the early setup check and the later explicit unlock/clear of access
state. The comparison is native equality rather than an invented less-than
condition. Ordinary refill timing and the existing map/save paths are unchanged.

## Confirmation and static review

The focused regression now confirms both access directions for the two shared
recharge types: full-resource collision stays solid without locking; becoming
full between setup and processing immediately releases input without a refill
message; depleted resources refill to maximum and release input after the
existing sequence; touching the completed full station does not relock control.

The PLM source gate correctly rejected the changed TryStepStation fingerprint.
Reviewed the change: it alters admission comparisons and restores the failed
admission unlock, without changing instruction widths, artwork reads, or typed
owner routes. Updated that fingerprint after focused confirmation. PLM,
queued-VRAM-DMA and enemy-visual gates pass (zero errors; 1,383 existing
argument-validation analyzer warnings). Player confirmation
remains pending; this does not claim whole-game validation.
