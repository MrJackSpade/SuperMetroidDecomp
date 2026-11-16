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
`RoomMainBehavior`, `SamusShotBehavior`, `KraidSinkStep`). A C# delegate the port passes is a genuine
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
`Addressing`). `Object` is not `Obj` (the hardware's object is read as a phrase, below), `Roman` is not `Rom`, `Registry` is not `Register`. A run of capitals is one word
(`OAMBuffer` is `OAM` + `Buffer`), and a digit stays with the word before it (`Mode7`).

A name that says `Tile` or `Metatile` names the hardware's tile and is reported too (#1726,
`CoreVocabularyAnalyzer.TileWords`): the eight-pixel picture the machine drew its backgrounds and sprites from, and the
block named as four of them. Core names what it means instead: a block animation (`BlockAnimation`,
`RoomBlockAnimations`, `TourianStatueBlockPicture`), a picture (`PictureFlip`, `BindScreenPictures`), a
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

A name that says `Ntsc` or `Pal` names the video standard and is reported too (#1848,
`CoreVocabularyAnalyzer.VideoStandardWords`), matched as a whole word case-sensitively in the casings `Ntsc`, `NTSC`,
`ntsc`, `Pal` and `PAL` (`IsPal`, `MetareeNtscSpeed`, `PALTiming`, an `ntsc` local). Core names the release whose value
differs (the Japanese and North American release, the European release), as SME6454 asks of prose. A lower-case `pal`
(English, or a palette's abbreviation) is not read, and whole-word matching keeps `Palette`, `Pale`, `Palm` and `Opal`.

The original's table positions and enemy record are reported too (#1765, `CoreVocabularyAnalyzer.TablePositionPhrases`),
matched like the hardware phrases: `MenuIndex`, the number the original gave a menu's step; `TableIndex`, a value's
place in one of its lookup tables (a `tableIndex` local, a `tableIndexDelta` parameter); and `EnemyRecord`, the
fixed-size record it kept each enemy in. Core names the menu's step, a speed curve's place (`curvePlace`,
`curvePlaceStep`), an animation's step (`wingStep`) or the enemy itself. Each word alone stays the game's own. SME6454
reads the same three in prose outside hardware owners, spaced or hyphenated (`menu index 13`, a `facing-table index`,
`table indices`, the `enemy records`); a table indexed by a value, the menu's index and an enemy's record are English,
and a compatibility service may describe the enemy record its reproduced behaviour reads.

A name that says `Sleep` names the animation interpreter's sleep and is reported too (#2021,
`CoreVocabularyAnalyzer.SleepWords`), with the plural and verb endings (`AfterSleep`, `ResumesAfterSleep`, a `Sleeps`
helper, a `sleeping` local, a `Sleeping` phase). The cartridge's lists ended a resting animation in a sleep instruction
that the AI stepped past, waking the list into whatever segment was stored after it. Core lets the resting animation
hold, and each wake starts, by name at its call site, the animation it means (Bomb Torizo's awakening, an egg's hatching
on the side it was laid facing, the next stage of a gate's travel). A creature asleep or dormant in the game says
`Asleep` or `Dormant`; `Sleeper` and `Sleepy` are other words.

A name that says `FrameBehavior` or `FrameBehaviour` names the original's per-frame routine pointer and is reported too
(#2022, `CoreVocabularyAnalyzer.FrameBehaviourPhrases`), with the plural ending (`RunKraidRockFrameBehavior`,
`UnauthoredBeamFrameBehaviors`). The cartridge's enemy and projectile records and Samus's projectiles held the routine
they ran each frame as a pointer their animations installed and cleared. Core names the action, motion or phase:
`FlyKraidRock`, `DropMotherBrainDrool`, `UnauthoredBeamShotBehaviors`; an animation's cue names the event that begins
it (`YardCueKind.BeginTurn`, `YardCue.BeginCrawling`). A delegate keeps no allowance. Each word alone stays the game's
own: a frame, a behaviour.

A name that says the hardware's object is reported too (#2004, #1977, `CoreVocabularyAnalyzer.HardwareObjectPhrases`):
`SpriteObject`, `ObjectSprite`, `CinematicObject` or `AnimatedObject`, with the plural ending (`RoomSpriteObjectKind`,
`_spriteObjects`, `objectSprites`, `IntroCinematicObjectSystem`, `SpawnAnimatedObjects`). The machine called its sprites
objects, and the cartridge named its sprite-object subsystem, its cinematic sprite objects and its animated tile objects
by that word. Core names what they are: the room's effect actors (`RoomEffectActor`, `RoomEffectActorKind`,
`EffectActorPlaces`), the opening's actors (`IntroNarrationActors`), the screen's sprites (`screenSprites`) and the block
animations (`SpawnStatueAnimations`). `Object` alone stays the game's own: the room's objects, which its object system
places, sets up and runs (`RoomObject`, `BlueDoorCapObject`, `DeleteObject`), and an object in plain English
(`SpaceObjects`, `SpawnedObjectOwner`).

