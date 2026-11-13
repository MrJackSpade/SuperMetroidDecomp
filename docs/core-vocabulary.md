# Core vocabulary (SME6438)

`SuperMetroid.Core` is a C# game. Its identifiers say what things are in the game, never which chip, memory or
cartridge structure the original program stored them in. This is the rule that detects what is left of the machine
(v0.9.0 manifesto, section 5); every v0.9.0 removal ticket drives its words to zero.

## Target shape

A reviewer reading Core cannot tell from its names that it was ported from the SNES. Sprites are frames of an actor's
sheet, not OAM entries; colours are owned colours and colour effects, not CGRAM slots; a backdrop is an image, not a
VRAM plane; a sound request is a request to a sound library, not an APU port write; an identity is a name, not a
pointer or a bank. Hardware behaviour the game genuinely needs (the timing of a sound acknowledgement, how far the door
loader has progressed, a counter that wraps) lives in a compatibility service named for what it does for the game.

## The rule

`CoreVocabularyAnalyzer` reports, in the `SuperMetroid.Core` assembly only, every declared identifier whose camel-case
words include a hardware word: types, type parameters, fields, properties, methods, events, parameters, locals, local
functions, query range variables, namespace segments and file names.

The words, kept in one place in the analyzer (`CoreVocabularyAnalyzer.HardwareWords`):

`Snes`, `Vram`, `Cgram`, `Oam`, `Obj`, `Dma`, `Hdma`, `Nmi`, `Apu`, `Spc`, `Dsp`, `Wram`, `Ppu`, `Native`, `Rom`, `Bank`,
`Pointer`, `Address`, `Cartridge`, `Mode7`, `Register`, and the CPU's return and jump mnemonics `Rts`, `Rtl`, `Jsr`,
`Jsl`, `Jmp`, `Jml` that routine labels carry (#1481), the storage widths, save memory, opcodes and program labels
`Word`, `Byte`, `Sram`, `Opcode`, `Plm`, `Eproj` (#1558), the interpreter's vocabulary for the original's streams
`Command`, `Operand`, `Instruction` (#1624: a Core cue signals, a reveal has a shape, a frame carries its gameplay and
a pose update has a momentum response), and `Header`, the cartridge's record headers that named rooms,
room objects, effects, projectiles and enemies by the structure the program stored them in (#1685), and the
cartridge's sprite and raster vocabulary (#1694): `Spritemap`, the frame the cartridge built from hardware sprite
pieces (a Core picture is a frame, a picture or a composition); `Scanline`, the display's raster line (a Core screen
row is a line); and `Tilemap`, the cartridge's one-word background tilemap (a Core page or layout is named for what it
shows). Three two-word names are listed too (`CoreVocabularyAnalyzer.HardwarePhrases`, #1701): `AiFunction`,
`EnemyFunction` and `FightFunction`, the enemy record's AI function field, which named an enemy's or a boss's behaviour
state by the routine the program stored (a Core behaviour state is a phase or an action: `KraidPhase`, `BeetomAction`).
The words must be consecutive, the last may take an ending (`PipeBugEnemyFunctions`), and `Enemy`, `Fight` and `Ai`
alone stay the game's own (`EnemyState`, `FightPhase`). An underscore-separated part that is a cartridge address (four or six
hexadecimal digits with at least one decimal digit, as in `RTS_86BB30` or `Instr_A0B3`) is reported as well.

A name that says `Callback` names the original's function-pointer dispatch and is reported too (#1624,
`CoreVocabularyAnalyzer.DispatchWords`): the cartridge stored a room's setup and main behaviour, a beam's frame behaviour
and each of Kraid's sinking steps as a pointer it called back, and Core names the behaviour instead (`RoomSetupBehavior`,
`RoomMainBehavior`, `SamusBeamFrameBehaviorChoice`, `KraidSinkStep`). A C# delegate the port passes is a genuine
callback and keeps the word: a delegate type and its file, an event, and a field, property, parameter or local whose type
is a delegate, or an array or constructed generic type of one (`Action? AudioCallback`, `Func<int> callback`,
`List<Func<int>> PendingCallbacks`). A method's own name is never a delegate, even when it returns one.

