# Godot type system

This is the list of the kinds of thing the v1.0.0 Godot project is made of: what each one is, what it owns, what it
manages, what it must not do, where it lives, and why it is shaped the way it is. It sits under
[the v1.0.0 manifesto](v1.0.0-manifesto.md) and beside [the architecture guide](godot-architecture.md).

**Work extends this system. It does not invent beside it.**

- Everything built is an instance of a type listed here. Section 2 decides which type; section 10 places every kind
  of thing the game contains.
- The system is closed at the top: no new base class, no new shared service, no new top-level folder, no new
  category of thing. The engine's shared services are exactly `Progress`, `GameRandom`, `GameClock`,
  `SoundRequests`, `Options`, `Camera`, `AudioDirector`, `InputSampler` and `ContentLibrary`; this game adds one of
  its own, `Escape`.
- It is open everywhere below that. No section here is complete, and none is meant to be. Each ends with a note,
  **Why, and extending it**, that says what the section is for and how to grow it. When the work meets something a
  section does not cover, the agent extends that section by its note and section 0, builds it, and writes the
  addition into this document in the same change. It does not stop to ask, and it does not build something this
  document would not describe.
- Which type a thing is depends on what it is in the game, never on where the old code filed it.

## 0. How this system was designed

Every section below was produced by the same few moves. To extend any of them, make the same moves.

**The goal.** The project is a metroidvania engine with Super Metroid as its first game. When Super Metroid is
finished, its code and assets are removed and a different game is built on the engine. So every decision is judged
twice: does it make Super Metroid play right, and does it leave the engine something a different game would want.

**The moves.**

1. **Take a thing apart into the independent things it is made of.** A door looked like one thing. It is a solid
   object that takes damage, and a region that leads to another room, and neither needs the other. Whenever one
   thing has two jobs that could exist apart, it is two things. The test: can you imagine either half used alone?
   A wall that takes damage with no exit behind it; an exit with nothing in front. If yes, split.
2. **Turn differences into settings, not types.** Blue, red, green, yellow and grey doors are one `Barrier` with
   different numbers. When several things differ only in amounts, in which kinds they accept, or in which of a few
   choices they take, they are one type with settings. A new type is for a new behaviour, never for a new value.
3. **The engine owns the rule; the game owns the names.** The engine knows that a barrier is damaged by listed hit
   kinds. It does not know what a missile is. Anything that is a list of this game's particular things (weapons,
   items, liquids, bosses, events) is a name the game supplies, and the engine's rule refers to names.
4. **The thing whose state changes owns the behaviour.** The room reports heat; the player takes the damage. The
   projectile delivers a hit; the target decides what it does. Nothing reaches into another thing to change it.
5. **Shared logic moves down.** If two things of this game do the same thing, it belongs in the engine: in their
   base class if they are the same kind of thing, in a component if they are not.
6. **A state holds its own data.** Anything with modes is a state machine, and data that only means something in
   one state exists only in that state. If a combination of fields can describe something impossible, the design
   is wrong.
7. **Values live in files.** Behaviour and structure are in the project; every number and word is in a data file
   and still shows in the editor.
8. **Start from the plain version.** The engine does what a sensible new game would want. Super Metroid's oddities
   are added on top, in the game's code, through a named place the engine offers.

**When something does not fit.** In order: check whether it is really two things (move 1). Check whether it is an
existing type with a setting it lacks (move 2), and add the setting. Check whether an existing type's rule needs
one more choice, and add the choice. Only when it is a new behaviour that no listed type has does it become a new
kind, component or state, added by section 9's recipe, and placed by the question "would a different metroidvania
want this, unchanged?"

## 1. Project structure

The project has three layers. The split is the point of the design.

| Layer | What it is | Rule |
| --- | --- | --- |
| `engine/` | A metroidvania engine: the loop, collision, rooms and world logic, and every game system and piece of shared logic, including the base classes that say what an enemy, a player, a projectile, a pickup, an item and a room object are and do. | Contains nothing a different metroidvania would not also want. It never names Samus, an enemy, an item or a room of this game, and never references `game/`. |
| `game/` | Super Metroid, in code: the particular things. Samus, each enemy, boss, item, weapon, room, screen and sequence, each a thin class on an engine base plus its own behaviour. | Depends on `engine/`. Holds only what is specific to this game, including `game/compatibility/`. |
| `assets/` | Super Metroid's art, audio, room layouts and every value (stats, timings, text), produced from the player's ROM. | Never committed, never shipped. Plain files in neutral formats. |

```text
godot/
  project.godot
  assets/          the asset extractor's output; git-ignored, never shipped
  engine/
    core/          GameSession, GameLoop, GameMode, Progress, SaveFile, Options, SoundRequests
    shared/        the values of section 4
    collision/     CollisionWorld, Collider, Mover
    input/         InputSampler, GameInput, GameAction, the input sources (the player's devices, a recording)
    audio/         AudioDirector
    content/       ContentLibrary, the values-from-files mechanism, the asset file formats
    actors/        Enemy, Projectile, Pickup, Effect, Remains
      player/      Player, its standard states, Inventory, Weapons
      movement/    the shared ways of moving (section 6)
      components/  Health, StateMachine, State, TimelinePlayer, ActorVisual
    world/         Room, Terrain, Liquid, RoomEffect, Backdrop, Camera, CameraZone, Trigger, RoomExit,
                   RoomObject, RoomSequence
      objects/<name>/    the general-purpose room object kinds
    ui/            ScreenFade, PixelFont, PixelText, TypedText, MessageBox, MapView, Cinematic
    tests/         checks of the engine, using only the engine's own stand-in content
  game/
    game_definition      this game's GameDefinition, the one thing the engine is pointed at
    samus/         Samus, her states, weapons, abilities, visuals
    enemies/<name>/      one folder per enemy
    bosses/<name>/       one folder per boss
    projectiles/<name>/  one folder per projectile fired by more than one actor
    pickups/  effects/
    objects/<name>/      room object kinds only this game has
    rooms/<area>/  one scene per room, with that room's RoomSequence beside it if it has one
    hud/  pause/  menus/  cinematics/  escape/
    data/          the names this game's code refers to: its items, story events, hit kinds, music and sounds
    compatibility/ this game's oddities that plug into the engine
    tests/         checks of this game's things
```

- A folder for one thing holds that thing's scene, scripts and settings, and nothing else.
- Something only one thing uses lives in that thing's folder: its states, a projectile only it fires. Something
  several things use lives in the shared folder for its type.
- C# files are named for their one class. Scenes and resources use Godot's snake_case. Namespaces mirror folders.
- **This game's names** (equipment, amounts, hit kinds, areas, bosses, story events, music, toggles, actions) are
  lists of constants in `game/data/`, one class per list. Code refers to the constant; data files, scenes and saves
  write the name itself. Every name is lower-case words joined by underscores, because a name is also a file name
  and a key in a text file. A boss's defeat is the fact `<boss>_defeated`. The lists were written from the old
  code's lists; the sounds are not listed yet.
- `game/SuperMetroid.cs` is this game's `GameDefinition` class and `game/game_definition.tres` its resource, which
  the project setting names. Samus's files are in the namespace `Game`, for the reason the player's are in
  `Engine.Actors`.
- The engine's stand-in content for its tests is a minimal player, a test enemy, a small hand-written room layout
  and a test `GameDefinition`.

**Stripping the game.** Deleting `game/` and `assets/` leaves an engine that still builds and passes its own
checks. Everything of Super Metroid is under those two folders and nowhere else: no Super Metroid name, file or
setting in `engine/` or at the project root, and the game is named by one project setting (its `GameDefinition`),
not by engine code.

**Engine or game.** Is it a system or shared logic, or is it one particular thing? Every game system, and all logic
shared by this game's things, is engine. Only the particular things are game.

- An engine base class distils what its kind of thing is and does, completely enough that a new game's version is
  a thin class on top. `Enemy` is what a metroidvania enemy is. The Waver is only what makes a Waver a Waver.
- When two things in `game/` share logic, that logic moves into the engine. `game/` holds no shared helpers and no
  base classes of its own.
- The engine never contains a list, a count or a name of this game's content.
- The test for a single line of code: would a different metroidvania built on this engine want it, unchanged?

**The compatibility layer.** Some of this game's behaviour is odd in a way no new game would want, yet it touches
something the engine owns: how a collision edge is tested, how a sound request is refused, what happens to
something that leaves the room.

- The engine does the plain, sensible thing by default, and where this game needs something else it offers a
  narrow, named place to supply it: an option on a call, or a small interface with a default.
- `game/compatibility/` holds this game's answers, each a small class named for what it does in the game, with one
  line saying what it reproduces.
- An oddity that belongs to one enemy, item or object of this game is not compatibility; it is ordinary code in
  that thing, which is already in `game/`.
- If removing `game/compatibility/` would leave dead code in the engine, that code was in the wrong layer.
- What is there now: `CartridgeRandom`, the cartridge's own sequence of random numbers, and `FrameRandom`, a session
  service that draws one number as every update of play opens (`IUpdateOpener`) and uses none, as the cartridge did
  every frame before Samus, the enemies and the room acted: much of this game reads the number last drawn
  (`GameRandom.Last`) without drawing, and sees a fresh one each update only so.
  `CartridgeSpeedReadings` is how the cartridge reads Samus's speed across against its limits (`ISpeedReadings`,
  which the engine answers plainly with one comparison): whole pixels and fraction each on its own, by the sign of
  their difference within one pixel's range, so that a fraction more than half a pixel over a limit's reads as
  under it.
  `KeptScrollCells` is the cartridge's fifty scroll cells, which outlive each room: a room with an explicit scroll
  layout loads all fifty (its own screens and what the layout's load writes after them, from
  `rooms/scroll_layouts.json`), a room with none writes only its own, and the camera reads the cells past a room's
  own screens at its edges. A read past the fifty, which on the cartridge reads the stored maps, is not built and
  fails.

**Why, and extending it.** The layers exist so the game can be thrown away and the engine kept. Everything else
here follows from that. When unsure which layer something goes in, imagine the fork: the new game has a different
hero, different enemies and different rooms, but it still has a hero who runs and jumps, enemies with health,
rooms joined by exits. If the new game would keep the thing and change only its numbers and looks, it is engine. A
new top-level folder is never the answer; a new subfolder under an existing one is, when a new kind of thing
appears (a new family of components, for instance), placed beside the things it is most like.

## 2. Deciding what type a thing is

Ask these in order and stop at the first yes.

1. Is it the player? **`Player`** for what any player does, **`Samus`** for what only she does.
2. Is it only looked at, with no collision and no effect on play? Short-lived: **`Effect`.** Part of a room's
   background: **`Backdrop`.** Part of an actor: a child of that actor's `ActorVisual`.
3. Was it launched by something to deliver a hit, and does it live on its own afterwards? **`Projectile`.**
4. Is it collected by touching it? Temporary: **`Pickup`.** Permanent: **`Item`.**
5. Does it have health that the player's weapons reduce, and does it act on its own? **`Enemy`.**
6. Is it solid, does damage or a signal open it, and does it otherwise do nothing? **`Barrier`.**
7. Is it part of the tile grid, at most two tiles by two, and is everything it does one of the block rules?
   **A block.**
8. Is it a region with no body? A screen whose scrolling something changes: **`CameraZone`.** A height the view is kept below: **`ViewFloor`.** Leads to another room: **`RoomExit`.** Does one
   thing on entry: **`Trigger`.** A body of liquid: **`Liquid`.** A room-wide condition: **`RoomEffect`.**
9. Is it a scripted sequence that belongs to one room? **`RoomSequence`.**
10. Is it a scripted sequence with no gameplay? **`Cinematic`.**
11. Otherwise it is a placed fixture: **`RoomObject`**, of a kind section 5 gives, or a new kind named for what it
    is.

Before accepting an answer, apply move 1 of section 0: is this one thing, or two?

Deciding who owns a behaviour:

- The thing whose state changes owns the behaviour.
- If two things change, each owns its own half and one calls the other's method.
- If a thing is part of something else and cannot exist without it, it is a child node of that thing. If it can
  outlive what made it, it is its own entity.
- If the same behaviour is needed by different types, it is a component. If it is needed by variants of one type,
  it is in that type's base. If it is needed once, it is where it is used.

**Why, and extending it.** The questions are ordered from the most specific kind of thing to the most general, so
that nothing lands in the catch-all (`RoomObject`) when it is really something narrower. If a new kind of thing
keeps landing on question 11 and none of section 5's kinds fits, that is the signal for a new room object kind. If
a new question is needed, it goes where it is more specific than everything after it.

## 3. Rules for every class

**Access**

- Members are private unless another type has to call them.
- State is changed only through its owner's methods. No public setters on gameplay state, no public fields.
- Nothing is made public only so a test can read it.

**Inheritance**

- At most one project base class between a Godot node type and a concrete class: `Waver : Enemy : Node2D`.
- The only base classes are `Player`, `Enemy`, `Projectile`, `RoomObject`, `Pickup`, `Effect`, `State`,
  `GameMode`, `RoomSequence` and `RoomEffect`, all in the engine. Each holds everything its kind of thing shares.
  No families in between, and no base class that only shares helpers.
- Behaviour shared by different kinds of thing is a component node, not a base class.
- A variant that differs only in values is the same scene with a different data file, not a subclass and not a
  second scene.
- Taking part in a phase of the game loop is an interface, never an empty virtual method.
- A capability one thing asks of another (can be latched onto, can be stood on, can be frozen) is an interface.

**Size**

- One entity per thing the player sees as one thing. Parts are child nodes.
- A class does the job this document gives its type. A second job belongs to another listed type.

