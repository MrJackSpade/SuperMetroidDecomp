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

A name that says `Dispatch`, `Dispatcher` or `Redispatch` names the original's jump through a table of routines and is
reported under the same delegate allowance (#1624, `CoreVocabularyAnalyzer.RoutineDispatchWords`): the cartridge ran a
game state, an enemy's AI, a pose's movement or a boss's state by indexing a table of routine pointers, and Core names the
phase or step that runs (`RunMainGameplay`, `IEnemyLifecycle`, `SamusPoseTraits`, `PhaseRandomStep`, `RunningGameState`).
A name that also says what it sends (`CoreVocabularyAnalyzer.SentThingWords`: music, a sound, audio, a request, a message
or an event) uses the English verb of sending and keeps the word (`PendingMusicDispatch`, `DispatchSoundRequests`), and
words that only share its letters stay the game's own (`Dispatchable`).

Three two-word names of the original's routines are reported too (#1721, `CoreVocabularyAnalyzer.RoutinePhrases`),
matched like the hardware phrases: `AlphaHandler` and `BetaHandler`, the two halves of Samus's frame the control
setups installed, and `SetupCode`, the routine a door record pointed at, which an interpreter replayed. Core names the
phase or the setup instead (`SamusInputMode`, `SamusMovementMode`, `DoorSetup` with its screen changes and room event).
Each word alone stays the game's own (`Alpha`, `Handler`, `Setup`, `Code`, `GrappleBeamHandler`).

A name that says `Function` names the enemy record's AI function field and is reported under the same delegate
allowance (#1701, `CoreVocabularyAnalyzer.BehaviourFieldWords`): the cartridge stored an enemy's or a boss's behaviour
state, a part's wait and the state a cue installs as the routine its record's function field held, and Core names what
that state is (`TorizoPhase`, `GunshipPhase`, `YardMovement`, `WallSpacePirateAction`, a state's `Phase` or `Action`,
a `SetPhase` cue, a `PhaseTimer` or `ActionTimer`). A `Func<int>` field or parameter, a delegate type and its file keep
the word, and words that only share its letters stay the game's own (`Functional`, `Func`, `Functor`).

Two synonyms of `AiFunction` are reported too, with no delegate allowance (#1738, `CoreVocabularyAnalyzer.AiRoutinePhrases`):
`AiRoutine` and `AiHandler`, the enemy record's initialization, main, grapple and hurt AI routine fields. A definition
names the response it selects (`EnemyGrappleResponse`, `EnemyHurtResponse` with its `Inert` member), and a species
exposes its lifecycle on its entity (`IEnemyInitialization.Initialize`, `IEnemyMainAi.RunMainAi`) instead of handing
out a handler delegate the record's pointer stood behind. Each word alone stays the game's own (`AiState`, `Routine`,
`EnemyHurtHandler`).

The cartridge's record is reported too (#1745, `CoreVocabularyAnalyzer.RecordPhrases`): the original kept a room's enemy
and object populations, the Baby Metroid's route, a corpse's touch rectangles and its timing and step tables as runs of
fixed-size records, and a name that says `Records`, `RecordCount` or `RecordIndex` (`TouchHitboxRecords`, `FromRecords`,
a `records` local, `BodyRecordCount`) names that structure, as does a type whose last word is `Record`
(`RoomEnemyPopulationRecord`, `CoreVocabularyAnalyzer.NamedForRecord`). Core names what the run holds: a room's
`RoomEnemyPlacement`s and a population's `Placements`, a `PlacedRoomObject` with its `PlacementIndex`, a
`BabyMetroidRouteLeg` and the route's `LegCount`, a corpse's `TouchRectangles`, a table's `StepCount`, a demo's
`DemoInput`s. The English verb keeps the word (`RecordKill`, `RecordDoorOpened`, a `Record` method), as do the game's
demo recordings (`DemoRecording`, `Recorded`). A method's or member's singular `Record` is not read, since it is the verb
as often as the noun (`TryRecord`); such a name is found by review.

A name that spells a cartridge value in hexadecimal is reported too (#1554): a run of two or more capital hexadecimal
characters after a lower-case letter that holds a hexadecimal letter and ends its word (`Unused0B`, `PoseFD`,
`Backdrop2A`, `Projectile86E314Frame`); a run of four or more hexadecimal characters of either case that starts with a
decimal digit after a letter and holds a letter (`Projectile86C76eFrame`); and every member of a type's family numbered
in hexadecimal, members sharing a stem and ending in capital codes of one length where one code holds a hexadecimal
letter and one a decimal digit (`Row9` beside `RowA`, `Backdrop28` beside `Backdrop2A`). Ordinary words and decimal
ordinals are not codes: a capital that starts a lower-case word begins that word (`Add`, `Bad`, `Face`, `Dead`, `Feed`;
`Shift8Add` is `8` and `Add`), a run of capitals that continues into other capitals is an acronym, a lone letter is a
word or ordinal (`BossA`), and decimal numbers are numbers whether or not they are zero-padded (`Frame05`, `Dachora19`).

A name that numbers an area by the cartridge's tileset is reported too (#1720, `CoreVocabularyAnalyzer.AreaNames`): an
area's name (`Crateria`, `Brinstar`, `Norfair`, `Maridia`, `Tourian`, `Ceres`, `WreckedShip`) carrying a decimal number,
run into it or as its own underscore-separated word, that another word follows (`Brinstar6DarkPattern`,
`Crateria23Mechanical`, `Norfair9And10CavernVertical`, `WreckedShip45EntranceHall`, `Crateria_2_BrickRoom`). A number
that ends the name is an ordinal, as a room's doors are numbered (`ElevToBlueBrinstar0`), and a number after any other
word is the game's own (`Metroids2Default`). An area followed by a lone capital letter is not read as the tileset's
letter, since `MaridiaBBrownWall` and `BrinstarETank`, an energy tank, have one shape; such a name is found by review.

Matching is by whole camel-case word, ignoring case, with plural and verb endings (`Banks`, `Addresses`, `Banked`,
`Addressing`). `Object` is not `Obj`, `Roman` is not `Rom`, `Registry` is not `Register`. A run of capitals is one word
(`OAMBuffer` is `OAM` + `Buffer`), and a digit stays with the word before it (`Mode7`).

A name that says `Tile` or `Metatile` names the hardware's tile and is reported too (#1726,
`CoreVocabularyAnalyzer.TileWords`): the eight-pixel picture the machine drew its backgrounds and sprites from, and the
block named as four of them. Core names what it means instead: a block animation (`BlockAnimation`,
`RoomSandBlockAnimationsState`, `TourianStatueBlockPicture`), a picture (`PictureFlip`, `BindScreenPictures`), a
block (`XrayRoomOverlayBlocks`, `TankBlock`), the part of a layer drawn behind or in front of the actors
(`ActorDepth.BehindFrontParts`, `GameplayLayer.BackgroundFrontPart`) or the eight-pixel cell
(`BackgroundCellSizePixels`). The game's own tiles keep the word (`CoreVocabularyAnalyzer.GameTilePhrases`), each tile
word judged by its own neighbours: the area map's tiles, `Map` before it (`MapTiles`, `IsMapTileExplored`,
`WidthInMapTiles`); a room layer named as the destination engine names it, `Map` after it (`TileMap`, `TileMapLayer`);
and the ceiling tiles that fall in Ceres station and the room named for them, `Falling` before it or `Fall` after it
(`FallingTile`, `CeresTileFall`). A field, property, parameter or local whose type is named for the map, or an array or
generic type over one, holds one of the area map's tiles and keeps a bare `tile` or `tiles`
(`MapCoordinateJsonDocument[] Tiles`, `Position16<MapTiles> tile`, `CoreVocabularyAnalyzer.NamedForMap`); a method's own
name is never typed for the map. `Tileset` and `Projectile` are other words.

A name that says `Underflow` names the CPU's arithmetic and is reported too (#1726,
`CoreVocabularyAnalyzer.MachineArithmeticWords`): a subtraction borrowing past zero. Core says what the value did in the
game (`speedPassedZero`).

The original's table positions and enemy record are reported too (#1765, `CoreVocabularyAnalyzer.TablePositionPhrases`),
matched like the hardware phrases: `MenuIndex`, the number the original gave a menu's step; `TableIndex`, a value's
place in one of its lookup tables (a `tableIndex` local, a `tableIndexDelta` parameter); and `EnemyRecord`, the
fixed-size record it kept each enemy in. Core names the menu's step, a speed curve's place (`curvePlace`,
`curvePlaceStep`), an animation's step (`wingStep`) or the enemy itself. Each word alone stays the game's own. SME6454
reads the same three in prose outside hardware owners, spaced or hyphenated (`menu index 13`, a `facing-table index`,
`table indices`, the `enemy records`); a table indexed by a value, the menu's index and an enemy's record are English,
and a compatibility service may describe the enemy record its reproduced behaviour reads.

`Tilemap` is matched as the one word the cartridge's sources spell. A room layer named as the destination engine names
it, `TileMap` or `TileMapLayer`, is the two words `Tile` and `Map` and stays the game's own, as does `MapTiles`; so does
`SpriteSheet` beside `Spritemap`, and `Scan`, `Scanned` or `FirstLine` beside `Scanline` (#1694). Documentation prose is
never read, so a comment may still say what a cartridge spritemap held.

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
the rule never reads it. Its documentation counterpart, SME6454, exempts compatibility services and encoding owners in
the same way, except for the machine's provenance, which it reads in every type: native words and bit patterns, the
cartridge's routines and values written as the machine's hexadecimal fixed-point (`0.1000`, `7.0000`) (#1743). It reads the original program's call structure and the C reimplementation in every type too: dispatchers, callables, a routine's labels (a merge or inverted label) and the upstream C rewrite (#1754). Outside the owners it also reads the original's data and number encodings: decompression, colour indexes, two's complement, unsigned compares and the `8.8` and `16.16` format names (#1754); an encoding owner such as `EightEight` names the format it owns. Every other occurrence is renamed for the game or removed with the thing it named. No baseline,
allowlist, count, severity change or scope change (project manifesto, section 6).

## Status

The rule is the milestone-wide detector: it ships at error severity with the whole v0.9.0 work list as its findings and
stays red until the last removal lands. Count the findings with the census (`docs/analyzer-cleanup.md`); while it is
red, build verification binaries with `-p:RunAnalyzers=false` and rely on the census for every SME rule.
