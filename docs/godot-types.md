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
    actors/        Enemy, Projectile, Pickup, Effect
      player/      Player, its standard states, Inventory, Weapons
      movement/    the shared ways of moving (section 6)
      components/  Health, StateMachine, State, TimelinePlayer, ActorVisual
    world/         Room, Terrain, Liquid, RoomEffect, Backdrop, Camera, CameraZone, Trigger, RoomExit,
                   RoomObject, RoomSequence
      objects/<name>/    the general-purpose room object kinds
    ui/            ScreenFade, TypedText, MessageBox, MapView, Cinematic
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
8. Is it a region with no body? Scrolling limits: **`CameraZone`.** Leads to another room: **`RoomExit`.** Does one
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
| `GameAction`, `GameInput` | An action by name; one update's input | The engine names its standard actions; a game adds its own by name. A `GameInput` says which actions are held and which were newly pressed. |
| `GameClock` | The count of updates played | One per session. Anything that acts "every N updates" reads it. Nothing keeps its own global counter. |
| `HitKind` | A named kind of damage | The engine provides contact and hazard. The game names the rest (here: each beam property, missile, super missile, bomb, power bomb, each special attack, each body power). |
| `Hit` | One hit being delivered | A damage amount, the set of `HitKind`s it carries, its direction, and who caused it. |
| `Condition` | A yes-or-no question about the game's state, held as data | One of: a named fact in `Progress` is set; this room has no live enemies; the player owns a named piece of equipment; a named thing placed in this room is dead or open; or all, any or none of a list of conditions. |
| `RoomEntry` | How the player arrives in a room | Which room, through which exit or station, and what the player carries in (speed, state). The room transition builds it; the room and the player read it. |

**Hits carry several kinds.** One shot can be several things at once. A hit carries every kind that is true of it,
general and particular together: a charged beam that is ice, wave and plasma carries beam, plasma, ice, wave and
charged.

- What is hit holds a list of reactions, each for a set of kinds. The reaction that applies to a hit is the one
  whose kinds the hit all carries and that names the most of them. A thing with no matching reaction takes the
  hit's full damage.
- A reaction says:
  - how much of the damage is taken: none, a fraction, all, a multiple;
  - any status it puts the thing in (frozen, for one), and when: always, only in place of dying, or never;
  - whether the thing is briefly unhittable afterwards, and for how long;
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
| `ItemType` | A permanent upgrade | What it grants, its message, its picture. |
| `PickupType` | A kind of drop | Which inventory amount it adds to, and how much. |
| `BlockType` | A reactive tile | Its rules (section 5). |
| `LiquidType` | A kind of liquid | How it slows movement, the ongoing damage it does, which equipment cancels each, how it looks. |
| `Area` | An area of the world as its map shows it | Its name, the fact that reveals its map, the cells of the map and the small picture each shows, which cells are secret, its markers, and the sheet the pictures come from. A room says which area it is in and which cell its top-left corner is. |
| `FrameTimeline` | A timed sequence gameplay follows | Ordered steps, each with a picture to show, how many updates it lasts, and optional named cues; and where it loops or ends. |
| `DemoRecording` | One title-screen demo | The room it plays in, the entry point the player starts at, and the actions held for each stretch of updates. `RecordedInputSource` plays it to the `InputSampler`. |
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
| `SoundRequests` | What gameplay has asked to hear and, as gameplay sees it, what is still sounding | Accepting or refusing each request by priority, and answering "is this sound still going". Advanced once per update. | Play anything. |
| `Escape` (this game's) | The escape countdown | Starting, counting, showing and ending the timed escape, and reporting that it is running. | Know which room the player is in. |

`SaveData` is the saved form itself: what a slot's file holds, captured from a session and put back into one.
`RoomVersion` is one set of a room's settings (its layout file, which of the layout's patches are in place, its music) with the `Condition` it applies under.

**The engine's modes.** The engine provides the modes any game has. This game adds title, attract demo, file
select, options, game over, time up, and one per cinematic.