A name that says `Controller` with a port number names the console's controller port and is reported too (#2079,
`CoreVocabularyAnalyzer.ControllerPort`), in any case (`_controller1`, `PreviousController1Input`, `controller2`). The
cartridge read the player's actions from its first controller port. Core names the player's actions (`_playerActions`,
`LastSeenPlayerActions`). `Controller` alone or before another word stays the game's own: a controller, a controller's
input. A name that says `Bts` names the level data's block type secondary, the byte a block's behaviour was stored in,
and is reported as a hardware word (#2079); Core names the block's behaviour (`behavior`).

SME6454 also reads the original's numbering of the game (#1769). In every type it reports a numbered routine or variable
label (`Func126`, `Func139–142`, `var4`). Outside hardware owners it reports a game state, load station or area by
number (`game state eight`, `station 18`, `area six`, `area-zero`); a pose by the original's number, in hexadecimal or as
a word (`pose A/B`, `pose two`, `A5 -> 02`); a sound queue named for its limit (`the max-fifteen sound queue`, `at most
six queued`, `a six-entry admission limit`); and a sprite part's stored placement and colour data (`palette0800`,
`X-10h`, `one part, at X=508`, `masks X to nine bits`). Core names the game state, the load station (the gunship, the
Zebes arrival), the area (Crateria, Ceres), the pose (`FacingLeftNormalPose`), the request's `SoundRequestPriority` and
the picture's place in pixels. A number that counts the noun after it (`a station two rooms away`, `the pose two frames
later`), an area number past the game's eight areas, a family's own animation frames by decimal ordinal (`Pose 3 of the
wave trail`) and a position by X or Y that no part is placed by stay English.

SME6454 also reads the original's routine slots for Samus (#1794). In every type it reports the timer/hack handler the
original called after her movement (`timer/hack handler`, `movement/hack handlers`, `hack handling`, `the timer hack`)
and `gamma`, the name a reading of the original gave that slot's routines (`grab gamma`). Core names who moves her, a
`SamusMovementOwner` that each owner takes by naming itself (`SamusMovementHandOver`), and the check that runs after her
movement (Draygon's escape, the drained get-up); a colour's gamma correction, curve or value and another hack stay
English. Outside hardware owners it reports a compound name a comment sets as code (`<c>MoveSamusWithDraygon</c>`,
`<c>Power.UpRight</c>`) even where a Core member still spells it: Core cites its own code with `see cref`, `paramref` or
`typeparamref`, which the compiler resolves, so a name set as code is the original's routine label. An expression set as
code (`<c>InitialAngle - 32</c>`), a quoted string such as an installed key (`<c>"Power.UpRight"</c>`) and a one-word
name stay legal.

Outside hardware owners SME6454 also reads the machine's display transfers, staging and video standard and the original's
tables and records (#1797): a frame, artwork, graphics, sprite, palette, picture, layer, display, queued or general
transfer and a transfer queue, buffer or slot; a staging buffer, a foreground or scroll staging, staging the foreground
and a scroll or position staged; `NTSC` and `PAL`, read case-sensitively; the common room elements; a selector table or
row; and a record table, an input-table record, an input record and a pose record. Core publishes whole frames, places
its views where they stand, names the release (the Japanese and North American release, the European release), names
the door, item and block pictures every room shares, and names the choice, the entry, the input transition or the pose.
The reserve tanks' transfer, transferring a value, a stage of an animation, a staged fight, a staging point, a common
room, a selector alone and a record of something stay English. The grapple rope's installed frames name their mirroring
(`Rope.Diagonal.2.MirroredVertically`), not the flip bits' value.

Outside hardware owners SME6454 also reads the original's indirect dispatch, the machine's bus, regionalized values and
the original's enemy entries named as records (#1812): "indirect" before a jump, target, entry, call, table, selection or
dispatch (`its indirect jump`, `all four indirect targets`, `the indirect main-AI entry`, `its indirect state table`); a
bus (`the room bus`); `regionalized` in any word form; and a record that is an enemy's entry in the room's enemy list: a
population, spawn, definition or boss record, a part's record (body, wing, arms, head, brain, tongue, tube, statue,
graphics, cosmetic), a parent, follower, partner, physical or composite record, a record by its place (`the second
record`), an empty, emptied, deleted or cleared record and an actor of so many records (`the three-record Evir actor`).
Core names the action, phase or movement that runs, the room binding that supplies what an enemy calls, the release whose
value differs, and the enemy, its part, its partner or its place in the room's list. Indirect G-Mode, the verb (`the
death records the drop origin`), a record of something, an enemy's record and a record alone stay English.

SME6454 also reads the original's routine handlers in every type (#1828), spaced or hyphenated: a handler call (`the
extra sound-handler call`, `two handler calls`) and a music, sound or sound-effect, colour-effect, timer, sprite or state
handler (`the music handler's fetch`, `the colour effect handler`, `Samus's timer handler`, `the generic sprite handler`,
`separately run state handlers`). Core names the queue's pass (`MusicQueuePasses`, `SoundQueuePasses`, a music-queue
pass), whether the effects run (`ColourEffectsRun`, `StopColourEffects`), the countdown's update (`RunEscapeTimer`), the
actor's frame or Samus's input or movement phase, and a control lock by what it stops (`LockOutPlayer`, `LockAtStation`).
A handler alone or of another kind (an exception or event handler), calling a handler and a call alone stay English.

#1855 widens the routine handlers, still read in every type, spaced or hyphenated: a handler named by the word before it
(`the movement handler`, `the room-object handler`, `the unable-to-stand handler`, `its own AI handlers`, a
`sub-handler`), a handler pair and a dispatch point (`the ordinary locked handler pair`, `the palette update's dispatch
point`). The original ran Samus's input and movement, each enemy's AI, the room objects and the HUD through routines it
installed in slots and called; Core names who moves her (`SamusMovementOwner`), the input mode (`SamusInputMode`), the
phase, the check (`CheckDraygonEscape`) or the step that runs (`RunRoomObjectsForDoor`), and what took effect
(`ItemSelectionTookEffect`). A handler alone or after an article, a determiner, a pronoun, a number or a possessive (`each
handler`, `its handlers`, `two handlers`, `the grapple beam's handler`), an exception or event handler, a point and a
dispatch alone stay English.

