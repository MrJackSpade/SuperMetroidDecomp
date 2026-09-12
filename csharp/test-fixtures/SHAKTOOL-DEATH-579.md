# Shaktool lethal-shot cleanup (#579)

## Reproduction

The full `--shaktool-audit` failed on clean baseline 58b22b6f and the #547
family-table migration: `RequireShaktoolState` rejected slot zero after a fatal
shot. The new `ShaktoolAudit.Death.cs` production-path regression reproduced the
same exception before changing the runtime.

## Native cause and implementation

`upstream-disassembly/src/bank_AA.asm`, $AA:DF34-DF5B, calls common shot AI
before reading health and the owner index. The disassembly explicitly documents
the shipped bug: common death clears the enemy RAM, so the owner index is zero.
The callback then writes literal Deleted properties to physical slots zero
through six, without asking whether those slots still identify Shaktool.
`upstream-sm/src/sm_aa.c` (`Shaktool_Shot`) and `sm_a0.c` (`EnemyDeathAnimation`)
confirm the ordering and common-slot memset.

The port instead queried its validated live-group lookup after common death.
That lookup correctly rejects a cleared enemy, but it is the wrong operation at
this native callback boundary. The fatal-shot tail now reads the post-clear
VariableE word and performs the seven property writes directly. It does not
recover a cached pre-death owner, suppress an exception, or change common death.
Other live Shaktool group operations retain their validation.

## Verification

`DebugRunner --shaktool-audit "Super Metroid.smc"` now checks lethal beams and
normal bombs on both vulnerable ends. Each must publish exactly one kill and
one death effect with the original header, native slot and position; clear the
struck common record; preserve other segment headers; and mark all seven records
deleted. A constructed nonzero-owner case runs real common death followed by the
same tail, proving the original owner is not restored and unrelated slots outside
zero through six remain untouched.

The complete encounter audit also passes natural linked movement, terrain
reversal, animation/drawing, contact, and unused authored attack-circle lifecycles.
The full Release Verification suite and Windows Release build pass as well.
This is a developer-discovered production defect, not a new player-version report.
