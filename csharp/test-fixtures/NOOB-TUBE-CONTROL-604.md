# #604: n00b-tube control and running-animation handoff

Affected player version: 0.2.1. Status: **in progress**. The running-animation
discrepancy is reproduced and fixed; a separate trigger-arming timing difference
still requires investigation. Do not mark the complete ticket awaiting validation.

## Exact reproduction

Load intact retail room $8F:CEFB ($04/$01), event $0B clear, Samus X128/Y395 on
the authored row26 floor. Equipment is Morph Ball/Bombs/Varia, 999 energy,
10 Power Bombs, no missiles/Supers and no cheats. Camera starts32/229. After
setup, only physical inputs change state:

- Down at0 and12, Item Select at24, Shoot at26, Up at40.
- Either no further input, or Right at124..153, or Right at125..154.
- Run700 frames through the real room, PLM, Power Bomb, movement and animation owners.

The Power Bomb consumes one round and reaches the tube's input-wake callback.
With Right at124, both executions enter pose9, X129/Y395, animation0/timer2 and
install the lock at124. Native retains that cursor through270; port previously
decremented the timer at125 and continued the running animation in place. The
exact comparison failed first at125: port timer1, native timer2. No permanent
softlock is claimed; both eventually restore control and fall after shattering.

## Root cause and correction

Bank84 instruction $D5E6 calls Samus command zero at $90:F109. It installs alpha
$E713 and beta $E8DC, which does not advance Samus animation. The old tube owner
only set `InputLocked`, suppressing movement/input but leaving the generic animation
pass active. The paired command-one unlock is $90:F117, called by $84:D5EE.

`SamusState.SetStationaryScriptControlLock` now represents that stationary command
pair separately from other input-locked owners that legitimately move or animate
Samus. The runtime samples its ownership before PLMs: a PLM lock/unlock affects
the next native beta, not the current frame's animation. It preserves the running
pose and exact animation cursor rather than forcing a standing pose. Ordinary
input unlock clears stale stationary ownership. The tube invokes this shared
command behavior for both lock and unlock.

The existing serialized input-lock backing-field identity is preserved. Older
Samus graphs migrate with an explicit warning: the absent stationary owner cannot
be reconstructed exactly, so their original input lock remains while animation
ownership defaults unknown/inactive until a new script command. Current mid-lock
states round-trip both owners. Tests verify legacy field order and generic unlock
clearing the stationary owner; no incompatible field rename is introduced.

## Verification and reproduction commands

All2,100 frames of the three controls compare input, world position, pose,
animation frame/timer and native beta lock ownership. Final assertions cover the
broken event, restored input/animation ownership and ammunition consumption. The
no-input control correctly remains intact and unlocked. Existing synthetic PLM
tests also assert the command ownership, not just the input flag.

```text
sm.exe --diagnostic-noob-tube-control ROM output.csv 124
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --noob-tube-control-audit ROM output.csv 124
```

Use wake `-1`, `124`, or `125`. The matching native probe and entrypoint patch are
under `movement-release`; restore/rebuild the ordinary native executable afterward.
The archive `movement-release/noob-tube-control-604.zip` contains numeric traces
only. Two independent captures per input share these SHA256 values:

| Wake | SHA256 |
| --- | --- |
| -1 | `3484FCD892D15B1101F99BCE96D1944FDD1080E34FDC2C316F38AA77D3DD54FF` |
| 124 | `8127F04068E7FA5195B74BB7F5B18901A23CED4C5E92243AEC87FD682D7F4003` |
| 125 | `D6B4781B589E537567CDF18796052294A068B859F7A4BE5A133B1E0477FB0387` |

Native source: upstream-sm `578f90b3cc49557bb70060ad033bb90b8cf8ac50`, original
65816 execution with initialization comparison patches undone. Disassembly pin:
`362be646929cf8e483f692b73a6561cfc2dc1d0d`. ROM SHA256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

## Resolved allocation timing finding

With Right first pressed at123 and held, the port locks but the native run only
arms its input-wake pre-instruction on that frame, so it needs a later new input.
Port arms one frame earlier (122). This is not hidden by the successful124/125
controls. A native radius trace identified the first discrepancy at fuse expiry:
both expire at85, but native leaves radius/speed zero, initializes them to1024/12288
on HDMA pass86, and first expands to13312/12160 on87. The port formerly performed
setup at85 and expansion at86. A new assertion failed at85 before production edits.

`PendingPreExplosionSetup` now represents the allocated list until its first HDMA
pass. It installs the normal pre-instruction without running it or drawing a window.
The enum value is appended, preserving existing debugger-state phase identities.
No room-specific delay or input suppression was added. The new wake123 control
matches all700 native frames, including the intentionally unbroken tube after an
early press stays held; wake124/125 still complete normally. All four controls
now compare PLM pre-instruction and instruction pointers on every frame as well
as Samus position, pose, animation, and lock ownership (2800 frames total).
Native's inert `$84:86D0` RTS is normalized to the managed cleared value zero;
all active pre-instruction identities and instruction cursors compare directly.

Numeric-only `movement-release/noob-tube-boundary-604.zip` contains the additional
`noob-tube-native-wake-123.csv`, captured twice identically. SHA256:
`465EA8AF4BBD130F7DDD4C9751688E86E0A05A8B58D9F46557C4F9586CB4EA2C`.
The same probe command accepts123; the earlier three captures remain unchanged.

Full bank-$80 verification passes. Its Crystal Flash fixture now activates one
NMI later: the globally eight-frame-aligned refill produces249 visible body frames
instead of250, with the same34 window frames. The independent4088-frame original-CPU
Crystal Flash lifetime comparison still matches both facings and all eight NMI phases.
Renderer fixtures explicitly check the non-drawing setup pass before the first flash.

The cartridge explicitly waits for new A/X/B/Y/Left/Right input at $84:D4BF;
removing that control gap outright would not preserve the native sequence.
