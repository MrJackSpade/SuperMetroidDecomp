# Cartridge encoding in production code (#641)

Gameplay code should describe what the game does: a facing, an animation frame and its duration,
a collision shape, a state transition, a published event. It should not interpret instruction
pointers, synthesize ROM words, or carry unexplained hexadecimal. This note covers the guard that
keeps new encoding out, the inventory of what remains, and the pattern subsystem migrations follow.

## The ratchet

`SuperMetroid.EnsureAnalyzer/CartridgeEncodingAnalyzer.cs` runs at error severity in every production
project (Core, AssetExtraction, Diagnostics, Desktop, Rendering.Direct3D11, Game, Android): each
carries `csharp/cartridge-encoding-baseline.txt` as an additional file. A file may only lose
occurrences, and a file absent from the baseline may hold none.

| Rule | Reports |
| --- | --- |
| SME6340 | A hexadecimal literal in functional code. Files named as definition catalogs (`*Definitions.cs`, `*Catalog.cs`, `*CodePointers.cs`, ... the same suffixes as the magic-number audit) own their data and are exempt. |
| SME6341 | A primitive compared with, offset from or switched over an address-valued code or instruction constant: runtime interpretation of cartridge pointers, catalogs included. |
| SME6342 | An integral array or collection literal of four or more elements holding hexadecimal words: opaque instruction or packed-table data, catalogs included. |
| SME6343 | A file now holds fewer occurrences than its baseline: lower the count so the gain is kept. |
| SME6344 | The baseline names a file the project no longer compiles: remove the entry. |

An intrinsic encoded value in functional code (an exact hardware mask, a hash, a wire-format
constant) carries a reviewed same-line allowance naming its class and a reason:

```csharp
int nibble = value & 0x0f; // cartridge-encoding: allow(Mask) - BCD digit extraction
```

The existing `// magic-number-audit: allow(Category) - reason` marker is accepted as the same review.

After a migration, regenerate the baseline and the inventory from the same scanner the build runs:

```
dotnet run --project csharp/src/SuperMetroid.CartridgeEncodingAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write
```

## The inventory

`docs/cartridge-encoding-inventory.json` lists every audited file grouped into independently
migratable subsystems. For each file and subsystem it counts the ratcheted rules, the catalog-owned
and reviewed literals, the proposed audit class of every literal, and the members that keep a
cartridge storage width (`byte`/`ushort`) outside catalogs. Each subsystem also lists its
address-interpreting types: the owners of its SME6341 dispatch.

The six audit classes are proposed by the scanner from syntax (array element, address range, enum or
`[Flags]` member, switch arm, bitwise operand, pointer-named context, equality or arithmetic). A
subsystem migration confirms or corrects each:

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
- Delete the word synthesizer, its opcode enum, its tooling catalog and its calculated selectors,
  then regenerate the baseline.