Outside hardware owners SME6454 also reads the original's numbering of Samus's movement (#1856), spaced or hyphenated,
in digits or words: a movement type (`movement type zero`), a mode (`acceleration modes one and two`, `mode zero`), a
collision result (`collision result one`) and a status (`the elevator's status from one to two`, `status 0`) by number.
Core cites the named member with `see cref` (`SamusMovementType`, `SamusHorizontalCollisionResult`), and an exception
prints the member's name. A number that counts the noun after it (`a mode two frames long`) and a mode, type, result or
status alone stay English.

Outside hardware owners SME6454 also reads the original's trigonometry tables (#1853), spaced or hyphenated: an angle
counted in the table's units, angles or steps (`eight table units`, `0 through 63 table angles`, `an eighth of a table
step`) and a sine, cosine or trigonometry table (`the shared sine table`, `a sine-table angle`, `the negative-cosine
table`, `the trigonometry tables`). Core counts an angle in steps, 256ths of a turn (`TrigonometryAngle`,
`RotationAngle`, `StepsIntoHalfTurn`, `ToTrigonometryAngle`), and computes the wave (`CoarseHalfWave`). The angle's
encoding owner names the encoding it owns, and a compatibility service may describe the table its reproduced behaviour
reads. A sine, a cosine, a table and a unit alone, a turn's steps and a speed table stay English.

Outside hardware owners SME6454 also reads the palette (#1892), in any word form and outside markup: a palette, palettes,
a palette's colours, a palette-cycle glow. The original coloured its pictures from palette rows and animated colours by
rewriting them; Core shows pictures in their shades, owns colours and colour effects (a glow, a flash, a fade, a colour
step) and names each for what it shows. A type named for a palette is cited with `see cref`, whose markup is not read;
a run-on symbol (`palette_change_num`) or a stored value (`palette0800`) is reported by its own rule.

Outside hardware owners SME6454 also reads the original animation interpreter's vocabulary (#1893), outside markup: its
sleep in any word form (`the script's sleep`, `a sleeping animation`, `sleeps on its frame`, `asleep`), its `bytecode`
and its streams (`the head stream`, `a cue stream`, `a delay stream`). Core's animations hold their last frame, rest or
are hidden (`CrocomireTongueSegment.Hidden`), a dormant enemy is dormant, and a cue or frame says what it does. An event,
audio, sound, file, data, level, random, input, output or memory stream and streaming in, into, through, from or out stay
English.

SME6454 also reads the review trail in every type (#1971, #1998, #2018): a review, check, verification, audit or
investigation cited by its ticket (`Reviewed for #1165`, `#1165 independently confirms`, `the #1165 research`,
`Issue #1039:`), anything independently reviewed, checked, verified or confirmed, a pair of ticket citations
(`#625 / #643`), an investigation in any word form, a proof, data pinned for reference (`the pinned retail data`, `the
pinned cartridge's sine words`), a release pinned by its version (`v1.0`) and a commit hash (`db3aa8b3bb`). How a value
was reviewed and which commit fixed it are the repository's history, not the game's. A ticket cited for the change it
names (`(#1989)`), a thing pinned in the game (Samus pinned by the beam) and a decimal stay English.

Outside hardware owners it reads the original's numbering of its tables, entries, slots, parameters and programs
(#1971): a table or list by its count of entries or rows (`the 16-entry shot-block table`, `the eight-entry list`, `a
two-row table`), an entry by number (`entry 23`, `entries 40..43`), a pool by its count of slots (`the eighteen-slot
pool`, `all eighteen projectile slots`), a slot or a parameter by number (`random slots 0 and 3`, `animation parameter
nine`) and a program (`a Torizo program`, `animation programs`). Core names the choice, the step, the place or the
animation. A number that counts the noun after it, a save slot by number, the rows of a screen, menu or panel (`the
two-row Yes/No prompt`), a slot or entry alone and a program joined into a name or key (`sound-program-heading`) stay
English. It reads the original's call structure (#1998, #2018): a caller in any form (`the caller reads first`,
`caller-owned`), a pace per call (`4.5 pixels per call`) and dispatch in any word form (`Boulder's main AI dispatches`,
`Dispatches the body's main behaviours`, `before dispatch`, `redispatch`), so a dispatch alone is read now; Core says
what runs, when and how often in the game (per update, per frame) and who owns a value. Sending a request, a sound, music,
a message, an event or audio (`the queue dispatches the request`, `audio dispatch`) stays English. It reads the sound
chip's voices and key-off (#2018): a voice, the noun (`a voice queue`, `the looping voice`) and a key-off (`a key-off
release`); Core names the sound library, the looping sound and the clip's fade, and the verb before its object (`the
particle voices the attack's sound`) stays English. In comments it reads a word run on to a number (#2018: `counter16`,
`hold3`, `contains100`), a garbled numeral or a label of the original; a file extension (`.ps1`) and a name Core's code
spells stay legal.

Outside hardware owners SME6454 also reads the original program's structure and machine arithmetic (#2080), spaced or
hyphenated: a parameter table (`calls through its parameter table`), a tail call in any word form (`tail-calls the
initializer`), a short multiply or multiplication, an extension array (`the enemy extension arrays`), a rotting table
(`the corpse's rotting table`) and a value modulo 256 (`Y modulo 256 reaches 240`). Core says what runs next and names
the circle product, the species state, the rot schedule, the sample's lowest 256-value field or the place within a
screen. It reads the display's colour math and screen designation (#2089): a fixed colour in either spelling (`the
fixed-colour flash`), the main-screen designation `TM`, read case-sensitively, and a per-line effect (`the update's
per-line effects`); Core names the flash, the wash, the endpoint colour, the page shown or the effect itself. It reads a
type by number, hyphenated (`the ordinary type-6 fall`, #2089), and, in every type, a review cited under its ticket with
or without its number sign (`reviewed under 1165`). A table of settings, Ridley's tail whip, a multiplication, a rotting
corpse, a value modulo another number, a fixed set of colours, a per-line scroll or window and a type alone stay English.

#2105 widens the review trail, read in every type: the review inventory (`see the review inventory's inputs`) and fix
history, an earlier implementation choice told by what it broke (`Hardcoding the early-game branch stranded the actor`,
`Omitting this queue made the pad flash`, `Treating the station's room as sufficient discarded the view`). Outside
hardware owners it also reads a check of the repository cited by its kind or its name: a lookup, verification,
regression, oracle or parity check, test or suite, a full-movie or movie check (`the Tourian animation lookup check`,
`the eye-fade verification checks every shade`, `the Ridley full-movie check`) and a check's name set as code
(`<c>VerifyRunningCadence</c>`, `<c>BeamCallbackTablesVerification</c>`). Core says what the game does; a compatibility
service may name the check that confirms the behaviour it reproduces. A check the game makes (`the awakening facing
check`), the condition a replay tests, treating a value as another with no breakage told, an inventory and a frozen
enemy stay English.

Outside hardware owners SME6454 also reads a controller button's letter used as the name of a gameplay action (#2113),
the letters read case-sensitively: a letter that is an action (`A is Jump`, `X here is the configured Shoot action`, `X is
the default shoot binding`), an action that is a letter (`Jump is A`), a letter before an action's button, binding or
action (`the B run button`) and a letter in brackets after an action (`Shoot (X)`). Core reads typed gameplay actions,
and which button the player binds to each is the host's, so Core names the action (Shoot, Jump, Dash). A coordinate
(`X is the sample`), an article (`A binding names a frame`), X-ray and a letter alone stay English.

#2133 widens the review trail to relocation history, read in every type, hardware owners included, outside markup: a
ticket followed by what a thing was or where it moved (`(#1620, was <c>Old.Name</c>)`, `(#1620, was in Bosses,
unmarked)`, `(#1416: moved from the deleted catalog)`), a parenthetical opening with what a thing was, formerly was or
where it moved (`(was in Bosses)`, `(moved from Samus's movement, #1621)`), and where a thing moved from when its clause
names no destination (`moved here from the ending catalog`). Which member a summary's subject was before a ticket
renamed or moved it, and which module it lived in, are the repository's history. A thing that moved from one state or
place to another (`the elevator moved from its arrival to its return`), a ticket cited for the change it names and a
parenthetical of what a thing is stay English.

#2123 adds to the review trail, read in every type, anything verified (`the verified animations`, `is not verified`,
`verified against the captured frames`): Core states what the game does, and a check the game or the installation makes
is checked. Outside hardware owners SME6454 also reads the desktop in any form (`the desktop runtime`, `a desktop-held
button`), the original's assembly loops, branches, labels, versions and implementation and an ascending or descending
assembly (`the ascending assembly loop`), a value stored as a number for a meaning (`stores the direction as zero for
left`) and a literal number (`any value other than literal four`, `the literal 190/66/2 branches`); Core names the
meaning (the direction is left or right, a pose faces left). Assembling a value from its parts, Wrecked Ship's Assembly
Line, a count of what is stored (`stores these as four separate flags`), a literal alone and the unverified stay
English.

