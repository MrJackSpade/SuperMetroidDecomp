# Map-station weapon admission (#569)

Affected player version: 0.2.0. Related facing report: #570 (not reproduced by this fixture).

## Reproduction

Run `SuperMetroid.DebugRunner --map-station-controls-audit <retail-ROM>`.
The fixture loads the Crateria map room through its real door, positions Samus
beside the access block, and attaches through normal Left input and collision.
It does not set the station's triggered/locked fields directly.

Before the production change, a fresh Shoot press allocated a new projectile on
fixture frame 23 while the station owned Samus. The failure assertion reads
`LastFiredProjectileSnapshot`, not the presence of previously fired shots.

## Cartridge evidence and correction

Pinned `upstream-sm` source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
`ActivateStationIfSamusCannonLinedUp` ($84:B146) calls command six.
`SamusCode_06_LockToStation` ($90:F1AA) installs alpha $90:E713 and an
empty beta handler. That alpha calls `HandleProjectile` and `Samus_Func10`,
not the normal HUD weapon producer.

The port already suppressed movement and HUD selection while input was locked,
but still enabled the humanoid weapon producer. Its existing producer-enable
argument now also respects that lock. The projectile update loop still runs;
this does not delete or freeze existing shots.

## Verification and limits

- Six retail-station runs: beam/missile/super missile, each with held Shoot and
  alternating fresh presses. No new shots during attachment or retraction.
- Opposite direction is held during the tested locked intervals; pose must stay
  unchanged. This did not reproduce #570 and is not evidence to close it.
- The actual download message is acknowledged, the automatic map opens, and
  Start dismisses it. Ammunition stays unchanged while locked and shooting works
  again after release.
- The test retains the original automatic-map/pause-gate assertions.

This is a source-confirmed, runtime-reproduced weapon-admission fix, not a native
emulator frame comparison. The fixture currently enters from the right side;
left-side facing, charge cancellation at attachment, and exact first-unlocked-NMI
timing remain separate coverage gaps. No player state, ROM, or images are tracked.

## Follow-up: #570

The subsequent [facing audit](MAP-STATION-FACING-570.md) extends this command to
twelve cases across both sides, supplies opposite input during the previously
neutral map fade, and checks the exact unpause-setup input-release boundary.
That extension reproduced a separate early-unlock defect; the initial six-case
result above must not be read as verification of the entire fade interval.
