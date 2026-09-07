# Native projectile-link audit — issue #283

This console diagnostic executes **original ROM instructions** with the upstream
65816 CPU interpreter. It does not call a C translation of projectile behavior.
Unmapped bus accesses, unexpected opcode hooks, instruction-budget exhaustion,
and assertion failures exit nonzero through stderr, without a GUI dialog.

## Run on Windows

From a Visual Studio x64 Native Tools command prompt at the repository root:

```bat
csharp\native\ProjectileLinkAudit\build.cmd
csharp\native\ProjectileLinkAudit\audit.exe "Super Metroid.smc"
```

Reference interpreter checkout: `upstream-sm` commit
`578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Tested unheadered ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
The executable expects the 3 MiB unheadered ROM; other revisions are not qualified.

## Reproduction and limits

The local player recording `SuperMetroid-input-20260906-105957-985.smrec`
reproduces the reported moving explosion at frame 31488, room `$D461`.
The focused native fixture transcribes the relevant owner/helper position,
velocity, link index, instruction lists, and collision pillar from that sequence.
It is not a full emulator save state: unrelated room blocks are air and enemy,
PLM, rendering, and controller processing are outside this fixture.

The room is 64 blocks wide; block 851 is `$8119` (solid). The following block
is `$3115` / BTS `$82` (special air). Both the level-size word and bank-$7F BTS
address must be initialized correctly: an earlier incomplete fixture omitted the
level-size word and falsely reported a collision on every subsequent sample.
That result is invalid and must not be cited as cartridge evidence.

After testing the supplemental collision routine `$90:B406`, the fixture resets
low WRAM and runs the complete descending-slot projectile handler `$90:AECE`,
including its native bank-$93 animation interpreter. Its assertions establish:

| Frame | Flying owner X/type | Helper X/type | Helper spritemap |
| --- | --- | --- | --- |
| 0 | `$0141 / $8200` | `$0139 / $8800` | `$A117` (blank) |
| 1 | `$0154 / $8200` | `$014B / $8800` | `$AA84` (explosion) |
| 2 | `$0168 / $8200` | `$015E / $8800` | `$AA84` (explosion) |

Thus the original ROM can produce a travelling explosion: its invisible
supplemental collision helper hits a narrow pillar while the owner passes it,
and the owner continues repositioning that now-visible helper. This is not
evidence of a second shot inheriting a previous slot's animation.

Separate calls to `$90:AE06` also assert two genuine translation discrepancies
found during this investigation: missile impacts do **not** add the beam's
leading-edge radius, and another impact on an existing explosion clears its
slot instead of restarting its explosion. The C# impact handler now matches
both properties; the focused managed regression exercises the real projectile
producer and frame handler, including a second pillar collision. The original
player replay now matches all three native helper positions above.

`--moving-missile-explosion-audit` remains a symptom finder: it deliberately
reports travelling explosions, including this native-ROM quirk. It is not a
cartridge-parity pass/fail gate. Suppressing all helper movement would change
the original game and is not part of this translation correction.

The hardware multiply registers are implemented synchronously; the tested ROM
routine already waits enough instructions before reading their result. No game
routine is stubbed. Returning through the interpreter's outer stack breakpoint
is the sole return hook.
