# Cartridge encoding in production code (#641)

Gameplay code should describe what the game does: a facing, an animation frame and its duration,
a collision shape, a state transition, a published event. It should not interpret instruction
pointers, synthesize ROM words, or carry unexplained hexadecimal. This note covers how encoded
values are classified and the pattern subsystem migrations follow.

## The rules

`SuperMetroid.EnsureAnalyzer/Contracts/CartridgeEncodingAnalyzer.cs` runs at error severity in every repository
project except the analyzer projects. Every occurrence is reported where it is; there is no baseline, count or
allowlist. Each finding is fixed, or carries its own reviewed justification at its site.

| Rule | Reports |
| --- | --- |
| SME6340 | A hexadecimal literal in functional code. Files named as definition catalogs (`*Definitions.cs`, `*Definition.cs`, `*Catalog.cs`, `*CodePointers.cs`, ...) own their data and are exempt, as is an enum member's value: an enum is a named definition set wherever its file lives. |
| SME6341 | A primitive compared with, offset from or switched over an address-valued code or instruction constant: runtime interpretation of cartridge pointers, catalogs included. |
| SME6342 | An integral array or collection literal of four or more elements holding hexadecimal words: opaque instruction or packed-table data, catalogs included. |
| SME6345 | A shift by a byte, word or bank width, or a byte/word/bank mask, in functional code (#223). |
| SME6348 | A hand-written sign-bit test on a countdown-named primitive (#1324): `& 0x8000`, `& 0x80`, a comparison with `0x8000` or `0x80`, or `(short)t < 0`/`(sbyte)t >= 0` where the operand is one member or local whose name ends in a timer, counter, delay, cooldown, countdown, ticks or frames word (`FunctionTimer`, not `TargetCounterClockwiseAngle`). The constant must be the operand's own sign bit: `0x80` for a byte, `0x8000` for a word (a word compared with `0x80` is a threshold). Signed comparisons of differences, velocities and packed flags are not timer idioms. |
| SME6349 | A wrapping self-decrement of narrow storage (#1324): `x = unchecked((ushort)(x - 1))`, its `byte` form, or `--`/`-= 1` on a `ushort` or `byte` field, property or element. |
| SME6346 | A value of a `[Flags]` enum cast to an integral primitive in functional code (#1323): the flags leave their type before the encoding boundary. Codecs convert with the type's `ToNativeWord`; a catalog, `[SnesEncodingOwner]` type or reviewed allowance is exempt. |
| SME6347 | A `byte` or `ushort` (or nullable) instance field, property or positional record parameter in functional code (#1323): gameplay state stored at the cartridge register width. Constants, statics, locals, parameters and arrays are not counted; catalogs, `[SnesEncodingOwner]` types and reviewed allowances are exempt. |

An intrinsic encoded value in functional code (an exact hardware mask, a hash, a wire-format constant) carries a
reviewed same-line allowance naming its class and a reason:

```csharp
int nibble = value & 0x0f; // cartridge-encoding: allow(Mask) - BCD digit extraction
```

The `// magic-number-audit: allow(Category) - reason` marker is accepted as the same review. A type that owns and
documents an encoding carries `[SnesEncodingOwner]`: its literals and width operations are definitions.

## Countdowns (#1324)

`Core/Hardware/NativeWordCounter.cs` owns the 65C816 timer idioms. A timer that gameplay starts,
decrements and tests is stored as `NativeCountdown`, which keeps the exact native word: a timer that
went negative holds `$FFFF` until something resets it, exactly as the cartridge leaves it. Each site
keeps the branch the cartridge takes:

| Native shape | Operation |
| --- | --- |
| `DEC` then `BPL` (expire on wrapping below zero) | `var step = t.Decrement(); t = step.Remaining; if (step.IsNegative) ...` |
| `DEC` then `BNE`/`BEQ` (expire on reaching zero) | `step.IsZero` |
| `DEC`, `BEQ`, `BMI` | `step.IsZeroOrNegative` |
| `LDA`, `BEQ`, `DEC` (saturating) | `t = t.DecrementToZero()` |
| 8-bit `DEC` of a low byte beside independent high-byte state | `NativeWordCounter.DecrementLowByte(word)` |

A site that also reads the timer as a number (a table index, a threshold other than zero or the sign)
reads `RawValue` explicitly. A timer view over an enemy slot word returns `NativeCountdown.FromRaw(...)`
and stores `RawValue`; the slot word stays the storage.

## Widened gameplay quantities (#1323)

A non-spatial gameplay quantity may be stored as `int` instead of its native word when all of these hold, and the
widening commit records the result for each field:

1. Every producer keeps the value inside a stated range, normally `[0, 0x7FFF]`.
2. Every wrapping write is immediately clamped or rejected, so `int` arithmetic followed by the same clamp is equivalent.
3. Signed word comparisons become ordinary comparisons only while both operands stay in that range.
4. Nothing reads the word through another name, an overlapping offset or a native projection, or that projection
   converts exactly.
5. A decrement that could wrap from zero is guarded by its caller; the conversion goes through a named operation that
   fails when the guard is violated (`SamusQuantity.DecrementPositive`), never through storage width.

Words exist only at the boundaries (SRAM, JSON saves, native snapshot import and word-level parity checks), which
convert through a named codec that rejects out-of-range words (`SamusQuantity.Decode`/`Encode`). A wrap that is part
of the behavior stays explicit: `NativeWordCounter` for counters, or a named operation on the owning type.
`SubunitHealth` stays a word because its borrow into `Health` is load-bearing.

## Classifying an encoded value

A subsystem migration decides what each raw value means:

1. **Numeric quantity**: use decimal and an ordinary or unit type.
2. **Domain identity**: an enum once the whole domain is proven.
3. **Composable bits**: `[Flags]` only where the cartridge composes them.
4. **Structured instruction**: a typed record, timeline or state transition.
5. **Cartridge address or encoded boundary value**: a typed catalog or decoder.
6. **Unknown**: kept at the boundary under an explicit unresolved name until its meaning is established.

## Migrating an instruction program

Botwoon is the reference migration:

- `Core/Game/AnimationTimeline.cs` models an animation as steps: show a frame for a duration, publish
  a cue, hold, or loop. It is generic over the frame and cue types.
- `Core/Game/BotwoonAnimations.cs` states Botwoon's semantics with no addresses:
  - facings and head and segment programs;
  - frames and cues (`ApplyFacingRadius`, `QueueSpitSound`, `ReachSpitFrame`);
  - the exact timelines, including the cartridge's longer open mouth when spitting left;
  - selection from the AI's angle and selector words.
- Owners keep the cartridge's timer gate. A frame expires on the update its native timer would, and
  a hold behaves like the native sleep: the timer wraps and the last frame stays. Botwoon's head
  advances in `AdvanceBotwoonHeadAnimation`. Projectiles run an `EnemyProjectileAnimation` held on
  the slot, which the interpreter yields to.
- A native program installed later (a shot or touch redirect, or a landed segment's shared
  explosion) goes through `RoomEnemyProjectileSlot.StartNativeProgram`. That call ends the typed
  animation structurally.
- `Core/Assets/BotwoonVisualDefinitions.cs` is the only place that knows the cartridge identities
  of the frames:
  - the head's bank-$B3 spritemaps;
  - the bank-$86 operands that name projectile artwork in installed content, which is immutable.
- Verification is a ROM oracle: a minimal reader walks the cartridge's own lists, and the typed
  animation must match it every update (timers, frames, radius, cues, new-frame flag). See
  `Program.BotwoonAnimations.cs`.
- Delete the word synthesizer, its opcode enum, its tooling catalog and its calculated selectors.
