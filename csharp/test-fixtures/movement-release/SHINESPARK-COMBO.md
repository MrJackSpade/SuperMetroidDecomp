# #466 Shinespark/combo crash investigation

Status: reproduced, not fixed; no awaiting-player-validation label.

Pinned retail ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host pin `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
disassembly pin `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

The cartridge's crash entry `$90:D2BA` does not clear `$0ABC`
(`SpeedEcho_YPosition2`). Crash phase one `$90:D396` reuses that word as angular
travel and exits once its signed comparison reaches 128. Released crash-echo
creation `$90:D40D` also uses the word as a Y coordinate. Its clearing/ownership
must be traced through native echo/projectile processing; combo presence alone
is not an appropriate replacement condition.

`native-spark-retained-probe.h` executes original crash entry and handlers up to
the finish handler, with facing left/right, retained word 0/64/128/192,
position (192,192), zero subpixels/RNG, and health 29 to enter the low-energy
crash branch. No cheats, no synthesized CPU instructions or GUI. These are
explicit handler-boundary seeds, NOT proof of which combo sequence produces
each retained value. Two independent captures are byte-identical.

Accepted CSV in `spark-retained-466-v1.zip`, SHA-256
`A448393C73B278C99EE0B6C9DDCD1B450A5808289F67950479C8C2D41997599A`.

`SparkRetainedAudit` seeds the modeled counter before invoking production crash
entry, then executes the real movement step and compares each frame's handler,
radius/subphase, angular travel and both echo coordinates. Reflection is confined
to boundary setup; it does not implement the crash routine. Both cleared-counter
controls match all 70 calls. Both facings fail with retained values:

| Retained word | Cartridge calls to finish handler | C# calls |
| --- | ---: | ---: |
| 0 | 70 | 70 |
| 64 | 54 | 70 |
| 128 | 39 | 70 |
| 192 | 39 | 70 |

The diagnostic currently returns nonzero for these real mismatches. It is not
part of the default verification suite and must not be described as a passing
regression until the implementation is corrected. Build succeeds.

Regenerate: apply `native-spark-retained-entrypoint.patch` to the pinned native
host, build Release x64 and run only the bounded/dialog-free
`sm.exe --diagnostic-spark-retained ROM NEW.csv`. Remove hooks afterward (done).
Compare: `SuperMetroid.DebugRunner --spark-retained-audit ROM CSV`.

Next required work: unify the modeled retained echo coordinate/crash counter,
trace native clearing on released-echo loss and projectile reset, then reproduce
successive sparks with active, just-ended and fully-cleared combos. The direct
counter fixture proves the present reset defect but does not satisfy the whole
ticket's resource/echo lifetime and successive-input acceptance requirements.
