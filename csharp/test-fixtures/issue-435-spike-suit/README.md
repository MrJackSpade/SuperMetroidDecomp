# Spike/electricity Shinespark Suit parity (#435)

This fixture executes the pinned Japan/USA cartridge CPU routines and compares
their frame-visible Samus state with the C# gameplay dispatcher. It covers the
Shinespark Suit technique created by taking spike/electricity damage as Morph
Ball, unmorphing during contact, and pressing Jump as knockback expires. This is
the indefinitely retained suit state, not merely an ordinary stored Shinespark
or an active Shinespark windup.

`spike-suit.csv` sweeps unmorph frames zero through two and Jump frames six
through twelve for 315 original-CPU frames. The successful dry cases unmorph on
frame one or two and press Jump on frame nine. The frame-eight neighbor starts
the ordinary `$90:D068` windup; frame ten has already missed the retained-suit
window. The comparison locks pose, position, fixed-point vertical motion,
movement-handler ownership, knockback, shine timer/palette, invincibility, and
health on every frame.

`underwater-reserve.csv` covers the suitless underwater extension described by
the source technique: automatic Reserve recovery freezes Samus during an
unmorph while global hurt timers continue. It sweeps freeze frames six through
twelve and post-recovery Jump frames four through eight for 875 original-CPU
frames. A 60-point refill plus freeze frames eight through ten and Jump frame
six retains the suit; the adjacent setup and input frames prove the boundary.
The room is a deliberate synthetic all-electricity field so the moving hurt
body remains in contact without coupling timing to unrelated retail geometry.

The native sequence invokes the real `$90:DDE9` hit interruption, bank-$91 pose
transition/animation routines, `$90:DF38` knockback mover, `$90:F411/$90:F2E0`
Reserve lock/unlock, and `$82:DC31` refill routine. The port comparison uses the
ordinary `SuperMetroidRuntime.StepFrame` path and
`SamusReserveAutoRecoveryState`; it does not set a retained-suit result directly.

The comparison reproduced three port defects. Hit-expiry was sampled after an
animation command had erased its direction marker; expiry then cancelled the
entire Shinespark state instead of replacing only its movement handler. During
Reserve command zero, special knockback movement and terrain hazard detection
still ran, and the stored-shine palette timer continued advancing. The runtime
now preserves the pre-transition expiry decision and models command zero as the
complete stationary alpha/beta owner while global hurt timers continue.

Pinned ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

Normalized LF UTF-8 trace SHA-256 values:

- `spike-suit.csv`: `8EA5FE301EAA2034EADABED8D9054244DA0DFA51375378365E0006DB297825CE`
- `underwater-reserve.csv`: `2E06311C877D780CFE480FAA961756816706C50B3E1C3F8FA13E93412B758D8C`

Generate with `DraygonCrystalAudit/audit.exe "Super Metroid.smc"
spike-suit` or `spike-suit-reserve`. Compare with DebugRunner
`--spike-suit-audit <rom> <trace>` or
`--spike-suit-underwater-reserve-audit <rom> <trace>`.

The technique description was cross-checked against
`https://wiki.supermetroid.run/Shinespark_Suit`; routine identities and ordering
were pinned against `upstream-sm/src/sm_90.c`, `sm_91.c`, and `sm_82.c`.