A name that says `Function` names the enemy record's AI function field and is reported under the same delegate
allowance (#1701, `CoreVocabularyAnalyzer.BehaviourFieldWords`): the cartridge stored an enemy's or a boss's behaviour
state, a part's wait and the state a cue installs as the routine its record's function field held, and Core names what
that state is (`TorizoPhase`, `GunshipPhase`, `YardMovement`, `WallSpacePirateAction`, a state's `Phase` or `Action`,
a `SetPhase` cue, a `PhaseTimer` or `ActionTimer`). A `Func<int>` field or parameter, a delegate type and its file keep
the word, and words that only share its letters stay the game's own (`Functional`, `Func`, `Functor`).

A name that spells a cartridge value in hexadecimal is reported too (#1554): a run of two or more capital hexadecimal
characters after a lower-case letter that holds a hexadecimal letter and ends its word (`Unused0B`, `PoseFD`,
`Backdrop2A`, `Projectile86E314Frame`); a run of four or more hexadecimal characters of either case that starts with a
decimal digit after a letter and holds a letter (`Projectile86C76eFrame`); and every member of a type's family numbered
in hexadecimal, members sharing a stem and ending in capital codes of one length where one code holds a hexadecimal
letter and one a decimal digit (`Row9` beside `RowA`, `Backdrop28` beside `Backdrop2A`). Ordinary words and decimal
ordinals are not codes: a capital that starts a lower-case word begins that word (`Add`, `Bad`, `Face`, `Dead`, `Feed`;
`Shift8Add` is `8` and `Add`), a run of capitals that continues into other capitals is an acronym, a lone letter is a
word or ordinal (`BossA`), and decimal numbers are numbers whether or not they are zero-padded (`Frame05`, `Dachora19`).

Matching is by whole camel-case word, ignoring case, with plural and verb endings (`Banks`, `Addresses`, `Banked`,
`Addressing`). `Object` is not `Obj`, `Roman` is not `Rom`, `Registry` is not `Register`. A run of capitals is one word
(`OAMBuffer` is `OAM` + `Buffer`), and a digit stays with the word before it (`Mode7`).

`Tilemap` is matched as the one word the cartridge's sources spell. A room layer named as the destination engine names
it, `TileMap` or `TileMapLayer`, is the two words `Tile` and `Map` and stays the game's own, as do `Tile`, `Tiles`,
`Metatile` and `MapTiles`; so does `SpriteSheet` beside `Spritemap`, and `Scan`, `Scanned` or `FirstLine` beside
`Scanline` (#1694). Documentation prose is never read, so a comment may still say what a cartridge spritemap held.

Members that override or implement a member declared outside Core keep the name the base declares. A record's
positional parameter and the property it declares are reported once.

## Exemplar

```csharp
// Before: the identity is a ROM pointer and the colour is a palette slot.
internal static SnesSpritemapPointer KraidHeadPointer => new(0xA7, 0x8DD2);
internal static int CgramIndex(int row, int column) => row * 16 + column;

// After: the identity is a name, and the colour belongs to the thing that owns it.
internal static KraidFrame Head => KraidFrame.Head;
internal OwnedColor ClawGlow { get; }
```

## Allowance

There is no identifier allowance. A type marked `[CompatibilityService]` (#1401) may say in its XML documentation which
hardware behaviour it reproduces (for example "as the SPC port handshake did"); documentation is not an identifier, so
the rule never reads it. Every other occurrence is renamed for the game or removed with the thing it named. No baseline,
allowlist, count, severity change or scope change (project manifesto, section 6).

## Status

The rule is the milestone-wide detector: it ships at error severity with the whole v0.9.0 work list as its findings and
stays red until the last removal lands. Count the findings with the census (`docs/analyzer-cleanup.md`); while it is
red, build verification binaries with `-p:RunAnalyzers=false` and rely on the census for every SME rule.
