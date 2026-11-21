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
names the response it selects (`GrappleEnemyReaction`, `EnemyHurtResponse` with its `Inert` member), and a species
exposes its lifecycle on its entity (`IEnemyInitialization.Initialize`, `IEnemyAi.RunMainAi`) instead of handing
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

A name that says `Retail` names the released cartridge and is reported too (#2322,
`CoreVocabularyAnalyzer.ReleaseWords`), matched as a whole word without regard to case (`IsRetail`,
`RequireRetailPermutation`, `RetailAreas`, a `retail` local): it named the game by the cartridge it was sold on, set
against a build or a program that is not it. Core is that game and names what the thing is (`CanBeEquipped`,
`RequireUniquePermutation`, `RoomDefinitions.All`). Words that only share its letters (`Retailer`, `Detail`) are the
game's own.

A name that says `Ntsc` or `Pal` names the video standard and is reported too (#1848,
`CoreVocabularyAnalyzer.VideoStandardWords`), matched as a whole word case-sensitively in the casings `Ntsc`, `NTSC`,
`ntsc`, `Pal` and `PAL` (`IsPal`, `MetareeNtscSpeed`, `PALTiming`, an `ntsc` local). Core holds the one game's values and
names no release (#2472), as SME6454 asks of prose. A lower-case `pal`
(English, or a palette's abbreviation) is not read, and whole-word matching keeps `Palette`, `Pale`, `Palm` and `Opal`.

The original's table positions and enemy record are reported too (#1765, `CoreVocabularyAnalyzer.TablePositionPhrases`),
matched like the hardware phrases: `MenuIndex`, the number the original gave a menu's step; `TableIndex`, a value's
place in one of its lookup tables (a `tableIndex` local, a `tableIndexDelta` parameter); and `EnemyRecord`, the
fixed-size record it kept each enemy in. Core names the menu's step, a speed curve's place (`curvePlace`,
`curvePlaceStep`), an animation's step (`wingStep`) or the enemy itself. Each word alone stays the game's own. SME6454
reads the same three in prose outside hardware owners, spaced or hyphenated (`menu index 13`, a `facing-table index`,
`table indices`, the `enemy records`); a table indexed by a value, the menu's index and an enemy's record are English,
and a compatibility service may describe the enemy record its reproduced behaviour reads.

The original's failed table lookup is reported too (#2525, `CoreVocabularyAnalyzer.TableLookupPhrases`), matched like
the hardware phrases: `LookupFailure`, the cartridge's search of a pose's input table that found no entry for Samus's
input, after which it ran a setup and a momentum response (a `SamusLookupFailureSetup` enum, a `LookupFailureResponse`
member, a `ForLookupFailure` method). Core names what happens: her input chooses no new pose (`SamusNoTransitionSetup`,
`SamusNoTransitionPose`, `ForNoTransition`). A lookup and a failure alone or apart stay the game's own.

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
its views where they stand, plays the one game whose values it holds (no release is named, #2472), names
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
placed on its orbit, the chain reverses. A unit of measure (`a speed in 1/65536 pixels`; #2427 reads the hyphenated form and #2546 this one),
things the game pairs, an addition alone, the player's controller with its input, buttons or port, and a helper or
controller nothing runs or calls stay English.

#2192 widens the review trail, read in every type, to fix history told by an adverb of error (`Testing it here
incorrectly advanced the animation`, `wrongly skipped the run check`), a fall-through (`Falling through ran Samus
movement`), an earlier order (`publishing first made the minimum read two`) and what an alternative would strand
(`omitting it would strand Metroid`), and to debug narration: a gameplay path told as a diagnostic's (`the old diagnostic
seam`, `direct diagnostic loads`, `its diagnostic pre-impact magnitude`, `the room diagnostic`, `direct/diagnostic step
use`), debug narration of the repository's tooling (`fresh debug sessions`, `debug scripts`, `debug tools`, `a non-debug
build`, constructing a debug room; the owner's ruling under #2248 and #2265 narrowed it: the original game's own developer
debug mode, its debug flag, its debug-only save icons and an area change with debug enabled, is the game's and stays
English) and the port's C# construction (`the C# room constructor`). Core states what the game does and gives every path it holds a gameplay
reason. A thing the game shows wrong, Samus falling through a platform, an order alone, a stranded X-ray beam, a name,
text or identity for diagnostics and C# alone stay English. Outside hardware owners SME6454 also reads the Japanese and
North American release named for what the game does (`The Japanese and North American release measures angles in 256ths
of a turn`), and since #2472 the European release and a Japan/USA region too, with or without the other release beside
it; a speed row
(`the population's linear speed row`); a variable label cited in backticks (`` `ranActorAi` ``); a sort order (`a normal
bomb's family sorts after the Power Bomb family`); a scroll table (`resets the background scroll table`); colours loaded
into a row (`which his fight loads into the projectiles' row`); a state, boss, encounter or death extension, extended
state and extra values (`the shared state extension`, `with its extra values`); and a literal write, store or assignment
(`these radii are literal writes`). Core names the speed, the value, the reaction, the distortion, the colours shown and
the state for what it holds. Two releases set side by side, an expression or member in backticks, a sorted list, a
per-line scroll, loading a room, a block's, neck's or tail's extension and a literal alone stay English.

Outside hardware owners SME6454 also reads (#2222) the original's cinematic objects as a background object, spaced or
hyphenated (`five cinematic background-object updates`); a palette's slices as the ranges they colour, outside markup (`text/portrait
ranges`, `gameplay and projectile ranges`, `colour ranges`); a colour write, overwriting colours and a fade's component
update or step (`the colour writes`, `overwrites the two glyph colours`, `thirty-two component steps`); a script's step
as the original's record (`character records`, `the katakana caption record`, `a start-delay record`); the dropped
drawing order (`the priority tiers that once interleaved them`, `a presentation quirk the model does not keep`); an
opcode mnemonic set as code in markup, read case-sensitively (`<c>INC</c> permits more than one ring`); a register
reloaded (`does not reload Y`); a field of the C reimplementation outside markup (`the Baby place's Enemy.health`); a
packed value (`the packed X and Y collision radii`, `a packed decimal pair`); a disassembly label set as a summary and
the original's Japanese text label (`Enemy projectile animation: Mother brains drool`, `time Bomb set japan text`); an
encounter's event, form, place or region by a number in digits (`body form to 4`, `scroll region 1`, `place 17`) and a
corpse, combat, final-combat, body or story event, form or state by a number in digits or words (`corpse state one`,
`final-combat state 2`); and
the retail population or initializer (`Mother Brain's retail population`, `retail part init`). Core names the actor, the
fade, the colours shown, the step, the drawing order, what a count does, the value, the animation, the story event, the
form, the scroll or the place, and the game's room populations. A background alone, a range of rows or a shot's range,
writing a picture's colours into a buffer, a record of something, interleaving alone, a capitalised name that is no
mnemonic, reloading a timer or a weapon, a file's name, a pack of enemies or a backend's packing, an enemy's animation,
a number that counts the noun after it or follows a verb (`places two pickups`), a room's or the beam's state by
number, a stage by number (the game's own Speed Booster stages and the stages its pictures show) and a retail cartridge
stay English.

#2248 and #2265 widen the review trail, read in every type, to fix history told inside a clause (`leaving this
assignment out produces a silent attack`, `reducing those branches to three/two loses one point`, `reading only the
uncharged row made Wave Shield disappear`). Outside hardware owners SME6454 also reads the original's enemies behind an
encounter's parts told as the physical ones (`the physical projectile pool`, `the physical Baby`, `tube enemy 1`); the
original's routines named as walkers and helpers, outside markup (`the extended-hitbox walker`, `the collision walkers`,
`the damage helper`, `the standing helper`, `the helper's hard floor`); bit masks, negative branches and unsigned values
(`RNG&7`, `masked RNG`, `branches negative`, `wraps negative`, `unsigned here`, `the speed, read unsigned`); the
original's program flow, outside markup (`the code installs the typing step`, `installs the wait phase`, `installs its
moving frame behaviour`, `the jump into the common deletion animation`, `the fallthrough`, `falls through into its first
step`, `falls directly into its successor`); data told as engine-owned, a default or combination row, colour zero and a
numbered or actor parameter (`engine-owned hitboxes`, `the default row`, `transparent colour zero`, `the actor
parameter's flag`, `its first parameter`); a literal write with up to two words between (`literal inventory writes`);
and a pace per any call (`one setup stage per AI call`). Core names the part, the collision test, the check, what the
choice does, that a count runs out, what the game does next and in which update, the hitboxes the game defines and the
colour no picture draws. Physical contact or a physical component, an enemy that walks (`a floor walker`), a helper
alone or a screen's helper text, an and between words, an unsigned value alone, installed content, Samus jumping to a
ledge, anything falling through into a room, a floor or the water, an engine alone, a row of blocks, a method's
parameter, a literal alone and a call alone stay English.

#2271 widens SME6454, outside hardware owners, to the machine's storage told for a value: a value shifted right or left
by a number of places (`health shifted right one`, `each axis shifted left eight bits`, `a right-shift`); state cleared
as the room loads (`clears on room load`, `cleared as the room's enemies load`, `the room's enemy setup clears`); a
table's physical order; both halves of one random number; a value kept in another quantity (`the game kept it in the
projectile's horizontal velocity`); and a vector or fixed-point helper (`the signed fixed-point vector helper`). Core
says what the value means: half his starting health, 256 pixels a screen, a trail that starts at the room's origin, the
frames in the order they are named, the places a random number chooses, the segment's phase. A shift by a counted noun
(`the camera shifts left two pixels`), a room that clears when Samus leaves, physical contact, the halves of a picture
and a value kept for an update stay English.

#2292 widens SME6454, outside hardware owners, to the original's routines and storage told in Samus, enemy, boss, World
and Runtime prose: a wrapper (`the turning wrapper`), a helper after any word but an article (`the rising helper`, `this
helper`), a scripted, drained, grabbed, tail, pogo, flight, fake-wall, formation or movement controller and an owner's
controllers (`Mother Brain's controllers`); a merge path; an equipment, state, suit, item, beam, facing or liquid bit and
an item's own bit (`the Screw Attack bit`, `both bits are equipped`, `this bit`); a stage by number outside the Speed
Booster's (`stage three equips the suit`, `the third-stage handoff`); updates counted as calls (`sixteen calls`, `the
nine-call cadence`, `one call of the destruction`, `calls left`, `call order`), in prose and in declared names
(`FlashCalls`, `ImpactSoundCall`, `DelayCallsPerRow`); a value compared, read or treated as signed; a deferred write and a
suit's colour copy; odd frames that return; hardcoded data; an offset written with a plus sign (`destination+31`, `the +31
allowance`, `at +0`); a count of bits (`wrapping at sixteen bits`, `a three-bit jitter`); position, policy, launch and
search rows, records and tables (`the 352 row`, `in search order`); arithmetic written as a summary (`the left edge (-24)
minus two`, `256 + 108 * 96 = 10624`); the retail game in any use; and a cursor through the original's lists (`the history
cursor`, `advances the cursor`). Core says what the game does: the step and what it does, the item equipped, the
suit's reveal, the updates a wait lasts, which way a distance lies, the room's wrap, the destination Kraid walks to, the
game itself. A helper alone, the helper text, the player's controller and its input, a scripted controller's input,
Shaktool's room controller, a bit more, the Speed Booster's stages, a call to another owner, one call per room, an input
built for one call, a signed value, a sum written with spaces, a signed list of deltas and a menu's cursor stay English.

#2321 closes the forms that escaped #2292's calls and storage: a call picked out with up to four words before it,
hyphenated or joined by a slash (`the next body AI call`, `the first post-hit enemy call`, `another beam/movement call`);
a `same-call` step; rot calls; the call as the update's own work (`true on the call after its last image`, `the image the
call shows`, `the call that takes it to zero`, `the call's result`); sound-effect and vulnerability flags or bits (`the
shared SFX flag`, `the vulnerability bit`); and a counter told by the range its storage holds (`the frame counter of 0
through 65535`). Core counts updates, says whether a sound plays or a shot is blocked, and names the updates a cadence
picks. The call alone and a call that returns to its caller stay English.

#2336 widens SME6454, outside hardware owners, to the original's routines and storage told in boss and enemy prose: an
initializer (`the shared initializer`, `the spin-jump initializer`); a writer (`the common writer emits his body`); an
execution counter, in prose and in declared names (`MainEnemyExecutionCounter`); ordinary projectile places and places
scanned in ascending order; a value wrapped into the coordinate range; a branch on a value (`branches on area index`);
a signed compare, comparison or branch; and the roaring or damaged flag. Core says what the game does: the enemy's,
projectile's or pose's setup, the common enemy drawing, the update count, Samus's shot places, a position off the
room's edge, what a reaction depends on, which way a distance lies, the roar ending. A C# construct's initializer (an
object, collection, field, property, array, static or module initializer), a projectile's branch and setup stay
English.

#2345 widens SME6454, outside hardware owners, to more of the original's arithmetic, program structure and storage: a
value told as divided by, modulo or the remainder of a number (`the sample divided by 256, modulo 4`, `the random number
modulo 64`, `the result's remainder by eight`); an if/else or else-if chain, a call chain, a leading call, a call the
code does not or never makes, a no-op and a routine's prologue (`the bug's update is an if/else-if chain`, `custom shot
AI does not call normal shot AI`, `the prologue requires its parent`); and storage told as left over or never read
(`nothing a previous Kzan left reaches a new one`, `which nothing reads`). Core says which of its choices a random number
makes (`one chance in four`, `taken for half the random numbers`) and what the game does. Dividing a picture, an
instruction not to call a method, Ceres, the prologue station, values nothing reads because they are identities, and what
a previous room hands the next stay English.


#2352 widens SME6454, outside hardware owners, to the machine's storage and the original's code told in projectile and
Samus prose: a position, velocity, speed or fraction overflow (`1/65536-pixel position overflow`); a range named by its
1/N-pixel storage (`far inside the 1/256-pixel range`); a scan's, target, collision or solid counter (`clears both scan
counters`); state that lies below what a clear reaches; an allocation that clears or installs and routes that allocate
(`Allocation clears the step`, `the collision routes allocate the trigger`); a check that runs to confirm a value (`the
radius check runs every update ... to confirm it`); and the original's code branches: this, that, both or every branch,
a branch of a step or calculation, a branch taken, entered, selected or run (`takes the dud branch`), a branch that
returns, writes, restores or moves something, a branch order and branching straight to something. Core says what the
game does in that case: a hit with no damage is a dud, the camera scrolls slowly, a block stops the shot, the effect
starts at its first shade. A room's state choice's branches, a projectile's or an afterburn's branches, an animation that
branches to another, a sum that overflows and a range in pixels stay English.

#2362 widens SME6454, outside hardware owners, to the original's calls, storage and program as frontend, effect, HUD,
escape, progression, input, save and animation prose tells them: updates counted as calls (`Calls elapsed`, `calls made
so far`, `later calls`, `the fourth, eighth, ... calls`, `the final character's call`, `the separate call that
completes the fade`, `calls the same update`, `an animation's call saves where it resumes`); a screen value's `0 through
255` told with its wrap, `the game's byte` or `wrapping counter` and a count that `would have wrapped`; the original's
`object handling` and `free object places`, `init parameters`, a `descending actor order`, an actor `being processed`, a
named storage bit (`the process-off-screen bit`), the `saved order` and a `projectile-backed` or `parameter-selected`
thing; and a `coroutine`, a handler `chosen` or `of the` category, a scan `backward`, a
`checksum`, an `emulator`, what `the original reads`, a match with `the game's table` and `engine definitions`. Core
counts updates and says what the game does: the subtitle runs on the same update, the latest button in the list wins, a
save reads back whole, the room's area decides. A call to something, a call that returns to its caller, the verb after a
subject (`the producer first calls the setup`), a range alone, a room object's place, a bit more and a handler alone stay
English.

#2372 widens SME6454, outside hardware owners, to the machine's coordinate arithmetic, storage and calls as enemy prose
tells them: a wrapped, wraparound or modular distance, difference, subtraction, displacement, position, proximity or
comparison, with up to three words between (`strict wrapped horizontal distance`, `the wrapped signed difference`, `the
modular absolute difference`, `modular Y proximity`); a coordinate's wraparound (`with room-coordinate wraparound`); a
move `wrapping within the coordinate range`; installing or loading art (`installs crawling art`, `installs hop art for
its heading`); the game storing, checking or subtracting a value (`the game subtracts one mark`); a `completion call` or
`draining calls`; `table multipliers`; and a value `of 0 through 255`. Core says how far apart two things lie and which
way, citing the compatibility service that reproduces the wrap (`DistanceAcrossRoomEdge`, SME6463); which animation starts; what the game does
with the value and on which update. A thing wrapped around, a turn's wrap, installed art and loading installed art, a
table, a multiplier and a range alone stay English.

#2377 widens SME6454, outside hardware owners, to the original's storage and program as enemy, boss and world prose tell
them: an enemy's presence, contact or processing told as a property, alone or in a list (`the intangibility property`,
`the animating, intangibility and invisibility properties`, `both of his contact properties`, `the plasma-blocking enemy
property`, `the hide-drawing property`); a `post-decrement` or `pre-increment`; a `main-AI call` or an `AI invocation`;
running the code's loop (`Runs the projectile-link loop`); `the handler's` guard; a `signed wrapped` comparison; and a
`strict modular` range. Core says what the enemy does (it is hidden or gone, ignores Samus, stops the Plasma Beam,
animates, keeps updating off screen), which update a count runs out on, what a step places and how far apart two things
lie. A property of a thing, an enemy's other properties, a projectile's property set or mask, a handler alone, an
animation's loop, a decrement and a call alone stay English.

#2386 widens SME6454, outside hardware owners, to the original program's and the port's machinery as world, runtime
and Samus prose tell it: an `allocator` (`the room-object allocator's false result`); a `renderer` other than the host's, the
application's own (`the death-state renderer`); a `streamer`, a thing `streamed` and a background, graphics, viewport, level, layer or
plane streaming or streaming one of those (`background streaming`, `a streaming background plane`); a `saved game
state`; a `matcher` or a `table match`; a thing `translated` (`has no translated movement-type mover`); and
progression, defeat, controller, button or input `bits`. Core says what the game does: the room's free room-object or
enemy-projectile places, what the display draws, the destination scrolling in through the door, the game's saves, the
input transition chosen, the movement a pose runs, the progression recorded, the bosses defeated, the buttons pressed.
Hair or jets streaming out, a thing streamed out, a position translated by an offset or to an origin and a save stay
English.

#2390 widens SME6454, outside hardware owners, to updates counted as the original's calls by number in Samus prose: a
call by its number (`on call two`), calls by number in a list or a range (`calls one, three, and five`, `calls one
through six`) or ending their clause, `odd` or `even calls`, and `super-special`, the name #2113 replaced. Core counts
updates (the flash's first, third and fifth updates, its impact update) and names the interruptions that outrank a
pose. A call that counts the noun after it (`calls two methods`), a call to something and updates counted by their
order stay English.

#2398 widens SME6454, outside hardware owners, to the port's narration of itself and the original's tables as Samus and
world prose tell them: anything `inspectable`; patches told as fixes to the game's code (`without per-input patches`,
`not room-name patches`); a table told as the original's storage (`the game table`, `the game's area/load-station
tables`, `the original's table`, a `retail`, `direction`, `lookup`, `label`, `name` or `colour table`, a `table height`,
`table result` or `table chord`, the `table-selected` poses); and, outside generated text, a repository path (`docs/...`,
`tools/...`, `csharp/...`, a `.md`, `.ps1` or project file). A declared name holding `Translated` or `Untranslated`
(`CaptureTranslatedRenderSnapshot`, `hasTranslatedBodyChange`) is reported too. Core says what the game does: one check
handles every transition, the slope's surface lies above the shot, the shot direction picks the pose, the input
transition matched. A picture's patch (a slot-field patch), a table alone or named for its data (a drop-chance table, a
speed table), an installed file's name and a generated file's header naming its generator stay English.

#2402 widens SME6454, outside hardware owners, to one value told as the original's storage for several meanings, as boss
prose tells it: `shared` or `common storage` and `storage slots`; a value counted as a number of 65536ths (`forty-eight
65536ths`, `544 65536ths`); the game keeping, holding or storing `one value for` several things, things kept `in one
value` (or a second or third), a value read `under another meaning` or `under each other's meaning`; and a step that
`leaves a marker in` a value. Core keeps each meaning on its own and says what the game does with it. A unit of
measure (`one 65536th of a pixel further`; #2546 reads `in pixels and 65536ths`), a value alone and a marker the game shows stay
English.

#2407 widens SME6454, outside hardware owners, to the original's storage and program and the port's reasoning about
them as boss prose tells them: what `the original gave` or `kept` (or gives or keeps); `unreachable` or `unused` frames
or animations told as `not authored`; a value `copied without halving` (or doubling, scaling or conversion); the game,
original or cartridge keeping, having or using `separate phases` (or states, steps or stages); and `fabricating` in any
word form. Core says what the game does: which looks have hitboxes, how far a hit area reaches, what each stage of a walk
does. What the game keeps or gives (`the game keeps colours lit`), a value copied, an unused room and a cue that is not
authored stay English.

#2412 widens SME6454, outside hardware owners, to the original's branches and storage and the port's reasoning about
them as boss prose tells them: the branch before, after or following a comparison (`the game omits the branch after its
ninety-hit comparison`); a choice told as `equivalent` (`selecting the colours here is equivalent`, `is not
equivalent`); the `new-frame flag`, spaced or hyphenated; the coordinate range's far end (`continuing from the far end of
the coordinate range`); colour sets `occupied` (`the two live colour sets occupied by his body`); and a thing `meant to`
do one thing `but reads` (or uses or takes) another (`the egg's setup meant to draw the wait at random but reads a fixed
value`). Core says what the game does: which health bands he shows, what picks his colours, that the clock notes a newly
shown frame, where a place lies (a negative height above the room), what colours show and that every egg waits the
same. A comparison, the equivalent of a thing (or one equivalent to it), a new frame, the far end of a room, a
set of colours and a thing meant to happen stay English.

#2413 widens SME6454, outside hardware owners, to the original's calls, areas, allocation and layers as room prose tells
them: an `initial call` (`the camera after the setup's initial call`); an area by its number, spaced or hyphenated (`the
developer area-seven room`, `area 7`), unless a unit follows; an `allocation loop`, routine, scan or search, and a thing
`newly allocated` (`the newly allocated room object`); and a background layer or plane by ordinal (`the second
background layer`, `background layer 2`). Core says what the game does: the camera after the setup's first update, the
developers' debug room, a full room losing the spawn, each turret taking the highest free place, the background's
position. A call, an area with a unit (`a hit area eight pixels wide`), an allocation and the background stay English.

#2417 widens SME6454, outside hardware owners, to the original's colour sets, colour groups, scroll and program as
frontend prose tells them: a colour by its place in a set (`its first background colour`, `the cinematic's first sprite
colours`, `the first of the narration's colours`, `the colours whose first is the space`); a colour group drawn in or
shared by two things (`both once drew in one colour group`, `the two share a colour group`, `another actor's colour
group`); a scroll or offset told as inherited (`the scroll both flashbacks inherit`, `the inherited flashback offset`); a
marker told as executing (`the subtitle creation marker has yet to execute`) and the `empty marker`; and a library's or
queue's handler (`each library's handler carries its phase`). Core says what the game does: the colour the opening
shows, each thing's own colours, the flashbacks' scroll, the page's opening pause and its subtitle, each library's
request handshake. A first colour in a sequence (`the flash's first colours`), a colour group, inheriting anything but a
scroll, a marker and a handler stay English.

#2423 widens SME6454, outside hardware owners, to the original's main loop, storage and colour slots as effects, HUD,
map and escape prose tells them: the main loop, spaced or hyphenated (`before the main loop's random number`,
`main-loop random state`), and `MainLoop` as a word of a declared name (`MovesLiquidBeforeMainLoop`); a `multibit`
value; a value's middle part or a half of its fraction (`the probe's middle part`, `the middle of the displacement`,
`the upper half of its fraction`, `the fractional half`); the colours a tint sets (`the last of the three colours a tint
sets`, `the last of the three a tint sets`); a value's bits by count or end (`rain reads two of its bits`, `its top
bits`); and `save bits`. Core says what the game does: the update draws its random number (`HasMovingLiquid`,
`PublishHeldInput`), a binding holds several buttons, a probe is limited in 256ths of a pixel, an untinted look shows
one colour black, the random number chooses one of rain's four winds, nothing is marked explored. The middle of a room,
the upper half of the screen, a tint's colours, a bit and a save stay English.

#2552 widens SME6454, in every type, hardware owners included, to the original's tables, routines, labels and storage
spelled as a word of a declared name: its labels for the Baby Metroid (`Shitroid`), the dust explosion (`MiscDust`), the
Geruta, Holtz, Oum and Squeept (`NorfairRio`, `MaridiaLargeSnail`, `NorfairLavaJumping`) and a crawler's surfaces
(`UpsideRight`, `UpsideLeft`, `UpsideUp`); its routines and their parts (`Routines`, `Prologue`, `Epilogue`, a phase's
head or tail, a respawn, no-drop, frozen or wrapper tail, `InitializationAi`, `ToBeDrawn`, `HudProducer`,
`DecideActionSamus`, `CaptureRenderSnapshot`); its storage (`FlareCounter`, `SharedProjectiles`, `SpeedAccumulator`,
`ElevatorFlags`, `JourneyFlag`, `BossFlags`, `GlobalEventFlags`, `LoadingGameState`, `FxMotion`, `Subpositions`,
`LiquidPhysicsType`, `EchoStorage`); and its branches and shared routines (`OneInFourBranch`, `NormalEnemyTouch`,
`NormalEnemyShot`, `CollisionPrelude`, `InputFallback`, `PseudoDoor`, `MouthControls`, `LowPowerMode`, `TimeBombSet`,
`FirstSecondDamage`, `DecelerationDisallowed`). Core names what each is in the game: the Baby Metroid, a crawler on the
left wall (`OnLeftWall`), the charge's held updates (`UpdatesHeld`), Samus's bombs (`SamusBombs`), an enemy's setup
(`SetUpEnemy`). Installed keys stay as their key tables write them; an upside-down Cacatac stays English.

#2427 widens SME6454, outside hardware owners, to the original's setup and storage as area prose tells them: a setup,
initialization or spawn that selects colours (`the setup selects the enemy projectiles' colours`, `statue initialization
selects the common sprite colours`), and a value named by its hyphenated 1/65536-pixel storage (`Samus's 1/65536-pixel
position`, `a signed 1/65536-pixel velocity`), which #2172 had left as a unit of measure. It also reads the species-owner
template (`The Ceres steam species owner: its per-room state, behaviour and the interactions it registers`) on the
summary of a type that neither is nor implements `IEnemyEntityFactory` or `IEnemySpecies`. Core says what the game
does: pictures arrive pre-coloured, a position or speed carries its fraction of a pixel in its type, and a type that owns
no species says what it is (`The Zebes escape's explosions`, `The falling sparks`). A setup
that selects anything but colours and an entity's parameter naming its species owner stay English.

#2433 widens SME6454, outside hardware owners, to the machine's coordinate wrap and the original's stores, calls and
writes as enemy prose tells them: a comparison across the room's wrap or on either side of it (`the coordinate difference
across the room's wrap`, `on either side of the wrap`); a distance measured as an encoding owner measures it (`measured
across the room as CoordinateAxis.Difference measures it`); `clearing stores` or writes; a thing that issues calls
(`leaving the room issues both calls`); and `overlapping writes` or stores. Core says what the game does: an Atomic
moves towards Samus, Samus lies to the block's left or right, the drain cancels her invincibility and knockback, leaving
queues the departure sound and then the spin-end sound, the release leaves five screens closed; a compatibility service states
the wrap once. A turn's wrap, a thing wrapped across something, clearing a thing, an issue, a call and overlapping things
that are not writes stay English.

#2440 widens SME6454, outside hardware owners, to the original's storage and program as enemy prose tells them: a
subposition (`both subpositions`, `the live Y sub-position`); a handshake in any word form (`the animation-to-AI
handshake`, `the corpse handshake`); an entry point no thing or place owns (`the two entry points`, `this entry point`);
a tail of the program (`the shared post-combat tail`, `the species tail`, `the common tail`); and a value told by its
type's range (`past the type's range`, `far inside its type`). Core says what the game does: only the whole pixels are
restored and the place within the pixel carries over, the animation's last frame lets the enemy rest or fall, the setup
places the platform, the reaction freezes the parts listed after it, a jump's speed stays within a few pixels an update.
A thing's or a place's entry point (`his entry point`), a fraction of a pixel, a body's tail and a
hardware owner's own range stay English.

#2446 widens SME6454, outside hardware owners, to the original's program, storage and tables as Samus prose tells them:
an epilogue in any form (`the common epilogue`, `the transition epilogue`); a tail named by what it ends (`the gameplay
tail`, `the firing tail`, `the long cleanup tail`, `the swinging and stuck tails`, `the death tail`, `the tail of the
routine`); a routine's entry named by its kind (`the normal entry`, `the explicitly named entry`, `the ordinary vertical
entry`, `the nonzero terrain entry`); an enum or flag told as nonzero (`a nonzero direction`, `any nonzero common
reaction`, `the bounce state is nonzero`); a value told by its storage's range or sign (`1/65536ths`, `0 through 32767`,
`held unsigned`, `a signed whole speed`, `signed-negative`, `the unsigned divide`, `the signed halving`); a table or record
named for what the original stored in it (`the origin and flare tables`, `the damage table`, `the post-grapple table`,
`the acceleration records`, `the right-facing records`); pictures that `share one sprite list`; and, read with markup,
poses named as a range of their order (`ShinesparkHorizontalRightPose through ShinesparkDiagonalLeftPose`). Core says
what the game does: the runtime clears the damage mode before every movement step, a fired beam's rope retracts by itself
once it connects, a ball that is bouncing takes the bouncing step, the accepted gap is never negative, the camera speed is
the distance Samus moved either way plus one, a solid spike block deals its own damage, the shinespark poses break bomb
blocks. A body's tail (`Ridley's tail`, `the tail cap`), an entry that is a place or a moment (`the room entry`, `the crash
entry`, `the entry door`), a number that is nonzero (`a nonzero timer`), a
turn that records, a sprite list alone and a hardware owner stay English.

#2461 widens SME6454, outside hardware owners, to the port's narration of its build and its scope and to the original's
animation layout: anything compiled in any word form (`compiled fixed values`, `the import compiles them`, `the
compilation`); anything modelled or modeled but the noun (`a newly modelled landing`, `the modeled shade`, `an unmodeled
reaction`); the port named as itself (`the port's scope`, `every behaviour the port reproduces`); and an animation laid out
against another (`an animation laid out before it`, `laid out after hers`), laid out by the animation that holds it (`as
the plain fall lays it out`) or told by its layout (`identical four-frame layouts`). Core says what the game does: an
animation no pose animates by is reached only by a frame read past another animation's end, the fast fall's first frame
is a place in the fall's animation, the fuse switches a bomb to the same frame of its fast animation, an attack that runs
the game's live state as code is outside the emulation scope limit. Where an animation lies against another is
`FramesBeyondAnimation`'s to describe, as the compatibility service that reproduces it. A model (`the display model`), a
body's or a controller's port (`the top port's spike`, `port 2`), a layout alone (`a room's scroll layout`), laying a
picture out (`lays out the caption`) and a hardware owner stay English.

#2456 widens SME6454, outside hardware owners, to the original's routines, per-frame code, copies and precision: a
routine's tail named by the reaction, stage or outcome it ends (`the frozen tail`, `the recoil tail`, `the respawn tail`,
`the no-drop tail`, `the deletion tail`, `the Metroid tail`, `a frozen-AI prelude/tail`), pointed at (`this same tail`,
`before that tail`, `a tail that still acts`), owned (`the frozen enemy's own tail`, `no tail of its own`) or of an update
(`the tail of a frozen Metroid's update`); a frame behaviour, the original's per-frame projectile code; common, normal or
ordinary shot, touch, contact or bomb AI, run or not run (`never calls common shot AI`, `instead of normal shot AI`); the
game's copies of a table (`both of the game's copies of the curve`); the longest duration a frame can have; and a
quantity told by the precision it is kept at (`a signed 1/256-pixel velocity`, `1/256-pixel steps`, `a 1/256-turn aim`,
`-1/256 px`, `1/256ths`). Core says what the game does: what a reaction does after the common damage (`a shot deals the
shutter no damage and starts it moving`), how a projectile moves each update, which speeds the curve gives, that a frame
holds for good and how fast a thing moves (`its velocity, in pixels per update`). A body's tail (`Ridley's tail`, `its
tail`, `the tail cap`, `a one-shot tail whip`), a particle's fading tail, a straddled block's lead and tail, main AI, a unit of measure (`a speed in 1/256 pixels`, `a 256th of a pixel`) and a
hardware owner stay English.

#2465 widens SME6454, outside hardware owners, to the original's program and storage and the repository's checks as boss
prose tells them: a frame behaviour, spaced or per-frame (`the shot's fired frame behaviour`, `a Zoa's per-frame
behaviour`); an early return; a thing left `unwritten`; a final or last store or write (`the runner's final store`); a
query or routine that returns zero or nonzero; `execution` (`does not leave execution at the setup`, `cue execution`);
`bookkeeping`; the real pool; the game's mover, routine, handler or subroutine (`as the game's projectile mover does`);
the checks that compare, read or assert something; a wrapped count, counter, value, word, timer or sum; a stored value,
word, byte or count; a respawn, flag, status, enable or done bit;
authored arguments or operands; a direct write of anything (`the Baby Metroid's direct camera X write`, `a direct terrain
write`); and a branch that also, then, first or only does something (`the riding branch also carries Samus`). In every
type, hardware owners included, fix history told inside a clause now also covers rebuilding, recomputing, resetting,
rewriting or replacing something here that doubles, halves, breaks, loses or desyncs something (`rebuilding a clean
rectangle here doubles close-range damage`). Core says what the game does: a shot flies each update, the eye breathes its
bubble whether or not its gaze turns, a deleted part's animation stays where it stood, one roll in 256 whips, the
Baby Metroid moves the camera, a hit leaves the mouth's bounds as it left them. A frame alone and a behaviour alone, a
return to a place, a count that wraps, a value kept, a bit of a thing, an authored path, a direct hit, a branch alone
and a hardware owner, which describes the program it reproduces, stay English.

#2472 widens SME6454, outside hardware owners, to any release named (the Japanese and North American release, the
European release, `the Japan/USA layout`), the two set side by side too (`the Japanese and North American release takes
80; the European release takes 99`): Core plays one game and holds its values. It also reads the original's waits,
screen lines and storage as late-fight boss prose tells them: a delay counted in frames by number (`the ordinary
eight-frame delay`, `a 16-frame delay`); the screen's first lines by number (`the first 256 lines`); a cross-enemy or
cross-actor write; a one-shot flag (`the one-shot cry flag`); indices that reach zero and a raise/lower pair of indices;
the room adapter; the requests other systems fulfil; and a graphics/animation state. Core says how long a wait lasts in
updates, where in the room a thing goes, what one enemy does to another, what a count or a state stands for and how a
neck moves. An animation frame's delay, a first line of text, a one-shot animation, a list's indices, an adapter or a
request alone, a state alone, the Japanese text and a hardware owner stay English.

#2475 widens SME6454, outside hardware owners, to the original's program, labels and storage as enemy prose tells them:
a developer's instruction to keep something, read with its capital (`Preserve that ordering`, `Preserve the start`,
`must preserve the history`); a flag told as a latch (`the swipe-animation latch`, `the wingless latch`, `the sweep and
joint-orientation latches`, `every latch`, an attachment, reaction, behaviour, mouth or reopen latch, a latch set or
cleared, `its latches`); a sprite's `graphics part`; the original's name for Mama Turtle (`the tatori family`); the
original's per-state routines named as their AI (hurt, shot, touch, contact, bomb, frozen or grapple AI, `its custom
hurt AI`); a random number's or seed's reuse; a `beam object`; an angle's units per turn (`in 256 units per turn`); a
count that would go round or a counter that goes round; and anything `fixed-point`. Core says what the game does: whether
the swoop's swipe has started, whether the Ki-Hunter has lost its wings, what a hurt enemy does, which shots and touches
pass through, that one random number chooses a jump's height and direction, which way an aim turns and how far a thing
moves, to a fraction of a pixel. A thing that preserves another (`morph art preserves the charge`), latching onto Samus,
the display latch, an enemy's main AI, a beam alone, a unit of measure alone, a fixed point (`a target at a fixed
point`) and a hardware owner, which describes the encoding it keeps, stay English.

#2478 widens SME6454, outside hardware owners, to a count's end, a velocity's sign and a placement's direction told by
the original's storage: a counter, count, countdown, delay, timer, cooldown or wait that passes zero (`the bounce counter
passes zero`, `the grab cooldown has passed zero`) or a count run until it passes zero (`counts the jump delay down until
it passes zero`); a velocity's, part's or speed's sign told through its whole part (`until the whole velocity becomes
nonnegative`, `the bounce's whole part is negative`); and every placement but one (`every placement but a left one`).
Core says what the game does: how long a wait lasts or which impact ends it, that a jump stops rising, that a bounce
carries the spark upward and which way a placement patrols. A speed or a width that passes zero (`a speed passing zero
at the apex`), a count that runs out, whole pixels, a placement alone and a hardware owner, which describes the storage
it keeps, stay English.

#2487 widens SME6454, outside hardware owners, to the original's tables and calls as room and world prose tells them: a
collision, pose, turn or controller pose table, spaced or hyphenated (`its own collision tables`, `selected from the pose
table`, `stale pose-table equipment state`, `the turn tables`, `controller pose tables`); a table's entry named by a
hyphenated kind (`the table's no-transition entry`); and an invocation of a thing, which counts updates as the
original's calls (`One invocation of the game's alignment step`). Core says what the game does: the pose's input
transitions, a landing with no transition, one update's step. A table alone, a collision or a pose alone, the pose's
input transitions, an entry alone or a table's next entry, a thing invoked and a hardware owner, which describes the
storage it keeps, stay English.

#2491 widens SME6454, outside hardware owners, to the original's storage and handler installation as frontend and effect
prose tells them: a storage unit on a field or property whose type already measures it, an angle, a turn, a zoom or a
fixed-point speed, displacement or position (`in 256ths of a turn` on a `RotationAngle`, `the zoom in 256ths of a floor
pixel` on a `FloorZoom`, `1/512 of a pixel` on a `SignedSixteenSixteen`); an animation timer numbered one, spaced or
hyphenated (`a timer-one actor`, `primes timer one`); and a handler installed and replaced (`installs an immediate
finish, and a downward elevator replaces it`, `replaces that phase with an inert one`, `installed with it`). The type
states the unit and the member's value spells the amount; Core says what the game does: the turning floor's angle, the
sprite shows its first frame on this update, an ordinary door goes straight on. The same unit on a member typed as a
number, a fraction told in words, a timer alone or one update on, a thing replaced or installed alone, an inert thing and
a hardware owner stay English.

#2501 widens SME6454, outside hardware owners, to the port's loading and its layers yet to come as runtime and gameplay
prose tells them: a load, loader, construction or operation called atomic (`the atomic loader`, `an atomically loaded
destination`, `atomic room construction`, `Loading above is atomic`), anything done atomically (`the room constructor
performs that setup atomically`, read line by line), `the setup call`, and a thing that waits until it gets its own
(`until that owner gets its own layer`). Core says what the game does: the destination's enemies, objects and effects are
set up on the loading update, and each following update moves the door's scroll. The Atomic, the Wrecked Ship's floating
atom, a setup or a call alone, a thing of its own and a hardware owner stay English.

#2515 widens SME6454, outside hardware owners, to the original's routines, flags and bits and the port's reasoning as
enemy and world prose tells them, read across a documentation comment's line breaks: an AI entry (`AI entry, initialized
for the starting wall`); an enemy's hurt, shot, touch, contact, bomb, frozen or grapple AI with its words on two lines
(`calls the normal enemy shot` / `AI`); a flip, curling or mirror flag; a bit by its ordinal (`the sample's fifteenth bit`,
`a sample whose ninth bit is set`); what the port avoided teaching or inventing (`without teaching the reusable Landing
Site runtime how to own enemies`, `instead of inventing an enemy for the Baby`); a value told against a representation it
no longer has (`rather than a signed offset`); and the camera's scroller distances or offsets. Core says what the game
does: which action the Pirate runs, which way the chain turns, one random number in two, where the Baby is drawn, how far
above Samus the camera aims. Main AI or an AI alone, an entry, a flag or a bit alone, a thing that flips or curls, an
Etecoon's teaching hops, a signed offset alone, the camera's scroll and a hardware owner stay English.

#2522 widens SME6454, outside hardware owners, to the original's storage of Samus's speeds, timers, positions and flags,
read across a documentation comment's line breaks: a value's two halves (`clears both velocity halves`, `both base-speed
halves`, `both halves of vertical speed`, `clears both halves`); a fraction bit or fractional bits (`its lowest fraction
bit`, `the lower fractional bits`); a negative test; a value that wraps to 65535 or 255 (`its next decrement wraps to
65535`, `a wrap below zero to 65535`); a position or coordinate told by its range (`whole-pixel Y, 0 through 65535`); a
wrapping comparison; a value negated back or temporarily negated; an expired timer or count that wraps; a slow-scroll,
water, liquid or submersion flag and a response or reaction flag (`the slow-scrolling flag`, `the common grapple-response
flag`); this or that flag read, tested, checked or rewritten; a list scanned, searched or walked last to first; a value
stored, kept or held as a whole; and a one-shot index (`a one-shot auto-cancel index`). Core says what the game does: the
speed is cleared, a held frame lasts one more update, a rise is capped unless outside forces reverse it, the camera
scrolls slowly while the swing is fast. Halves of a picture or a body, a fraction of a pixel, a random sample or frame
counter of 0 through 65535, a count that wraps, a flag alone, an animation played last to first, a whole step of the turn
and a hardware owner stay English.

#2530 widens SME6454, outside hardware owners, to a wait's length restated by number and the original's routines told
as calculators, read across a documentation comment's line breaks: a cooldown, timer, countdown, lifetime, charge or
afterglow counted in frames, updates, ticks or steps by number, with up to two words between (`a 21-frame cooldown`,
`the five-frame knockback timer`, `a 180-update shinespark charge`, `the 32-step afterglow`); a delay or wait counted in
updates, ticks or steps by number (`a 15-update delay`, `a sixteen-update wait`), as #2472 reads one counted in frames;
and a `calculator` (`the leftward calculator`, `the deceleration-allowed calculator`). Core cites the named duration a
wait lasts for (`the Hyper Beam's cooldown (SamusProjectileCooldownDefinitions.HyperBeamShot)`) and says which way a
speed moves Samus. A timer, cooldown or countdown alone, a one-update wait, timer or delay, which runs out on the next
update, a calculation and a hardware owner stay English.

#2543 widens #2530's waits to those the frontend restates, and reads them across a run of line comments, one under
another, as across a documentation comment's line breaks: a hold, fanfare, pause, wait, blank or delay counted in
frames, updates, ticks or steps by number (`the 60-frame hold`, `the ninety-frame final-image hold`, `the mandatory
360-frame item fanfare`, `a 60-update blank`); a thing that waits, holds, delays, pauses or lasts for a number of frames,
updates, ticks or steps, with up to six words between (`waits nine updates`, `holds a blank screen for sixty updates`,
`delays the Samus fade by sixteen updates`, `a wait of 80 updates`, `lasts eight more updates`); an animation frame's
duration by number (`six duration-3 frames`); a duration's value restated after its meaning (`The updates each letter
shows before the next is typed, eight.`); and a count seeded with a number (`always seeds 60`). A wait counted in prose
is no longer English. Core names the wait and cites the definition that holds its length (`holds the explosion for its
hold (EndingExplosionFadeDefinitions.HoldUpdates)`). A wait for one update, a wait whose length is a
named duration, a thing held to a bound, a held of frames, and line comments a blank line or code apart stay English.

#2549 widens SME6454 to a value restated by number, and reads the import pipeline's assembly (`SuperMetroid.AssetExtraction`)
for it as well as Core's, for nothing else: the pipeline may name the cartridge. Outside hardware owners it reports a
cadence a comment or string counts by number, read across a documentation comment's line breaks and a run of line
comments: every second, eighth, 64th or 128th update, frame, tick, pass, step, draw or visit, or every two, four or
sixteen of them, with up to two words between (`every eighth update of his body's own update count`, `every 64th enemy
pass`, `every fourth cinematic frame`), and every eighth or sixteenth of the updates, frames or passes. In every type it
reports a declared name that spells a cadence, a wait or a size by number: `Every` followed by a count or an ordinal
(`GrappleShockEveryEighthUpdate`, `BreathBubbleEvery128thUpdate`, `RocksEvery16Frames`), a count of updates, frames,
seconds or ticks (`DroolFormingForTwoUpdates`, `WaveBeamThreeUpdateTrail`, `FourSecondHold`) and one count by another
(`CentredSixteenByThirtyTwoBox`). A cadence's period, a wait's length and a box's size are the named member's value:
the name says what the member paces, waits for or covers (`GrappleShock`, `DroolFormingShort`, `CentredBox`), and its
documentation says where in its period a cadence falls (`counted on his body's own update count at the period's first
place`) or cites the member that holds the value (`each for <see cref="CeresSteamAnimations.PuffPictureUpdates"/>`),
never repeating the number. Every update, every other room, every 256th of a pixel kept, two of every four, a thing every
step of the way, a name that spells one (`OneUpdateLate`), a fraction of a unit (`QuarterTurn`), a position in order
(`SecondPhase`) or a number glued to a word (`Bg2Frames`), and a hardware owner, which describes the program it
reproduces, stay English.

#2533 widens SME6454, outside hardware owners, to the original's storage units, random bits and program bytes as enemy
prose tells them, read across a documentation comment's line breaks: a value counted as a number of 256ths, in digits or
words (`twelve 256ths of a pixel`, `1023 through 2815 256ths`, `32 or 64 256ths of a mark`, `four pixels and 80
256ths`); a random number told by its oddness (`faces right on an odd number`, `an odd sample`, `an odd roll`, `the random
number is odd`) or its parity (`the parity of the roll`); code, data, words, definitions, entries, lists or tables
stored, kept or placed after, before, around or past another thing (`drifting by the code stored after them`) and the
code that follows or precedes one; a definition, entry, table or record pointing a thing at a reaction, routine,
animation, handler, list, entry, definition, program or segment (`definitions intentionally point touch and shot at the
same private reaction`, `points the caret sprite at its blinking animation`); and, on a parameter of a method,
constructor, delegate, indexer, local function or primary constructor typed as a measure (the #2491 types), the unit its
type already states (`its crawl speed, in 1/256 pixels per update` on a `SignedEightEight?` parameter). Core says what the
game does: how fast a thing moves in the game's fractions (three sixty-fourths of a pixel an update), one time in two,
which drift the roll past the authored launches takes, that a touch starts a corpse's rot as a shot does. A unit of
measure (`in 256ths of a pixel`, `a 256th of a pixel`, `one 256th`), an odd number of things, odd updates, frames, columns
and counts, a frame counter's parity, a thing stored alone, a thing pointing at another (`the missile points at the
answer`), the same unit on a parameter typed as a number and a hardware owner stay English.

#2540 widens SME6454, outside hardware owners, to the original's tables counted and its fractions listed as boss prose
tells them, read across a documentation comment's line breaks: a number of the game's, the original's or the cartridge's
launches, entries, rows, values, records, copies or variants (`all eight of the game's nail launches`, `the game's four
launches`, `all 128 of the game's values`), a number of launches, entries, rows, records, copies or variants said to be
all identical (`four launches, chosen at random, all identical`), and fractions listed as zero (`X=-1, fractions zero`,
`leave the fractions zero`, `zero fractions`). Core says what the game does: every launch flies at these speeds, one
pixel an update leftward, whole pixels only. A number of the game's rooms, doors or areas (`the game's 262 rooms`), a
thing all identical with no count, a fraction of a pixel, no fraction, a zero fraction and a hardware owner stay English.

#2546 widens SME6454, outside hardware owners, to the original's storage units and the original program as room, area
and effects prose tells them, read across a documentation comment's line breaks: an amount of a pixel written over 256
(`104/256 of a pixel`, `255/256 pixel per frame`, `1/256 of a pixel`); a count of 65536ths in any form (`in pixels and
65536ths`, `in energy and its 65536ths`); a value in 1/65536 pixels (`held in 1/65536 pixels`, `wrapping in 1/65536
pixels`), which #2172 and #2427 had left as a unit of measure; subpixels in any spelling (`in pixels and subpixels`,
`Samus's Y subpixel`, `a sub-pixel position`), which #2440 had left as English; and anything the original's (`the
original's setup rewrites nothing but the block's type`, `the original's closing switches the object it finds`). Core
says what the game does: how fast a thing moves in the game's fractions (thirteen thirty-seconds of a pixel an update,
five pixels an update), that a position or a speed carries its fraction of a pixel, which part of a block a setup
changes, which closing a door starts. A speed in 1/256 pixels, a 256th or one 65536th of a pixel, a fraction of a pixel,
an original alone, the original game and a hardware owner stay English.

SME6479 (#2550) widens SME6454's storage, original-program and port-narration vocabularies, under its own id, to every
phrasing the #1423 review found in Core's documentation. Outside hardware owners, each comment is read with its markup
removed and its line breaks joined, and a run of line comments, one under another, is read as one text for a phrase that
runs from one line to the next. It reports a value told by its storage's range or a machine number (`the short frame
counter, 0 through 255`, `the lowest 16384 of the 65536 samples`, `-32768 through 32767` on a screen coordinate, `its
lowest bit`, `a sample divisible by four`); a unit of the original's storage (`in 256ths of a pixel`, `whole 256ths`,
`24/256`, `128ths of the whole`, `fifteen nontransparent colours`, `the fraction's overflow`); reasoning about a value's
storage range (`never nears a wrap`, `stays far inside its range`, `at most fifty screens`, `a few thousand units of
rest`, `The comparison is signed`); the original's program flow (`falls directly into`, `returns early`, `main/hurt
entry`, `the frame tail`, `is never reached`); its routines and engine parts (`the projectile prelude`, `the common
mover`, `init AI`, `collision walks`, `the pose-definition fallback`); installed animations and loaded or copied colours
(`installs the laser head animation`, `which his initialization loads`, `the runaway colour copy`, `the records of`,
`master brightness`); the display hardware (`the low-priority frames`, `sprite entries`, `drawn partly as a background
layer`, `the icon's cells`); storage named for what the original kept (`the animating flag`, `set bits`, `the
load-station record`, `population entries`, `rewrites only the block's type and parameter`, `the held controller
mask`); its tables, entries and allocation (`the area's own tables`, `past-the-end`, `allocation order`, `a normal second
enemy`); the port's reasoning and history (`deliberately owns only`, `Models the`, `Without this`, `rather than a saved
perch coordinate`, `Despite its traditional name`); and the animation segment template and the original's drawing
history (`one segment per animation a spawn, branch or wake enters`, `the second showing`, `the game draws it for two
updates`). Core says what the game does: one explosion in four bursts as smoke, the Baby moves by its velocity, the wait's
first update is the setup's, the block keeps its picture, the statue joins the statues that are animating. A 256th of a
pixel, the background, a fall through a bridge, the game's music, the installed artwork, a cycle of sixteen colours, a
turn's steps and a hardware owner stay English. Other assemblies, the import pipeline among them, tell the cartridge's
storage and are not read.

SME6459 (#2309) reads the summary of every member of a Core `*Segment` enum, one run of an actor's animation, and rejects
one that only restates the member's name: `The facing left shooting animation.`, `The idle.`, `The loop.`, and (#2465)
any summary whose words are all the name's, the actor's the enum is named for, articles, possessives or `animation`,
however ordered or punctuated (`Draygon body animation: facing left, reset.` on `DraygonSegment.BodyFacingLeftReset`,
`Ceres Ridley's left-facing lunge.` on `CeresLungeFacingLeft`). The summary says what the run shows: what the actor
does, which way it faces, which pictures it passes through and for how long, what it starts or sounds (`His body facing
left opening its jaw wide to spit goop with its sound, then shutting it.`). A summary holding a `see` reference is not
read.

SME6460 (#2511) keeps installed keys written out. Text marked `[InstalledKey]` (a shade's effect, an artwork animation's
or frame's name, an encounter look's name, a boss band's or fade's key, and the helpers that hand such text on) is a name
the import writes into the installation and the game looks content up by. In Core and AssetExtraction no enum member's
name is written into it: an interpolation hole, `ToString()`, a concatenation, a `string.Concat`, `Format` or `Join`
argument, `Enum.GetName` or a `nameof` of an enum member, followed through interpolation, concatenation, string
transforms such as `ToLowerInvariant()`, conditional branches, switch-expression arms and one local. Renaming the member
would change the key without a format change. Each key is a literal or an entry of a table of the identity's keys
(`RidleyShades.BabyKeys`, `TorizoLook`'s keys, `WorkRobotShades`' glow keys).

#2520 finishes that shape. An identity whose members the installation, an installed document or a save names has one
codec, its `<Identity>Keys.Installed` table (`InstalledKeys<TIdentity>`): every member's key written out, `Of` to spell
a member, `Parse` and `TryParse` to read one back, and `<Identity>JsonConverter` (`InstalledKeyJsonConverter`) for a
document's values and dictionary keys; a save spells the same keys in camel case through `SaveKeyJsonConverter` and
`SaveFlagsJsonConverter`. SME6460 also reads a member overriding a marked one, a value typed `System.Enum`, a
`Utf8JsonWriter`'s written text, and a LINQ or span query's selector result and the collections gathered from it; the
sheet lookups (`SpriteSheet.Frame`, `Picture`, `ShowsNothing`, `HoldsPicture`), the frame and sheet name helpers and the
document properties that carry names are marked. SME6462 reports an identity keyed or read by its members' names: an
enum member's name made a dictionary's or set's key (`Add`, `TryGetValue`, `ContainsKey`, an indexer, a key selector),
`Enum.Parse`, `Enum.TryParse`, `Enum.GetNames` and `JsonStringEnumConverter` in any form. Diagnostic text, exception
messages and an import run's own fingerprints and content identities, which nothing installs, stay free to name members.

#2548 widens both rules to every source name. A `nameof` of any member, a property, field, method or event, or of a type
is that member's or type's source name, and a name handed to a parameter of the same assembly is followed into whatever
the parameter's references reach, through the parameters and helpers it is handed on to; a record's positional parameter
reaches whatever its property's reads reach. So a catalog property found again by its own name (`Find(nameof(Parlor))`
into a dictionary keyed by the definitions' names) and a definition named by its member (`new(nameof(LandingSite), ...)`
into a marked or looked-up name) are reported. Each catalog property is its definition, built where it is declared with
its key written out (`RoomDefinitions.LandingSite` is `new("LandingSite", ...)`, the authored list gathered from the
properties); a pairing table is keyed by the definition or the identity itself (`RoomStateEnemyGraphicsSetDefinitions.ByState`
by the room state, the palette rows and members by the graphics set); and an installed name is never an address
(`music-xx-phrase-NNN` names a phrase by its place, a track repeats from the place of one of its instructions, enemy-tile
import data files a sheet under its enemy's key).

SME6465 (#2555) keeps measures in named units. Outside the type that declares the factory and outside an
`[EncodingOwner]` type, Core passes no constant to a raw-unit factory (`FromTwoHundredFiftySixths…`, `FromSixteenths…`,
`FromParts`, `FromFractionsOfStep`, `FromEighthsOfStep`, `FromHalfSteps`, `FromFraction`) or to a parameter counting a
raw fraction (`fraction`, `twoHundredFiftySixths`, `sixteenths`, `fractionsOfStep`, `subposition`), writes no constant
fraction to a power-of-two fraction factory (`FromPixelFraction`, `FromStepFraction`, `FromMarkFraction`,
`FromEnergyFraction`, `FromPlaceFraction`, `FromFloorPixelsPerScreenPixel`) outside its lowest terms, and shifts no
computed value by a literal count. An amount is zero, whole units (`FromPixels`, `FromWholePixels`, `FromSteps`) or a
fraction in its lowest terms (three eighths of a pixel is `FromPixelFraction(3, 8)`); a position's part of a pixel is a
`PixelFraction`; an installed speed or step is one amount of pixels through its fixed-point type's codec.

SME6471 (#2569) keeps velocities signed. Outside an `[EncodingOwner]` type and a `[CompatibilityService]`, Core chooses
neither between a value and its reversal by a condition (`left ? -speed : speed`, `v.IsNegative ? -offset : offset`,
`facing == Right ? speed : speed.Negated()`) nor between adding and subtracting the same change
(`v.IsNegative ? v - a : v + a`), and declares no enum of signs (`Positive` and `Negative`). A speed, distance or turn is
taken toward its direction by `Toward(direction)`, a velocity's sign is its direction, the speed types own speeding a
velocity up away from rest (`SpedUpBy`) or dragging it toward rest (`DraggedBy`), and Samus's vertical velocity is one
`SamusVerticalVelocity`. Where the original's unsigned-magnitude arithmetic is observable, a compatibility service
reproduces it (`SpeedAlongAngle`, `MultiviolaLeftOrUpSpeed`, `BullBackwardStep`).

SME6476 (#2558) keeps magic values from standing for a role or absence. In Core and the import pipeline no array or
sequence is filled with minus one or `int.MaxValue` (`Array.Fill(places, -1)`), no choice gives one beside a computed
value (`found ? index : -1`, `index ?? -1`), no method returns one, no field or property is seeded with one, and no local
is seeded with one that its body then tests against zero or minus one. An identity enum (`…Id`) has no `None`, `Empty`,
`Blank` or `Nothing` member, and an identity or type value (`…Id`, `…Type`) no static of that name. Absence is a typed
optional (`int?`, a nullable reference, a `TryGet`) or a closed type's case, and a role is a named member. A named
constant, a sign beside another constant, a comparison's order and a library's own result are not sentinels.

SME6484 (#2559) keeps behaviour reading properties rather than testing which identity a value is. In Core and the
import pipeline no test lists two or more members of one identity enum (an enum named `…Id` that is not `[Flags]`) to
decide something about a value: no pattern test that combines them (`pose is SamusPoseId.SpaceJumpRightPose or
SamusPoseId.SpaceJumpLeftPose`, `area is not (AreaId.Brinstar or AreaId.Tourian)`) and no chain of equality tests of one
value against them (`enemy == EnemyDefinitionId.Ridley || enemy == EnemyDefinitionId.RidleyCeres`). The trait the members
share is declared once, as a property of each member's definition or a named family the definitions list
(`SamusPoseMetadata.IsSpaceJumpPose(pose)`, `SamusPoseTraits.IsIn(pose, SamusPoseFamily.DrainedCrouch)`), and the behaviour
reads it. A switch that maps each member to its own value is a lookup, and a test of one member or of a named constant is
not a listing.

SME6488 (#2574) gives each thing one identity named for what it is. In Core and the import pipeline no enum mirrors
another member for member: no switch expression or dictionary initializer maps every member of one enum to the member of
the same name of another enum with exactly the same members, each to its own. The users name the one identity, or the
definition carries the member it stands for (a palette effect's `ColourEffectId`, a growth resume's `KraidHeadFrame`, a
shell piece's picture). A narrower enum embedded into a wider one (`HorizontalDirection.Left =>
SamusCollisionDirection.Left`, the two horizontal directions among the four collision directions), or a wider one
collapsed onto a narrower one, converts between two domains and is not a mirror. A map onto other names (a pose to the
picture it shows) is a definition carrying its property, and enums sharing member names that nothing maps onto each other
are separate identities. No member is named by joining the names of two or more members of another type, each
of two words or more (`EastAqueductQuicksandWestAqueductQuicksand`): a thing several users share is named for what it is
(`QuicksandShaftsFallingSand`, `SaveRoomDoorOnLeft`), generated catalogs included. A frame is named for what its picture
shows, never for the hitbox or depth its record carried (`PhantoonBody`, whose hit area is the fight's
`PhantoonBodyHitArea`), and data is never reached through another thing's identity (a shot flame drops from
`ShotFlameDropChances`, not through his eye's enemy definition).

SME6472 (#2575) keeps definition data in domain catalogs. Outside a catalog file, Core builds no value of a Core type
wholly from constants with a nonzero numeric literal among them (`new Offset16<RoomPixels>(1, 0)`,
`SignedSixteenSixteen.FromWholePixels(1)`, `UpdateCountdown.Lasting(1)`) and passes no nonzero numeric literal by name
(`count: 5`, `ticks: 1`). Behaviour code is every member of an instance type, its static fields included, and every
member of a static type that decides something; a static type's member that holds no decision or procedure is a table
of data. The duration, speed, distance, count or shape is a member of the domain's catalog with its meaning on the
member's summary (`DachoraMotionDefinitions.WaitingFloorProbe`, `GoldNinjaSpacePirateDefinitions.DropScatterCount`). A
value type names its own units (`SignedSixteenSixteen.OnePixel`, `SignedEightEight.FinestStep`, `Offset16<T>.OneDown`,
`UpdateCountdown.LastingOneUpdate()`); zero, a literal beside computed state and a literal passed by position are not
definitions.

SME6482 (#2556) keeps relations as references, never list positions. Outside an `[EncodingOwner]` type and a
`[CompatibilityService]`, Core reads no place's `Ordinal`, its position in the list it belongs to, except to spell it in an
interpolated message: no partner is found by counting places from another (`part.Ordinal - 1`), and no place number is
handed to a lookup, stored or compared (`Inherit(place.Ordinal, ...)`). A member of a multipart enemy finds the member
playing another role through the one owner of the population's listing (`EnemyCompositeListing.Member`, with the roles
in listed order in the species' `EnemyComposite`) and keeps the place it finds; a rule the game makes depend on placement
itself is a compatibility service (`FollowUpAfterStruckPart`, `BabyMetroidVictim`); a place is handed on as itself.

SME6485 (#2560) keeps one closed choice as one typed value, in Core and AssetExtraction. No conditional on a bool
parameter, field or stored property chooses between the two members of a two-member enum (`left ? Side.Left : Side.Right`), and
no comparison with a two-member enum's member is stored into a bool field or property or passed to a bool parameter
(`_facingLeft = facing == HorizontalDirection.Left`, `Show(language == TextLanguage.Japanese)`): the code holds and passes
the enum itself. Where several values stand for one choice the choice is one enum, or a closed record set whose cases
carry their own data; `[Flags]` only where the game composes the bits; a value that must exist is not nullable, and
absence belongs to the absent thing. A file format that keeps a bool for compatibility converts through one declared map
at its codec (`GameSaveJsonCodec`'s saved Japanese-text field). A comparison that only answers a question, a query's
result, a condition or a local, is not a stored choice, and neither is a computed property asked of a value
(`speed.IsNegative ? HorizontalDirection.Left : HorizontalDirection.Right`).
SME6487 (#2565) pairs state and data by identity, never by list position. In Core and the import pipeline, outside an
`[EncodingOwner]` type and a `[CompatibilityService]`, no `Zip` joins two collections that are not one
(`frames.Zip(durations)`), no element is read at a position found in another collection
(`colours[names.IndexOf(name)]`), no projection's index reads another collection
(`looks.Select((look, place) => uploads[place])`), and no loop reads a static definition table and another collection at
its own index. Each pairing is one record holding both (`KraidHealthSequenceBand`, `IntroEggShellPiece`), a keyed lookup by
the identity it belongs to (a sheet shade's ink-to-colour map, Phantoon's health colours by band), or a reference (a packed
picture carries its corner). Positions within one authored sequence are not pairings: a run's colours from its first ink,
a cycle's steps (`ListStep<T>`), neighbouring entries of one list (`list.Zip(list.Skip(1))`) and a list written at the
index it is read from.

SME6483 (#2557) states repeated literal structure once, in Core's and AssetExtraction's hand-written catalog files. A
list of two or more entries is not written out again, entry for entry, in another list of the same file, and a list
writes no entry out three or more times in a row; entries compare by their text with its spacing removed. The shared
list is one named definition the entries reference (`ChestOpenUploads`, `NorfairRidleyFirstShades`), a mirrored or
reversed run is derived from the one it repeats (`AnimationBeats<TFrame, TEvent>.Backwards`, `OtherWayRound`,
`SpritePart.MirroredVertically`), a run sharing a duration is built from its frames and that duration once
(`AnimationBeats<TFrame, TEvent>.Steady`), and a placement names its object once, by its setup, whose kind follows from
it (`RoomObjectKindDefinitions`). A generated catalog (`<auto-generated/>`) is its generator's to state once: the
room-object populations place each scroll trigger with the reach its extensions give it (`ScrollTriggerPlacements`).

SME6486 (#2563) keeps the random number's places out of gameplay. A random choice in Core names its roll by its
outcomes: the number's ordinary roll of so many values (`RandomSampleField.OneOf(64)`), its second roll taken alongside
the ordinary one (`RandomSampleField.SecondOf(64)`, a height beside a column), a part of its range
(`RandomSampleField.RangePart(4)`, one chance in four taken by the lowest part), the whole number or nothing. A choice
the game took from other places of the number reads a member of a compatibility service named for that choice
(`KraidRandomRolls.SpitLaunch`, `TorizoRandomRolls.LowHealthDrool`), the only place a `RandomSampleField` is built from a
position and a width. Counter phases are rhythms, never places of a counter: a flash white for two updates of every four
is an `UpdateAlternation` of two updates each way, a shake through four offsets held two updates each is an
`UpdateCycle` of four steps of two, and an action every eighth update is an `UpdateCadence`. Narrow integer storage of
gameplay state is SME6347's; this rule adds no allowance for it.

SME6474 (#2566) keeps every result read and every parameter needed, in Core and AssetExtraction. A method outside the
assembly's public surface whose result no caller in the assembly reads, each caller discarding it (`_ = Step();`) or
calling it as a statement, returns nothing (a spawn no caller acts on a refusal of reports nothing); an out result no
caller reads is not given, and a call does not throw away an out result with the rest of its report. A method that
returns null on every path returns nothing (a cue handler whose cues never start another animation plays through
`AdvanceApplyingCues`), and a method takes no parameter it discards (`_`). A result some caller reads is the operation's
report, which another caller may leave unread; the `Ensure` guards return the value they checked, and a method a
delegate refers to, an override and an interface implementation keep their contract's shape.

SME6481 (#2553) keeps Core from checking a state its types and data rule out. Outside an `[EncodingOwner]` type Core
calls no `Enum.IsDefined` on an enum value: SME6450 leaves no way to make a value that names no member outside the
codecs, and the codecs only yield members. No lookup that can miss (`TryGetValue`, `ContainsKey`, `GetValueOrDefault`,
`Ensure.Listed`) reads a field or property whose initializer is a dictionary naming every member of its enum key; a
catalog with a value for every member is an `IdentityTable<TIdentity, TValue>`, which checks once when it is built that
every member has its value and is read by member with no guard (`SamusPoseDefinitions.Poses[pose]`,
`KraidMouthHitboxDefinitions.Rectangles[shape]`). No downcast whose failure throws (`x is T t ? t : throw`,
`if (x is not T t) throw`, `x as T ?? throw`) narrows a closed hierarchy, one whose every constructor is private or
private protected: the value is typed as the case the code needs, or the hierarchy answers the question itself. The rest
of the shape is the same decision without a detector: an animation that raises no event plays through the silent
playback (`EnemyAnimationPlayback.AdvanceSilently`, `PlaySilently`) with no cue handler; a switch whose every member has
an arm has no throwing default, and a switch expression whose arms are data reads an `IdentityTable`. A guard on input
from outside the program, an installed file, a save or the controller, is not impossible and stays.

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
cartridge's routines and values written as the machine's hexadecimal fixed-point (`0.1000`, `7.0000`) (#1743). It reads the original program's call structure and the C reimplementation in every type too: dispatchers, callables, a routine's labels (a merge or inverted label) and the upstream C rewrite (#1754). Outside the owners it also reads the original's data and number encodings: decompression, colour indexes, two's complement, unsigned compares and the `8.8` and `16.16` format names (#1754); an encoding owner such as `SignedEightEight` names the format it owns. Every other occurrence is renamed for the game or removed with the thing it named. No baseline,
allowlist, count, severity change or scope change (project manifesto, section 6).

## Status

The rule is the milestone-wide detector: it ships at error severity with the whole v0.9.0 work list as its findings and
stays red until the last removal lands. Count the findings with the census (`docs/analyzer-cleanup.md`); while it is
red, build verification binaries with `-p:RunAnalyzers=false` and rely on the census for every SME rule.