#2139 widens the review trail, read in every type: anything reviewed or approved (`the reviewed wording`, `the
reviewed 1e-19 margin`, `the six approved launch bodies`) and the port's history told against its present (`replacing
it with defaults used to hide bad data; the loader now gets the bindings`, `the update used to be gated`). Outside
hardware owners it also reads the port's narration of itself and of the original: managed code set against the original
(`the managed menu`), the original's steps, programs and code (`an otherwise unused original step`), backing up and
restoring graphics (`backing up gameplay graphics`), and a story event named by a number or as a bit (`the event number`,
`eventNumber`, `story-event bits`). Core names the wording, the margin and the choice, says what the game does now, and
names each `StoryEvent` and whether it has happened. A thing used to do something with no present told against it (`the
counter used to pace the dust`), original alone, restoring a value, graphics alone, a story event, a number and a bit
stay English.

Outside hardware owners SME6454 also reads, outside markup (#2152), a slot as an enemy's, a projectile's or a bomb's
place, alone or named for what holds it (`Which Kraid occupant a slot holds`, `occupies its slot`, `the brain slot`, `the
projectile slots`, `a bomb slot`, `the highest-free-slot search`): Core names the enemy's place in the room's list, the
projectile's or the bomb's place in its pool, or the enemy or projectile itself. The game's save slots stay its own: a
slot by its letter (`slot C`, `slots A and B`), a slot in a clause that names a save or a file (`a save slot`,
`selecting an empty slot starts a new file`) and every slot in a type named for saves or files (`SaveSlotStore`,
`FileSelectMenuState`). It reads a frame named as a map, after what the frame shows or a count of frames (`the one-frame
hitbox map`, `the direction-specific frozen map`, `extended body maps`, `the four-map spin`); the map the player reads, a
map station's access arm and the verb (`every pose maps to it`) stay English. It reads a subtraction's borrow in any word
form (`the subtract/borrow formulation`, `the game's borrow chain`, `borrows from the frame`, `borrowing one whole
pixel`); Core says what the value did, and borrowing a thing the clause names, ammunition, a round, a unit, a fuse or a
path (`a borrowed ammo unit`, `the Chainsaw beam's borrowed fuse`), stays English. And it reads the repository's checks:
verification and a verifier (`verification's catalog relates each member`, `the reusable head verifier`), a fixture (`a
synthetic room fixture`) and a movie (`the recorded Ridley movie`). An assembly's name (`SuperMetroid.Verification`), a
demo's recording and a hardware owner's cited recording stay as they are. It also reads the original's tables, indexes
and bugs: a retail or original bug (`that retail bug`, `its original wall bug`), a retail order (`retail-ordered
places`), a random, angle, place or linear-speed table, a table's convention, entries, samples or selections
(`pose-table entries`, `a water/lava table selection`), identical entries, a collision or same index, the alpha pass,
definition bits and the typed representation. Core names the places a room lists, the random choices, the enemy places,
the speeds and the frame shown; a table alone, a speed table, an index alone and a bug that is not the original's stay
English.

Outside hardware owners SME6454 also reads, outside markup (#2172), the machine's 16.16 arithmetic named by its
sixty-five-thousandths: a 1/65536-pixel or 1/65536th addition, add, sum, helper, form, space, constant or arithmetic, with
up to two words between (`the game's 1/65536-pixel additions`, `one 1/65536-pixel add`, `the 1/65536-pixel collision
helper`, `in signed 1/65536-pixel form`); paired additions, adds, subtractions or sums, spaced or hyphenated (`the game's
paired additions`); and a routine run or called as a helper or controller (`Runs the linked-segment placement helper`,
`runs the last segment's collision/reversal controller`, `calls the previous-pose helper`). Core says what the value or
the step does: the speed moves the position, a fraction past a whole pixel carries into the whole pixels, the segment is
placed on its orbit, the chain reverses. A unit of measure (`a speed in 1/65536 pixels`, `a 1/65536-pixel velocity`),
things the game pairs, an addition alone, the player's controller with its input, buttons or port, and a helper or
controller nothing runs or calls stay English.

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