| Mode | Does | Hands over to |
| --- | --- | --- |
| `PlayingMode` | Owns the room being played and steps it. Writes a save when one was asked for. A game says what is stopped while the rest runs by overriding one method. | The mode after death when the player has died; a `MessageBoxMode` when a message was asked for; a `RoomTransitionMode` when the player entered an exit; the pause mode when pause is pressed while the player is in control. |
| `RoomTransitionMode` | Fades out, takes the old room down and sets the next up with the player arriving at the exit the old one led to, fades in. Neither room is stepped meanwhile. | The same `PlayingMode`. |
| `PausedMode` | Nothing; play is still beneath it. A game's pause screen is its own mode doing the same. | The same `PlayingMode`, on pause. |
| `MessageBoxMode` | Shows a message from `GameText`; one that asks yes or no gives the answer to whoever asked. | The mode it was laid over, on confirm. |
| `LoadingMode` | Begins the session's game again and puts the session's save slot into it, if the slot holds one. | A new `PlayingMode` in the saved room, or the first room. |
| `Cinematic` (base, `engine/ui/`) | Plays update by update; confirm skips it unless it says otherwise. | The mode it names. |

- **Laid over, or replacing.** A mode says which mode it is laid over (`Keeps`). That mode stays on screen, is not
  stepped, and is told when control comes back (`Resume`). Any other hand-over ends the mode that finished, and
  everything it was laid over.
- **Asking, not reaching.** A room object does not start a mode. It asks the session: `RequestMessage` for a
  message, `RequestSave` for a save. The mode in control takes the request and acts on it.
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
  map cell the player is in as explored, starting an earthquake when something asks, and tearing itself down when
  left.
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
- A plain tile is data: empty, solid, or a slope with a height per pixel column.
- A **block** is a reactive piece of terrain, one or two tiles wide and high, placed as one unit with a
  `BlockType`. It is not a node. `Terrain` owns whether each block is intact, broken, hidden or regrowing.
- A `BlockType` is a combination of these rules and nothing else. The rules are the engine's; the hit kinds, powers
  and abilities a block type names are the game's:

| Rule | Choices |
| --- | --- |
| Solid or passable | Solid; passable. |
| Broken by | Nothing; a `Hit` carrying a listed kind; being stood on (crumble). |
| After breaking | Gone until the room is next set up; regrows after a set time. |
| Hidden until | Never hidden; hit by a listed kind; seen by a revealing ability. |
| On contact | Nothing; ongoing damage (spike); carries sideways (conveyor); pulls down (sand); can be latched onto; can be latched onto once and then breaks. |

- `Terrain` does not know the player, enemies or weapons by name. It is told "this rectangle touched you" or "this
  `Hit` landed here" and answers from the block's rules. The player's body breaking a block is a `Hit` like any
  other (section 6, "The body as a weapon").

