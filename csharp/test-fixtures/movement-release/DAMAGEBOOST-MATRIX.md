# Replaying the accepted damage-boost matrix

Run from the repository root. `damageboost-native-captures.zip` contains 97
original-CPU CSV captures, not ROM bytes or player saves. It preserves the accepted
fixtures independently of disposable `test-temp` files. Every archived file was
SHA-256-compared with its original before commit.

Archive SHA-256:
`F2057D05AD366755EAEA014C98D5059B597BA1D671AFEF556E27DA4835D09144`.

Pinned ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Pinned C source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Pinned disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Extract to a new directory; do not overwrite existing captures:

```powershell
Expand-Archive csharp/test-fixtures/movement-release/damageboost-native-captures.zip csharp/test-temp/damageboost-accepted
dotnet build csharp/src/SuperMetroid.DebugRunner -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File csharp/test-fixtures/movement-release/Verify-DamageBoostMatrix.ps1 -TraceDirectory csharp/test-temp/damageboost-accepted
```

Pass `-Dotnet PATH` when using an isolated SDK. The execution-policy override is
limited to that PowerShell process. The script checks all required files before
starting, uses only explicitly accepted capture names, and stops on the first
nonzero comparison exit. It does not substitute historical invalid FX captures.

The 616,608 samples comprise six seeded-hurt captures, 24 forward-contact captures,
12 carried-speed captures, six electric-block captures, two running approaches,
42 pre-held-Jump captures, and five no-Jump negative-input captures. Per-frame
comparisons retain position/subpixels, pose/animation, hurt timers/directions,
vertical and horizontal velocity, pose history, and source damage where recorded.

The no-Jump captures use `DiagnosticDamageBoostInputs` with `jumpHeld=0` and
`jumpDisabled=1`, sources 1/2/3/4/7, air, continued direction. Their new final CSV
field is validated on every row, and the comparer explicitly rejects damage-boost
poses in either native or managed results. These are real contacts: all five
captures contain damaged frames, while none contains a damage-boost pose.

Native regeneration and the implementation history are documented in
[DAMAGEBOOST.md](DAMAGEBOOST.md). The probe wrappers preserve earlier callers;
29-column output adds `jumpDisabled` without changing the earlier field meanings.
Cheats are disabled in these constructed, bounded encounters. This is not a PAL,
modified-ROM, full-route, or all-enemy-behavior validation claim.