**Why, and extending it.** One level of inheritance keeps each base class an honest statement of what a kind of
thing is, which is exactly what the fork inherits. Deeper trees are where "what an enemy is" gets smeared across
families. When several enemies share behaviour, the pull is to make a family base class; the answer is a component
(section 6, "Shared movement") or more in `Enemy` itself if every enemy has it. Access is tight so that the only
way to change a thing is to ask it, which is what keeps move 4 true.

**How the engine's code spells these rules.** The engine in `godot/engine/` is written; these are the conventions
it settled, and new code follows them.

- **Exact places beside Godot's.** Everything placed in a room has `new PixelPosition Position`, hiding `Node2D`'s
  floating-point one. Gameplay reads and writes the exact one only; `EntityNode.Show` copies it to the node once per
  update for drawing, and `EntityNode.StartingPlace` reads where a scene placed the node, once, as it enters its
  room. Nothing else reads a node's own position.
- **Distances in the inspector.** A distance or speed that is a setting is exported as a plain number, so the
  inspector can show and edit fractions of a pixel, and is turned into `Pixels` with `Pixels.FromNumber` once, as
  the values are filled. No arithmetic is done on the number.
- **What every placed thing shares** is in `EntityNode` (its place, its colliders, reading collider sizes from its
  data file), which is a set of functions the base classes call, not a base class.
- **Small interfaces name capabilities:** `IRoomEntity` (enters a room and has a place), `IPlaced` (present only
  when a condition holds), `IHittable` (takes hits), `ITriggerTarget` (can be told something by a `Trigger`),
  `IMoves` (a movement component: it is stepped and reports what it met), `IHasValues` (its values come from a data
  file).
- **Namespaces follow folders,** with two exceptions made so that code reads naturally: the player's files in
  `engine/actors/player/` are in `Engine.Actors`, because `Player` is both the folder and the class; and the
  project does not use C#'s implicit usings for threading, because `Barrier` is the engine's type.
- **An enemy can be asleep** (`Sleep`, `Wake`, its own methods): asleep, it is not stepped and cannot be hit, as
  one waiting for an encounter to begin. Separately, the room does not step an enemy further from the view than
  the room's active margin.

## 4. Values, resources and core

### Values (`engine/shared/`)

| Type | What it is | Holds and does |
| --- | --- | --- |
| `Pixels` | An exact distance along one axis | Whole pixels plus a fraction of a pixel, the fraction always a non-negative part of one pixel. Adding, comparing, replacing either part alone, converting for display. Never floats. |
| `PixelPosition`, `PixelOffset`, `PixelVelocity` | A place in a room; a distance and direction; a signed speed | Each is two `Pixels`, across and down. A velocity is the offset moved in one update. Position plus offset is a position; position minus position is an offset. |
| `PixelRect` | Integer rectangle | A collider's shape. |
| `Facing`, `Aim` | Left or right; where a weapon points | Plain enums used by every actor. No per-actor direction enums. |
| `Countdown` | A timer in updates | Counts down, reports when it ends. |
| `WrappingCounter` | A counter that wraps | Wraps by itself at its own limit. Used only where the wrap is something the player can observe. |
| `GameRandom` | The gameplay random generator | One per session. Cosmetics never use it. It draws from an `IRandomSequence`: the engine's plain one by default, or one the `GameDefinition` supplies when a game's outcomes must follow a particular sequence. |
| `GameAction`, `GameInput` | An action by name; one update's input | The engine names its standard actions; a game adds its own by name. A `GameInput` says which actions are held and which were newly pressed, and whether anything at all is held or was pressed; it can be given on without one action, for something that must not see it. |
| `GameClock` | The count of updates played | One per session. Anything that acts "every N updates" reads it. Nothing keeps its own global counter. |
| `HitKind` | A named kind of damage | The engine provides contact and hazard. The game names the rest (here: each beam property, missile, super missile, bomb, power bomb, each special attack, each body power). |
| `Hit` | One hit being delivered | A damage amount, the set of `HitKind`s it carries, its direction, and who caused it. |
| `Condition` | A yes-or-no question about the game's state, held as data | One of: a named fact in `Progress` is set; this room has no live enemies; the player owns a named piece of equipment; the player can carry some of a named amount; at least so many enemies have been killed in this room since it was set up; the room was entered at a named entry point; a named thing placed in this room is dead or open; or all, any or none of a list of conditions. |
| `RoomEntry` | How the player arrives in a room | Which room, through which exit or station, and what the player carries in (speed, state). The room transition builds it; the room and the player read it. An arrival by no way in gives the place itself (`At`): a demonstration that begins in the middle of a room. |

**Hits carry several kinds.** One shot can be several things at once. A hit carries every kind that is true of it,
general and particular together: a charged beam that is ice, wave and plasma carries beam, plasma, ice, wave and
charged.

- What is hit holds a list of reactions, each for a set of kinds. The reaction that applies to a hit is the one
  whose kinds the hit all carries and that names the most of them. A thing with no matching reaction takes the
  hit's full damage.
- A reaction says:
  - how much of the damage is taken: none, a fraction, all, a multiple;
  - any status it puts the thing in (frozen, for one), and when: always, only in place of dying, or never;
  - whether the thing is briefly unhittable afterwards, and for how long: after taking the damage, and after
    being put in the status;
  - whether the projectile that delivered it is turned back.
- So a simple enemy needs one or two reactions ("beam: none", "plasma: all") and an awkward one lists the exact
  combination it treats specially. Nothing is computed from the parts.
- Barriers and blocks use the same hits: each lists the kinds it accepts and the amount each takes off.

This was checked against the old code's fifty-two enemy tables. Most are a reaction per general kind; two give a
combination a result its parts do not predict; a charged shot's result replaces the uncharged one's rather than
scaling it; and freezing has exactly the three "when" cases above. Matching the most specific reaction covers all
of them, where combining reactions by arithmetic does not.

**Conditions are data.** Nothing writes its own check of the game's state. Whether a barrier is sealed, whether a
placed thing is present, which settings a room uses, whether a trigger fires: each holds a `Condition`.

### Resources (beside what they describe)

Each is a set of values, and so comes from a data file for this game (section 8, "Values from files").

| Type | Describes | Holds |
| --- | --- | --- |
| `EnemyStats` | An enemy | Health, contact damage, its reactions to hits, its `DropTable`. |
| `DropTable` | What something leaves behind | The chance of each `PickupType`, and of nothing. |
| `WeaponStats` | A weapon | The `Hit` it delivers, speed, cooldown, cost, how many may be live at once, and its projectile's settings. |
| `ItemType` | A permanent upgrade | What it grants, its message, its picture or the timeline that shows it, the sound of collecting it, and the fanfare played while its message shows and how long the message is held. |
| `PickupType` | A kind of drop | Which inventory amount it adds to, and how much. |
| `BlockType` | A reactive tile | Its rules (section 5). |
| `LiquidType` | A kind of liquid | How it slows movement, the ongoing damage it does, which equipment cancels each, and how it looks: the timeline of the strip drawn along its surface and that of the picture tiled under it, whether they brighten, darken or cover what is under them, and how far they move across each update where a room's liquid drifts. Liquids of one kind that are coloured differently are types of their own with one name. |
| `Area` | An area of the world as its map shows it | Its name, the fact that reveals its map, the cells of the map and the small picture each shows, which cells are secret, its markers (`AreaMarker`: a sign at a cell, one of the map's small pictures over the cell or a picture of its own at a place in the map's pixels, showing while its cell shows, only once its cell has been explored, or always), and the sheet the pictures come from. A room says which area it is in and which cell its top-left corner is. It may give a second sheet for cells that show but are not explored and a third for the cell the player is in; without them such cells are the explored pictures, dimmed. It may give labels (`AreaLabel`): pictures of their own laid over the map at a place in its pixels, each showing while one fact is held and another is not. |
| `FrameTimeline` | A timed sequence gameplay follows | Ordered steps, each with a picture to show, how many updates it lasts, and optional named cues; and where it loops or ends. |
| `DemoRecording` | One title-screen demo | The room it plays in, the entry point the player starts at, and the actions held for each stretch of updates. An action is newly pressed when it is first held; a stretch may say otherwise ("pressed": one pressed again without being let go, or held and never pressed). A recording may end "holding" its last stretch for good. `RecordedInputSource` plays it, giving each update's input. |
| `PixelFont` | One font | A picture of its characters, each one cell of the same size, in the order the font lists them. The game definition names the font the game's words are drawn in (`FontFile`); anything with a font of its own names it in its data. |
| `GameText` | The game's words | Every message, menu label, narration line and credit, by name, per language. |
| `GameDefinition` | The game the engine is running | The one thing the engine is pointed at: the first screen, the player's scene, the HUD, this game's lists (equipment and ammunition kinds, hit kinds, item types, bosses and story events, areas, music and sounds, input actions, options, extra game modes and session services) and its compatibility answers. Everything the engine knows about a particular game, it learns from this. |

**Drops follow need.** A `DropTable` is rolled with `GameRandom`, and what it offers depends on the player:

- A pickup the player has no use for (ammunition they are full of, or have no capacity for) is not offered, and its
  chance goes to the others in the way the table states.
- A table may favour a listed pickup while a listed inventory amount is below a threshold (energy, when the player
  is low).
- Both are rules of the engine's drop table, with their numbers in the game's data. A different game can leave
  them off.

### Core (`engine/core/`; `Escape` is this game's, in `game/escape/`)

| Type | Owns | Manages | Does not |
| --- | --- | --- | --- |
| `GameSession` | `Progress`, `GameRandom`, `GameClock`, `SoundRequests`, the current `Room`, the player, `Camera`, and any session-long service the `GameDefinition` adds | Building everything a play session needs from the `GameDefinition` and handing each object what it depends on. | Hold gameplay rules, or name anything of a particular game. |
| `GameLoop` | The fixed order of phases, and which of them run this update | Advancing the game one update at a time. Groups of things (enemies, projectiles, room objects, the player) can be stopped while the rest run; the game says what stops what. | Know what any participant does inside its call. |
| `GameMode` (base) | Its own state and screen | One top-level state of the game. Exactly one is in control; each names what it hands over to. | Reach into another mode's state. |
| `Progress` | Every remembered fact, by name, plus the map explored and play time | Answering and recording "has this happened". | Decide consequences. |
| `SaveFile` | The save slots; in each, the saved form of `Progress`, the player's `Inventory`, and where the player resumes | Reading, writing, copying and clearing slots. | Exist as live state during play. |
| `Options` | Button bindings, language, and the toggles the `GameDefinition` lists | Reading and writing them outside any save slot. | Change during an update. |
| `SoundRequests` | What gameplay has asked to hear and, as gameplay sees it, what is still sounding | Accepting or refusing each request by priority, and answering "is this sound still going". Asked to stop everything, every channel falls silent at once (the music plays on), as when the game is paused. Advanced once per update. | Play anything. |
| `ISessionService` | Nothing; it is what a session-long service of a game's is | Being begun again with the session, stepped once for each update of play after the room has been stepped, and asked whether it wants a mode of its own in control. The `GameDefinition` makes them; anything asks the session for one by type. One with something to do before the room acts is an `IUpdateOpener` as well: the room calls it as each of its updates opens, before the player's input is read, whatever is stopped. | Belong to a room. |
| `Escape` (this game's) | The escape countdown | Starting, counting, showing and ending the timed escape, and reporting that it is running. A session service; when time is up it hands over to its time-up mode. | Know which room the player is in. |

`SaveData` is the saved form itself: what a slot's file holds, captured from a session and put back into one. It
can give what it remembers as `Progress` of its own, for showing a saved game without loading it. A game saved to
resume at an entry point records the fact that it has been saved there (`SaveData.SavedAtFact`), which is in the
save.
`RoomVersion` is one set of a room's settings (its layout file, which of the layout's patches are in place, its music) with the `Condition` it applies under.

**The engine's modes.** The engine provides the modes any game has. This game adds title, attract demo, file
select, options, game over, time up, and one per cinematic.

| Mode | Does | Hands over to |
| --- | --- | --- |
| `PlayingMode` | Owns the room being played and steps it. Writes a save when one was asked for, recording first that the game has been saved at that place. A game says what is stopped while the rest runs by overriding one method. It can be started in a room at a given `RoomEntry`. | The mode after death when the player has died; a `MessageBoxMode` when a message was asked for; a `RoomTransitionMode` when the player entered an exit; the pause mode when pause is pressed while the player is in control.  |
| `RoomTransitionMode` | Fades out, takes the old room down and sets the next up with the player arriving at the exit the old one led to, fades in. Neither room is stepped meanwhile. | The same `PlayingMode`. |
| `PausedMode` | Nothing; play is still beneath it. A game's pause screen is its own mode doing the same. | The same `PlayingMode`, on pause. |
| `MessageBoxMode` | Shows a message from `GameText`; one that asks yes or no gives the answer to whoever asked. A message may come with a fanfare, after which the music there was comes back, and may be held up a set time before it can be dismissed. A game that names how its messages are shown (`GameDefinition.MessageBoxFile`, read as a `MessageBoxStyle`) has each shown as a picture through a window that opens and closes about a line of the screen: nothing shows for a set time, the window opens a step an update, the message is held, a plain message then goes at any action held and a question looks at the player's presses every so many updates (confirm answers with the choice showing, cancel answers no, the style's move actions show the other choice, with a sound the update after), the window closes a step an update, and after a set time more the mode beneath comes back. A question may name what follows a yes (`MessageAfterYes`: a sound, a wait and a second message), after which the answer is given. | The mode it was laid over: on confirm for a game whose messages are their words, once its window has closed for a game with a style. |
| `LoadingMode` | Begins the session's game again and puts the session's save slot into it, if the slot holds one. | A new `PlayingMode` in the saved room, or the first room. |
| `Cinematic` (base, `engine/ui/`) | Plays update by update; confirm skips it unless it says otherwise. | The mode it names. |

