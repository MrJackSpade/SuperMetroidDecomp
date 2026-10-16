# Cartridge encoding in production code (#641)

Gameplay code should describe what the game does: a facing, an animation frame and its duration,
a collision shape, a state transition, a published event. It should not interpret instruction
pointers, synthesize ROM words, or carry unexplained hexadecimal. This note covers how encoded
values are classified and the pattern subsystem migrations follow.

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
