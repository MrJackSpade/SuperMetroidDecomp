# Cartridge encoding in production code

Gameplay code describes what the game does: a facing, an animation frame and its duration, a collision
shape, a state transition, a published event. It does not interpret instruction pointers, synthesize ROM
words, carry hexadecimal, or identify anything by an address. This note covers how encoded values are
classified and the pattern subsystem migrations follow. The standard it serves is in `manifesto.md`: code a
reviewer could read without learning it was ported from a cartridge.

## The rules

`SuperMetroid.EnsureAnalyzer/Contracts/CartridgeEncodingAnalyzer.cs` runs at error severity in every repository
project except the analyzer projects. Every occurrence is reported where it is; there is no baseline, count or
allowlist. Each finding is fixed, or carries its own reviewed justification at its site.

| Rule | Reports |
| --- | --- |
| SME6340 | A hexadecimal literal in functional code. Files named as definition catalogs (`*Definitions.cs`, `*Definition.cs`, `*Catalog.cs`, ...) own their data and are exempt, as is an enum member's value: an enum is a named definition set wherever its file lives. The exemption is for data. A catalog-named file that contains `if`, `switch`, loops or decisions is functional code hiding behind its name; the logic moves out, and the file is never renamed to gain the exemption. A name that says the file holds addresses, pointers, ROM data or tables (`*Addresses.cs`, `*CodePointers.cs`, `*Pointers.cs`, `*RomData.cs`, `*Tables.cs`) is not a catalog name (#1411, `docs/cartridge-addresses.md`). |
| SME6341 | A primitive compared with, offset from or switched over an address-valued code or instruction constant: runtime interpretation of cartridge pointers, catalogs included. |
| SME6342 | An integral array or collection literal of four or more elements holding hexadecimal words: opaque instruction or packed-table data, catalogs included. |
| SME6345 | A shift by a byte, word or bank width, or a byte/word/bank mask, in functional code (#223). |
| SME6348 | A hand-written sign-bit test on a countdown-named primitive (#1324): `& 0x8000`, `& 0x80`, a comparison with `0x8000` or `0x80`, or `(short)t < 0`/`(sbyte)t >= 0` where the operand is one member or local whose name ends in a timer, counter, delay, cooldown, countdown, ticks or frames word (`FunctionTimer`, not `TargetCounterClockwiseAngle`). The constant must be the operand's own sign bit: `0x80` for a byte, `0x8000` for a word (a word compared with `0x80` is a threshold). Signed comparisons of differences, velocities and packed flags are not timer idioms. |
| SME6349 | A wrapping self-decrement of narrow storage: `x = unchecked((ushort)(x - 1))`, its `byte` form, or `--`/`-= 1` on a `ushort` or `byte` field, property or element. |
| SME6346 | A value of a `[Flags]` enum cast to an integral primitive in functional code: the flags leave their type before the encoding boundary. Codecs convert with the type's `ToNativeWord`; a catalog, `[EncodingOwner]` type or reviewed allowance is exempt. |
| SME6347 | A `byte`, `sbyte`, `short` or `ushort` (or nullable) instance field, property, parameter or local (written or inferred; `foreach`, `out` and pattern variables included) in Core: gameplay state held at a cartridge register width (#1419). Constants, statics and arrays are not counted. Only `[EncodingOwner]` codecs and the wrapping stubs of the `SuperMetroid.Core.Compatibility` namespace may hold a narrow width; a catalog name or a same-line comment does not excuse one. See "Narrow storage". |

An intrinsic encoded value in functional code (an exact hardware mask, a hash, a wire-format constant) carries a
reviewed same-line allowance naming its class and a reason:

```csharp
int nibble = value & 0x0f; // cartridge-encoding: allow(Mask) - BCD digit extraction
```

The `// magic-number-audit: allow(Category) - reason` marker is accepted as the same review. A type that owns and
documents an encoding carries `[EncodingOwner("format")]`, naming the file or stored form it owns (#1420): its
literals and width operations are definitions.

## Narrow storage

Gameplay state is never held at the cartridge's register width (SME6347). Each narrow declaration is classified by
what the value means, and the change records the decision at the site:

| The value is | It becomes |
| --- | --- |
| A wrap or borrow the game observes (a timer decremented through zero, a counter that wraps, a low byte stepped beside independent state, health and ammo borrow, level-data allocation tails, over-read windows) | A named wrapping stub in `Core/Compatibility` (`WrappingCountdown`, `WrappingCounter`, `CountdownStep`, `LowByteCountdownStep`, `TickAccumulator`) or a new one whose summary states the gameplay behaviour the width produces, with a focused check that exercises the wrap. |
| A width an encoding needs (a save word, a cartridge record byte, a packed tile or level word, a file-format field, a host pixel) | The encoding's single `[EncodingOwner]` codec (`RoomLevelWord`, `PcmWaveFile`, `RgbaPixel`); gameplay holds the decoded domain value or the codec's value type. |
| Spatial | The branded spatial types (`docs/spatial-types.md`); a value passing through one axis is an `int`. |
| An index or count | `int`, with `[IntegerQuantity]` where the analyzer needs it. |
| A flag word | A typed `[Flags]` enum, converted at its codec. |
| Incidental (the width only mirrors the register) | `int` or the domain type. |

Widening a local or parameter is exact when the value it receives is already a word (or byte): an `int` holds the same
value, compares and computes the same way, and the arithmetic that produced it keeps its own explicit wrap
(`unchecked((ushort)(a + b))` where a producer can exceed the range and the game observes the result). What a widening
must change is any mutation the width wrapped implicitly: `x++`, `x--` and compound assignments of narrow storage
become an explicit wrap or a stub, or are shown not to reach the boundary. Where a value leaves gameplay for an owner,
the owner's operation takes `int` and checks the range (`checked`, `Ensure.BetweenInclusive`), so a value that was
wrongly left unwrapped fails at the boundary instead of being truncated silently.

## Countdowns

`Core/Compatibility/WrappingCounter.cs` owns the timer idioms. A timer that gameplay starts,
decrements and tests is stored as `WrappingCountdown`, which keeps the exact word: a timer that
went negative holds `$FFFF` until something resets it, exactly as the cartridge leaves it. Each site
keeps the branch the cartridge takes:

| Native shape | Operation |
| --- | --- |
| `DEC` then `BPL` (expire on wrapping below zero) | `var step = t.Decrement(); t = step.Remaining; if (step.IsNegative) ...` |
| `DEC` then `BNE`/`BEQ` (expire on reaching zero) | `step.IsZero` |
| `DEC`, `BEQ`, `BMI` | `step.IsZeroOrNegative` |
| `LDA`, `BEQ`, `DEC` (saturating) | `t = t.DecrementToZero()` |
| 8-bit `DEC` of a low byte beside independent high-byte state | `WrappingCounter.DecrementLowByte(word)` |

A site that also reads the timer as a number (a table index, a threshold other than zero or the sign)
reads `RawValue` explicitly. A countdown is stored as `WrappingCountdown` by the state that owns it. A timer
view over an enemy slot word (`WrappingCountdown.FromRaw(...)` on read, `RawValue` on write) is scaffolding for a
family whose state still lives in multipurpose slot words; the destination is per-family typed state holding
the countdown directly, and no new timer is added as a slot-word view.

## Widened gameplay quantities

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
of the behavior stays explicit: `WrappingCounter` for counters, or a named operation on the owning type.
`SubunitHealth` stays a word because its borrow into `Health` is load-bearing.

## Fixed-point values, angles and carries

A whole value and its fraction are one quantity. Speeds and accelerations are `SignedSixteenSixteen` or
`UnsignedSixteenSixteen`; a position with its subpixel is a `FixedCoordinate`. Code never carries the two words
side by side, in a tuple or as two parameters (SME6351).

An angle's type states its convention (SME6352). `RotationAngle` is the 8.8 turn word whose high byte indexes the tables;
`ByteAngle` is that table index, with `QuarterTurn`, `HalfTurn` and `AddTableUnits` wrapping in one byte; and
`Bearing` is the cartridge angle routine's result, zero pointing up, converted by `ToTableAngle()`. Where a routine
reads a bearing directly from the table, the conversion is written out at that call. Steps, deltas, masks and offsets
in table units stay integers.

Processor flags do not cross routine boundaries (SME6353). `WordCarry.AddWithCarry`, `SubtractWithBorrow` and
`AfterCompare` model the arithmetic where a carry is load-bearing, and a result names what the flag meant
(`ReachedVerticalBoundary`, `GrappleBlockReaction`).

## Classifying an encoded value

A subsystem migration decides what each raw value means:

1. **Numeric quantity**: use decimal and an ordinary or unit type.
2. **Domain identity**: an enum once the whole domain is proven.
3. **Composable bits**: `[Flags]` only where the cartridge composes them.
4. **Structured instruction**: a typed record, timeline or state transition.
5. **Cartridge address or encoded boundary value**: decoded once by the import or extraction codec into a
   semantic identity or a typed definition. The address does not enter the simulation. A typed address catalog
   inside the simulation is a step on the way out, never the classification's destination.
6. **Unknown**: kept at the boundary under an explicit unresolved name until its meaning is established.

## Migrating an instruction program

Botwoon is the reference migration:

- `Core/Programs/AnimationTimeline.cs` models an animation as steps: show a frame for a duration, publish
  a cue, hold, or loop. It is generic over the frame and cue types.
- `Core/Bosses/Botwoon/BotwoonAnimations.cs` states Botwoon's semantics with no addresses:
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
- The frames are named semantically in the simulation. The one place that knows which cartridge spritemap
  or artwork operand a frame came from is the extraction that produces the installed artwork; a definitions
  file in Core that still names frames by bank and address is scaffolding left from the migration, not the
  pattern to copy.
- Verification is a ROM oracle: a minimal reader walks the cartridge's own lists, and the typed
  animation must match it every update (timers, frames, radius, cues, new-frame flag).
- Delete the word synthesizer, its opcode enum, its tooling catalog and its calculated selectors in the same
  change that lands the typed program. Nothing is kept beside its replacement.