- **Laid over, or replacing.** A mode says which mode it is laid over (`Keeps`). That mode stays on screen, is not
  stepped, and is told when control comes back (`Resume`). Any other hand-over ends the mode that finished, and
  everything it was laid over.
- **Asking, not reaching.** A room object does not start a mode. It asks the session: `RequestMessage` for a
  message, `RequestQuestion` for one answered yes or no (the answer comes back to whoever asked, after the sound, wait and
  second message the asker may name to follow a yes), `RequestSave`
  for a save, `RequestPause` for the pause screen once the messages asked for with it have been shown. The mode in
  control takes the request and acts on it.
- **Death.** There is no death mode in the engine: the player plays its own dying, and when it is over
  `PlayingMode` hands to whatever the `GameDefinition` makes for after a death. The engine's default reloads; this
  game's is its game-over screen.
- **Where a game plugs in** is always the `GameDefinition`: the pause mode, the mode after death, the first mode,
  how a room is made, the folders its words, sounds and music are in, how many save slots it has, its own actions
  and the buttons they are bound to until the player chooses otherwise.

**What is remembered.** A room forgets everything when it is left: enemies return, blocks regrow, barriers close.
The only things that survive are facts in `Progress`.

- Anything that must survive has a setting, "remembered as", holding a name. An item collected, a boss defeated, a
  barrier opened for good, a story event: each is a name in `Progress`.
- The name is given where the thing is placed. It is unique across the whole game, and the game is checked for
  duplicates and for a remembered thing with no name when it loads. The `GameDefinition` lists every room's scene;
  as the game loop starts, `RememberedNames` looks at everything placed in each and the game does not start if any
  name is shared or anything that must be remembered (an item, a boss encounter) has none. Every mistake is
  reported at once, with its room and its thing. A new kind of thing that must be remembered says so itself
  (`MustBeRemembered`).
- A thing asks `Progress` about its own name when its room is set up and sets itself up accordingly. Nothing else
  restores it.

**Why, and extending it.** These are the things with no body: values passed around, definitions, and the session's
bookkeeping. `HitKind`, `Condition` and "remembered as" exist for the same reason: each replaces a pile of
special-case code with one piece of data that many types read. A hit that needs a new consequence is a new
reaction, available to everything that takes hits. A new question about the game's state is a new `Condition`
choice, available to everything that holds a condition. A new kind of definition is a new resource here, holding
values only. If you find yourself writing `if` over the game's state inside a barrier, a room or a trigger, the
condition is missing a choice; add it there, not in the caller.

## 5. World (`engine/world/`; this game's rooms and own object kinds in `game/`)

### Room

A scene. One per room of the game.

- **Owns:** its `Terrain`, its placed enemies and room objects, its exits, zones and triggers, its `Liquid`,
  `RoomEffect`s and `Backdrop` if any, its `RoomSequence` if any, its music, its place on the map, its
  `CollisionWorld`, and the lists of live enemies, projectiles, pickups and effects.