**Terrain that changes.** A layout file may hold named patches: a rectangle of tiles to put in place of what is
there. Anything may tell `Terrain` to apply a patch or take it back by name: a boss dropping the ceiling, a
sequence opening a wall, a bridge giving way. Nothing edits tiles one at a time, and nothing but `Terrain` holds
tile data. A patch applied for good is remembered like anything else.

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
| `Station` | Holding the player while it works, then granting or recording the result. | What it does: save; reveal the area's map; refill listed inventory amounts. |
| `Carrier` | Taking hold of the player and moving them along a path, then letting go. | Path; what starts it; whether it ends by letting go or at a `RoomExit`. |
| `Platform` | A solid surface that moves, and so carries what stands on it. | Path; whether it reacts to weight. |
| `Spawner` | Producing enemies at a place by a rule. | What it spawns; how often; how many at once. |
| `Hazard` | Hurting or grabbing the player on contact, without being killable. | The `Hit` or ongoing damage it does, or its hold. |
| `BossEncounter` | Changing the music, starting the boss, and recording the defeat in `Progress`. | Which boss; remembered as. |
| `Creature` | A harmless animal that moves by its own behaviour and cannot be hurt. | — |
| `Scenery` | A fixture that reacts visibly to the player or events, with no effect on play beyond an optional solid body. | — |
| `Gunship` (this game's) | Landing, taking off, saving and refilling. | — |

Every kind here is the engine's except `Gunship`.

**Barriers and exits.** A door is two separate things, and neither knows about the other.

- A `RoomExit` is a region at the edge of a room. When the player enters it, the room transition begins. Its
  settings are the room and exit it leads to, and how the transition looks: scrolling across, scrolling up or down,
  or fading. Every way out of a room is one, whether or not anything stands in front of it.
- A `Barrier` is a solid object with health that only certain damage wears down. It stands wherever something must
  be got through. It knows nothing about exits.

| Barrier setting | Meaning | In this game |
| --- | --- | --- |
| Health | How much damage opens it. | A blue door, 1. A red door, 5. |
| What damages it | The amount each accepted `HitKind` takes off. A kind not listed does nothing. | Blue: any, 1. Red: missile 1, super missile 5. Green: super missile only. Yellow: power bomb only. |
| Held shut by | An optional `Condition`. While it is false, nothing damages the barrier and it shows as sealed. | Grey doors: enemies cleared, a boss defeated, a story event. Eye doors: the eye, an ordinary enemy, is dead. |
| Opened by | Its health running out; or being told to open by a trigger, a switch or a sequence. | Shutters and gates are told to. |
| When open | Gone; or slid aside and able to close again. | A door cap is gone; a gate slides. |
| Comes back | Never; when the player has passed; when its room is next set up. | A door cap returns when the room is next set up. |
| Remembered as | A name; once opened and remembered, it is set up open, or as a plain barrier. | A red door, once opened, is a blue door from then on. |

Door caps, gates, shutters, breakable walls and the shell around a held item are all `Barrier`s.

**Things built from these, not types of their own.** Apply move 1 before adding a kind.

- **An elevator** is a `Carrier` whose path ends at a `RoomExit`.
- **An item held in a shell** (a statue's orb) is an `Item` with a `Barrier` around it. The item cannot be touched
  until the barrier is open, because the barrier is solid.
- **An item hidden in the wall** is an `Item` behind a block. It cannot be seen or touched until the block is gone.
- **A boss room's locked doors** are `Barrier`s held shut by "this boss is not defeated". The `BossEncounter` does
  not touch them.
- **A switch** is a `Barrier` with one health and no body to speak of, whose opening tells another barrier to open.

### RoomSequence (base)

A scripted sequence belonging to one room. It lives beside its room's scene.

- **Owns:** its own timeline and progress through it.
- **Manages:** calling the methods of the room's objects, terrain, effects and camera at the right moments, and
  recording a story event when it finishes.
- **Does not:** hold behaviour that belongs to an object in the room, or exist outside its room.

### Other room parts

| Type | Owns and manages | Does not |
| --- | --- | --- |
| `RoomExit` | Above. | Have a body or a look, or know what stands in front of it. |
| `Liquid` | A room's body of liquid, of a `LiquidType`: its level, its rise and fall, and answering "is this point submerged". | Apply physics or damage. The player and enemies ask it and act on the answer. |
| `RoomEffect` (base) | A room-wide condition: its look, and reporting that it is active and any ongoing damage it does. The engine provides the base and the earthquake; each other one is the game's. | Reach into the player or enemies. |
| `Backdrop` | What is drawn behind and in front of the room: background layers, a scrolling sky, parallax. | Affect play. |
| `Camera` | Following the player within the room's limits. One per session. | Affect play. |
| `CameraZone` | A region with its own scrolling limits, which can be opened or closed. | Anything else. |
| `Trigger` | A region that calls one named method when the player enters it, optionally only when a `Condition` holds. | Hold logic. |

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
- **Provides the standard states**, each working and tunable: standing, running, crouching, crawling, turning,
  jumping, falling, wall jumping, knocked back, held, carried, pushed, using a station, scripted, dying.
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
| `Weapons` | The selected weapon, charge, cooldown, cost, the limit on live projectiles, combining switched-on properties into one `Hit`, and spawning the `Projectile`. | Each weapon and each property. |
| Abilities | Equipment changing what a state does, and an ability with its own lifetime running as a child of the player. | Each ability. |
| Body powers | Below. | Each power. |
| Ongoing damage | Below. | Which equipment cancels which source. |
| Being moved | Anything that moves the player against their will asks for the matching state and passes its data. Nothing sets flags or velocities on the player. | Extra such states. |

- Facing and aim are separate values, never part of a state's name.
- Something that has hold of the player can ask what a hit would cost them as equipped (`EnergyLostTo`), make them
  wait before their next shot (`Weapons.WaitAtLeast`), and halve their speed across a number of times
  (`SpeedHalvings`).
- **Does not:** know any enemy, boss, room object or item by type. It contains nothing only Samus does.

**The body as a weapon.** A body power, while active, makes the player's own body deliver a `Hit` of a named kind
to whatever it touches: enemies, barriers and blocks alike. The engine knows that a power is active, what hit it
gives, and that the player takes no contact damage from what that hit kills. The game says what makes each power
active. Nothing else treats the player's body as an attack.

**Ongoing damage.** Damage that arrives a little every update (heat, a damaging liquid, spikes held against) is
not a stream of hits. Each source adds its amount to the player's ongoing damage for the update, naming itself;
equipment cancels sources by name; the total is taken off once, at one point in the update, with fractions carried
to the next. Enemies that take ongoing damage use the same mechanism.

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
| Done to her | Bomb jumping, drained, grabbed. |
| Not in control | In the gunship, collecting a suit, refilling from reserves. |

- **The morph ball is the engine's crawl**, shown as a ball; the morph ball item switches it on, and spring ball
  switches on jumping while crawling. Wall jumping and crouching are the engine's, switched on for her. Her scene
  says so: crawling needs `morph_ball`, jumping while crawling needs `spring_ball`.
- **Her equipment** changes what a state does; it does not add states. Space jump lets a spin jump again. The suits
  cancel sources of ongoing damage and what liquids do to movement.
- **Her weapons:** the beam, whose switched-on properties (charge, ice, wave, spazer, plasma) combine into one
  `Hit`; missiles, super missiles, bombs, power bombs; the special attacks.
- **Her body powers:** screw attack while spin jumping, speed boost at full run, the shinespark, and a charged
  spin.
- **Her own children:** `SamusGrapple`, `SamusXray`, `SamusVisual`.
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
  judging a hit itself (`TakeHit` can be overridden, part by part); firing its data's shots; leaving effects and
  drops; moving by the shared `Mover` or carrying the player with it; flashing for a set time; and asking whether
  the player rests against its solid.
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
| Surface crawl | Following the surface it is on, around corners, in a set direction. |
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

A projectile needs its own class only for behaviour no setting gives it.

### Pickup (base) and Effect (base)

- A `Pickup` is a temporary drop of a `PickupType`. It owns its position, what it gives and how long it lasts; it
  gives itself to the player on contact and expires. It records nothing in `Progress`.
- An `Effect` is a short-lived visual. It owns its position and animation. It does not collide, deal damage, change
  gameplay state or use `GameRandom`.

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
| `MessageBox` | `engine/ui/` | Showing a message, waiting for it to be dismissed, and returning the answer when it asks yes or no. | Grant the item it announces or perform the save it asks about. |
| `MapView` | `engine/ui/` | Drawing an `Area`'s map from what `Progress` says is explored and revealed. The minimap, the pause map and the file-select map are all this, at different sizes. | Decide what is explored. |
| `ScreenFade` | `engine/ui/` | Fading, flashing or whiting out the whole screen over a set number of updates, and reporting when it is done. | Know why it was asked. |
| `TypedText` | `engine/ui/` | Showing text one character at a time with its sound. | Hold the words; they come from `GameText`. |
| `Cinematic` | base in `engine/ui/`, each one in `game/cinematics/` | A scripted sequence with no gameplay. Each is a `GameMode`. | Run gameplay rules. |
| `Hud` | `game/hud/` | Showing energy, ammunition, the selected weapon and the minimap. | Change any of them. |
| `PauseScreen` | `game/pause/` | The map and equipment screens; switching equipment through the player's `Inventory`. | Hold inventory state. |
| Menus | `game/menus/` | Title, file select, options, game over. Each is a `GameMode`. | Touch a running session. |
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
- Mini-bosses and bosses, each one scene: Bomb Torizo, Golden Torizo (one scene, two data files); Spore Spawn;
  Fake Kraid; Kraid (arm, foot and belly spikes are parts; thrown nails and spikes are `Projectile`s); Crocomire
  (tongue is a part); Phantoon (eye, tentacles, mouth are parts); Botwoon; Draygon (eye, tail, arms are parts);
  Ridley and Ceres Ridley (two scenes over one shared body class, `RidleyBody`, because the two fights share only
  how the body flies and its tail; breakup pieces are `Effect`s); Mother Brain (her brain, the joints of her neck
  and her beam are parts of one enemy written across several files, one per part of her fight; her glass and her
  turrets are enemies; her rings, bombs, drool and the glow at her hand are `Projectile`s of their own).
- What a boss does to the room (the camera's limits, doors, lights, the alarm and its countdown) is not in the
  boss. It tells the things listed on it (`ToldOf...`, `ToldWhen...`) a word, and a room object or the room's
  sequence acts on it; a change to the tile grid is a named terrain patch.

### Room objects

- `Barrier`: blue, red, green, yellow, grey and eye door caps, in every facing; the Ceres doors; the three
  shutters; the downward gate, opened by its switch; the Maridia glass tube; the old Tourian escape wall; the wall
  the animals break; Botwoon's wall; the shell around a statue-held item. Mother Brain's glass turned out to be an
  enemy (missiles that reach her count against it), the tubes of her room `Effect`s that fall as terrain patches
  clear where they stood, and her escape gate a terrain patch with thrown pieces as `Effect`s.
- `RoomExit`: every door, and the far end of every elevator.
- `Carrier`: every elevator; the Maridia transport tube; the Chozo statues that lower Samus.
- `Platform`: Tripper, Kamer, the suspensor platform.
- `Station`: save, energy refill, missile refill, map.
- `Item`: every upgrade and expansion.
- `Gunship`: the ship, as one object.
- `BossEncounter`: one per boss room. The Metroid rooms whose doors open when the room is cleared need none; their
  barriers are held shut by "this room has no live enemies".
- `Spawner`: pipe bug pipes; Rinka sources.
- `Hazard`: the Samus-eating plants; Ceres steam.
- `Creature`: the Etecoons and the Dachora, during play and during the escape.
- `Scenery`: the Tourian entrance statue. Bomb Torizo's hand (`BombTorizoHand`) and the morph ball eye
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
- `RoomEffect`: heat, rain, fog, spores, fireflea darkness, Ceres haze, earthquake, and room-wide colour changes.
- `Backdrop`: room backgrounds, the scrolling sky, the Ceres rotating floor.
- `CameraZone` and `Trigger`: every scroll region and scroll change.
- Room versions: every case where the old code had more than one "state" of a room.

### Room sequences

Ceres: the elevator arrival, the falling debris, Ridley's getaway. Crateria: the gunship landing. Brinstar and
Norfair: the Dachora shaft camera, Crocomire's room shaking. Maridia: the transport tube ride. Tourian: the
entrance statue's descent, the rising acid. The escape: explosions and shaking in each escape room.

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