- **Manages:** setting itself up when entered, placing the player from the `RoomEntry`, admitting and removing live
  things, deciding which enemies are active (one far enough outside the camera's view does not update), marking the
  map cell the player is in as explored, starting an earthquake when something asks, putting itself on screen
  where the camera's view has it (thrown about by an earthquake, which does not throw the player about), and
  tearing itself down when left.
- **Steps, in this order:** the opening of the update: the liquid moves, each running sequence and then each live
  projectile with a part in the opening (`IOpeningStep`) plays it, and then the session services that open an
  update (`IUpdateOpener`) play theirs; the room effects, all but the earthquake, and the liquid's pictures; the
  first part of the player's update; the player's own shots; the enemies; the rest of the player's update; triggers
  and exits; every other projectile, pickups and remains; room objects, terrain, the earthquake and the sequence;
  effects.
- **Does not:** contain any enemy's or object's behaviour, or carry state to the next room. What crosses an exit is
  carried by the room transition, explicitly.

**One room, several versions.** A room can differ by what has happened: a world asleep or awake, a ship powered or
dead, a boss alive or beaten. It is still one scene.

- Anything placed in a room may have a "present when" `Condition`. When the room is set up, a thing whose condition
  is false is not there.
- The room's own settings (music, tile layout, liquid, effects, backdrop) are an ordered list of versions, each with
  a `Condition`; the first whose condition is true is used, and the last has none.
- There is never a second scene for the same room.

**A room's place on the map.** A room's settings say which `Area` it is in and which map cells it covers. The room
turns the player's position into a cell; `Progress` records it; `MapView` draws it.

### Terrain and blocks

- `Terrain` is the room's tile grid. It owns every tile's collision kind, the live state of every block, and the
  drawing of its tiles, including tiles that animate.
- The tile layout is an asset. A room names its layout; `Terrain` loads it, in the editor and in the game. A room
  scene never stores its tiles.
- A plain tile is data: empty, solid, or a slope with a height per pixel column: the pixels of the column that are
  solid, counted up from the tile's bottom, or down from its top for a ceiling. A floor's slope may let only a share
  of a move across it through (`ground_share`, in 256ths), which the player's movement takes off. A slope may say
  that it holds things up by their middle (`"holds": "middle"`): to a mover that treats slopes so
  (`SlopeTreatment.ClimbUnderMiddle`, which the player's body does) only the middle column of the body meets it, so
  the body's edges pass into it and the mover stands where its surface is under their middle, and such a mover
  climbs only slopes of that kind; a slope that does not say so is a step or a wall and meets the whole body. To
  everything else a slope is solid along its surface, whichever kind it is.
- A **block** is a reactive piece of terrain, one or two tiles wide and high, placed as one unit with a
  `BlockType`. It is not a node. `Terrain` owns whether each block is intact, broken, hidden or regrowing.
- A `BlockType` is a combination of these rules and nothing else. The rules are the engine's; the hit kinds, powers
  and abilities a block type names are the game's:

| Rule | Choices |
| --- | --- |
| Solid or passable | Solid; passable. |
| Broken by | Nothing; a `Hit` carrying a listed kind; being stood on (crumble). |
| After breaking | Gone until the room is next set up; regrows after a set time. |
| Hidden until | Never hidden; hit by a listed kind; seen by a revealing ability, which shows it for what it is only for as long as it sees it. A block with a picture of its own for what it is starts hidden whether or not anything reveals it, and shows that picture once it has broken and come back, unless its type says it hides again. |
| How it looks | The pictures of the tiles it lies on; a picture of its own once revealed, or one for each of its tiles; a timeline of pictures from the moment it breaks; a timeline of pictures up to the moment it comes back. |
| Heard | Nothing; a sound as it breaks. |
| On contact | Nothing; hurts, by a hit that is not repeated while its victim is still safe from the last (spike), by ongoing damage, or both; carries sideways, always or only what is not rising or falling (conveyor); pulls down (sand), and may hold up what has its feet in it as ground does, pull harder what rises or falls in it, cap how fast it lets anything rise, take away momentum, and be eased by equipment (quicksand); the player is moved by the blocks their body is in each update, over and above their own movement; can be latched onto; can be latched onto once and then breaks, when it is let go of or a set time after it was first latched onto. |

- A block is drawn at every tile it covers, and its type's pictures are the same at each. A type's pictures may
  name the room's set of graphics by its placeholder, so one type serves every set.
- `Terrain` hands the room the sounds of the blocks that broke each update, and the room asks for them.
- `Terrain` does not know the player, enemies or weapons by name. It is told "this rectangle touched you" or "this
  `Hit` landed here" and answers from the block's rules. The player's body breaking a block is a `Hit` like any
  other (section 6, "The body as a weapon").

**Tiles that move as they are looked at.** A layout's tile may name several pictures and how long each shows:
lava's surface, spikes, a treadmill. That changes only what is drawn. A tile may also name the picture it shows, or that it shows nothing, for as long as a revealing ability sees it: a wall that is air shows as nothing to the X-ray scope. `Terrain` is told which tiles are seen each update, as it is for blocks.

**Terrain shown shifted.** `Terrain` can be shown a few pixels from where it is, for a wall that rattles; that
changes nothing about where it is.

**Terrain that changes.** A layout file may hold named patches: a rectangle of tiles to put in place of what is
there. Anything may tell `Terrain` to apply a patch or take it back by name, and may ask whether the layout has
one of a name: a boss dropping the ceiling, a sequence opening a wall, a bridge giving way, a statue's feet
breaking through one block after another (a patch for each block that can be broken). Nothing edits tiles one at a time, and nothing but `Terrain` holds
tile data. A patch applied for good is remembered like anything else.

**Terrain that changes over some updates.** A change that plays out, such as a wall crumbling through several
pictures before it is gone, is a `PatchSequence`: named patches applied one after another, each standing its own
number of updates, with a sound a step may ask for. It is data of whatever sets it off, which hands it to the room
to play; the room steps it once an update until it is over, so it outlasts a boss that is gone before its wall has
finished falling. The patch that is left is the one a room that begins after the change is given.

### RoomObject (base)

A placed, interactive fixture of a room that is not an enemy. One folder per kind: `engine/world/objects/<name>/`
for a kind any game would use, `game/objects/<name>/` for one only this game has.

- **Owns:** its position, its colliders, its own state machine, its visuals, its settings.
- **Manages:** its own reaction to the player, to hits and to `Progress`.
- **Does not:** act on its own as an enemy does, or change another object's or the room's state except through
  their methods.

| Kind | Manages | Settings |
| --- | --- | --- |
| `Barrier` | A solid object with health that only certain damage wears down, or that opens when told to. | Below. |
| `Item` | Granting its `ItemType` when the player touches it, recording it in `Progress`, showing its message. | `ItemType`; remembered as. |
| `Station` | Holding the player while it works, then granting or recording the result. | What it does: save (and the entry point a game saved there resumes at); reveal the area's map; refill listed inventory amounts. How it is used: by standing in it (it puts the player at its middle, facing out; used or refused, it waits for the room to be entered anew, as it does when the game resumed at it), or by running against either side of it, facing it and aiming ahead (it is then solid over its own area, by the solid collider of its scene). Which way the player must face; a question it asks first; the message it shows when done, how long that is held, and whether the game then pauses on its pause screen; how it looks at rest, at work, and at rest once it has nothing more to do (a map station whose area's map is revealed, which cannot be used again); an effect it leaves as it starts, which may be another for its right side; and its sounds. A refill station does nothing for a player who is full. |
| `Carrier` | Taking hold of the player and moving them along a path, then letting go. A player who arrives where its path ends is brought back along it to where it rests. Its data's timelines show it at rest and moving. | Path; speed, and how much it gains each update up to what limit; what starts it (standing in its region, or a press of a named action there); where it holds the player; its sounds. |
| `Platform` | A solid surface that moves, and so carries what stands on it. | Path; whether it reacts to weight. |
| `Spawner` | Producing enemies at a place by a rule. | What it spawns; how often; how many at once. |
| `Hazard` | Hurting or grabbing the player on contact, without being killable. Its data's timelines show it at rest, holding and letting go; one that holds by its timeline bites on that timeline's cues, and one whose reach changes as it is shown says so by cues ("area:left:top:width:height", "safe"). | The `Hit` or ongoing damage it does (and whether its contact is a hit even when it does no damage), or its hold: for how long, whether at one place, what each bite takes, and how long the player is safe after; the most updates it waits, by chance, before it first appears. |
| `BossEncounter` | Changing the music, starting the boss, and recording the defeat in `Progress`. | Which boss; remembered as. |
| `Creature` | A harmless animal that moves by its own behaviour and cannot be hurt. The base shows the timelines its data names, passes it their cues, and moves it through terrain. | — |
| `Scenery` | A fixture that reacts visibly to the player or events, with no effect on play beyond an optional solid body. | — |
| `Gunship` (this game's) | Landing, taking off, saving and refilling. Its question of saving, answered yes, is followed by the sound of saving, a wait and its message of having saved before its hatch opens again. | — |

Every kind here is the engine's except `Gunship`.

**Barriers and exits.** A door is two separate things, and neither knows about the other.

- A `RoomExit` is a region at the edge of a room. When the player enters it, the room transition begins. Its
  settings are the room and exit it leads to, and how the transition looks: scrolling across, scrolling up or down,
  or fading. Every way out of a room is one, whether or not anything stands in front of it. The exit a player
  arrived at takes no one until they have been in control and clear of it once, so that a player something brings
  in through it is not taken straight back.
- A `Barrier` is a solid object with health that only certain damage wears down. It stands wherever something must
  be got through. It knows nothing about exits. One opened and remembered as plain comes back as a plain one: its
  data may give it other timelines and other kinds of hit that open it for then, as a red door's cap comes back
  blue.

| Barrier setting | Meaning | In this game |
| --- | --- | --- |
| Health | How much damage opens it. | A blue door, 1. A red door, 5. |
| What damages it | The amount each accepted `HitKind` takes off. A kind not listed does nothing. | Blue: any, 1. Red: missile 1, super missile 5. Green: super missile only. Yellow: power bomb only. |
| Held shut by | An optional `Condition`. While it is false, nothing damages the barrier and it shows as sealed. | Grey doors: enemies cleared, a boss defeated, a story event. Eye doors: the eye, an ordinary enemy, is dead. |
| Opened by | Its health running out; or being told to open by a trigger, a switch or a sequence; or the player coming within a set distance of it, in which case it closes again when they have gone. | Shutters and gates are told to. The Ceres doors open when Samus is near. |
| When open | Gone; or slid aside and able to close again. | A door cap is gone; a gate slides. |
| Comes back | Never; when the player has passed; when its room is next set up. | A door cap returns when the room is next set up. |
| Remembered as | A name; once opened and remembered, it is set up open, or as a plain barrier. | A red door, once opened, is a blue door from then on. |
| Tells when opened | Things told a word as it opens. | A gate's switch tells its gate to open. |
| Looks and sounds | From its data: how it shows shut, held shut, hurt, opening, open and closing, and the sounds of opening, closing, a hit that wears it down and a hit while it is held shut. One that shows itself opening goes on blocking until it has, and one held shut as its room is entered may show itself shutting first. | A door cap flashes when a missile wears it down and swings open. |

Door caps, gates, shutters, breakable walls and the shell around a held item are all `Barrier`s.

**Things built from these, not types of their own.** Apply move 1 before adding a kind.

- **An elevator** is a `Carrier` whose path ends at a `RoomExit`, started by a press of up or down. The room it
  leads to has a carrier of its own whose path ends where the player arrives, and it brings them to rest. The path
  runs on past the exit, so the exit takes the player on the way; where it ends is where a player coming the other
  way arrives.
- **An item held in a shell** (a statue's orb) is an `Item` with a `Barrier` around it. The item cannot be touched
  until the barrier is open, because the barrier is solid.
- **An item hidden in the wall** is an `Item` behind a block. It cannot be seen or touched until the block is gone.
- **A boss room's locked doors** are `Barrier`s held shut by "this boss is not defeated". The `BossEncounter` does
  not touch them.
- **A switch** is a `Barrier` with one health and no body to speak of, whose opening tells another barrier to open.
  A gate's switch never opens itself: each hit that would open it tells its gate to "toggle", which opens a shut
  gate and shuts an open one, and is ignored while the gate is still moving.

### RoomSequence (base)

A scripted sequence belonging to one room. It lives beside its room's scene.

- **Owns:** its own timeline and progress through it.
- **Manages:** calling the methods of the room's objects, terrain, effects and camera at the right moments, and
  recording a story event when it finishes. One with something that must happen before anything else in an update
  is an `IOpeningStep` as well: the room calls that part as the update opens, while the sequence is running, and
  the rest where every sequence is stepped.
- **Does not:** hold behaviour that belongs to an object in the room, or exist outside its room.

### Other room parts

| Type | Owns and manages | Does not |
| --- | --- | --- |
| `RoomExit` | Above. | Have a body or a look, or know what stands in front of it. |
| `Liquid` | A room's body of liquid, of a `LiquidType`: its level, its rise and fall, and answering "is this point submerged". Its surface moves as an update opens, before anything acts; its pictures play on with the room's effects. Its type names the splash of something crossing its surface and the sounds of going under and of coming out, which the player leaves as their feet do. It may wait a set time before it moves once told, and may have a tide that lifts and lowers its surface. It draws itself across the view as its type says, and says whether its pictures drift. | Apply physics or damage. The player and enemies ask it and act on the answer. |
| `RoomEffect` (base) | A room-wide condition: its look, and reporting that it is active and any ongoing damage it does. It is stepped early in an update, before the player and the enemies act, so that what it is told during an update it first shows in the next; the earthquake alone is stepped late, after the enemies and room objects that may start it, and shows a shake on the update it is started in. The engine provides the base, the earthquake (side to side, up and down, or corner to corner), `OverlayEffect` (pictures tiled over the view and drifting, as the data file of its look says: which pictures in turn, how far they move each update and with which of several winds, how much they stay with the room as the view moves, whether they brighten, darken or cover what is under them, and whether they lie behind the room's terrain: fog, rain, spores, a haze; one with no look shows nothing: heat) and `TintEffect` (a colour over the view that steps through a list: alarm lights, a room going dark); a condition with rules of its own is the game's. | Reach into the player or enemies. |
| `Backdrop` | What is drawn behind and in front of the room: background layers, a scrolling sky, parallax. A layer may lie at a place in the room, fill only a band of it, and drift by itself, which is how a sky's bands of cloud are made. `TurningFloor` is the backdrop that can be turned and brought nearer or farther about a point, with the visuals under it turning with it. | Affect play. |
| `RoomTurn` | A turn the room is shown with (`Room.ShownTurn`): the player and the room's objects are drawn where turning them about a point puts them. | Change where anything is. |
| `Camera` | The view onto the room. A room is a grid of screens, 256 pixels each way, each closed to the view, open, or open with the view aligned to its top (`ScreenScroll`). Each update, once everything has moved, the view follows the player by how far they moved and a pixel more, toward keeping them at the place in the view the room gives (`Room.PlayerInView` by the way the view leads, `ViewAboveWhileRising`, `ViewAboveOtherwise`); it is stopped by the room's edges and by closed screens, and on an update the player did not move a whole pixel it drifts clear of a closed screen by itself. Its place has a fraction of a pixel. It can be placed (`Place`), moved by a script (`MoveTo`), told where the player is to be taken to have stood (`TakePlayerToHaveStoodAtX`, `...Y`), made to follow slowly (`TrackSlowly`), and kept at or below a height (`ViewFloor`). A room may hold the view at one place instead (`Room.ViewHeld`, `ViewHeldAt`). One per session. | Affect play. |
| `IScrollCells` | How each screen of the room being played lets the view in: what the camera reads and what zones write. The engine's plain keeper (`RoomScrollCells`) holds the room's own screens, loaded from the room's version (`RoomVersion.Screens`), and every screen outside the room is closed. | Know why a screen changes. |
| `CameraZone` | One screen of a room, by where it stands, as something that can be told how to let the view in: "close", "open" (the way the zone opens, `OpensTo`), "open_fully", "open_to_top". It may be told something as the room is set up (`ToldAtSetup`). It may stand below the room's last row, for a game that keeps such a screen's state. | Hold the screen's state, which is the room's. |
| `ViewFloor` | A height the view's top is kept at or below while it holds; told to "release" or "hold". | Anything else. |
| `Trigger` | A region that calls one named method when the player enters it, optionally only when a `Condition` holds. | Hold logic. |
| `Relay` | Turning a word it is told into what the room does about it, by its rules: telling another thing a word, putting a terrain patch in place or taking one back, changing the music, shaking the room, putting a thing into the room, recording a fact. A boss that only knows it has woken tells a relay; the relay's rules open the camera zone and close the door. | Hold behaviour, or accept a word it has no rule for. |

**Why, and extending it.** A room is a container that forgets. That one decision is why "remembered as" and
"present when" exist: instead of each object managing its own persistence and each room having variants, there is
one rule for surviving and one rule for being there. The room object kinds are deliberately few and general,
because most fixtures of the old game turned out to be a `Barrier`, a `Carrier` or a combination. Before adding a
kind, try to build the thing from existing kinds placed together; the list under "Things built from these" is the
pattern. A fixture needs its own kind only when it has behaviour none of them has. When a barrier, carrier or
station needs to do one more thing, that is one more setting or one more choice within a setting. When a block
needs a new rule, it is a new choice in the block rules table, and the question is whether it is still "a tile
with a rule" or has become an object; if it needs its own state machine, it is an object.

## 6. Actors (bases in `engine/actors/`; this game's actors in `game/`)

### Player (base)

The engine's player controller: what a metroidvania's player character is. A real class with real behaviour. Lives
in `engine/actors/player/`.

- **Owns:** position and velocity, facing and aim, the state machine that says what the player is doing, the
  `Inventory`, the `Weapons`, health, hurt and invincibility, ongoing damage, colliders, and the `ActorVisual`.
- **Manages:** turning input into movement and actions, moving with the shared `Mover`, firing, taking a `Hit`,
  collecting pickups and items, and applying to itself what the room reports.
- **Provides the standard states**, each working and tunable: standing, running, crouching, crawling, changing
  posture, turning, jumping, falling, turning in the air, wall jumping, knocked back, held, carried, pushed, using
  a station, scripted, dying.
- **Standard mechanics are properties of the controller**, switched and tuned by the game, not code the game
  writes: whether the player can wall jump, crouch, crawl, or jump while crawling, and each one's numbers. Crawling
  is the low form: a smaller body that fits low gaps, with its own movement on the ground and in the air, and the
  change into and out of it. A game that shows its low form as something else supplies the look, not the mechanic.
- **A mechanic may need equipment.** Each of those mechanics has a second setting naming the equipment that must be
  switched on for it to work, empty when it needs none. The states ask "may the player do this now", which is the
  switch and the equipment together. A game ties an item to a mechanic by naming it there, with no code.

| System | What the engine does | What the game supplies |
| --- | --- | --- |
| States | The state machine, the standard states, and the rule that a state holds its own data. | Extra states of its own. |
| `Inventory` | Equipment owned and switched on, energy and reserves, ammunition and capacities; saving and loading. | Which equipment and ammunition kinds exist. |
| `Weapons` | The selected weapon, charge, cooldown, cost, the limit on live projectiles, combining switched-on properties into one `Hit`, and spawning the `Projectile`. A weapon that charges fires a plain shot on the first update its attack is held and counts the hold from there; let go, it fires charged, or plain where its data says a part charge fires. One that does not charge fires on a press, and while held where its data gives a wait for that. A variant may have its own sounds and waits, and may say the weapon does not charge while it applies. | Each weapon and each property. |
| Abilities | Equipment changing what a state does, and an ability with its own lifetime running as a child of the player. | Each ability. |
| Body powers | Below. | Each power. |
| Ongoing damage | Below. | Which equipment cancels which source. |
| Being moved | Anything that moves the player against their will asks for the matching state and passes its data. Nothing sets flags or velocities on the player. | Extra such states. |

- **The order of an update.** A state does not change the moment a button is pressed. Each update: the state the
  player is in is asked what the input wants of it (`Asked`: another state, itself to carry on, or nothing); the
  player then moves as the state they are still in (`Move`) and settles where that leaves them (what they stand on,
  what the blocks they are in do to them, the room's ongoing damage); and only then does a change take effect, by
  this rank: the state's own end (`Ended`, a turn done); something that interrupts the player (`Player.Interrupt`:
  a knockback, and whatever else a game throws them with), which stays waiting when the state's end comes first;
  what the movement itself met (a landing, a floor walked off); what the input asked for; and last, when the input
  asked for nothing, what the state does unasked (`Unasked`). The state entered sets itself up as the update ends
  and first moves on the next, so a jump asked for while running still runs that update and rises on the next. A
  game's player asks for states of its own through `ChooseState`, ahead of the state's own asking. `ChangeState`
  is for whatever takes the player over at once (a hold, a script, a station) and drops an interruption that was
  waiting.
- **The update has two parts, with the enemies between them.** In the first (`BeginUpdate`) weapons fire, from the
  state, the place and the aim the last update left the player in, and then the input is asked. The room steps the
  player's own shots, then the enemies. In the second (`FinishUpdate`) the player moves, the change takes effect,
  and active body powers are delivered. So an enemy acts on where the player stood before this update's movement,
  a touch it lands interrupts the player on this same update, and a state an enemy puts the player into moves them
  on this update, having been asked nothing. A game says which body powers are at work as the second part begins
  (`BeforeMoving`), so they hold for whatever touches the player until then.
- **Speed across.** A state that steers keeps the player's own speed across as a size, carried the way they
  travel, with one of three conditions (`Steering`): *steered* (it gains by the state's move values, and the top
  speed replaces a speed that reads as past it); *unsteered* (nothing drives it: on the ground it slows to rest and
  is steered again, in the air it carries on as if steered); *reversing* (the player has turned and still moves the
  old way, against the way they now face, slowing to rest, then steered). A speed slowed exactly to rest keeps its
  condition one update more. `Player.Steer` and `SteerInAir` are the two rules; how a speed is read against its top
  and a dash against its cap is the game's to say (`ISpeedReadings`, from the `GameDefinition`; the engine's own is
  a plain comparison). A dash is a second speed on top, carried the same way, kept through the air while the player
  has momentum; `FoldDash` adds it into the player's own speed and leaves none. Meeting a wall brings all of it to
  rest. States that throw the player about (a knockback, a push, a script) set `Velocity` outright.
- **Standing** keeps no speed: each update a dash's speed alone still carries the player, and then everything across
  is at rest. **Running** builds the dash while its action is held. A runner whose input asks for nothing still moves
  that update at full speed; then, if they had speed, they stay running, the dash is folded into their speed, their
  momentum ends, and they slow, unsteered, each update, even if steered again, until past rest; they then gain one
  step and stand. A runner who lets the direction go but still aims stands at once. A runner stopped by a wall
  stands. **Turning** on the ground faces the new way from its first update and reverses by the turn's own move
  values for as long as the turn lasts, ending the player's momentum on each of its updates; it ends standing, and
  its end gives way to a jump asked for on its last update. Letting the new direction go during a turn makes the
  player steered for the rest of it. With no turn time set the player turns at once and runs on, reversing.
- As a change the input asked for (or the state's unasked carrying on) is about to take effect, the game's player
  is told (`Entering`) and may do something of its own or name another state. A state the input asked to carry on
  is told so (`CarriedOn`), which is where a change within a state takes effect.
- **Up and down.** A rise is a rise until something ends it (`Player.IsRising`): gravity slowing it to rest, or past
  rest, does not. The state the player is in turns it into a fall from rest on the update after it has passed rest
  (`TurnAtTop`), moving nothing down that update, so a rise that gravity brings exactly to rest takes one update
  more; letting go of the jump while rising cuts it to the game's cut speed, and where that is none it is a fall
  from rest at once. A move down is by the speed the player had; gravity then acts on that speed for the next update
  (`MoveDownUnderGravity`); a fall gains up to the fastest fall, and a rise past its top that no state turns gains
  without that limit. Landing (`Player.Land`) brings the player to rest across and makes them steered; a dash is
  left for standing to spend.
- **In the air.** A jump keeps the jump's own move values from its start until the player lands: `JumpingState` is
  the whole jump, and `FallingState` is for walking off a floor and for being let go in the air. A jump from the
  ground may first gather itself for `jump_start_updates`, moving nothing of the player's own. Both steer by the air
  rule (`SteerAndMoveInAir`): steered with neither direction held, the player stops across at once and does not move
  across that update; unsteered or reversing they carry on. A player taking up a new stance in the air (`AirStance`:
  their aim, and aiming level in a jump whether they steer the way they face or hold the attack, `AirArm`) has their
  steering set afresh: unsteered while a dash still carries them, else steered. A jump holds a stance from its
  start: one from the ground starts in the stance of the aim the player has as it begins, their arm at rest, and
  holds it while it gathers, the input asked nothing; as the gathering ends the game's player is told
  (`JumpGathered`) and may name another state, which takes effect as a state's own end does. A jump that carries on
  from something else starts in the stance it is given (`JumpingState(AirStance)`), or failing that the one the
  input asks for as it begins. A change of stance within a jump takes effect only when the body can take the form
  of the new one, and the jump says so for that update (`TookUpStance`), which the game's player can read as it is
  told (`Entering`). An update with nothing at all held makes a jumper who has speed unsteered
  (the dash is folded into their speed and their momentum ends) and takes a faller's dash and momentum. Steering
  against the way they face turns them in the air (`AirTurningState`): they face the new way at once and reverse by
  the turn's move values for as long as a turn lasts, ending their momentum each update; the turn cannot land, is
  not cut short, keeps its upward speed against a ceiling, and gives way to the jump or the fall it came from, a
  jump aiming level then being in the stance of steering the way they now face. A landing ends standing whatever is
  held.
- **Postures.** Crouching down, standing up, taking the low form and leaving it each take the updates the game
  gives them (`crouch_updates`, `stand_updates`, `crawl_enter_updates`, `crawl_leave_updates`; none is at once). For
  that long the player is in `ChangingPostureState`: the body has its new form from the first update, the input is
  not asked, they cannot attack, and they move across only as far as a dash carries them, their own speed kept; a
  change into or out of the low form made in the air goes on rising and falling. The low form is taken as from a
  crouch wherever the player is: the body shrinks about its middle and the middle drops by as much as a crouch is
  taller, or as far as there is room. Leaving it leads to a crouch even in the air, which then walks off into a fall
  from rest. **Crouching** keeps no speed across: a dash still carries the player for the update and is then spent,
  while their momentum lasts until an update whose input asks nothing of the crouch. A press of the direction
  behind turns them round crouched (`TurningState`, crouched, which ends in a crouch and listens only for whether
  anything is held); the direction ahead held stands them at once, with no change between. A fall entered by
  walking off a floor starts from rest unless the player is rising.
- **The low form** rolls on the ground while a direction is held, by the ground rule, turning by reversal; let go,
  it loses everything across after that update's move and sits still. Rolled off a floor it falls from rest. In the
  air, steered with no direction held it loses everything across; where it cannot jump it otherwise keeps falling
  unsteered once an update has asked nothing of it. Coming down at `crawl_bounce_fall_speed` or faster it bounces
  at `crawl_bounce_speed`, and on its next landing once more at `crawl_second_bounce_speed`, and is on the ground
  at the landing after; slower, it is on the ground at once, at rest across. Where it can jump, the jump is a rise
  cut short like any jump and steered by its own move values (`crawling_jumping`) for the whole jump, and landing
  with the jump held jumps again at once. Up leaves the low form, and so does the jump for a player who cannot jump
  in it.
- **On the ground.** The player's body meets terrain in one way everywhere (`Player.BodyMoves`): sloped ground
  that holds things up by their middle holds only its middle column, and is no wall to it. A move across climbs
  such ground by up to `climb_per_pixel` for each pixel. A player with no
  speed up or down who is about to carry the leading edge of their body into sloped ground moves only the share of
  the distance each such slope lets through, counted in 256ths of a pixel with the finer part dropped. Once they
  have moved across, the state they are in presses them to the ground (`PressToGround`): down by as far as they set
  out to move across and one pixel more, stopping at ground. When nothing stops them they have walked off the floor,
  already that far down, and fall from rest.
- **In a liquid.** The player is in a liquid when the pixel row tested (their feet's, or their head's) lies below
  its surface line, not on it (`Player.IsUnder`). Their medium is found afresh each update, before they move. A
  medium may give its own lengths for the timed changes (`"updates"`, by the name of the value that gives the length
  elsewhere: a turn, a change of posture, a jump's gathering), which a state reads as the change begins
  (`Player.InMedium`).
- A player aiming straight down in a jump or a fall takes the form `aiming_down` where the game gives one, a
  smaller body, and the form of the jump or the fall again when they aim elsewhere and it fits.
- **A jump held a little early.** Where the game sets `jump_buffer_updates`, a jump that has been held for fewer
  updates than that as the player lands on their feet, or as a turn on the ground ends, counts as pressed afresh on
  the next update (`Player.JumpPressed`, which every state asks instead of the input). A jump pressed on the very
  update of the landing is not counted. A player who has stood since landing for as long as a landing takes to
  settle (`landing_updates`) is given the same chance once more. How long a landing takes to settle may differ by
  the state it ends, which the game's player says (`SettlingUpdatesAfter`, asked by `AirStance.FallAndLand`).
- A wall jump of the plain kind leaves the wall at a set speed, facing away, with steering locked for a moment, and
  then carries on as a jump. One of the steered kind (`wall_jump_steered`) leaves it with the player's own speed
  across at rest (a dash and momentum kept) and the way they face unchanged, gains speed by the wall jump's own move
  values whether or not a direction is held, and lasts for as long as the jump is held; let go, it carries on as a
  jump.
- **A knockback** interrupts: it takes the player when they have next moved. It throws them away from the hit,
  facing as they were, for as long as the player's knockbacks do unless whoever knocks them back says how long. It
  does not start from rest: whatever speed across they had goes on changing by the usual ground rule with the
  knockback's own move values and carries them the way they are thrown, a dash still on top; each update they are
  made steered again and their momentum ends. It rises by the medium's knockback speed under gravity with no turn
  at its top, unless the player was holding the way they face as it began, which keeps it low: they are pressed
  down by as far as they move across and a pixel more each update instead. A floor or ceiling stops the speed
  across and the rise, not the knockback. When its time is up they fall from rest, and whatever the input asked
  that update is dropped; an upright player is then set down for one update (pressed down once more) and stands if
  that met ground or falls from rest. The low form is thrown in its own form by its own move values, always
  upward, and goes straight back to its own movement. Some states hold an interruption off (`HoldsOffInterruptions`:
  the turns and the changes of posture); it waits, and a knockback that waited is shorter by as long as it waited
  (`StillInterrupts`).
- The effect left at the player's feet on landing may come from their data.
- Facing and aim are separate values, never part of a state's name.
- Something that has hold of the player can ask what a hit would cost them as equipped (`EnergyLostTo`), make them
  wait before their next shot (`Weapons.WaitAtLeast`), and halve their speed across a number of times
  (`SpeedHalvings`).
- **Does not:** know any enemy, boss, room object or item by type. It contains nothing only Samus does.

**The body as a weapon.** A body power, while active, makes the player's own body deliver a `Hit` of a named kind
to whatever it touches: enemies, barriers and blocks alike. The engine knows that a power is active, what hit it
gives, and that the player takes no contact damage from what that hit kills. The game says what makes each power
active. Nothing else treats the player's body as an attack. An active power's hit is also delivered to the blocks in
the way of each move before the player makes it, so the body breaks what it can as it meets it and is not stopped by
what it breaks.

**Ongoing damage.** Damage that arrives a little every update (heat, a damaging liquid, spikes held against) is
not a stream of hits. Each source adds its amount to the player's ongoing damage for the update, naming itself;
equipment cancels sources by name; the total is taken off once, at one point in the update, with fractions carried
to the next. Enemies that take ongoing damage use the same mechanism. On each update the room itself (its heat, its liquid) does the player ongoing damage the player is told so (`OnRoomDamage`), which the game's player answers with its sound.

**Input actions.** The engine names the actions its standard states need: move in four directions, jump, attack,
aim, crouch, and pause, confirm and cancel for menus. The game adds its own to the `GameDefinition` (here: run,
item select, item cancel, angle up, angle down). States read actions, never buttons; `InputSampler` turns buttons
into actions by the player's bindings.

### Samus

This game's player: `Samus : Player`, in `game/samus/`. She is the engine's player controller plus what is hers.

| Group | Her states, added to the standard ones |
| --- | --- |
| On the ground | Moonwalking. |
| In the air | Spin jumping. |
| Abilities | Grappling, shinesparking, crystal flash, X-ray. |
| Done to her | Bomb jumping, drained, grabbed, shoved (thrown aside by Ridley as he leaves Ceres). |
| Her weapons' own ways | A full charge with power bombs selected and just one of the wave, ice, spazer and plasma beams carried lets loose that beam's special attack (`SpecialAttack`, four lights that move about her by their kind). A full charge carried into the morph ball throws a spread of five bombs, higher the longer down is held (`Bomb` can be thrown). A shinespark's crash holds her while two echoes of her circle, and then lets them fly (`ShinesparkEcho`). |
| Not in control | Facing forward where something puts her (in the gunship, on the pad that sets her down in Ceres), collecting a suit, refilling from reserves. |

- **The morph ball is the engine's crawl**, shown as a ball; the morph ball item switches it on, and spring ball
  switches on jumping while crawling. Wall jumping and crouching are the engine's, switched on for her. Her scene
  says so: crawling needs `morph_ball`, jumping while crawling needs `spring_ball`.
- **Her crouch and her ball.** Crouching out of a run at full boost stores a shine, as the crouch begins; the dash
  then slides her through the change and the first update of the crouch, and her boost lasts while the crouch is
  held aiming. A jump from a crouch with her arm level starts `crouch_jump_lift` pixels off the ground. In the air a
  press of down while already aiming down, or in a wall jump, curls her up.
- **Her boost** is counted in strides of her run. The stride's ten pictures each last their own updates
  (`run_stride_updates`), or the dash's (`dash_stride_updates`), or, with the speed booster on, those of the stage
  of the boost she is at (`boost_stride_updates`); a run taken up afresh starts the stride again. Each stride
  completed with momentum and the dash held counts one off the stage (`boost_stride_counts`), and with none left
  the stage is done; at the last she is boosting. A dash begun partway through a stride reaches its first stage as
  that stride ends.
- **Her liquids.** Her feet in lava end a run the speed booster has begun to boost, and its dash, whatever suit she
  wears. Her landing, on her feet or as a ball, leaves its dust and makes its sound only if it is a hard one or came
  down at other than a whole number of pixels an update.
- **Her damage boost.** In a knockback, the jump and the direction behind her held together turn her that way and
  carry her off steered like a jump by the boost's own move values, with the speed and the rise the knockback gave
  her. What the input asks of it is the first of these that holds: the jump with left, then the jump with right,
  each keeping the boost when it is the way the boost carries her and otherwise leaving it, facing as she did, for
  a jump in the stance of steering that way; anything else leaves it for a jump with her arm at rest. When the
  knockback she left runs out her rise ends at once, and she flies on and falls. It holds off interruptions.
- **Her jumps.** A jump from standing or a crouch is a plain jump whatever is held, gathered for one update; a jump
  from a run is a spin, and so is a turn on the ground while the jump and the way she then faces are held. The
  first pose of a plain jump aims as the press chose (`Samus.Entering`): from standing, up with up held, else
  diagonally up with the angle-up action, else diagonally down with the angle-down action, else level; from a
  crouch or a turn, level. A spin keeps her own speed across only while its top speed capped it this update, while
  she is reversing, or while the way she faces is held; otherwise that speed is cleared, and a dash still carries
  her. What the input asks of a spin is the first of these that holds: the attack pressed afresh straightens her
  out into a jump with her arm held out level, whatever else is held; the attack held with up, with down, with the
  angle-up action or with the angle-down action straightens her out aiming up, down, diagonally up or diagonally
  down; the jump and the way she faces held keep the spin; up, the angle-up action, the angle-down action or down
  straightens her out aiming so; the way she faces keeps the spin; the other way turns it (dash folded in, momentum
  ended, reversing). She is straightened out once the update is done.
- **Her stored shine** is spent as a jump takes up a stance aiming level with her arm at rest, aiming up or aiming
  diagonally up (`ShinesparkState`), and at no other time: as the first pose of a jump from the ground has gathered
  (`Samus.JumpGathered`), so the update after the press, and not when that pose aims diagonally down; as such a
  stance is taken up within a jump; as a spin or a wall jump is straightened out into one where the taller body
  fits; and as a damage boost is left with her arm at rest. A jump from a run is a spin and spends nothing. Begun
  out of a jump, its first pose included, the windup starts `shinespark_lift` pixels higher, moved there whatever
  is above her.
- **Her wall jump** is from a spin only. A wall within `wall_jump_reach` on the side away from the direction held
  makes the spin ready from the next update for `wall_jump_ready_updates`; ready, a fresh jump with the wall still
  there jumps off it, and that update she moves neither up nor down. A spin held steadily from the ground does not
  look for walls: it does once an update's input has asked nothing of it, or it has turned, or it came out of a
  wall jump. What the input asks of the wall jump is the first of these that holds: a press of down curls her up;
  the direction behind her makes it a spin that way; the angle-up action makes it a jump aiming diagonally up, the
  angle-down action one aiming diagonally down, the attack one with her arm held out level; the jump held keeps it;
  anything else lets it go into a spin, which keeps what the wall jump gave her across only as a space jump or a
  screw attack. The wall jump is heard as a landing. A wall jump that lands settles as a spin's landing does
  (`spin_landing_updates`, by `Samus.SettlingUpdatesAfter`) and is shown by the spin's landing pictures.
- **Her equipment** changes what a state does; it does not add states. Space jump lets a spin jump again. The suits
  cancel sources of ongoing damage and what liquids do to movement.
- **Her run's one pixel.** As the cartridge does, a run about to be taken up in a pose she is not already running
  in first moves her one pixel on, the way she was headed: as a run starts, each time her running pose changes (her
  arm coming out after a shot, her aim going diagonal or level again: "arm pumping"), and on each update she runs on
  with nothing asked of her. A wall in that pixel stops the run and she stands against it. It is hers alone, so it
  is in `Samus.Entering`.
- **Moonwalking** walks her backwards by its own move values while the attack and the direction behind her are
  held. A jump turns her (the engine's turn), and the turn becomes a spin jump while the jump and the way she then
  faces are held; with the attack let go she turns; steering the way she faces she runs; walking off a floor she
  falls facing the way she was going.
- **Her aim in the air.** An aim straight down stays, with nothing held or only the jump, until a direction,
  another aim or the attack is asked; her body is compact for as long (`aiming_down`).
- **Her aim on the ground.** Running with a direction held she aims level or diagonally, never straight up; down
  aims diagonally down only with a direction held. Standing with both angle actions held she aims straight up and a
  direction the way she faces does not start a run.
- **Her weapons:** the beam, whose switched-on properties (charge, ice, wave, spazer, plasma) combine into one
  `Hit`; missiles, super missiles, bombs, power bombs; the special attacks.
- **Her body powers:** screw attack while spin jumping, speed boost at full run, the shinespark, and a charged
  spin. Each update's movement says anew which are at work, and they stay so for whatever touches her until her
  next update; the speed boost's is at work from the update after she reaches full boost.
- **Her own children:** `SamusGrapple`, `SamusXray`, `SamusVisual`.
- **How she is shown.** `SamusVisual` plays the timeline named for her look, the way she faces and her aim, falling
  back to the look and facing, then the look alone; with "_low" after any of them while her energy is low. As her
  look changes, a timeline named `<from>_to_<to>_<facing>` is played once between the two if there is one (a
  landing, a crouch, curling up). Her pictures are written once for each suit; `{suit}` in a path names the folder.
  Some looks carry more than the state: the spin is `spin_jumping`, `space_jumping` or `screw_attacking` by her
  equipment; swinging is `grappling_<n>` for the way the line runs from her, of thirty-two; the X-ray looks end in
  `_up`, `_raised`, `_level`, `_lowered` or `_down` for how the scope is swung.
- **Where her shots leave her** is data: `muzzles` and `grapple_origins` by stance, facing and aim, raised by the
  `shot_heights` of her look (and of her look and aim where an aim has its own). In the morph ball what she lays is
  laid where she is.
- **Her bomb jump** interrupts the ball. Its first update only sets the rise; then the ball rises out of her
  control, taking up the speed across it had when thrown to a side; a wall stops that speed and not the rise; the
  throw is over at the top of the rise or at a ceiling. A ball already being thrown is thrown afresh by another
  bomb.
- **Her bombs.** A bomb throws the morph ball lying against it when its fuse has `bomb_jump_fuse_left` updates left,
  and its burst strikes the block it lies in and the four beside that one. A power bomb's blast reaches as far on
  each update as its data lists (`blast_reaches`), and can hit the same thing again after `rehit_after` updates.
- **A special attack** is let loose while the attack is still held, once it has been held `special_attack_updates`.
- **A shinespark** gains the pull of gravity across each update up to its top speed; rising, its gain itself grows
  by gravity and it rises no more than `shinespark_most_rise` whole pixels an update.
- **Hurts.** A hit from the room itself (a spike) has its own invincibility and knockback lengths. The low-energy
  warning is started once as her energy falls low and stopped once as it recovers.
- **A jump with the speed booster on** adds half the dash's whole pixels and all of its fraction to the jump's
  speed, each on its own with no carry, as the cartridge does.
- **Grabbed** is being seized and carried by something that still lets her aim and fire (Draygon, Ridley). The
  holder says how many fresh presses of a direction free her, or none.
- **Drained** is what Mother Brain's beam and the giant Metroid leave her in. Whatever drains her puts her in a
  pose: pinned in the beam, on her knees, standing, or frozen while she takes in the hyper beam. On her knees, Up
  gets her to her feet and back in control if she has the strength, and otherwise she sinks back. She can glow
  through every colour, slower as told. `DrainedState.GiveHyperBeam` gives her the hyper beam: the equipment
  `hyper_beam`, which nothing collects or switches, with charge, wave and plasma on and ice and spazer off; her
  beam's data says what it fires while `hyper_beam` is on.

If something here turns out to be what any metroidvania player would do, it moves down into `Player`.

### Enemy (base)

The base is the engine's. One scene per enemy in `game/enemies/<name>/`, per boss in `game/bosses/<name>/`.

- **The base owns** what every enemy has: health, reacting to a `Hit` by its `EnemyStats`, being hurt and flashing,
  statuses such as frozen, dying, dropping from its `DropTable`, contact damage, and being stepped by the room.
- **The concrete enemy owns** its behaviour as a state machine, its position and velocity, its colliders and its
  visuals.
- **Does not:** read or write another enemy's state, depend on its place in any list, or reach into the player. If
  two enemies must cooperate, one owns the other as a child, or they share a link set when the room places them.
- **Colour and strength variants** are one scene with different data files.
- **What the base offers a concrete enemy** beyond the above: starting a timeline and acting on its cues, with a cue
  that starts another timeline taking effect in the same update; marking its place in a timeline and resuming there;
  judging a hit itself (`TakeHit` can be overridden, part by part); keeping health part by part (`HealthOf` says
  which health a hit on a part is taken from); firing its data's shots; leaving effects and
  drops; moving by the shared `Mover` or carrying the player with it; flashing for a set time; and asking whether
  the player rests against its solid.
- **An enemy's own visual** is its `ActorVisual` child named "Visual", wherever it stands among its visuals, so a
  part drawn behind the body can come before it in the scene; with none of that name, its first.
- **A boss is an `Enemy`.** One scene. Its parts are child nodes with their own hurtboxes, and the boss decides what
  a hit on each part does. There is no boss base class.
- A part that comes off and then only falls or fades is an `Effect`. One that can still hit the player is a
  `Projectile`. One that can still be killed is another `Enemy`.

**Solid actors.** Any actor may have a solid collider, switched on or off. A solid blocks the player and can be
stood on, and a solid that moves carries what stands on it; that is the `Mover`'s job, the same for a platform, a
frozen enemy and a boss's back. Frozen is a status: the enemy stops, its solid switches on, and a countdown ends
it. There is no special case for standing on enemies.

**Shared movement.** Ways of moving that more than one enemy uses are components in `engine/actors/movement/`, each
moving its owner by the shared `Mover` and reporting what happened (hit a wall, reached an edge, landed). The enemy
decides what to do about it.

| Component | Moves its owner by |
| --- | --- |
| Surface crawl | Following the surface it is on, around corners, in a set direction. It can be started pressing any way across and down; one started pressing away from its surface turns about until it finds something to cling to. It can feel for its surface some way ahead of itself, so that it meets a corner early; its owner can ask which way it presses. |
| Hop | Jumping in arcs, with settings for height, distance and the wait between. |
| Wave flight | Travelling in a direction while rising and falling in a wave. |
| Dive | Waiting, then dropping or swooping at a target and returning or not. |
| Patrol | Walking or flying back and forth, turning at walls or edges. |
| Home in | Steering toward a target at a limited turn rate. |
| Orbit | Circling a point or another thing at a set radius and speed. |
| Bounce | Travelling straight and reflecting off what it meets. |
| Follow path | Moving along a set path. Platforms and carriers use this one too. |
| Pursuit | Working out the speed of a flier drawn toward a place: harder the farther off, turned about sharply when going the wrong way, more sharply when well out of view. The flier moves itself. |

Taking hold of the player (a Metroid, a Beetom, a Yapping Maw) is not a movement component. The enemy asks the
player to enter the held state, moves with them, and adds ongoing damage while it holds.

This set was checked against the old code by counting which enemies do each: every one of the nine is used by
several enemies. The old code shares almost none of it, so each must be worked out from the enemies that use it,
not lifted from a shared routine.

### Projectile (base)

- **Owns:** its position and velocity, its side (the player's or the enemies'), the `Hit` it delivers, its
  lifetime, its collider and its visual.
- **Manages:** moving, hitting what it meets, and removing itself.
- **Does not:** decide what its target does about the hit, or know its target's type.

How a projectile behaves is settings, not code:

| Setting | Choices |
| --- | --- |
| Meeting terrain | Ends; ends and delivers its hit to the tile; passes through; passes through and still delivers its hit to each tile it crosses; bounces. |
| Meeting a target | Ends; passes through and can hit others; passes through and can hit the same one again once that one can be hit again. A target that takes no damage from it, or turns it back, stops it either way. |
| Met by the other side's shot | Destroyed; stops the shot and is unharmed; ignored. |
| Motion | Straight; accelerating; in a wave; homing; does not travel (a bomb); with gravity. |
| Lifetime | Until it leaves the view by a margin; a set number of updates; until its animation ends. |
| Ending | Nothing; an `Effect`; another `Projectile` (an explosion with its own hit). |
| Size | Its settings' own; a step of its timeline may give it another with the cue `size:<width>:<height>`, so a shot can swell or spread as it flies. |
| Gaining speed | One gain along each axis; one launched diagonally may have its own. |
| Shaking the room | A `RoomShake` (strength, length, way) started as terrain or a solid ends it, and one started the first time its hit lands. |

A projectile needs its own class only for behaviour no setting gives it. One whose timelines drive it (its harm
turned on and off, stopped, shifted, scattered by `GameRandom.Scatter`, ended, each by a cue) and that moves its own way is a
`CuedProjectile`, the engine's: by itself a burst that plays out where it is, and the base of those that fly.

### Pickup (base), Effect (base) and Remains

- A `Pickup` is a temporary drop of a `PickupType`. It owns its position, what it gives and how long it lasts; it
  gives itself to the player on contact and expires. It records nothing in `Progress`.
- An `Effect` is a short-lived visual. It owns its position and animation. It does not collide, deal damage, change
  gameplay state or use `GameRandom`. Its timeline's cues may ask for sounds, and may change how it drifts.
- `Remains` are what a destroyed enemy leaves where it died, when its data names a death to play: the timeline of
  that death, which may differ by the kind of hit that destroyed it. The enemy itself is gone at once. The remains
  play the death, asking for the sounds its cues name and scattering the effects they name, placed by `GameRandom`;
  on the death's "drop" cue, or as it ends, they roll the enemy's `DropTable` and leave the pickup; and they hand
  the enemy that takes its place, if one does, to the room, which adds it once the pickup is gone. So a drop appears after the explosion,
  not under it. An enemy with no death to play leaves its drop at once.

**A shot meets what is solid and hittable.** A projectile that reaches something solid that is not terrain, such as
a door's cap or a frozen enemy, delivers its hit to what is there before the solid stops it. Terrain stops it first.

**A shot's harm is its own, unless its launcher settles it.** A `ShotType` launches its projectile with the damage
its data gives (`Launch`). Fire that does the harm of what it came of is launched with that harm in its place
(`LaunchDoing`): a flame of Ridley's does what the fireball it came of does, which differs between its two fights.

**Pictures for a room's graphics.** A layout names the set of graphics it is drawn with. A picture's path may hold
`{graphics}`; a room loads such a picture from the folder named for its own set, so that something the old game
drew with the room's blocks looks as that room's graphics draw it.

**Where a timeline's pictures lie.** A timeline's data may give an offset: where the top-left corner of each of its
pictures lies from the place of the thing shown. It is added to whatever offset the thing has of its own, so
pictures cut to one shared size need no thing to know that size. A step may give an offset of its own, added to
the timeline's for that step: a picture that moves about as the timeline plays. A pickup shown by a timeline is
drawn where the timeline's offset puts it; one shown by a lone picture has it centred.

### Components (`engine/actors/components/`, `engine/collision/`)

| Component | Job |
| --- | --- |
| `Collider` | A rectangle with a role: body, hurtbox, hitbox or solid. Holds no behaviour. |
| `Mover` | Moving a body through terrain and solids, one axis at a time, carrying what stands on a moving solid, and reporting what stopped it. |
| `Health` | Current and maximum health, and reporting when it runs out. |
| `StateMachine`, `State` (base) | One current state; each state holds its own data and names what it changes to. |
| `TimelinePlayer` | Stepping a `FrameTimeline` once per update when its owner says, and reporting the step and any cue reached. The owner acts on the cue. |
| `ActorVisual` | Showing the frame gameplay selects, hurt flashes, glows, colour changes and dissolves. It can show its picture row by row, each row naming the row of the picture drawn there. Never advances gameplay. |
| `RowCrumble` | Working out which rows of a picture show where as it crumbles away from the bottom up, after an optional wait. An `ActorVisual` shows them. |
| `ColliderPoses` | Where a thing's collider nodes are in each of its named poses, from its data. Taking a pose moves the colliders it names and switches off the others any pose names. A timeline's cue takes the pose that goes with the picture it shows; the owner decides what else a pose means. |
| `PatchSequence` | A change to the room's terrain that takes some updates: each of its named patches in turn, played by the room (section 5, "Terrain that changes over some updates"). |

**Why, and extending it.** The base classes are the engine's answer to "what is one of these?", written so the
fork gets a working player, enemy and projectile by supplying only what is its own. So the pressure is always
downward: when Samus or an enemy needs something, first ask whether any game's player or enemy would, and if so
build it in the base as a property, a state or a system, switched on by the game. Wall jump, crawl and jumping
while crawling were all placed that way. Game-layer states are the exception, kept for things with no general
version. The tables here (standard states, projectile settings, movement components, body powers) are starting
sets drawn from this game, not limits. A new row is right when a second thing needs the same behaviour or when the
behaviour is plainly general; a new setting is right when two things differ in a way the existing settings cannot
say. What must never happen is the old shape: one actor made of several entities, state held in flags beside the
state machine, or one actor reaching into another.

## 7. Collision (`engine/collision/`)

| Type | Job | Does not |
| --- | --- | --- |
| `CollisionWorld` | One per room. Knows every registered `Collider` and the `Terrain`; answers overlap questions in a stable order. | Decide what an overlap means. |
| `Collider`, `Mover` | See components. | |

**Why, and extending it.** Collision finds; owners decide. That is why there is one mover and one overlap query for
everything, with differences as options on the call. A new need (a new way of treating slopes, a new edge rule) is
a new option whose default is the plain behaviour. If the option exists only for one of Super Metroid's oddities,
the non-default choice is supplied from `game/compatibility/`. The architecture guide has the rest.

## 8. Presentation, audio, input and content

| Type | Lives in | Job | Does not |
| --- | --- | --- | --- |
| `MessageBox` | `engine/ui/` | Showing a message, waiting for it to be dismissed, and returning the answer when it asks yes or no. A game that keeps its messages as pictures (`GameDefinition.MessageBoxFile`) has each shown as its picture, drawn for the room's set of graphics, across the screen with its middle on the style's line, as much of it as the window about that line shows; a question has a picture of each answer chosen, and a message that names the button of an action has one for each button the style lists and one for none of them, the first listed button the player has bound to the action being the one shown. | Grant the item it announces or perform the save it asks about.  |
| `MapView` | `engine/ui/` | Drawing an `Area`'s map from what `Progress` says is explored and revealed. The minimap, the pause map and the file-select map are all this, at different sizes. An unexplored cell and the cell the player is in (`Here`) are drawn from the area's own sheets for them where it has them. A view may draw the cells alone or the markers and labels alone, as where the cells show through an opening and what lies over them does not. | Decide what is explored.  |
| `ScreenFade` | `engine/ui/` | Fading, flashing or whiting out the whole screen over a set number of updates, smoothly or in a set number of even steps, and reporting when it is done; or showing the screen covered so many parts of so many, for whoever paces a fade of its own. | Know why it was asked. |
| `PixelText` | `engine/ui/` | Drawing words in a `PixelFont`, a character to a cell and line under line; showing only the first so many characters; laying each line at the left, middle or right of its width. Every word the game shows is drawn by one. | Hold the words, or draw in anything but its font: with none, words given it cannot be drawn. |
| `TypedText` | `engine/ui/` | A `PixelText` showing its words one character at a time with its sound, for every letter or every so many; going on to the next line takes no time, and a set number of characters at the start may each be followed by the next at once. It says when all of it shows, and when the wait after its last character has passed as well. | Hold the words; they come from `GameText`. |
| `Cinematic` | base in `engine/ui/`, each one in `game/cinematics/` | A scripted sequence with no gameplay. Each is a `GameMode`. One that waits on the player reads what they are doing this update; one that shows remembered play steps a room of its own from recorded input. | Run gameplay rules of its own. |
| `CinematicSprite` | `engine/ui/` | One thing shown in a cinematic: a picture at a place on the screen that plays a timeline, drifts at a speed (round a loop, if it has one), and is over when its timeline has played out. Its cinematic steps it and acts on the cues it reports. | Decide what its cues mean. |
| `MosaicPicture` | `engine/ui/` | A picture that can be shown in coarse blocks, each taking the colour of its top-left pixel. | Anything else. |
| `GameHud` (base) | `engine/ui/` | Being what is shown over play: kept on screen by `PlayingMode`, presented once as play begins and told to show things as they are as each room is set up and after each update. The `GameDefinition` names a game's scene of one. | Know what a particular game shows. |
| `Hud` | `game/hud/` | Showing energy, ammunition, the selected weapon, the minimap and, during an escape, the countdown; its places and pictures come from its data file. Under the minimap lies the picture of a cell that shows nothing; the countdown is its label and three pairs of digits about one place. | Change any of them.  |
| `PauseScreen` | `game/pause/` | The whole of a pause: the screen darkening over play that goes on (it steps the playing mode itself, without the pause action, and gives way to whatever play hands over to), every sound stopping, the pause screen brightening, in use and darkening, and the screen brightening over play again; a level of darkness at a time, with one black update each way. The HUD stays over it. The map page shows the area's map through the opening of the page's frame, with the arrows of the ways it can be moved and the sign of where Samus is; the equipment page switches equipment through the player's `Inventory` (the selector moves first, so a press on the update it moves switches what it moved onto), and her reserves, once started pouring, pour until she is full or they are empty and are left empty. Turning the page (angle up from the map, angle down back) and leaving (pause) answer to an action only once what is held has stayed the same a while. Its backdrop, buttons, fades, the pictures of each thing switched on and off, the selector's looks and places, her suit's outline, her reserves and its sounds all come from `pause/pause.json`. | Hold inventory state. |
| `ChoiceList` | `engine/ui/` | A list of lines with one marked: up and down move the mark, and confirm and cancel are reported to whoever steps it. The lines not chosen may be tinted (`UnchosenTint`), for a font with no mark to write before the chosen one. | Hold words or act on a choice.  |
| Menus | `game/menus/` | Title (`TitleMode`), file select (`FileSelectMode`), options (`OptionsMode`), game over (`GameOverMode`). Each is a `GameMode`; their words come from `GameText`. The game starts on the title, and the game over screen is what follows a death. The title reads its opening (cards of typed words, moves across its backdrop, the backdrop drawing back, the title and the copyright fading in), its backdrop, the larva on it, its pictures at rest and its sets of demos from `menus/title.json`; a press cuts the opening short, and a demo comes back to the title at rest; `DemoSchedule`, a session service, keeps which set is due. A demo (`AttractDemoMode`) starts Samus at a place in a room with what its file gives her. The game over screen reads its page, the crying larva and the cursor from `menus/game_over.json`. File select reads its pages, what it lays over them for each saved game, its helmets, cursor and copy arrow from `menus/file_select.json`; a saved game chosen there goes on to the options. The options read their pages, what is laid over them for the language and each toggle, their headings and cursor from `menus/options.json`; their first row starts the game, and their page of buttons lists each action with its button in the game's font. A saved game shows where it was saved before it starts (`SaveMapMode`, from `menus/save_map.json`, when the game was saved at a place that file marks): black for a moment, then the planet opening from the middle of the screen with every area the game has been saved in named and the one it was saved in lit; confirm opens a window from that area's name onto the area's own map as that screen colours it (`menus/save_map/areas/`), with the place saved at pulsing, the stations found, the ship and the bosses; the directions move it, confirm there fades out and starts the game, cancel closes the window back to the planet, and cancel on the planet goes back to the options. `ScrollingMap` (`game/pause/`) is an area's map laid behind a frame and moved, which that screen and the pause screen's map page both use. | Touch a running session.  |
| `AudioDirector` | `engine/audio/` | Playing the music a room or event asks for and the sounds `SoundRequests` accepted. | Affect gameplay by its timing. |
| `InputSampler` | `engine/input/` | Producing one `GameInput` of actions per update from the player's devices and bindings, or from a recorded demo. | Let anything else read devices. |
| `ContentLibrary` | `engine/content/` | Loading art, audio, layouts and data files, by their path in the assets tree. | Read the ROM or reference the extractor. |

**Assets.** Everything of this game that can live in an external file does. They come from the player's ROM, are
never committed and never shipped, and producing them is the asset extractor's job and no part of the Godot
project.

- The line: **behaviour and structure are in the project; values are in files.** The project holds code, each
  actor's scene (which nodes it is made of), and each room's scene (what is placed where). The files hold the art
  and sound, the tile layouts and their patches, and the numbers and words. If a thing could be either, it is a
  value in a file.
- Every asset is a plain file in a common format: text, PNG, WAV. No binary blobs, no engine-specific formats, no
  packed archives. Text files use names, not numbers standing for names, and are laid out to be edited by hand.
- The engine defines each format and documents it beside `ContentLibrary`. The asset extractor is one producer of
  those formats; a person with an editor is another.
- `godot/assets/` is where a developer drops the extractor's output. It is git-ignored. Godot imports it, so the
  assets are visible in the editor.
- The exported game does not contain `assets/`. It loads the same files from the folder the extractor installed
  them to.
- A scene or resource never holds a direct Godot reference to a file under `assets/`. It holds the file's path
  within the assets tree, on the component that shows or plays it.
- `ContentLibrary` is the one place that turns a path into a texture, a sound, a layout or values, and the one
  place that knows which build it is in: an editor build reads `res://assets/`; an exported build reads the
  installed folder.
- A missing, misspelt or wrong asset or value is an error naming the file and what is wrong, in the editor and in
  the game. Never a blank or a default.

**Values from files, visible in the editor.** Every value is still an ordinary property of the node or resource it
belongs to, so it appears in Godot's inspector. Click a Yapping Maw in a room and its health is there.

- A node or resource that holds values has one extra setting: the data file its values come from.
- **When a data file is named:** the values are read from it, in the editor and in the game. The inspector shows
  them read-only. They are never saved into the scene or resource, so nothing from the ROM is committed.
- **When no data file is named:** the same properties are ordinary editable, saved properties. A game with no
  external files edits its values in the editor.
- This is one mechanism in the engine, used by every type that holds values. No type reads its own file its own
  way, and none knows whether its values came from a file.

**Why, and extending it.** Two readers are being served. The community gets Super Metroid as files they can open
and change, which is why formats are plain and named. The fork gets an engine where the same values are edited in
the Godot editor with no files at all, which is why a value is always an ordinary property first and a file entry
second. So when adding anything with numbers or words in it, make them properties of the thing they describe and
let the one mechanism fill them; never add a second way of loading data, and never put a value in code. The UI
types split the same way as everything else: the engine has the parts any game's screens are made of, the game has
its particular screens. A new screen is the game's; a new part two screens share is the engine's.

## 9. Adding something

Each recipe is everything that changes. If a recipe seems to need more, the thing is in the wrong type; go back to
section 2. Whatever is added is written into this document in the same change.

- **An enemy:** a folder under `game/enemies/`; a scene; a class deriving `Enemy`; its states; its data file.
- **A boss:** the same under `game/bosses/`, and a `BossEncounter` placed in its room.
- **A projectile, pickup or effect:** settings on the base where they suffice; otherwise a scene and a class
  deriving the base.
- **A weapon, item, liquid, pickup or hit kind:** a data file or a name, listed in the `GameDefinition`.
- **A block:** a `BlockType` made from the block rules.
- **A fixture:** existing room object kinds placed together, with settings. A new kind only by section 5's note.
- **A room:** a scene under `game/rooms/<area>/`, and a `RoomSequence` beside it if it has one.
- **A Samus state:** a class deriving `State` in `game/samus/`, and a row in section 6.
- **A mechanic any game would want:** in the engine first, as a property, state, setting or component of the type
  it belongs to, switched on by the game.
- **A capability one thing asks of another:** an interface beside the type that asks.
- **A compatibility answer:** a small class in `game/compatibility/`, listed in the `GameDefinition`, supplying a
  place the engine offers. If the engine offers no such place, add the narrowest one first, with the plain
  behaviour as its default.

## 10. Where everything in the game goes

Every kind of thing the old code contains, placed. All of it is this game's: it lives under `game/`, built on the
engine types named. The old code's grouping is shown only so each thing can be found there.

### Enemies

One `Enemy` scene each unless noted. Names on one line are one scene with different data files.

- Crawlers (surface crawl): Zoomer, Orange Zoomer, Armored Zoomer, Zeela, Sova; Sciser; Zero; Yard; Owtch.
- Flyers: Waver; Skree; Metaree; Rio; Geruta; Holtz; Squeept; Mellow, Mella, Maridia Fly; Multiviola; Viola;
  Ripper, Jet Ripper, Grapple Ripper (the last can be latched onto); Fireflea; Choot; Kago; Atomic; Spark; Coven.
- Hoppers and the rest: Sidehopper, Large Sidehopper, Tourian Sidehopper; Dessgeega, Large Dessgeega; Boyon; Puyo;
  Beetom; Powamp; Cacatac; Zoa; Skultera; Bull; Alcoon; Polyp; Fune; Namihe; Magdollite; Nuclear Waffle; Hibashi;
  Yapping Maw; Boulder; Oum; Mama Turtle with her Baby Turtles as children; Evir; Dragon; Shaktool; Work Robot (the
  unpowered one is the same scene, switched off).
- Pipe bugs: Zeb, Zebbo, Gamet, Geega. Each pipe is a `Spawner`.
- Metroids: Metroid; Mochtroid; the giant Metroid of Tourian (`BabyMetroid`, with the Sidehopper it feeds on,
  `DeadSidehopper`); and the giant Metroid of Mother Brain's fight (`MotherBrainBaby`, which she makes). The two
  share only the engine's `Pursuit`.
- The husks of Tourian are enemies, not scenery, because shots and bombs set them crumbling: `Corpse` (the Zoomers,
  Rippers and Skree, one scene with a data file each), `DeadSidehopper`, and `DeadTorizo` with its heap of sand.
- Ki-Hunters: green, yellow, red. Wings are part of the Ki-Hunter; shed wings are an `Effect`.
- Space Pirates: wall, walking, ninja. Three scenes, their colours data files.
- Kzan: one enemy; its top and bottom are parts.
- Rinka: an enemy; its source is a `RinkaSpawner`, a `Spawner` that shares its places with the others in the room.
- Zebetite (one node that stands again as each barrier in turn); Draygon's wall cannons; the eye of an eye door
  (`EyeDoorEye`, with the growth around its door as two visuals and its spit a `Projectile` of its own): an enemy
  each. An eye door's cap is a `Barrier` held shut by "the eye placed here is gone", and the eye is placed only
  while its destroyed fact is not recorded.
- Mini-bosses and bosses, each one scene: Bomb Torizo, Golden Torizo (one scene, two data files); Spore Spawn
  (the pieces of its shell and its core are hurtboxes that its timelines' "pose" cues move to where each frame has
  them, a shot meeting the first of them in the scene's order; how hurt it is and each step of its hardening are
  sets of its pictures, its stalk's with them; the places its spores come from are visuals whose own timeline lets
  each spore go; its ceiling crumbles through a list of patches before the one that opens it; and the view is kept
  above a `ViewFloor` at the top of its chamber, which it releases as it dies); its spore (a `Projectile` that, shot, goes on
  drifting while it bursts and leaves its drop as the burst ends);
  Fake Kraid; Kraid (its head, arm and foot are visuals of the one enemy; where shots meet its mouth, head and the
  ledges of its body, and where its arm touches, are `ColliderPoses` taken as its head and arm change picture; how
  hurt it is, the flash of a hit in the mouth and its dying are sets of its pictures; what it does to its room, the
  ceiling it breaks, the platforms and spikes that crumble, are `PatchSequence`s; the three spikes of its belly and
  its two nails are enemies of their own the room places, there until it is beaten; its rocks are `Projectile`s; the
  screens it keeps the view from until it has grown are `CameraZone`s it opens); Crocomire
  (its tongue is a visual; the pieces of it a shot or Samus meets are ten hurtboxes its `ColliderPoses` place for
  each picture, each pose saying what every piece does and how far its mouth is open; what it does in the acid is a
  script in its data; the bridge's blocks, the walls that hold Samus in while it dies and the opened spike wall are
  patches; the pieces of the spike wall are a `Projectile` of their own class, the bridge's pieces and its fire
  plain ones; a room where it has been beaten places the same scene as its skeleton); Phantoon (eye, tentacles, mouth are parts; while its fight lasts the door of its room is wall, a patch of the room's terrain, and the door's cap is out of sight); Botwoon
  (its head is the enemy, sized by a cue for the way it points, and the thirteen segments of its body are visuals
  with hurtboxes that stop shots and are not hurt; how hurt it is is a set of its pictures; its wall falls as a
  `PatchSequence` the room goes on playing after it is gone, and the `CameraZone` its wall kept shut is opened as
  the wall begins to fall); Draygon (eye, tail, arms are parts; its body's pictures are kept once for each band of its health; its data, not
  its cannons, says which cannon fires on which of its turns and from where, and each cannon in the room says which
  it is);
  Ridley and Ceres Ridley (one scene, two setups: one enemy written across three files, what both fights share and
  a file for each fight, chosen by its `OnCeres` setting; breakup pieces are `Effect`s; its pictures are kept once
  for each of the colours it is in, the dark it waits in, each step of its eyes' and body's showing and each look
  of its health, and its data says which parts of it can be touched and shot in each pose); Mother Brain (her brain, the joints of her neck
  and her beam are parts of one enemy written across several files, one per part of her fight; her glass and her
  turrets are enemies; her rings, bombs, drool and the glow at her hand are `Projectile`s of their own).
- What a boss does to the room (the camera's limits, doors, lights, the alarm and its countdown) is not in the
  boss. It tells the things listed on it (`ToldOf...`, `ToldWhen...`) a word, and a room object or the room's
  sequence acts on it; a change to the tile grid is a named terrain patch.

### Room objects

- `Barrier`: blue, red, green, yellow, grey and eye door caps, in every facing; the Ceres doors (they open by
  themselves while Samus is near, and the one to Ridley's room shows itself shutting as she comes in and is held
  shut until he has gone); the downward
  gate, opened by its switch; the old Tourian escape wall; the wall
  the animals break; Botwoon's wall; the shell around a statue-held item. Mother Brain's glass turned out to be an
  enemy (missiles that reach her count against it), the tubes of her room `Effect`s that fall as terrain patches
  clear where they stood, and her escape gate a terrain patch with thrown pieces as `Effect`s.
- `RoomExit`: every door, and the far end of every elevator.
- `Carrier`: every elevator; the Maridia transport tube. The two Chozo statues that come to life are one enemy
  scene (`ChozoStatue`) with two setups, not carriers: the one walks through terrain by its timeline's strides with
  Samus in its hand, and the other lowers the room's liquid.
- `Platform`: none placed. The Tripper is an enemy (it can be frozen and shot), and the suspensor platform is the
  Tripper's scene with its own data, because it sinks under Samus's weight the same way. The shutters are
  enemies too, because shots set some of them off and destroy others: `Shutter` (the shootable shutter, the Kamer
  and the destroyable shutter, one scene whose settings say what sets it moving, how far and whether it comes
  back) and `GrowingShutter`.
- `Station`: save, energy refill, missile refill, map.
- `Item`: every upgrade and expansion.
- `GlassTube` (this game's, `game/objects/glass_tube/`): the Maridia glass tube. It is not a barrier: a power
  bomb only arms it, and it bursts when Samus next acts, holding her while it does.
- `CeresElevatorDoor` (this game's, `game/objects/ceres_elevator_door/`): the blocked way to Ceres's elevator,
  blown apart once the self-destruct has begun; it also keeps the shaft's platform and glow flickering.
- `Gunship`: the ship, as one object (`game/objects/gunship/`). Samus aboard it is in her state of facing forward and being put where something puts her.
  It is told to "land" by the landing's room sequence and tells what is listed on it when it has left the planet.
- `BossEncounter`: one per boss room. The Metroid rooms whose doors open when the room is cleared need none; their
  barriers are held shut by "this room has no live enemies".
- `Spawner`: Rinka sources. The pipe bugs are enemies that go back into their pipes by themselves.
- `Hazard`: the Samus-eating plants; Ceres steam (its reach follows its plume by its timeline's cues, and each
  vent waits a different, chance length before it first blows).
- `Creature`: the Etecoons and the Dachora, during play (`Etecoon`, `Dachora`) and during the escape
  (`EscapeEtecoon`, `EscapeDachora`), in `game/objects/etecoon/` and `game/objects/dachora/`.
- `Scenery`: none placed. The Tourian entrance statue is a plain node holding the pictures its hall's sequence
  shows and moves. Bomb Torizo's hand (`BombTorizoHand`) and the morph ball eye
  (`MorphBallEye`, in `game/objects/morph_ball_eye/`) are room objects of their own, because each has a state
  machine no setting of `Scenery` gives; the face blocks and the corpses are enemies.

### Terrain

- `BlockType`s: shot, bomb, power bomb, super missile and speed blocks, regrowing or not; crumble blocks, regrowing
  or not, in each size; blocks hidden until shot, bombed or seen by X-ray; spikes; conveyors and treadmills; sand;
  grapple blocks, lasting and breaking.
- Patches: everything the old code's scripts wrote into the tile grid during play (a collapsing bridge, a boss's
  room changing, a wall opening).

### Room conditions

- `Liquid`: water, lava, acid, including the rising acid of the escape.
- `RoomEffect`: heat, rain, fog, spores and Ceres haze are the engine's `OverlayEffect`, each with a look (heat with
  ongoing damage as well); room-wide colour changes are its `TintEffect`; the earthquake is the engine's. The
  Fireflea rooms' dark is this game's own (`FirefleaDarkness`, in `game/effects/`), told to darken by each Fireflea
  that dies; it looks at the deaths as it is stepped, so a death shows from the update after.
- `Backdrop`: room backgrounds, the scrolling sky (a backdrop is drawn behind everything else in its room); the Ceres rotating floor and the tilting picture of its
  elevator shaft are `TurningFloor`s.
- `CameraZone` and `Trigger`: every scroll region and scroll change.
- Room versions: every case where the old code had more than one "state" of a room.

### Room sequences

A sequence is told "start", and one that knows other words acts on them. Each lives beside its room, under
`game/rooms/<area>/`; one that rooms of several areas use (`TypedWarning`, `EscapeRoom`) lives in `game/rooms/`.
`TypedWarning` types
a warning across the screen, from the update after it is started, and tells what is listed once the wait after its
last letter has passed; the words stay written. Mother Brain's room uses it. How a typed
warning is laid out and typed (its font, where its words begin, how long a letter takes, how many letters at the
start are followed at once, how many letters go to a sound, and the pictures of a second wording) is a data file
named for it under `text/typed/`.

Ceres: the elevator arrival (`CeresArrival`: the pad that sets Samus down); Ridley's getaway
(`CeresGetaway`: he flies off over the turning floor and throws Samus aside, then the self-destruct's warning comes
up, is typed out and stays up flashing, and the countdown begins); the station shaking once the self-destruct has
begun (`CeresShaking`); the falling debris (`CeresDebris`); the elevator shaft tilting and the elevator taking Samus
(`CeresShaft`, which tells the `Escape` service she has left). Crateria: the gunship landing. Brinstar and
Norfair: the Dachora shaft camera (`DachoraShaftCamera`: once the shaft's top is open to the view, the zone beside
it is closed), Crocomire's room shaking (`CrocomireRoomShaking`: the terrain is shown shifted by as much as he
rattles his wall). Maridia: the transport tube ride, which is a `Carrier` that gains speed. Tourian: the entrance
statue's hall (`TourianStatueHall`: the guardians' eyes, each one's release once its boss is beaten, and the
statue's descent, which is its part of an update's opening (`IOpeningStep`), played before the update's random
number is drawn; the statue is the room's backdrop, which the hall sinks, with a picture over it for each
guardian's eyes and for each turned grey, and its floor crumbles away as a change to the terrain its data gives), and the rising acid, which is a `Trigger` telling the room's `Liquid` to rise. A watch on a
room (`RoomWatch`, in `game/rooms/`) records facts once its condition holds: each Metroid room cleared, and Zebes
waking when a room's guard is killed. Norfair also has the lava that chases Samus out with the Speed Booster
(`SpeedBoosterLavaquake`). The escape:
explosions and shaking in each escape room (`EscapeRoom`). The plants of Brinstar that seize Samus (`SamusEaters`, in `game/rooms/brinstar/`): one in each Brinstar room finds the blocks of the terrain that are plants, by their block type, and plays each: it holds her, shows its chewing as patches of the terrain named for where the plant is, hurts her at each bite, lets her go and stays shut a while.

### Projectiles and effects

- Samus's projectiles: the beam with each combination of properties, missile, super missile, bomb, power bomb, and
  each special attack.
- Enemy and boss projectiles: settings on `Projectile` where they suffice, a class in the owner's folder where they
  do not; one several enemies fire goes in `game/projectiles/`.
- Effects: explosions and death bursts; dust, splashes, bubbles and lava spray; hit sparks; speed echoes; falling
  debris and shed parts; drop sparkles.

### Samus and the screens

- Samus: section 6. The old code's pose list and movement types reduce to the engine's standard states plus hers,
  with facing and aim as separate values.
- `GameMode`s: section 4. `Cinematic`s: the intro, Ceres departure, Ceres destruction, the ending and the credits.
  The departure (`CeresDeparture`) darkens the screen over the shaft and leads to the destruction
  (`CeresDestruction`), which is also where time running out on Ceres leads, with Samus still aboard. The intro
  (`Intro`, written across three files: its pages, the scenes between them, and the flight to the colony) is what a
  new game begins with. Its two flashbacks are rooms played by Samus herself from recorded input, each with the
  thing she met there in it: `IntroMotherBrain` (an enemy) and `IntroEgg` (a room object), both in
  `game/cinematics/` because they exist only for the intro, as do the two rooms the flashbacks play in
  (`intro_mother_brain_room.tscn`, `intro_discovery_room.tscn`), which a flashback's recording names with the entry
  point `Start`. The ending and the credits are one cinematic
  (`Ending`, written across four files: its order of events and what it shows, the escape and explosion, the
  credits and Samus waiting, and her reward and sign-off). It follows the gunship leaving Zebes: the ship tells the
  room's `ZebesDeparture` sequence, which darkens the screen and tells the `Escape` service she has left, and the
  service takes down the clear time, the share of items found and whether the animals escaped for the ending to
  show. The ending does not end; the game is left on her sign-off.
- The pause screen's map and equipment pages, the HUD, the minimap and message boxes: section 8.

### Not carried over

Things in the old code that are not things in the game: "held place" objects, slot hand-overs and other storage
left-overs; the old compatibility module's contents as a set (each is re-examined, and only an oddity that touches
something the engine owns reaches `game/compatibility/`); scroll "extension" objects; block "extension" tiles;
elevators, doors and item containers as types of their own; room "states" as separate rooms; debugger save states
and input recording tools.

**Why, and extending it.** This section is the worked example of sections 0 and 2 applied to the whole game, so
that placing a thing is looking it up, not deciding. It was built from the old code's lists of names, not from
reading every behaviour, so expect to find things it misses or misplaces. When that happens, run section 2 on the
thing, apply move 1, place it, and correct this section. If a thing listed here turns out, once understood, to be
two things or to be an existing type with a setting, the list is wrong, not the rule.
