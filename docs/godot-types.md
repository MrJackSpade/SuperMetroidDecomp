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
  It also stirs the number each time lava or acid moves (`ILiquidWatcher`), in view or out of it and before the
  update's number is drawn: the two halves of the number change places, as the cartridge's liquid step does.
  `CartridgeSpeedReadings` is how the cartridge reads Samus's speed across against its limits (`ISpeedReadings`,
  which the engine asks of the game and has no reading of its own for): whole pixels and fraction each on its own, by the sign of
  their difference within one pixel's range, so that a fraction more than half a pixel over a limit's reads as
  under it. It reads how fast she moves up or down the same way (`ReachesWhole`), from the size of her speed the
  way she is held to be going (`Player.HeldSpeedUpOrDown`): the speed of a moonfall, or of a rise worn past its
  top, is a rise's run below nothing, which the cartridge holds at the top of its range and reads as below
  whatever a fall is asked about. So a moonfalling ball does not rebound, the landing is no hard one, no fast fall
  shows, and the Baby's drain does not cut it.
  Samus's body is moved as the cartridge moves it (`SuperMetroid.BodyMoves`): each move is tried only where it
  would end (section 7, "How much of its way a move tries"), so she keeps her part of a pixel beside what she has
  not yet run into, rests at the far end of her pixel on the ground and against a wall to her right, passes what a
  fast move steps over, is not stopped by a frozen enemy she is already inside, and is put on a slope's surface
  under her middle however steep the step. A taller form is fitted the cartridge's way too (`Samus.FitForm`, in
  `Samus.Forms.cs`): the body she has feels for the growth above and below with moves that are tried and not made
  (`Mover.TryDown`), for something solid and then, apart, for the blocks, a growth of half a block or more in two
  goes; with nothing either way she grows about her middle; stopped one way she is moved the other by what the
  growth lacks, or stays as she is when there is no room for that; stopped both ways she is put in the crouch, or
  stays the ball. The tries leave her the part of a pixel a stopped move would have.
  `CrashOrbitTravel` is how far a shinespark crash's echoes have circled, which the cartridge keeps in the height
  of the first echo let go and does not start over: a crash counts its circling on from what the last crash, or
  that echo while it flew, left, and entering a room leaves nothing; it moves the update she is free again.
  `EnergyFraction` is the part of one energy she holds beyond her whole energy: ongoing damage (heat, lava,
  acid, spikes, a chewing plant) comes off it first and borrows a whole energy the moment it asks for more than is
  held, so such damage takes an energy on its first update; a suit divides the damage counted in 256ths.
  `CartridgeAngles` is the cartridge's own sines, its aim at a point and the products it makes with them, for
  every enemy, boss and shot that reckons with an angle: the half wave in 65535ths and in 256ths with what is finer
  dropped, the fine sine that peaks at 32767, the aim read from a slope counted round, a speed along an angle in
  256ths, a whole speed's share along an axis, a length's reach rounded down, the short product that places a thing
  on a circle, and the grapple beam's own angle, counted clockwise from up. The engine's
  `Turns` are the engine's (rounded, in 65536ths) and give other numbers; an enemy takes the member its cartridge
  code takes (the engine hands out the sine's share of any peak, `Turns.HalfWave`, for such a table).
  `KeptScrollCells` is the cartridge's fifty scroll cells, which outlive each room: a room with an explicit scroll
  layout loads all fifty (its own screens and what the layout's load writes after them, from
  `rooms/scroll_layouts.json`), a room with none writes only its own, and the camera reads the cells past a room's
  own screens at its edges. A read past the fifty, which on the cartridge reads the stored maps, is not built and
  fails.
  `CartridgeSounds` is the cartridge's way with sound (`ISoundFlow`), which is part of how the game plays: a door
  waits for the sounds and the music waiting to run out, and the speed booster reads how many sounds were waiting.
  Every sound belongs to one of three libraries, told by the first part of its name ("samus", "enemy", "room"); a
  request that only stops sounds is named and asked for like a sound ("samus_stop", "samus_stop_spin_jump") and
  waits its turn like one. A library lets a sound in while fewer wait than its priority bears (one, three, six,
  nine or fifteen; fifteen is the most that wait), and one let in is dropped all the same while a door's passage,
  a demonstration or a power bomb's blast turns sounds away. On each turn a library hands its oldest waiting sound
  to the sound program and goes through a fixed exchange with it before the next (`SoundLibraryQueue`); the program
  takes up what it is asked every 512 of its samples, 534 of which pass in an update (`SoundProgramClock`), and a
  sound is heard when the program starts it, on the channel of its library. Music waits in a queue of its own, each
  piece at least eight turns once at the front (`MusicQueue`), and a change of music withdraws what every library
  asks for eight turns.
  The cartridge's music comes in sets, one loaded at a time (`audio/music_sets.json`: the set loaded at power-on
  and each set's theme). Loading a set waits its turn in the music's queue like a piece, stops the music, and makes
  the sound program drop what it was asked. A room whose version names a set that is not loaded has a silence and
  that set's load asked for; its music is then asked for unless it keeps what plays, or its piece (or the loaded
  set's theme, where it asks for the theme) is playing already and the set does not change. A version changes the
  music only when it is not the one last asked about, and never during a demonstration. Through a door only the set
  is asked for as the room is set up; the door's passage asks for the room's music when its wait for the music
  ends, and there a room that names no set is taken to differ from the one loaded, as the cartridge takes it. The
  game's own code asks for a set with `CartridgeSounds.AskMusicSet`.

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
7. Is it part of the tile grid, and is everything it does one of the block rules? **A block**, however many
   tiles it covers.
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
| `GameRandom` | The gameplay random generator | One per session. Cosmetics never use it. It draws from an `IRandomSequence`: the engine's plain one by default, or one the `GameDefinition` supplies when a game's outcomes must follow a particular sequence. |
| `GameAction`, `GameInput` | An action by name; one update's input | The engine names its standard actions; a game adds its own by name. A `GameInput` says which actions are held and which were newly pressed, and whether anything at all is held or was pressed; it can be given on without one action, for something that must not see it. |
| `GameClock` | The count of updates played that count | One per session. Anything that acts "every N updates" reads it. Nothing keeps its own global counter. An update counts unless the mode that played it says otherwise (`GameMode.CountsOnClock`): a message box's updates do not, the one that hands back among them, so what is timed by the clock stands still with what the message covers. A game that starts over within a session starts the count over (`Restart`). The count may come round to zero after a number of updates the game's definition gives (`ClockPeriod`; never, unless it says). Beside it runs a second, short count of the same updates (`ShortUpdates`, with its own `ShortClockPeriod`), which a game may start over by itself (`RestartShort`): this game's door does on every update from placing Samus for the next room's load to finishing its scroll, so that what is timed by the short count begins together in the next room. To tell one update from another, whatever the clock counts, there is the session's `UpdateNumber`. What is timed by the enemies' passes and not by the clock reads the session's `EnemyPasses`: one for each update of a room, held still or not, and one for each update of a passage between rooms on which the enemies take their turns; kept from room to room and started over with the game. |
| `HitKind` | A named kind of damage | The engine provides contact and hazard. The game names the rest (here: each beam property, missile, super missile, bomb, power bomb, each special attack, each body power). |
| `Hit` | One hit being delivered | A damage amount, the set of `HitKind`s it carries, its direction, who caused it (`Source`: a shot's launcher, not the shot), where what delivers it is as it lands (`At`, a shot's place), whether what it harms makes no cry of its own (`Quiet`), and how many updates fewer than usual what it harms shows the hurt (`FlashFewer`). |
| `Condition` | A yes-or-no question about the game's state, held as data | One of: a named fact in `Progress` is set; this room has no live enemies; the player owns a named piece of equipment; the player can carry some of a named amount; at least so many enemies have been killed in this room since it was set up; the room was entered at a named entry point; a named thing placed in this room is dead or open; the player's middle is above a height in this room; or all, any or none of a list of conditions. |
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
  - whether the hit passes: it takes no notice of the thing, nothing is done to it, and what delivered the hit
    goes on unspent (`Reaction.Passes`, "passes" in data). Doing no damage is not that: such a hit is a dud or
    is absorbed, and is spent. This game's extractor writes a passing reaction for what the cartridge's enemy
    ignores and leaves unmarked: a power bomb, Samus's body as a weapon, the grapple's line, a plasma beam, and
    every shot on the face block and the Spark;
  - whether the thing swallows the hit: nothing is done to it and nothing is shown, and what delivered the hit is
    spent on it and gone (`Reaction.Swallows`, "swallows" in data; the answer is a dud that leaves nothing). The
    extractor writes it for the other shots of an enemy the cartridge's shots are ignored by: there a beam that is
    not plasma, a missile or a super missile is marked as collided and deleted, and the super missile shakes the room.
- What is hit may say, for a hit, that what delivered it and would pass through stops on it (`IHittable.Stops`):
  a shot that goes on through what it hurts ends there as a shot that does not. This game's Ridley, Mother Brain
  and baby say so of a beam's hit, which is why a plasma beam does not pass them; a blast's hit still sweeps on.
- What is hit may turn the shot another way (`HitResult.TurnedBack`). It then says which way (`ITurnsShots`), and
  the player whose shot it is launches it anew that way from where it is (`Weapons.LaunchAnew`): at the speed the
  weapon gives a shot fired that way, in the picture and size of one, still the player's. A projectile of its
  own class that is no weapon's shot says itself what becomes of it (`Projectile.OnTurned`). No reaction in data
  turns a shot; what does decides it in its own class. This game's gold ninja pirate does, by the box of its
  picture the shot lands on (its poses name each box "plain", "vulnerable" or "armour"): a beam or missile that
  would do it no harm, and any on its armour, goes up and away when it was fired level and straight down otherwise.
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
| `GameMode` (base) | Its own state and screen | One top-level state of the game. Exactly one is in control; each names what it hands over to. It says what of the sounds' own work each of its updates does: as the update begins, whether the music waiting gets its turn, and once it has played it, what closes it (the usual, unless the mode says otherwise). A mode laid over another hands control back through that mode's `TakeBack`, within the update that ends it: the mode beneath does then whatever of its own update it had left undone and says what is in control next. A mode beneath another plays, when the one over it says so, what of it goes on while covered (`StepBeneath`; nothing, unless it says otherwise). | Reach into another mode's state. |
| `Progress` | Every remembered fact, by name, plus the map explored and play time | Answering and recording "has this happened". The play time counts the updates of play, up to the most the `GameDefinition` says it counts (`MostPlayUpdates`; this game's stops at 99:59:59 and 59 updates, and what is timed by it stops with it), or without end when it says none. | Decide consequences. |
| `SaveFile` | The save slots; in each, the saved form of `Progress`, the player's `Inventory`, and where the player resumes; and, beside the slots, the record that the game has been completed | Reading, writing, copying and clearing slots; keeping and dropping that record, which a game asks for what it shows only afterwards (this game's last set of demos). | Exist as live state during play. |
| `Options` | Button bindings, language, and the toggles the `GameDefinition` lists | Reading and writing them outside any save slot. | Change during an update. |
| `SoundRequests` | The session's `ISoundFlow` | Being where gameplay asks to hear something: a sound by name, on a named channel, with a `SoundPriority` (how readily it gives way when sounds are waiting), answered with a `SoundAdmission` (whether it was let in, and how many were waiting); a name may say its own priority after a colon ("enemy_cry:low"), as a sound named in a data file or a timeline's cue does when its priority is not the usual one, and is then asked with that; every sound to stop (the music plays on), as when the game is paused; music by name, with a wait. Answering whether sounds or music asked for are still waiting. | Play anything, or decide by itself what is let in or when it starts. |
| `ISoundFlow` | Whatever a game's way with sound keeps: what waits, and what is playing as gameplay sees it | Deciding how what is asked for comes to be heard: whether each sound is let in, how long it and the music wait, and on which update each starts. A room set up to be played asks it for the room's music once its version is chosen and before anything of the room enters, so that music the room's things ask for as they enter comes after the room's own (`EnterRoom`; a plain flow asks for the version's music at once unless the version keeps what plays). The session opens every update with it before the mode in control plays (the music's turn) and closes every update with it after (the sounds' turns); the mode says, as its update begins, whether the music gets its turn (`GameMode.GivesMusicItsTurn`), and once it has played the update, what closes it: whether the music gets a turn there, and how many turns the sounds get (`SoundWorkAtClose`, a `ClosingSoundWork`; one turn of the sounds unless the mode says otherwise). The engine's own, `ImmediateSoundFlow`, lets everything in and starts it at once on the channel asked, and nothing waits; a game supplies another through `GameDefinition.CreateSoundFlow`, which is handed the game's content to read its data from. It is game state, moved on once an update. | Depend on a sound device, or play anything. |
| `ISessionService` | Nothing; it is what a session-long service of a game's is | Being begun again with the session; stepped once for each update of play, by the room as play's part of its update, once the view has followed the player and before the room's sequences play; saying, without changing anything, whether it wants a mode of its own in control (`WantsToTakeOver`), which play asks before the player's energy is settled; and making that mode at the update's end when play lets it (`TakesOver`). The `GameDefinition` makes them; anything asks the session for one by type. One with something to do before the room acts is an `IUpdateOpener` as well: the room calls it as each of its updates opens, before the player's input is read, whatever is stopped. One with something to do each time the room's liquid has moved is an `ILiquidWatcher`: the room calls it right after the liquid's move, wherever the liquid is moved. | Belong to a room. |
| `PlayTerms` | What may follow from one update of play: whether a player with no energy may begin to die, whether what they are may save them, whether the pause may be asked for, whether a session service may take play over, whether the update counts toward the play time | Nothing; `PlayingMode` reads it. Play in its own hands is on the usual terms, on which everything may; a mode that plays play beneath itself gives the terms it holds play to (`PlayingMode.PlayBeneath`) | Say what of the room is stopped (that is `Stopped`), or step anything. |
| `IRoomPlay` | Nothing; it is what plays a room and has a part of its own in the room's update | Being asked by the room at the two points of its update that are play's: once the view has followed the player, when the session's services play their update; and when a player with no energy is settled, for whether they may begin to die and whether they may be saved. `PlayingMode` is it; a room stepped by nothing else is given none | Step the room's own things. |
| `Escape` (this game's) | The escape countdown | Starting, counting, showing and ending the timed escape, and reporting that it is running. It plays, and shows, only on the updates Samus is in her own hands: held for a refill, dying, riding an elevator, putting on a suit or aboard her ship, it stands still and is not shown, unless whoever has her says it still shows, as Ceres's elevator does. A session service; when time is up it hands over to its time-up mode. | Know which room the player is in. |
| `ElevatorJourney` (this game's, `game/objects/elevator/`) | The elevator ride under way, and whether an elevator's door is armed | Outlasting the room a ride begins in: begun by the elevator left, turned into an arrival by the door's passage, ended by the elevator reached. A session service. Every elevator of a room reads the one ride, as the cartridge's do. | Move an elevator or Samus. |
| `SharedFadeCount` (this game's, `game/compatibility/`) | The count of steps the cartridge's gradual fades share | Saying when a fade completes: a door's fade out and in, the black-out of Samus's death, the time up's white-out, the fades of Kraid's backdrop and a Torizo's body each take their step by it, move it on, and put it back to nothing as they complete; none puts it back as it starts, but Kraid, whose room is drawn anew first. A fade begun after one that stopped part way so completes sooner (a Torizo's waking leaves the count at one). What a fade shows on the way is its own count of steps. A session service, begun again with each game; the giant Metroid's membrane holds still while it is past nothing. It is here because recordings can tell: when a death's black-out ends decides when the game over takes input. |
| `CeresDebrisCount` (this game's, `game/rooms/ceres/`) | The count the Ceres hall's falling pieces go by | Keeping it from room to room: the elevator shaft leaves each step of its turn, the hall waits for its first piece by what it finds and leaves the updates before its next, so a hall entered again goes on from there. It is the escape's `HarderShakeLeft` over again: a count the cartridge keeps where no one room owns it. | Know anything of the rooms but the two that leave it. |

`SaveData` is the saved form itself: what a slot's file holds, captured from a session and put back into one. It
can give what it remembers as `Progress` of its own, for showing a saved game without loading it. A game saved to
resume at an entry point records the fact that it has been saved there (`SaveData.SavedAtFact`), which is in the
save.
`RoomVersion` is one set of a room's settings (its layout file, which of the layout's patches are in place, its music) with the `Condition` it applies under. A version may show none of its blocks to an ability that reveals by sight (`HidesBlocksFromSight`, asked through `Room.ShowsBlocksToSight`), as this game's scope shows none in the statues' room, the glass tunnel, and a room one of five bosses is in. A version may say the room is a boss's (`HasBoss`, asked through `Room.HasBoss`): one of the things it places is a boss, beaten or not, which this game's generator writes from the cartridge's own list and its Samus asks before her footsteps are heard. Its music is a piece by name, silence, or keeping whatever plays; for a game whose music comes in sets it also names the set its music is of (`MusicSet`, empty for none) and says whether it asks for the theme of whatever set is loaded (`MusicIsTheme`), which the engine's own flow of sound does not read.

**The engine's modes.** The engine provides the modes any game has. This game adds title, attract demo, file
select, options, game over, time up, and one per cinematic.

| Mode | Does | Hands over to |
| --- | --- | --- |
| `PlayingMode` | Owns the room being played and steps it. A message asked for by the time the room's objects have played stops the update there (`Room.UpdateIsSuspended`): the message is shown at once, the sounds get no turn on that update, and the rest of it, the room's (the shots tried against the player and against one another, what a projectile leaves for the update's end, the view following the player, the session's services, the room's sequences, the settling of a player with no energy, its shaking, its effects, and what leaves and arrives, in that order) and play's own (play time, unless the update withheld what follows from the player's energy running out; a save asked for; the HUD; death; a service taking over; a message; the pause; an exit entered), is played on the update the message hands back, with the input the update began with. Another message waiting then opens as the first hands back, and the rest waits for it too. A message asked for later in the update is shown with nothing left to play. While a message covers play the room opens an update of its own only when the message says so, with nothing of the session's. Writes a save when one was asked for, recording first that the game has been saved at that place. What of the room waits while the rest runs is what the state the player is in holds still (`PlayerState.Stops`: nothing, unless the state says so, as Samus's X-ray scope and her refill from the reserves hold the enemies, every shot and pickup and the room itself still); a game's own play may say otherwise by overriding one method. While the room waits the view does not drift by itself. It can be started in a room at a given `RoomEntry`. | The mode after death when the player has died; a `MessageBoxMode` when a message was asked for; a `RoomTransitionMode` when the player entered an exit; the pause mode when pause is pressed while the player is in control.  |
| `DoorTransitionMode` (this game's) | The passage through a `Door`, update for update as the cartridge makes it: the room being left fades out over fifteen updates while its enemies, and the effects left in it, go on; the view is brought square to a screen a pixel an update; the next room is set up and scrolls in, four pixels an update, while Samus is carried along the door's way at the door's own speed from her place within her screen; she is aligned and moved clear of the doorway; and the room fades in over fifteen updates while its enemies, their shots and its effects go on. Each update opens as an update of play does, with the random number alone, or with neither, as the old one does. A game supplies its own passage through `GameDefinition.CreateRoomTransition`; a room's parts are played for it by `Room.OpenUpdate` and `Room.StepForTransition`, and the player is brought in at a place with the view kept (`RoomEntry.At`, `KeepsView`). | The same `PlayingMode`. |
| `MessageBoxMode` | Shows a message from `GameText`; one that asks yes or no gives the answer to whoever asked. A message may come with a fanfare, after which the music there was comes back, when the session's flow of sound says (`ISoundFlow.AskFanfare`, `EndFanfare`: the engine's plain flow as the message closes, a game's by a count of its own), and may be held up a set time before it can be dismissed. The game names how its messages are shown (`GameDefinition.MessageBoxFile`, read as a `MessageBoxStyle`), and each is shown as a picture through a window that opens and closes about a line of the screen: nothing shows for a set time, the window opens a step an update, the message is held, a plain message then goes at any action held and a question looks at the player's presses every so many updates (confirm answers with the choice showing, cancel answers no, the style's move actions show the other choice, with a sound the update after), the window closes a step an update, and after a set time more the mode beneath takes control back, playing then what it had left of its own update. Such a message does the sounds' work itself while it covers play: the music gets no turn as its updates open, and each of its updates closes with a turn of the music and one of the sounds, but for the update that shows a question's choice moved, which gives neither, and for the updates on which nothing shows before the window opens and after it has closed, each of which does what the style says of it (`MessagePauseUpdate`: the music's turn, the sounds' turns, and whether the room beneath opens an update). A style may have every sound asked to stop as a message is shown, unless the message opens as another hands back. A question may name what follows a yes (`MessageAfterYes`: a sound, a wait and a second message), after which the answer is given. | The mode it was laid over: on confirm for a game whose messages are their words, once its window has closed for a game with a style. |
| `LoadingMode` | Begins the session's game again and puts the session's save slot into it, if the slot holds one (`GameSession.LoadSlot`, which a game's own loading mode calls too). | A new `PlayingMode` in the saved room, or the mode a new game begins in. |
| `Cinematic` (base, `engine/ui/`) | Plays update by update; confirm skips it unless it says otherwise. | The mode it names. |

- **Laid over, or replacing.** A mode says which mode it is laid over (`Keeps`). That mode stays on screen, is not
  stepped, and is told when control comes back (`Resume`). Any other hand-over ends the mode that finished, and
  everything it was laid over. The player lasts the session: one standing in a room that goes with a mode ended so
  is taken out of the room first (`Room.Teardown`), as on leaving any room, and goes on to whatever room comes
  next, as Samus does from Ceres to her landing; a player the session has already replaced goes with its room. A new mode may be handed to already laid over a mode that has not had control
  either, as this game's arrival is over the play its load made: each is put on screen and enters on that update,
  from the one at the bottom up, the mode handed to last.
- **Play beneath another mode.** A mode laid over play that lets play go on beneath it for a while plays it by
  `PlayingMode.PlayBeneath`, one whole update at a time, on the terms it gives (`PlayTerms`): what it holds off
  for as long as it covers play. Whatever play hands over to on such an update takes control in place of the mode
  laid over, which is done with. This game's pause screen holds off death and the pause while it darkens and
  brightens the picture; its time up holds off death, the pause and the escape's own take-overs under its
  white-out; its departure from Ceres holds off those and the reserves too. A player saved by what they are on
  such an update takes the update from the mode laid over (`Room.EnergySettled`), which hands back to play.
- **One thing follows from an update.** Where more than one thing would take play elsewhere on the same update,
  the order is: a save from having no energy, which takes the place of everything else the update began; a
  session service's mode; a message; an exit entered; the pause, which is not given on an update that entered an
  exit, nor where the game says the pause is not to be asked for (`GameDefinition.PauseMayBeAsked`: this game's
  not on Ceres, nor while a power bomb's blast is under way). A
  player with no energy does not begin to die on an update that is going elsewhere (a service wants control,
  the pause is asked for, an exit was entered): they are asked again on a later update, and the update that
  withheld it does not count toward the play time.
- **Asking, not reaching.** A room object does not start a mode. It asks the session: `RequestMessage` for a
  message, `RequestQuestion` for one answered yes or no (the answer comes back to whoever asked, after the sound, wait and
  second message the asker may name to follow a yes), `RequestSave`
  for a save, `RequestPause` for the pause screen once the messages asked for with it have been shown. The mode in
  control takes the request and acts on it.
- **Energy gone.** A hit that takes the last of the player's energy is a hit like any other: they are knocked back,
  play the update on, can be hit again and can be given energy back. What follows from having none is settled
  once in each update, by the room, when its sequences have played (`Player.SettleEmptyEnergy`), on what whoever
  plays the room says may follow (`IRoomPlay.EmptyEnergyTerms`): the player is saved if what they are saves them
  (Samus's reserves, which forget an exit entered on the same update), and otherwise begins to die, unless the
  same update took them into an exit or play says the update leads elsewhere, when they play on with none and are
  asked again after. What was settled is the room's to tell (`Room.EnergySettled`: not gone, saved, dying or
  withheld). The room does not shake on the update a save or a death begins.
- **Death.** There is no death mode in the engine. A player who begins to die is in `DyingState`, in which they
  neither move nor act and time stands still for everything else, and on that update `PlayingMode` hands to the
  mode the `GameDefinition` makes to play the death (`CreateDeathMode`), which is given the play it takes over
  from and says what comes after. The engine's default reloads at once. This game's is `DeathMode`
  (`game/samus/`): a last update of play, everything but Samus going black over play that still goes on with
  time standing still (`PlayTerms`, which here also say the updates do not count toward the play time), a wait
  for the music's queue, then her death's pose, flash, breaking up and whiteout, which the mode counts itself
  and shows through `SamusVisual.ShowDying` and her death's colourings, the last black-out on the screen's
  darkness, and the game-over screen.
- **Where a game plugs in** is always the `GameDefinition`: the pause mode, the mode that plays a death, the first mode,
  how a room is made, the folders its words, sounds and music are in, how many save slots it has, its own actions
  and the buttons they are bound to until the player chooses otherwise. The definition is abstract, and what a
  game must always say has no default of the engine's behind it: the first mode, the mode a new game begins in, the
  mode that plays a death, the passage between rooms, whether a pause may be asked, the session's services, the
  standard bindings, whether a status lasts, whether remains leave their drop, and how the player's body moves. An
  engine default behind a member this game always answers was never reached, and is not kept (#3124, #3326).

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
  map cell the player is in as explored (unless something of the room has said its map is hidden, `Room.MapShown`,
  as Ridley does, whose room shows and explores none of it), starting an earthquake when something asks, putting itself on screen
  where the camera's view has it (thrown about by an earthquake, which does not throw the player about), and
  tearing itself down when left.
- **Steps, in this order:** the opening of the update: the liquid moves, each running sequence and then each live
  projectile with a part in the opening (`IOpeningStep`) plays it, and then the session services that open an
  update (`IUpdateOpener`) play theirs; the
  first part of the player's update; the player's own shots; the enemies; the rest of the player's update; triggers
  and exits; every other projectile, pickups and remains; room objects, terrain, the liquid's pictures, the room effects and the sequence;
  effects.
- **Its liquid** may be held back from the player (`Liquid.HeldBack`): it is there, moves and is drawn, and the
  player moves as though it were not, until something of the room lets it in (`Liquid.LetIn`). This game's water
  behind Maridia's glass tube is let in by the tube, as it breaks or as it finds itself broken; the other room
  whose water is held back has nothing that lets it in.
- **Has the view follow the player** each update in the ordinary way, unless the state the player is in says the
  view follows them slowly (`PlayerState.ViewFollowsSlowly`, a `SlowFollowing`: so many pixels an update toward
  keeping them within a part of the view), as this game's fast swing on the grapple beam does.
- **Where play begins in it:** at an exit, at anything with a place, or at an `EntryPoint`, a place that is
  nothing else, which may say where the view is put when play begins there (this game's save points, elevators
  and landings put it where the cartridge's table of them does, keeping the parts of a pixel the view had);
  otherwise the view is put about the player.
- **Draws at set depths** (`RoomDepth`), from the back: the colour shown wherever nothing of the room is drawn
  (a layout may give one as its "backdrop"; the terrain spreads it behind everything, and it changes colour with
  the tiles); the backdrop; what lies behind the room; the room's tiles; what a thing shows behind everything
  standing in the room (a larva circling behind the player, a shot told to go behind); what stands far behind;
  everything in the room, in the order it stands there; what stands just behind the player; the player, whom the
  room gives a depth of their own as it takes them in (`Player`), so nothing is in front of them or behind them by
  when it came into the room; what is launched and left in play (`Launched`: shots, what a shot or a death leaves,
  what is dropped); what the player passes behind (`InFrontOfThings`, as a Ceres door's shutter); what is frontmost;
  the part of
  the backdrop's picture that lies in front of what stands in the room (`BackdropInFront`: a backdrop's picture may
  have that part beside it as a tile's may, the backdrop makes a `BackdropFront` and draws it again there, and so
  it covers the room's things and the tiles behind them, as the cartridge's background does where its blocks are
  drawn in front); the part of
  the tiles that lies in front
  of what stands in the room (`TerrainFront`, which the terrain makes and draws: a tile's picture may have that
  part beside it, and it is drawn again over the room's things); what a thing puts in front of every tile; the
  liquid; the overlays. A thing has the depth of the room's things unless it takes another of these, as a boss's
  head in front of everything does. No thing picks a depth of its own outside them. A backdrop's depth is given
  by its room as the room sets it up: a backdrop shown by a screen with no room (the title's floor, the opening's
  flight, Ceres's destruction, the ending) has none, and is drawn in its place among that screen's own things.
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
  solid, counted up from the tile's bottom, or down from its top for a ceiling. A slope may let only a share of a
  move along it through (`share`, in 256ths): the player's movement takes it off a move across a floor's slope, and
  the surface crawl goes along a floor's or a ceiling's by that share of its speed. A slope may say
  that it holds things up by their middle (`"holds": "middle"`): to a mover that treats slopes so
  (`SlopeTreatment.ClimbUnderMiddle`, which the player's body does) only the middle column of the body meets it, so
  the body's edges pass into it and the mover stands where its surface is under their middle, and such a mover
  climbs only slopes of that kind; a slope that does not say so is a step or a wall and meets the whole body. To
  everything else a slope is solid along its surface, whichever kind it is.
- A **block** is a reactive piece of terrain, as many tiles wide and high as its layout says (the cartridge's
  own are one or two each way, and the wall the animals leave by in the escape is three high), placed as one unit
  with a `BlockType`. It is not a node. `Terrain` owns whether each block is intact, broken, hidden or regrowing.
- A `BlockType` is a combination of these rules and nothing else. The rules are the engine's; the hit kinds, powers
  and abilities a block type names are the game's:

| Rule | Choices |
| --- | --- |
| Solid or passable | Solid; passable. |
| Broken by | Nothing; a `Hit` carrying a listed kind; being stood on (crumble). |
| After breaking | Gone until the room is next set up; regrows after a set time. |
| Remembered | Nothing; or a fact recorded as it breaks (`records`), for a wall whose breaking the game goes on by: the wall the animals leave by in the escape records that they have. |
| Hidden until | Never hidden; hit by a listed kind; seen by a revealing ability, which shows it for what it is only for as long as it sees it. A block with a picture of its own for what it is starts hidden whether or not anything reveals it, and shows that picture once it has broken and come back, unless its type says it hides again. |
| How it looks | The pictures of the tiles it lies on; a picture of its own once revealed, or one for each of its tiles; a timeline of pictures from the moment it breaks; a timeline of pictures up to the moment it comes back. |
| Heard | Nothing; a sound as it breaks; a sound of its own for a hit of a listed kind that breaks it (a block one weapon breaks silently and another with a crumble). |
| On contact | Nothing; hurts, by a hit that is not repeated while its victim is still safe from the last (spike), by ongoing damage, or both; carries sideways, always or only what is not rising or falling (conveyor); pulls down (sand), and may hold up what has its feet in it as ground does, pull harder what rises or falls in it, cap how fast it lets anything rise, take away momentum, and be eased by equipment (quicksand); the player is moved by the blocks their body is in each update, over and above their own movement; can be latched onto; can be latched onto once and then breaks, when it is let go of or a set time after it was first latched onto. |

- A block is drawn at every tile it covers, and its type's pictures are the same at each. A type's pictures may
  name the room's set of graphics by its placeholder, so one type serves every set.
- `Terrain` says whether the tile at a point draws a picture as the layout has it (`ShowsTileAt`): scenery, as
  against open space. This game's escape asks it, whose blasts out of doors go off only over scenery.
- `Terrain` hands the room the sounds of the blocks that broke each update, one for each block in the order they
  broke, and the room asks for each: several blocks broken together ask several times.
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
number of updates, with a sound a step may ask for; a step may take a patch back as well as, or in place of,
applying one, so a change set off in an enemy's turn that the cartridge shows at the terrain's own turn (the wall
at Phantoon's door) is a sequence of one step. It is data of whatever sets it off, which hands it to the room
to play; the room steps it once an update until it is over, so it outlasts a boss that is gone before its wall has
finished falling. The patch that is left is the one a room that begins after the change is given.

### RoomObject (base)

A placed, interactive fixture of a room that is not an enemy. One folder per kind: `engine/world/objects/<name>/`
for a kind any game would use, `game/objects/<name>/` for one only this game has.

- **Owns:** its position, its colliders, its own state machine, its visuals, its settings.
- **Manages:** its own reaction to the player, to hits and to `Progress`.
- **Does not:** act on its own as an enemy does, or change another object's or the room's state except through
  their methods.
- **Its turn:** with the room's objects, after the player and the projectiles. A kind that says it acts with the
  enemies (`ActsWithEnemies`) takes its turn among them instead, in the order all joined the room: before the
  player moves, held still when the enemies are, and on the updates of a passage between rooms on which the
  enemies take their turns. A thing the player's movement must find already moved, as an elevator, is such a kind;
  and any object does where its placing says so (`TurnsWithEnemies`), which this game's rooms say for everything
  the cartridge steps as an enemy: the gunship, Ceres's doors and steam, the animals, the morph ball's eye, the
  Rinka sources. The other way about, an enemy whose kind says it acts with the room's objects
  (`Enemy.ActsWithObjects`) takes its whole turn after them, wherever the view is, and none in the enemies' pass:
  what takes hits and acts as an enemy but was a fitting of the cartridge's room, as an eye door's eye.

| Kind | Manages | Settings |
| --- | --- | --- |
| `Barrier` | A solid object with health that only certain damage wears down, or that opens when told to. | Below. |
| `Item` | Granting its `ItemType` when the player touches it, recording it in `Progress`, showing its message. While something covers it, it is neither seen nor touched, and on the update it is uncovered its looks start again from the first. Its pickup ends as its message closes, on the update it was touched (it is an `IFinishesAfterMessages`: the room calls what asked, as the update goes on after its objects, once every message asked for by then has been shown). The player is told at both ends (`Player.OnItemCollected`, `OnItemPickupEnded`: this game's Samus clears her charge and shows a new beam's colours at the first and begins putting a suit on at the second). As it ends the item tells what its scene names (`Tells`, `TellsWhat`: the Speed Booster's room sets its lava rising so), shows nothing for one update and leaves the room on the next. | `ItemType`; remembered as; what it tells as its pickup ends. |
| `Station` | Holding the player while it works, then granting or recording the result. The sound of its finishing may come a set number of updates after it, and for a set number of updates after finishing it takes no one, until it is back at rest. The picture it shows as the room is set up stands through its first update there, its display moving on from the update after; and one with no look for working keeps its display at rest running unbroken through its work. One that pauses the game afterwards keeps hold of the player through that pause: whoever asks for a pause may say what is done as it ends (`GameSession.RequestPause`), and the mode that was the pause says when (`GameSession.EndPause`). One that is run against takes a player who moved through the update as one who has run into a wall (`Player.MovedAgainstWall`, which the game answers: in the wall-stop pose, for Samus, which is the update after the wall stopped her), facing it and against it, whether or not a direction is still held. | What it does: save (and the entry point a game saved there resumes at); reveal the area's map; refill listed inventory amounts. How it is used: by standing in it (it puts the player at its middle, facing out; used or refused, it waits for the room to be entered anew, as it does when the game resumed at it), or by running against either side of it, facing it and aiming ahead (it is then solid over its own area, by the solid collider of its scene). A question it asks first, whose yes takes hold of the player and begins the work as the question hands back, within the update that asked; the message it shows when done, how long that is held, and whether the game then pauses on its pause screen; how it looks at rest, at work, and at rest once it has nothing more to do (a map station whose area's map is revealed, which cannot be used again); an effect it leaves as it starts, which may be another for its right side; and its sounds. A refill station does nothing for a player who is full. |
| `Spawner` | Producing enemies at a place by a rule. | What it spawns; how often; how many at once. |
| `Hazard` | Hitting the player on contact, without being killable. Its data's timeline shows it at rest; one whose reach changes as it is shown says so by cues ("area:left:top:width:height", "safe"). | The `Hit` it does, and whether its contact is a hit even when it does no damage; the most updates it waits, by chance, before it first appears. |
| `BossEncounter` | Starting the boss, recording the defeat in `Progress`, and having the room's own music asked for again once the boss is gone. The fight's music is the boss's own to ask for. | Which boss; remembered as. |
| `Creature` | A harmless animal that moves by its own behaviour and cannot be hurt. The base shows the timelines its data names, passes it their cues, and moves it through terrain. | — |
| `Gunship` (this game's) | Landing, taking off, saving and refilling. Its question of saving, answered yes, is followed by the sound of saving, a wait and its message of having saved before its hatch opens again. | — |

Every kind here is the engine's except `Gunship`.

**Barriers and exits.** A door is two separate things, and neither knows about the other.

- This game's exits are `Door`s (`game/rooms/doors/`): a `RoomExit` that also says the way Samus travels through it, the screen of the next room the view comes to rest on, and how far she is moved on each update of its scroll. Every door of this game is passed by the door's own passage. An elevator's door is passed as any other: entered while an elevator's door is armed, the passage first lets a ride down go on for its wait, and as it sets the next room up it turns a ride that was leaving into one arriving.
- A `RoomExit` is a region at the edge of a room. When the player enters it, play asks the game for the mode that
  takes them through (`GameDefinition.CreateRoomTransition`). Its settings are the room and exit it leads to and
  where the player arrives. Every way out of a room is one, whether or not anything stands in front of it.
  The engine has no change of rooms of its own. The split this section is about is between what stands in the way
  (a `Barrier`) and the way out (a `RoomExit`); it does not need the engine to carry a passage, and how a game
  passes between its rooms is that game's own look and timing. The engine once had a plain fade
  (`RoomTransitionMode`, with a style on the exit and a length on the definition); every way out of this game is a
  `Door` with its own passage, so nothing reached it but the engine's own checks, and it was removed (#3124). The exit a player
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
| Looks and sounds | From its data: how it shows shut, held shut, hurt, opening, open and closing, and the sounds of opening, closing, a hit that wears it down and a hit while it is held shut. One that shows itself opening goes on blocking until it has, and one held shut as its room is entered may show itself shutting first. A look
that plays once gives way on the update after its last, so a barrier hit on one update whose opening lasts eighteen
stops blocking as the eighteenth update after that one is played, and what moves next passes. A look's
steps may say whether the barrier stands in the way meanwhile, by the cues "passable" and "blocking" (an item's
shell is air, then solid, then air as it bursts), and how far down from its top its solid part reaches, by
"solid_height:" and the pixels (a gate is solid a block further at each image of its closing, and its housing
stays solid while it is open); what the first step of its open or shut look says holds from the moment the look is
taken up. One whose data says so (`opens_around_player`) opens, resting shut, as the player's middle comes to lie
inside its solid part: a gate never keeps the player in its column. An `Item` under a barrier that has not finished opening
(`Barrier.HasOpened`) can be neither seen nor touched, however passable the barrier is. A `BlockType` that
`blocks_while_crumbling` goes on blocking, broken, while the pictures of its breaking or regrowing show: the block
an item is hidden in does, so the item can be reached only between them. | A door cap flashes when a missile wears it down and swings open. |

Door caps, gates, shutters, breakable walls and the shell around a held item are all `Barrier`s.

**Things built from these, not types of their own.** Apply move 1 before adding a kind.

- **An elevator** of this game is not built from these: it is the game's own `Elevator` (`game/objects/elevator/`),
  because the cartridge's ride is its own thing, update for update. Samus arms it by standing on the blocks it
  rests on, as she moved upright and settled; the elevator hears of that on its next turn, and a fresh press of its
  way on that turn starts the ride (without one the door is disarmed again). She then rides in a state of her own
  (`RidingElevatorState`): facing forward, held a set height above the elevator each time it takes its turn, and
  moved by her own step down of one pixel, which only the terrain under her feet stops. While the ride is leaving
  that step is what meets the door of the shaft, so the door is entered when her feet reach it and not when her
  body first overlaps it; she meets no exit by overlapping it while she rides. The ride itself is the session's
  (`ElevatorJourney`), since it outlasts the room: the door's passage turns a ride that was leaving into one
  arriving, the elevator of the room reached starts from the height rides arrive at with her on it, and after the
  passage comes back to where it rests, asks for its sound to stop and lets her go, still in the front view
  (`StandingForwardState`): there she moves nothing, fires nothing and does not stand at the elevator's door, until
  right or left is held and turns her that way. Resuming from the pause
  disarms the door and leaves the ride as it stands, so a ride down that reaches its door disarmed begins the
  passage without its wait.
- **Maridia's tube** is a room sequence of its room (`TubeRide`, `game/rooms/maridia/`), one for each end, started
  by coming in at that end. While it plays it holds the player's own update still, so Samus stays exactly as she
  came in; at the end of each update it puts her on the tube's middle line, moves her by the ride's speed through
  the terrain, has the room test its exits again, and lets the speed gain unless that would pass the fastest. The
  doors at both ends are passed as any other.
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
  the rest where every sequence is stepped. One that holds part of the room still while it plays says so
  (`Stops`), as a ride that moves a player whose own update must not run; play adds that to what the player's
  state holds still. What must hold from the first moment, before any update is played, is done as it starts (`OnStarted`), as the room is set up for one that starts by itself; it knows its room from then on (`Room`). One that moves the player for the view to follow on the same update plays before the view does (`PlaysBeforeView`); every other plays after. One that moves the player has the room test its exits again (`Room.MeetExits`), since the
  room's own test came earlier in the update.
- **Does not:** hold behaviour that belongs to an object in the room, or exist outside its room.

### Other room parts

| Type | Owns and manages | Does not |
| --- | --- | --- |
| `RoomExit` | Above. | Have a body or a look, or know what stands in front of it. |
| `Liquid` | A room's body of liquid, of a `LiquidType`: its level, its rise and fall, and answering "is this point submerged". A rise or fall goes by stages (an update that only begins its wait, the wait, the move), and the liquid says whether its last update was one of a rise or fall under way (`IsAstir`); what goes with a rising liquid, as this game's quake of lava and acid, keeps the room shaking by that (`Room.SustainShake`, which keeps a shake going for at least so long without starting it over). Its surface moves as an update opens, before anything acts; its pictures play on with the room's effects. Its type names the splash of something crossing its surface and the sounds of going under and of coming out, which the player leaves as their feet do. It may wait a set time before it moves once told, and may have a tide that lifts and lowers its surface. Something of the room may also set it heading for a height at a speed and after a wait of that thing's own (`Liquid.HeadFor`), as Norfair's Ridley raises the acid for his fight and drains it at his death. It draws itself across the view as its type says, and says whether its pictures drift. | Apply physics or damage. The player and enemies ask it and act on the answer. |
| `RoomEffect` (base) | A room-wide condition: its look, and reporting that it is active and any ongoing damage it does. It is stepped as an update ends, after everything that may start it or tell it something. The engine provides the base, the earthquake (side to side, up and down, or corner to corner) and `OverlayEffect` (pictures tiled over the view and drifting, as the data file of its look says: which pictures in turn, how far they move each update and with which of several winds (read from the number the room's chance last drew, at the digits its look names, with no number drawn: what is only for show never moves the chance on), how much they stay with the room as the view moves, whether they brighten, darken or cover what is under them, and whether they lie behind the room's terrain: fog, rain, spores, a haze; one with no look shows nothing: heat); a condition with rules of its own is the game's. `ColourEffect` is a change of colours over time: it plays the steps of its data file, each showing some colours of the room's pictures as others for a number of updates, on the room's terrain, its backdrops and the placed things it names; it shows its first step on its first update, goes back to the step its data names or stays on its last, ends once a `Condition` it is given holds (its colours taken back), and begins again at its first step on every update another holds. Whatever starts one that the scene does not place adds it to the room (`Room.AddEffect`); it takes its turns here, before the player, and so first shows on the update after it is added, as a change the cartridge plays from its list of colour animations does. A change the cartridge's enemy writes to the colours itself is not an effect: its owner shows it at once (`Room.Recolour`). | Reach into the player or enemies. |
| `Backdrop` | What is drawn behind and in front of the room: background layers, a scrolling sky, parallax. A layer may lie at a place in the room. `TurningFloor` is the backdrop that can be turned and brought nearer or farther about a point, with the visuals under it turning with it. | Affect play. |
| `BandedBackdrop` | A `Backdrop` whose picture slides sideways in bands of rows, each at its own speed, as the layers of a sky do: the picture lies in the room as its scroll share says and each band is shown moved along by how far it has drifted, in whole pixels. | Its bands' drifts, from its data file. A backdrop that only drifts as one piece is a plain `Backdrop`. This game's scrolling sky is one: its rows are the room's, so it moves down the screen with the room, and it does not follow the view across. |
| `VisualFront` | The part of an `ActorVisual`'s picture that lies in front of the things standing in a room, drawn at the depth of the tiles' own such part: a picture made from the room's blocks (a station's display, a door's cap, a gate) has that part beside it as a tile's has, and the visual makes this the first time its picture has one. Without it such a picture lay under the front part of the tiles it stands on. | Be made or placed by anything but its visual. |
| `Enemy.InFrontOfTiles` | That an enemy's pictures lie in front of every tile of the room, as its data says ("in_front_of_tiles", written where the cartridge draws every one of its frames so): it is drawn at `RoomDepth.InFrontOfRoom`, over the player too, whatever its standing. The cartridge can show a sprite over the front tiles and still under the player; a room here draws in one order, so the tiles win, and an enemy for which standing behind the player matters more says so in its class (Ridley in Norfair). | Be said for one picture of an enemy and not another. |
| `Standing` | Where something stands among what stands in a room, against the player: rearmost, far behind, behind (where a thing stands unless it says otherwise), just behind, in front, frontmost. Each is one of the room's depths (`RoomDepth.Of`), and the player has a depth of their own between them, so which is drawn over the other does not depend on when each came into the room. An enemy's data names its own ("standing") and its class sets it where it changes in play (`Enemy.Standing`); one picture of a thing stands apart from the rest by `RoomDepth.Stand`. | Order things of one standing, which stand in the order they are in the room; say where a picture lies against the room's tiles. |
| `RoomTurn` | A turn the room is shown with (`Room.ShownTurn`): the player, and the objects of the room the turn names, are drawn where turning them about a point puts them; what it does not name is shown where it stands. Ceres's tilting shaft names its steam and not its doors, and shows them at the tilt its picture had the update before. | Change where anything is. |
| `Camera` | The view onto the room. A room is a grid of screens, 256 pixels each way, each closed to the view, open, or open with the view aligned to its top (`ScreenScroll`). Each update, once everything has moved, the view follows the player by how far they moved and a pixel more, toward keeping them at the place in the view the room gives (`Room.PlayerInView` by the way the view leads, `ViewAboveWhileRising`, `ViewAboveOtherwise`; rising is the player's own word for it, `Player.IsRising`, which a rise slowed to rest or past it still is until their state turns it into a fall); it is stopped by the room's edges and by closed screens, and on an update the player did not move a whole pixel it drifts clear of a closed screen by itself. Its place has a fraction of a pixel. It can be placed (`Place`), moved by a script (`MoveTo`), told where the player is to be taken to have stood (`CapFollowAcross` and `CapFollowUpOrDown` (which hold the view to following a player by so many pixels and one more, as this game's Yapping Maw and Draygon hold it to twelve where they place Samus, and her shinespark's flight to fifteen across and fourteen upward), `TakePlayerToHaveStoodAtX`, `...Y`: a door's passage tells it where its scroll left her, and the player tells it whenever a change of form moves the middle of their body, so crouching, curling up, standing up and landing are not followed as moves), made to follow slowly (`TrackSlowly`), and kept at or below a height (`ViewFloor`). A room may hold the view at one place instead (`Room.ViewHeld`, `ViewHeldAt`). One per session. | Affect play. |
| `IScrollCells` | How each screen of the room being played lets the view in: what the camera reads and what zones write. The game supplies the keeper (`GameDefinition.CreateScrollCells`), which is given the room's own screens from the room's version (`RoomVersion.Screens`); the engine has none of its own. | Know why a screen changes. |
| `CameraZone` | One screen of a room, by where it stands, as something that can be told how to let the view in: "close", "open" (the way the zone opens, `OpensTo`), "open_fully", "open_to_top". It may be told something as the room is set up (`ToldAtSetup`). It may stand below the room's last row, for a game that keeps such a screen's state. | Hold the screen's state, which is the room's. |
| `ViewFloor` | A height the view's top is kept at or below while it holds; told to "release" or "hold". | Anything else. |
| `Trigger` | A region that calls one named method when the player enters it, optionally only when a `Condition` holds. It is the player's body that enters it, or, where it says so (`MeetsMiddle`), their own place: the escape's wall and rising acid are set off by where Samus is. | Hold logic. |
| `Barrier`'s solid by its look | A barrier's timeline may say, step by step, how it stands in the way: "passable" or "blocking"; "solid_height:" and so many pixels from its top; "solid_ends:" and so many pixels at each end of its length, the middle letting through (its far end is a second solid collider of its scene), and "solid_whole" again. This game's door caps are solid at their two end blocks only while nearly open, where the cartridge's are. |
| `Relay` | Turning a word it is told into what the room does about it, by its rules: putting a terrain patch in place, and shaking the room. A door come in by tells a relay to start the treadmill it starts; the escape tells one to open the animals' passage. | Hold behaviour, or accept a word it has no rule for. |

**Why, and extending it.** A room is a container that forgets. That one decision is why "remembered as" and
"present when" exist: instead of each object managing its own persistence and each room having variants, there is
one rule for surviving and one rule for being there. The room object kinds are deliberately few and general,
because most fixtures of the old game turned out to be a `Barrier`, a `Station` or a combination. Before adding a
kind, try to build the thing from existing kinds placed together; the list under "Things built from these" is the
pattern. A fixture needs its own kind only when it has behaviour none of them has. When a barrier, platform or
station needs to do one more thing, that is one more setting or one more choice within a setting. When a block
needs a new rule, it is a new choice in the block rules table, and the question is whether it is still "a tile
with a rule" or has become an object; if it needs its own state machine, it is an object.

## 6. Actors (bases in `engine/actors/`; this game's actors in `game/`)

### Player (base)

The engine's player controller: what a metroidvania's player character is. A real class with real behaviour. Lives
in `engine/actors/player/`. It is abstract: nine things are a game's to say and have no answer here, since this
game gives its own for every one. They are how gravity adds to a fall and where it stops (`AfterGravity`), the aim
the input asks for (`AimFromInput`), what a hurt does beyond its damage (`OnHurt`), the state a wall leaves a
runner in and when the player has moved as one run into a wall (`StoppedByWall`, `MovedAgainstWall`), how ongoing
damage becomes whole points (`OngoingPointsTaken`), what a shot inherits of the player's movement
(`ShotVelocity`), the energy a hit takes (`DamageFrom`), and the state the player is in next as one gives way to
another (`Entering`). The engine's checks play a stand-in player that gives the plainest answer to each
(`StandInPlayer`, with the checks).

- **Owns:** position and velocity, facing and aim, the state machine that says what the player is doing, the
  `Inventory`, the `Weapons`, health, hurt and invincibility, ongoing damage, colliders, and the `ActorVisual`.
- **Manages:** turning input into movement and actions, moving with the shared `Mover`, firing, taking a `Hit`,
  collecting pickups and items, and applying to itself what the room reports.
- **Provides the standard states**, each working and tunable: standing, running, crouching, crawling, changing
  posture, turning, jumping, falling, turning in the air, wall jumping, knocked back, held, using
  a station, dying.
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
| `Weapons` | The selected weapon, charge, cooldown, cost, the limit on live projectiles, combining switched-on properties into one `Hit`, and spawning the `Projectile`. A weapon whose shots inherit the player's movement asks the player for the speed a shot leaves at (`Player.ShotVelocity`), which is the game's rule to give: Samus gives the cartridge's, kept in `game/compatibility/` (`ShotInheritance`: the whole pixels of her last move the shot's way, the part of a pixel of the record before it, a quarter for a shot upward), fed by each move of her body (`Player.OnBodyMoved`). The limit is a pool of places (a projectile a game launches for the player by itself can be had to hold one, `Weapons.HoldPlace`, and a pool's size asked, `PlacesIn`: a shinespark's echoes, let go, each take one of her shots' places): weapons that name the same pool share its places (this game's beams, missiles and super missiles share five), one firing takes the places its weapon says (a beam one however many projectiles show it, a super missile two) or one for each projectile, and a place is given up only once the projectile has ended and what it left on ending, its effect or its burst, is gone too. A weapon that charges fires a plain shot on the first update its attack is held and counts the hold from there; let go, it fires charged, or plain where its data says a part charge fires. One that does not charge fires on a press, and while held where its data gives a wait for that. A variant may have its own sounds and waits, and may say the weapon does not charge while it applies. | Each weapon and each property. A charge being built is the player's and not the selection's: it stands through a change of weapon and while a weapon that does not charge, or one that cannot be used, is selected, and ends only when it is let go or dropped (`DropCharge`). |
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
  waiting. The places of a pool of the player's shots are numbered from nought and what is fired takes the lowest free, so a game can ask where what holds a place stands (`Weapons.StandsIn`), as this game's Metroids are pushed from the first. A hold may keep what the player was doing (`Player.HoldAsTheyAre`): the state is covered, neither left nor played (`StateMachine.Cover`, `Uncover`), and letting go has them go on in it with the speed they had, as this game's Yapping Maw lets Samus go. Left and right held together are no direction of their own: the player's intent reads them as the state they are in reads them (`PlayerState.ReadsBothAcross`: the way ahead running, crouched, turning and in a spin; the way behind standing, in a jump and falling upright; rightward in the low form; neither where a state does not say). A state names the form it is entered in (`PlayerState.EntersIn`), and the form is fitted before anything of the state is entered (`Player.FitForm`): fitted, the player is put where the fitting leaves them and the state is entered; not fitted, they stay in the state they were in and nothing of the new one happens; and where the fitting answers with another form (a game's own fitting may: a body with no room to stand is put in a crouch) they are in the state that goes with that form instead (`Player.StateFor`). The state machine asks its owner what it makes of each state before leaving the current one (`StateMachine`'s `admits`). A form changed in the midst of a state is asked for by `TrySetForm`, which says whether it was taken and takes no other form in its place. The engine's own fitting takes the first place the form fits, with the feet where they are on the ground, and about the middle, then the feet, then the head in the air. A change of form moves the middle of the body at once, but the body and the hurtbox keep the shape they had until the player's next update begins, so whatever meets the player in between (the room's exits, the enemies' shots, pickups, objects) meets the body they had. The player keeps the state they last moved as (`MovedAs`), for whoever must know what they were doing
  through the latest movement whatever they have become since. A state says whether the player is in another's hands in it (`InAnothersHands`: held, at a station or dying, and not a knockback or any move of their own), which whatever runs only while the player plays reads. A state says what of the player the room's triggers are tried against (`TriggerReach`: their body, unless it says otherwise, or nothing). A state may say the player meets no exit by
  overlapping it (`MeetsExits`); whoever moves the player then takes them through one (`Room.EnterExit`).
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
  and a dash against its cap is the game's to say (`ISpeedReadings`, from the `GameDefinition`; the engine has no
  reading of its own), and so is whether a speed up or down reaches so many whole pixels, which the ball's rebound
  asks of the speed it came down at (`Player.LandedAtLeast`) and a game may ask of the speed now
  (`MovesUpOrDownAtLeast`). A dash is a second speed on top, carried the same way, kept through the air while the player
  has momentum; `FoldDash` adds it into the player's own speed and leaves none. Meeting a wall brings all of it to
  rest. States that throw the player about (a knockback, a push, a script) set `Velocity` outright.
- **Standing** keeps no speed: each update a dash's speed alone still carries the player, and then everything across
  is at rest. **Running** builds the dash while its action is held. A runner whose input asks for nothing still moves
  that update at full speed; then, if they had speed, they stay running, the dash is folded into their speed, their
  momentum ends, and they slow, unsteered, each update, even if steered again, until past rest; they then gain one
  step and stand. A runner who lets the direction go but still aims stands at once. A runner stopped by a wall
  stands. **Turning** on the ground faces the new way from its first update and reverses by the turn's own move
  values for as long as the turn lasts, ending the player's momentum on each of its updates; it ends standing, or in
  whatever state it was given to lead to (`TurningState(then)`), and its end gives way to a jump asked for on its
  last update. Letting the new direction go during a turn makes the
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
  left for standing to spend. A player may also be left adrift (`Player.Drift`, `IsAdrift`): in the air at rest,
  neither rising nor falling, by something that put them there without starting a fall. Gravity draws one adrift
  down as it wears a rise away, with no fastest fall and no top to be turned at; a jump is not cut short; a turn in
  the air and a change of posture press them down as they do on the ground, gravity left out; the low form is
  taken as on the ground, and falls from rest on its first move. They are adrift until they land or something
  gives them a rise, a fall or a speed up or down of its own.
- **In the air.** A jump keeps the jump's own move values from its start until the player lands: `JumpingState` is
  the whole jump, and `FallingState` is for walking off a floor and for being let go in the air. A jump from the
  ground may first gather itself for `jump_start_updates`, moving nothing of the player's own. Both steer by the air
  rule (`SteerAndMoveInAir`): steered with neither direction held, the player stops across at once and does not move
  across that update; unsteered or reversing they carry on. A player taking up a new stance in the air (`AirStance`:
  their aim, and aiming level in a jump whether they steer the way they face or hold the attack, `AirArm`; a stance
  with the arm held out keeps itself until some other stance than the arm at rest is asked for) has their
  steering set afresh: unsteered while a dash still carries them, else steered. A jump holds a stance from its
  start: one from the ground starts in the stance of the aim the player has as it begins, their arm at rest, and
  holds it while it gathers, the input asked nothing but on the updates of a gathering before its last
  (`GatheringListens`; only where it lasts more than one, as in a liquid), when the jump held with the way behind
  turns the player and the jump with an aim or the way ahead, or the attack, ends it; as the gathering ends the game's player is told
  (`JumpGathered`) and may name another state, which takes effect as a state's own end does. A jump that carries on
  from something else starts in the stance it is given (`JumpingState(AirStance)`), or failing that the one the
  input asks for as it begins. A change of stance within a jump takes effect only when the body can take the form
  of the new one, and the jump says so for that update (`TookUpStance`), which the game's player can read as it is
  told (`Entering`). An update with nothing at all held makes a jumper who has speed unsteered
  (the dash is folded into their speed and their momentum ends) and takes a faller's dash and momentum. It also has a
  jump that aims up, or diagonally either way, let its aim go for the stance of steering the way the player faces,
  so that the same aim asked again is a stance taken up anew. Steering
  against the way they face turns them in the air (`AirTurningState`): they face the new way at once and reverse by
  the turn's move values for as long as a turn lasts, ending their momentum each update; the turn cannot land, is
  not cut short, keeps its upward speed against a ceiling, and gives way to the jump or the fall it came from, a
  jump aiming level then being in the stance of steering the way they now face, and a turn made aiming diagonally
  down coming out aiming straight down. A landing ends standing whatever is held.
- **Postures.** Crouching down, standing up, taking the low form and leaving it each take the updates the game
  gives them (`crouch_updates`, `stand_updates`, `crawl_enter_updates`, `crawl_leave_updates`; none is at once). For
  that long the player is in `ChangingPostureState`: the body has its new form from the first update, the input is
  not asked, and they move across only as far as a dash carries them, their own speed kept. Through a change between
  crouching and standing they can attack, aiming as they did when it began (this game's Samus fires from heights of
  the change's own, `changing_to_crouch` and `changing_to_stand` in her shot heights); through one into or out of the
  low form they cannot. A stand asked of a crouch with no room to stand is refused and the player stays as they
  are: it was asked, so nothing else is that update, and their momentum is not ended. A
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
- A state names the form it is entered in (`PlayerState.EntersIn`), and the player fits it first (`FitForm`):
  where it does not fit the state is not entered. A state may ask for its form unfitted (`FormAsked.Unfitted`):
  the body changes about its middle where it is, into whatever is there, as this game's crystal flash stands
  her up out of the ball.
- The effect left at the player's feet on landing may come from their data.
- The game's player is told as a shot of theirs is launched (`Player.OnFired`): this game's Samus holds her own
  bombs off her for a count from each beam or missile fired, as the cartridge does.
- Facing and aim are separate values, never part of a state's name.
- Something that has hold of the player can ask what a hit would cost them as equipped (`EnergyLostTo`), make them
  wait before their next shot (`Weapons.WaitAtLeast`), and halve their speed across a number of times
  (`SpeedHalvings`).
- **Does not:** know any enemy, boss, room object or item by type. It contains nothing only Samus does.

**The body as a weapon.** A body power, while active, makes the player's own body deliver a `Hit` of a named kind
to whatever it touches: enemies, barriers and blocks alike. The engine knows that a power is active, what hit it
gives, and that the player takes no contact damage from what that hit kills. The game says what makes each power
active, whether what a power harms makes no cry (`BodyPower.Quiet`) and shows the hurt for less (`FlashFewer`), and what follows each blow
(`Player.OnBodyPowerLanded`, told what became of it: here the contact's sound and the end of her invincibility after one that harms, and a charged spin's charge spent by any touch on an enemy). Nothing else treats the
player's body as an attack. An active power's hit is also delivered to the blocks in
the way of each move before the player makes it, so the body breaks what it can as it meets it and is not stopped by
what it breaks.

**Ongoing damage.** Damage that arrives a little every update (heat, a damaging liquid, spikes held against) is
not a stream of hits. Each source adds its amount to the player's ongoing damage for the update, naming itself;
equipment cancels sources by name; the total is taken off once, at one point in the update, with fractions carried
to the next. Enemies that take ongoing damage use the same mechanism. On each update, for each thing of the room itself (its heat, its liquid) that does the player ongoing damage, the player is told so with its name (`OnRoomDamage`), which the game's player answers with its sound, at a priority of the source's own.

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
| Stopped by a wall | A wall she runs into leaves her in the cartridge's wall-stop pose (`WallStoppedState`; the engine asks the player what a runner stopped by a wall becomes, `Player.StoppedByWall`): she moves as she does standing, and her input is asked in an order of its own, in which a jump is always a plain one and walking backwards needs an angle action. Set down by an elevator she stands in the front view until right or left turns her (`StandingForwardState`). |
| Not in control | Facing forward where something puts her (in the gunship, on the pad that sets her down in Ceres), collecting a suit, refilling from reserves. |

- **The morph ball is the engine's crawl**, shown as a ball; the morph ball item switches it on, and spring ball
  switches on jumping while crawling. Wall jumping and crouching are the engine's, switched on for her. Her scene
  says so: crawling needs `morph_ball`, jumping while crawling needs `spring_ball`.
- **Her crouch and her ball.** Crouching out of a run at full boost stores a shine, as the crouch begins; the dash
  then slides her through the change and the first update of the crouch, and her boost lasts while the crouch is
  held aiming. A jump from a crouch with her arm level starts `crouch_jump_lift` pixels off the ground. In the air a
  press of down while already aiming down, or in a wall jump, curls her up. Nothing she presses is read while she
  changes into or out of the ball, or crouches down or stands up: the cartridge reads no input in those changes, though
  its tables list the attack with a direction as breaking out of the change out of the ball.
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
  faces are held; with the attack let go she turns, with the jump if it is held; steering the way she faces she
  runs; walking off a floor she falls facing the way she was going. A turn made with the jump that runs out leads
  to a spin with no jump begun (`SpinJumpingState.Start.Adrift`): she is adrift in the spin's smaller body, cannot
  space jump, and is drawn down with no fastest fall until she lands, which is the cartridge's moonfall.
- **Her aim in the air.** An aim straight down stays, with nothing held or only the jump, until a direction,
  another aim or the attack is asked; her body is compact for as long (`aiming_down`).
- **Her aim on the ground.** Running with a direction held she aims level or diagonally, never straight up; down
  aims diagonally down only with a direction held. Standing with both angle actions held she aims straight up and a
  direction the way she faces does not start a run. Crouched, up does not aim: a press of it stands her up with her
  arm level, and she aims up once she has stood; only both angle actions aim a crouching Samus up.
- **Her weapons:** the beam, whose switched-on properties (charge, ice, wave, spazer, plasma) combine into one
  `Hit`; missiles, super missiles, bombs, power bombs; the special attacks.
- **Her body powers:** screw attack while spin jumping, speed boost at full run, the shinespark, and a charged
  spin. Each update's movement says anew which are at work, and they stay so for whatever touches her until her
  next update; the speed boost's is at work from the update after she reaches full boost.
- **Her own children:** `SamusGrapple`, `SamusXray`, `SamusVisual`, `SamusFlare`.
- **The flare at the end of her arm cannon** (`SamusFlare`): while she holds a charge, a glow and two rings of
  sparks, each on a timeline and a count of its own, drawn in front of her from the pictures of the beams she has
  switched on and in her colours. It starts as the charge does, shows from the updates her data gives, moves on by
  her updates whether or not the charge's count does, and is hidden, still moving on, while her pose does not hold
  her arm out (`Samus.HoldsArmOut`). It lies where her data places it by how she stands, faces and aims
  (`Samus.FlarePoint`), raised as a shot's start is. While her grapple beam is out it shows the glow alone, on a
  count of its own taken up where the glow loops, where the line leaves her (`Samus.GrappleFlarePoint`), which is
  also where the line is laid from; the charge's flare neither shows nor moves on meanwhile.
- **The X-ray scope starts** (`Samus.ScopeStarts`) with the scope chosen and the run action held, from a Samus who
  stands, runs, crouches or is stopped by a wall and was so as her update before ended as well. Once started it
  takes its opening updates before the run action is looked at at all (`XrayWarmUpUpdates`), and once that is found
  let go it holds her and the room its closing updates more and lets go on the update after
  (`XrayClosingUpdates`). It does not start
  while she settles from a landing (`StandingState.IsSettling`), while she moves up or down, with her grapple beam
  out, while a power bomb's blast is under way, or while something that has hold of her withholds it
  (`Samus.ScopeWithheld`, which the giant Metroid sets for as long as it drains her).
- **The grapple beam's end takes up pickups** (`SamusGrapple.TakePickups`): while the line is out, every pickup
  that has lain `PickupDelay` updates and lies within `PickupReach` pixels of the line's end, across and down, is
  collected (`Pickup.Collect`), judged as the pickup's own turn of that update would find it.
- **Her swing on the grapple beam** (`GrapplingState`) is the cartridge's, not a weight on a string: a line of a set
  length turning about what holds it, its angle counted clockwise from straight up in 65536ths of a turn and its
  turning in 256ths of an angle step. She is put at the line's end each update, by the way the line runs and the
  way she faces; nothing moves her body through the room, and the ground is met only at the points along the
  line the swing tests. Its numbers and the places of her body are data (`GrappleSwingRules`, the group "swing"
  of the grapple's values). A line that takes hold swings her when she was moving up or down or it was fired
  upward, and otherwise holds her in place as she stands or crouches (`GrappleLockedState`); let go of a swing she
  flies by a movement of its own (`GrappleReleasedState`), begun on the update she lets go. The line itself
  (`SamusGrapple`) reaches out in tested substeps and ends on the update after it is given up or let go.
- **How she is shown.** `SamusVisual` plays the timeline named for her look, the way she faces and her aim, falling
  back to the look and facing, then the look alone; with "_low" after any of them while her energy is low. As her
  look changes, a timeline named `<from>_to_<to>_<facing>` is played once between the two if there is one (a
  landing, a crouch, curling up); with "_suited" after any of them while she wears the varia or the gravity suit
  (she faces out of the screen in another pose). Her pictures are written once, in their sheet's own colours.
- **The view's measure of her last move across** (`Camera.Speed`) is one pixel whenever the session's game starts
  afresh and its player is a new one (`GameSession.BeginAgain`, `Camera.TakePlayerAsNew`): what reads it before
  the view next follows a move, as a kicked Yard does, reads that.
- **Landing on Zebes after Ceres she is made anew** (`GameSession.RenewPlayer`, which a game calls between rooms to
  set its player up afresh partway through): a new player as the game definition makes one, carrying only what the
  one before carried (`Inventory.TakeAllOf`), her energy filled by the landing's load. Her speeds, a charge held,
  the safety a hurt left and what she had selected do not come with her, as on the cartridge.
- **How far a spin's pictures have got** is counted by her, not asked of her visual (`Samus.CountSpin`, as her
  update ends, where the visual steps the same pictures): how many updates she has been in a spin, and how many
  since a wall jump began, through the spin state it lets go into. A wall jump shows its launch and then, chosen
  once as the launch runs out by what is switched on and whether her head is under a liquid, the plain spin, the
  space jump's or the screw attack's (`WallJumpLook`: "wall_jump_spinning", "wall_jump_space",
  "wall_jump_screw"). Her spin's sound to ask for again (`SpinSound`) and the screw attack's glow go by these.
- **A crystal flash's blast is its power bomb's.** A power bomb whose blast ends tells her and waits; if a
  crystal flash begins she keeps the bomb, which goes on holding its place among her power bombs, and as the
  flash's rise is done the same bomb plays the flash's own blast from where she has risen to
  (`PowerBomb.BeginFlashBlast`, by its `flash_blast_reaches`): it sweeps the enemies as any blast does, strikes
  no terrain and offers no flash. With no flash the bomb leaves on the update its blast ended.
- **A crystal flash is upright where she is, and is no shield** (`CrystalFlashState`). She takes her upright
  form as it begins with no feeling for room (`FormAsked.Unfitted`), rises two pixels an update for ten through
  whatever is there, and finishes for nine updates after the one that takes the last of her ammunition: three
  pictures, then standing, set down a pixel an update while she is off the height she rose to. What a hurt
  left her is ended as it begins, as the rise ends, on every update of the taking and as it lets her go, so
  whatever touches her while her ammunition is taken hurts her every update; nothing knocks her back.
- **Leaving water in a spin** asks for her spin's sound again, right after the splash's own: the engine's player
  tells the game's as its feet have crossed a liquid's surface (`Player.OnSplashed`).
- **Her colours** (`SamusColours`) hold the colouring her suit shows and the colouring her beams show. Every colouring,
  her suits' own among them, is a change of the colours of her one set of pictures, named in `samus/colours.json`
  with every glow (`SamusGlow`: the colourings it shows in turn, each for so many updates, and the step it goes
  back to; `SamusGlowPlay` plays one whose colourings each hold for their updates); Her beams' colours are a colouring too, for the beams she has switched on, and colourings may be laid over it (a crystal flash's bubble and body): her pictures draw some of their colours from her beams', so these show on her over her suit's. `SamusVisual` shows her colouring on her picture and her echoes. Nothing tints her for a colouring her
  data has. What makes her glow keeps its own place in its glow and shows the next colouring in its turn.
- **What colours her is chosen once an update** (`Samus.StepColours`), after she has moved and just before she
  is shown, from what she is doing, and set nowhere else while she is played. Each of her glows moves on while
  what makes it glow holds, and her suit shows the first that holds of: a drained Samus's glow in the hyper
  beam's colours (`DrainedState.Glows`), the flash of whatever drains her
  (`SetDrainFlash`), a hurt's flash, the shine's glow (the shinespark's, or the stored shine's), the screw
  attack's glow, the speed booster's, a shot's glow (a charged shot's flash, a hyper shot's glow) and a full
  charge's pulse; with none, the colours of the suit she wears, which is also all she shows while a suit is
  being put on. A crystal flash's colouring is laid over her beams' colours. This is the Godot design, not the
  cartridge's order of palette writes: which update a colouring shows on is presentation and is not matched.
  The stored shine's time is the shine's own (`CountStoredShine`), counted every update with its sound. The
  count of a hurt's flash is kept with her colours: a knockback or energy found lost after she is shown starts
  it, a flying shinespark holds it, and it times her hurt's sound and the asking again of the sound the hurt cut short. A family not yet moved here is a ticket, not a tint to copy.
  Some looks carry more than the state: the spin is `spin_jumping`, `space_jumping` or `screw_attacking` by her
  equipment; aiming level, a look may have pictures for how her arm is held (`Samus.HoldName`: `moving` in a
  jump steering the way she faces, `arm_out` with the attack held in the air or once held in a run); a turn in the
  air is `air_turning_jump` or `air_turning_fall`; swinging is `grappling_<n>` for the way the line runs from her, of thirty-two; the X-ray looks end in
  `_up`, `_raised`, `_level`, `_lowered` or `_down` for how the scope is swung.
- **Where her shots leave her** is data: `muzzles` and `grapple_origins` by stance, facing and aim, raised by the
  `shot_heights` of her look (and of her look and aim where an aim has its own); while she settles from a landing
  the height is the one named `landing`, and while a jump from the ground gathers itself (from its start until
  `Samus.JumpGathered`) the one named `jump_start`, as the cartridge's landing and jump-start poses have heights of
  their own. In the morph ball what she lays is
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
- **Which enemies take a turn.** One that is always active, or frozen, does. Any other does while its middle lies
  within the view widened as the game's definition says (`EnemyTurnMarginAcross`, `EnemyTurnMarginAbove`,
  `EnemyTurnMarginBelow`), and to either side by the enemy's own half-width besides (its body collider's, which
  no pose reshapes; of one with no body, its first hurtbox's); the edges count as within.
  This game's are nothing across, eight pixels above and twenty-four below, which is the cartridge's window.
- **An enemy's turn.** The room steps each such enemy once an update. Its timers move on; then, unless it could
  not be hit as its turn began (lately hurt, the update that ends that too), its hits are settled, before it acts
  and where it stood before moving: the player's shots are tried against it, each where the shot stands, and then
  its touch against the player (not while frozen). Then it behaves, unless a status or its hurt stops it, and
  its timeline moves on, unless it is frozen: a hurt holds the behaviour and not the timeline. A hurt holds it
  from the turn the hit lands on; its flash runs down a step at the end of each
  turn, once the turn has been shown, and the hurt lets go when no more of the flash is left than the game's
  definition says (`EnemyHurtEndsAtFlashLeft`: none, so the whole flash, unless it says; this game's is eight).
  A status's time is counted after the hits, and not while a hurt holds the enemy; the turn that finds the time
  run out, or finds that the status no longer lasts (`GameDefinition.StatusLasts`: this game's frozen enemies thaw
  once the ice beam is switched off), is still held by the status, which ends with it. Being frozen ends the
  hurt's flash. The player's shots do not hit enemies as they move, an enemy out of view or asleep is not
  hit, and an enemy's body is no wall to a shot: an enemy stops a shot only by what it does when the shot hits it.
- **An enemy's part of the update's end.** An enemy with a part of its own to play once everything in the room has
  had its turn is an `IClosingStep`, as a projectile is ("When it hits", below): the room calls it after the shots
  have been tried against the player and one another and the projectiles' own closing parts, unless enemies are
  stopped, and only an enemy that takes its turn. What it launches there first takes a turn on the next update, and
  the last random number it finds is the update's last. This game's Mother Brain plays her brain there: its timeline
  and cues, her breath, and what shows it; her neck and where her head is are her turn's.
- **A shot that does it no harm** leaves what the enemy's data gives for that (`"dud"`: an effect where the shot
  is and a sound) and is spent: the enemy answers `HitResult.Dud`, and the shot is gone with nothing of its own
  left, where a hit merely absorbed ends the shot with its own ending. A hit that comes of nothing with a place
  (the player's body, a blast) leaves none. An enemy whose class leaves its own, part by part, has no `"dud"`.
- **Colour and strength variants** are one scene with different data files.
- **What the base offers a concrete enemy** beyond the above: starting a timeline and acting on its cues, with a cue
  that starts another timeline taking effect in the same update; having a timeline begin where the step showing
  ends (`PlayAfterStep`), so that the picture showing stays out its time, as this game's Yard turns round when it
  is touched; marking its place in a timeline and resuming there;
  judging a hit itself (`TakeHit` can be overridden, part by part); keeping health part by part (`HealthOf` says
  which health a hit on a part is taken from); firing its data's shots; leaving effects and
  drops; moving by the shared `Mover` or carrying the player with it; flashing for a set time, and, for one that
  shows its hurt its own way, how much of the flash is left (`HurtFlashLeft`, as this game's Mother Brain times her
  brain's white and her shake by it); and asking whether the player rests against its solid.
- **An enemy's own visual** is its `ActorVisual` child named "Visual", wherever it stands among its visuals, so a
  part drawn behind the body can come before it in the scene; with none of that name, its first.
- **A boss is an `Enemy`.** One scene. Its parts are child nodes with their own hurtboxes, and the boss decides what
  a hit on each part does. There is no boss base class.
- A part that comes off and then only falls or fades is an `Effect`. One that can still hit the player is a
  `Projectile`. One that can still be killed is another `Enemy`.

**Solid actors.** Any actor may have a solid collider, switched on or off. A solid blocks the player and can be
stood on, and a solid that moves carries what stands on it; that is the `Mover`'s job, the same for a platform, a
frozen enemy and a boss's back. Frozen is a status: the enemy stops, its solid switches on, and a countdown ends
it. An enemy whose scene gives it no solid is given one by the engine for that status alone, the shape of its own
size (its body, or its hurtbox where it never meets terrain and has no body), made as it freezes and taken away
as it thaws, so that no class going through its own colliders ever meets it: the cartridge's frozen enemy is solid whatever else it is, and no scene
has to say so. There is no special case for standing on enemies. A solid actor is solid to the player: an
enemy's or a creature's own move is stopped by terrain and by the solids that stand as part of the room (a door's
cap, a gate, an item's shell) and by no other (`MoveOptions.AmongBlocks`), as the cartridge's enemies meet blocks only.

**Shared movement.** Ways of moving that more than one enemy uses are components in `engine/actors/movement/`, each
moving its owner by the shared `Mover` and reporting what happened (hit a wall, reached an edge, landed). The enemy
decides what to do about it.

| Component | Moves its owner by |
| --- | --- |
| Surface crawl | Following the surface it is on, around corners, in a set direction. It can be started pressing any way across and down; one started pressing away from its surface turns about until it finds something to cling to. It can feel for its surface some way ahead of itself, so that it meets a corner early; its owner can ask which way it presses. |
| Hop | Jumping in arcs, with settings for height, distance and the wait between. |
| Dive | Waiting, then dropping or swooping at a target and returning or not. |
| Patrol | Walking or flying back and forth, turning at walls or edges. |
| Orbit | Circling a point or another thing at a set radius and speed. |
| Bounce | Travelling straight and reflecting off what it meets. |

Taking hold of the player (a Metroid, a Beetom, a Yapping Maw) is not a movement component. The enemy asks the
player to enter the held state, moves with them, and, where it is one that hurts by its hold, adds ongoing damage
while it holds (a Yapping Maw only holds: its touch on the cartridge takes hold of her and does nothing else). The
held state keeps what the player was doing as they were taken hold of (`HeldState.Was`), and this game goes on
showing Samus so while she is held.

This set was checked against the old code by counting which enemies do each: every one of the nine is used by
several enemies. The old code shares almost none of it, so each must be worked out from the enemies that use it,
not lifted from a shared routine.

### Projectile (base)

- **Owns:** its position and velocity, its side (the player's or the enemies'), the `Hit` it delivers, its
  lifetime, its collider and its visual.
- **Manages:** moving, hitting what it meets, and removing itself. It can be shown a change of colours
  (`IRecoloured`), which its visual shows: this game shows Samus's colours on a shot as she fires it, and on the
  effect and the burst the shot leaves as it ends (an `Effect` can be shown a change of colours as well), so that
  the opening's flashbacks show her missiles and their blasts in the opening's own colours.
- **Does not:** decide what its target does about the hit, or know its target's type.
- **When it hits.** The player's shot delivers its hit as it moves, to whatever it comes upon but an enemy or an
  enemy's shot: each enemy tries the player's shots against itself in its own turn ("An enemy's turn", above), and
  the room tries the player's shots against the enemies' shots, each of the enemies' in turn against each of the
  player's where they stand, at the end of its update, after the enemies' shots have been tried against the player.
  The two meet when they stand in the same square of the room, squares of `GameDefinition.ShotMeetingSquare` pixels
  counted from its top left corner, whatever their sizes: a shot just over a square's edge passes the other by.
  A projectile with a part of its own to play there, after both, is an `IClosingStep` (as one with a part in the
  update's opening is an `IOpeningStep`): this game's power bomb grows its blast as the update opens, strikes the
  terrain along the blast's border in its own step among the player's shots, and lands its hit on what the blast
  reaches as the update ends. An enemy's shot does not hit as it moves either:
  it moves, and the room tries each enemy's shot once, where it then stands, at the end of the room's update, after
  the room's objects have played (and so after a message that suspended the update). The room first asks the player
  once whether any is tried at all (`Player.BeginEnemyShots`: none once the player is dead or dying, and a game may
  say more), and tells them when all have been (`EndEnemyShots`); whether a hit is shrugged off for a hurt a moment
  ago is the player's to say (`ShrugsOff`: while invincible, unless the game says otherwise). Samus says no shot is
  tried while she is invincible as the trying begins or while she is speed boosting, shinesparking or screw
  attacking, and that once it has begun every shot that overlaps her lands, as the cartridge reads her
  invincibility once for them all.

How a projectile behaves is settings, not code:

| Setting | Choices |
| --- | --- |
| Meeting terrain | Ends; ends and delivers its hit to the tile; passes through; passes through and still delivers its hit to each tile it crosses; bounces. |
| What of it meets terrain | Its whole body at every pixel of its way, stopped by anything solid in it and by the room's edge; its middle point alone, once for each way it moves, where the move ended, so that a fast one passes what it steps over and one that is stopped ends inside what it met, as this game's missiles do; or the edge it leads with, once for each way it moves, where the move ended (`Terrain.MeetShotEdge`, answering with a `ShotEdgeMeeting`): it is stopped only when every tile across that edge stops it, its hit lands on every tile the edge reached, and it is tried once where it was launched before it first moves. By its middle or its edge nothing outside the room stops it, and a way out of the room does (`Room.HasExitOver`). Samus's missiles meet terrain by their middle and her beams by their edge; a beam fired down beside a wall flies past it. Or the side it leads with, as the cartridge's enemy shots do: tried where each move ended, stopped by any tile in it that stops a body (`Terrain.StopsEdge`), and resting against that tile on whole pixels; the pieces of Ceres's ceiling fall so, out of the ceiling they start in. |
| Meeting a target | Ends; passes through and can hit others; passes through and can hit the same one again once that one can be hit again. A target that takes no damage from it, or turns it back, stops it either way. |
| Met by the other side's shot | Destroyed; stops the shot and is unharmed; ignored. |
| Motion | Straight; accelerating; in a wave; homing; does not travel (a bomb); with gravity. |
| Lifetime | Until it leaves the view by a margin; a set number of updates; until its animation ends. |
| Ending | Nothing; an `Effect`; another `Projectile` (an explosion with its own hit). Either is left at its middle, or at its front: half its size on along each way it was launched, as a beam's burst is. |
| Size | Its settings' own; a step of its timeline may give it another with the cue `size:<width>:<height>`, so a shot can swell or spread as it flies. |
| Where it stands | In front of the player with whatever else is launched, or a `Standing` of its own against the player: Samus's beams and missiles stand just behind her, as the cartridge draws them. |
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
  Being left is the first update of its first picture, and it shows the step each update belongs to
  (`TimelinePlayer.Shown`), so a later picture and the cues that come with it come together; one that does not
  loop is gone on the update after its last. The room gives the effects their turns as the enemies' pass ends, and
  not while the enemies are held still; one left later in an update first takes a turn on the next. An effect that
  holds a place takes its turn as the update ends.
- `Remains` are what a destroyed enemy leaves where it died, when its data names a death to play: the timeline of
  that death, which may differ by the kind of hit that destroyed it. The enemy itself is gone at once. The remains
  play the death, asking for the sounds its cues name and scattering the effects they name, placed by `GameRandom`;
  on the death's "drop" cue, or as it ends, they roll the enemy's `DropTable` and leave the pickup; and they hand
  the enemy that takes its place, if one does, to the room, which adds it once the pickup is gone. So a drop appears after the explosion,
  not under it. An enemy with no death to play leaves its drop at once.

**A projectile may become a shot of another type** where it is and going as it goes (`Projectile.BecomeShotOf`):
its hit's damage and kinds, its size and its timeline or picture become that type's. The spazer attack's leading
lights become the trailing pair's shot so as they start to fall.

**A projectile shot down** is gone at once, leaving what it drops, unless its data gives it an end to play
(`"shot_timeline"`). Then it stays what it is and where it is while that plays: it holds still, stops shots no
longer, harms what it touches whatever its data said of it and ends on it; and as the timeline ends what it drops
is rolled and left in the place it held. Phantoon's flames and the Torizos' orbs end so.

**Places for what enemies launch and leave.** A room may keep a count of places (`GameDefinition.EnemyPlaces`;
none, unless the game says) for the enemies' shots, the remains of the destroyed and the pickups dropped. Each
holds one place while it is in play; every projectile of the enemies' holds one, and an effect holds one where whoever leaves it says so (`Effect.Of`, as the dust and
sparkles the cartridge counts among its enemies' projectiles do), playing with the room's other effects all the same. A thing takes the highest free place, and with none free it is not made (`Room.Spawn`
says so): a shot is not fired, a death leaves no remains, and a drop made outright is neither made nor rolled.
They take their turns from the highest place down, each place asked as its turn comes, so what takes a lower
place during the turns has its turn among them and what takes a higher one waits an update; the enemies' shots
are tried against the player, and the player's shots against them, in the same order. A place is free the moment
what held it is taken out of play. The pickup remains leave takes the place they held (`Room.SpawnInPlaceOf`)
and waits an update for its first turn; the game may say that remains in some place leave no drop though they
roll for it (`GameDefinition.RemainsLeaveDrop`). A spent drop goes on holding its place: when a pickup is taken or
runs out, when remains leave nothing, and when a drop made outright rolls nothing, something that shows nothing
holds the place for `GameDefinition.SpentDropUpdates` more updates (`Room.HoldPlaceOf`, `Effect.HoldingPlace`). A part of an enemy or a fitting of the room that the game counts among those places holds one the same way, by something that shows nothing until its owner takes it out (`Effect.HoldingPlace(at)`), and goes without where none is free: this game's Yapping Maw holds four for the links of its neck. And
what takes a destroyed enemy's place arrives as that pause ends. This game keeps eighteen places, and its remains in the first
leave none, as the cartridge's death explosion in its first place never becomes a pickup.

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
| `Enemy`'s turn while hurt | A hurt holds an enemy's behaviour off while it flashes, where its data says it is stunned so; its timeline goes on, with its cues, as the cartridge's animation does through a hurt, and stops only while the enemy is frozen. What else of it goes on meanwhile it does in `WhileHurt`, called in the behaviour's place on those turns: parts of it that are things of their own on the cartridge, or what its hurt itself does (Draygon takes the current of Samus's grapple there). An enemy may make itself hurt with no hit (`BeHurtFor`), and may end its hurt and its flash at once (`EndHurt`), as one whose death begins on the hit that killed it does. A hurt that has no flash ends with the turn it began on. The flash is shown as the turn ends, while it still runs once the turn has counted it down, on the passes the game's definition gives it (`EnemyFlashPassesEachWay`: for this game two passes of every four, by the count of the enemies' passes once the pass is counted), and a frozen enemy's look shows steadily until few updates of the freeze are left and then blinks (`FrozenLookSteadyUpdates`, `FrozenLookBlinkUpdatesEachWay`); an enemy that shows its hurt its own way says so (`ShowsHurtFlash`, as Kraid does). An enemy's own count of its turns (`Updates`) reads, while a turn is played, the turns it had before that one, so its first turn reads none: the count moves on once the turn's behaviour has been played, a hurt's or a frozen turn too, and before its timeline moves on. What an enemy times by its own count (a cadence, a choice a number drawn is added to) reads it so. |
| `StateMachine`, `State` (base) | One current state; each state holds its own data and names what it changes to. An owner with a say in what it enters is asked what it makes of each state before the current one is left: it gives the state asked for, another in its place, or the current one when it stays as it is. |
| `TimelinePlayer` | Stepping a `FrameTimeline` once per update when its owner says, and reporting the step and any cue reached. The owner acts on the cue. It gives two steps: the one it is on (`Current`), which the next update falls in and which is already the following step as a step's last update is played; and the one the update last played belongs to (`Shown`), which is what is shown, so that a step's picture comes on the update its cues do. An owner whose start itself counts as the first update of its first step plays one update as it starts. Another timeline may be made to follow the step it is on (`FollowWith`): the step keeps its updates, and the other begins from its first step where this one's next would have. |
| `Enemy`'s shake | An enemy may be shown shaken for a number of updates (`Enemy.ShakeFor`): a pixel to the right or left by turns, two of its turns each way by its own count. Only what is seen moves. This game shakes an enemy Samus launches a wall jump from. |
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

**A sloped tile cut in steps** (`SlopeShape.Step`, a layout's "step" on the tile; this game's half and quarter
blocks are cut in steps of eight) stops a move tried only where it ends at the start of the step it would have
ended in, whatever lies in the step before, so a move long enough to end in the far half of such a tile rests
there, as on the cartridge; and a move down that it stops leaves the body held as sloped ground holds it, so its
owner presses down to it by a pixel. A shape with no step is met pixel by pixel.

**How much of its way a move tries** is an option on the call (`MoveOptions.Tries`, a `WayTried`). The plain move
tries every pixel of the way: it stops flush against the first thing it comes to, at the start of the pixel it stops
in, and never carries part of a pixel into what stopped it. A move tried only where it ends (`WhereItEnds`) is the
other way:

- Solids are tried first. The first registered that the body would overlap where the move is tried, and does not
  overlap already, stops it: the body goes the whole pixels between them and keeps its part of a pixel, losing its
  part of a pixel up and down when it lies exactly against the solid. The move is tried one pixel beyond the one it
  reaches, unless it ends exactly on a pixel. Terrain is then not tried. A move up or down asks after solids even
  when it goes nowhere.
- Otherwise only the terrain the body's leading edge would end in stops the move (`ITerrainShape.StopsEdge`): what
  lies on the way, short of the end, is passed, and a move that ends beside something, part of a pixel short of it,
  is not stopped. The tiles the edge would lie in are tried in turn from its top or left end, or for a move up or
  down from its right end where the call says so (`MoveOptions.TriesFromRight`: this game says so on the clock's
  odd updates, as the cartridge's scans alternate), and the first that stops the move decides and is reported
  (`MoveResult.StoppedByTile`), so a player stopped by one tile of the ground stands on that tile alone. Stopped,
  the body rests against that tile, never further back than where it began: at the far end of its pixel when it
  was going right or down, at the pixel's start going left or up. A move so stopped went whole pixels.
- A solid that stands as part of the room, as a wall does (`Collider.StandsAsTerrain`), is met as terrain is and
  not as a thing in the way: by the leading edge alone, the body resting against it at the end of its pixel. This
  game's door caps, gates, gate switches, item shells and stations stand so, having been blocks of the cartridge's
  rooms; what was an enemy there, frozen or solid, does not.
- Sloped ground that holds things up by their middle stops no move across. A move across may be tried as going
  further than it goes (`triedAs`), as one the ground lets only a share of through is. Once it has moved, or tried
  to, the body is put where such a slope's surface is under its middle, however far (`MoveResult.PutOnSlope`): up
  out of a floor its feet have sunk into, then down from a ceiling its head is in or against. Down, such a floor
  stops only the body's middle column, and stops a move that would end resting exactly on it as well; the body then
  goes the whole pixels of its move, less what the slope takes back, and keeps its part of a pixel. Up, such a
  ceiling stops the middle column and the body does not move at all. A floor of the kind stops no rise and a
  ceiling no fall.

The game says which way its player's body is moved (`GameDefinition.BodyMoves`). The player's own code then reads only what a move reports: a player moved the second way has landed
when ground stopped a fall, not when they come to lie on it, stands on the terrain that stopped a move down of
their own, and, once a slope has taken hold of them, is pressed to the ground one pixel only until they next move
up or down.

**Why, and extending it.** Collision finds; owners decide. That is why there is one mover and one overlap query for
everything, with differences as options on the call. A new need (a new way of treating slopes, a new edge rule) is
a new option whose default is the plain behaviour. If the option exists only for one of Super Metroid's oddities,
the non-default choice is supplied from `game/compatibility/`. The architecture guide has the rest.

## 8. Presentation, audio, input and content

| Type | Lives in | Job | Does not |
| --- | --- | --- | --- |
| `MessageBox` | `engine/ui/` | Showing a message, waiting for it to be dismissed, and returning the answer when it asks yes or no. Each is shown as its picture (`GameDefinition.MessageBoxFile`), drawn for the room's set of graphics, across the screen with its middle on the style's line, as much of it as the window about that line shows; a question has a picture of each answer chosen, and a message that names the button of an action has one for each button the style lists and one for none of them, the first listed button the player has bound to the action being the one shown. | Grant the item it announces or perform the save it asks about.  |
| `MapView` | `engine/ui/` | Drawing an `Area`'s map from what `Progress` says is explored and revealed. The minimap, the pause map and the file-select map are all this, at different sizes. An unexplored cell and the cell the player is in (`Here`) are drawn from the area's own sheets for them where it has them. A view may draw the cells alone or the markers and labels alone, as where the cells show through an opening and what lies over them does not. | Decide what is explored.  |
| `LitColours` | `engine/ui/` | The colours of some pictures that stay lit while everything else is dark: a list of colours, by region where a colour is lit in one region of a picture and not another, and the way of drawing that shows those colours alone of whatever is drawn with it. What stays lit under a cover of the screen is drawn again over the cover with it: a room's tiles through a door's passage (`LitTiles`, in `engine/world/`, which draws a terrain's tiles a second time wherever it is put; the room left gives its terrain up for it, `Room.ReleaseTerrain`). | Fade anything, or know why a colour is lit. |
| `ColourSteps` | `engine/ui/` | Steps of colours played over time on whatever they are shown on: each step shows some colours of the pictures as others for a number of updates, with a step to go back to after the last, and a sound a step may name, handed to its owner as the step begins. Its first update shows its first step. A room's `ColourEffect` plays one on the room's pictures; a screen that is no room makes its own and gives it its turn each update, as the flight to Ceres does for the gunship engine's flicker and the station's navigation lights. | Know what it is shown on beyond being told, or ask for a sound itself. |
| `ScreenFade` | `engine/ui/` | Fading, flashing or whiting out the whole screen over a set number of updates, smoothly or in a set number of even steps, and reporting when it is done; or showing the screen covered so many parts of so many, for whoever paces a fade of its own. | Know why it was asked. |
| `ScreenDarkness` | `engine/ui/` | How dark the whole screen is beneath whatever shows play, in the game's number of levels, with the pace of the fade stepping it. The session keeps one (`GameSession.Darkness`), so it outlasts whichever mode set it: a mode that darkens or brightens the screen steps it and leaves it where it stands, and whoever fades it next goes on from there (a pause's darkening cut short by a refill, a door or the time up leaves the screen that dark; the time up goes black from there; a door forces it fully lit; a game started afresh starts lit). It is drawn over everything, only while play is on screen or kept beneath the mode in control. | Fade anything but the screen beneath play: a mode that has let play go shows its own screen with a `ScreenFade`. |
| `PixelText` | `engine/ui/` | Drawing words in a `PixelFont`, a character to a cell and line under line; showing only the first so many characters; laying each line at the left, middle or right of its width. Every word the game shows is drawn by one. | Hold the words, or draw in anything but its font: with none, words given it cannot be drawn. |
| `TypedText` | `engine/ui/` | A `PixelText` showing its words one character at a time with its sound, for every letter or every so many; going on to the next line takes no time, and a set number of characters at the start may each be followed by the next at once. It says when all of it shows, and when the wait after its last character has passed as well. | Hold the words; they come from `GameText`. |
| `Cinematic` | base in `engine/ui/`, each one in `game/cinematics/` | A scripted sequence with no gameplay. Each is a `GameMode`. One that waits on the player reads what they are doing this update; one that shows remembered play steps a room of its own from recorded input. | Run gameplay rules of its own. |
| `CinematicSprite` | `engine/ui/` | One thing shown in a cinematic: a picture at a place on the screen that plays a timeline, drifts at a speed (round a loop, if it has one), and is over when its timeline has played out. Its cinematic steps it and acts on the cues it reports. | Decide what its cues mean. |
| `MosaicPicture` | `engine/ui/` | A picture that can be shown in coarse blocks, each taking the colour of its top-left pixel. | Anything else. |
| `GameHud` (base) | `engine/ui/` | Being what is shown over play: kept on screen by `PlayingMode`, presented once as play begins and told to show things as they are as each room is set up and after each update. The `GameDefinition` names a game's scene of one. | Know what a particular game shows. |
| `Hud` | `game/hud/` | Showing energy, ammunition, the selected weapon, the minimap and, during an escape, the countdown; its places and pictures come from its data file. Under the minimap lies the picture of a cell that shows nothing; the countdown is its label and three pairs of digits about one place. | Change any of them.  |
| `PauseScreen` | `game/pause/` | The whole of a pause: the screen darkening over play that goes on (it steps the playing mode itself, without the pause action, and gives way to whatever play hands over to), every sound stopping, the pause screen brightening, in use and darkening, and the screen brightening over play again; a level of darkness at a time, with one black update each way. The HUD stays over it. The map page shows the area's map through the opening of the page's frame, with the arrows of the ways it can be moved and the sign of where Samus is; the equipment page switches equipment through the player's `Inventory` (the selector moves first, so a press on the update it moves switches what it moved onto), and her reserves, once started pouring, pour until she is full or they are empty and are left empty. Turning the page (angle up from the map, angle down back) and leaving (pause) answer to an action only once what is held has stayed the same a while. Its backdrop, buttons, fades, the pictures of each thing switched on and off, the selector's looks and places, her suit's outline, her reserves and its sounds all come from `pause/pause.json`. | Hold inventory state. |
| `ChoiceList` | `engine/ui/` | A list of lines with one marked: up and down move the mark, and confirm and cancel are reported to whoever steps it. The lines not chosen may be tinted (`UnchosenTint`), for a font with no mark to write before the chosen one. | Hold words or act on a choice.  |
| Menus | `game/menus/` | Title (`TitleMode`), file select (`FileSelectMode`), options (`OptionsMode`), game over (`GameOverMode`), and how play is come into: the load (`GameLoad`) and the arrival (`Arrival`). A death the screen is already black for, as Ceres blowing up with Samus aboard and time running out on Zebes end, ends on one update of its own (`DeathBlackOut`) on which the update's random number is drawn and nothing else is done, and the game over screen opens on the next. `GameLoad` is the one update a game is loaded on, in the engine's `LoadingMode`'s place: the update's random number is drawn, then the game is set up (the save slot's, a new game, or play going on at the landing after Ceres) and play is made with its room, laid under an `Arrival`; a new game's arrival saves the game to the session's slot as it is made, its room and player set up, to resume in the game's first room at its first entry point. A saved game taken up from its map is one Samus appears in: the load says so to the sound flow (`CartridgeSounds.SavedGameAppears`), and the room's entering then asks for its set, her fanfare after fourteen turns of the music's queue and the room's music or silence after 365 more, in place of the room's music. The `Arrival` holds the screen black for the load's waits (67 updates; 76 for the landing), on which no number is drawn and neither the music nor the sounds get a turn, then plays play beneath itself for thirty updates while the screen comes up a level every other update, on terms that let nothing follow (no death, no refill, no pause, no take-over), and hands back to play. Each is a `GameMode`; their words come from `GameText`. The game starts on the title, and the game over screen is what follows a death. The title reads its opening (cards of typed words, moves across its backdrop, the backdrop drawing back, the title and the copyright fading in), its backdrop, the larva on it, its pictures at rest and its sets of demos from `menus/title.json`; a press cuts the opening short, and a demo comes back to the title at rest; `DemoSchedule`, a session service, keeps which set is due. A demo (`AttractDemoMode`) starts Samus at a place in a room with what its file gives her, and as its file says: standing, facing forward as her ship leaves her, rolling, falling, or already flying a shinespark (`ShinesparkState.Launched`); the place is her middle in that body. It also says where the view is as it begins (`RoomEntry.ViewAt`), which the room then puts there in place of about her. The game over screen reads its page, the crying larva and the cursor from `menus/game_over.json`. File select reads its pages, what it lays over them for each saved game, its helmets, cursor and copy arrow from `menus/file_select.json`; a saved game chosen there goes on to the options. The options read their pages, what is laid over them for the language and each toggle, their headings and cursor from `menus/options.json`; their first row starts the game, and their page of buttons lists each action with its button in the game's font. A saved game shows where it was saved before it starts (`SaveMapMode`, from `menus/save_map.json`, when the game was saved at a place that file marks): black for a moment, then the planet opening from the middle of the screen with every area the game has been saved in named and the one it was saved in lit; confirm opens a window from that area's name onto the area's own map as that screen colours it (`menus/save_map/areas/`), with the place saved at pulsing, the stations found, the ship and the bosses; the directions move it, confirm there fades out and starts the game, cancel closes the window back to the planet, and cancel on the planet goes back to the options. `ScrollingMap` (`game/pause/`) is an area's map laid behind a frame and moved, which that screen and the pause screen's map page both use. | Touch a running session.  |
| `AudioDirector` | `engine/audio/` | Playing the music a room or event asks for and the sounds the session's `ISoundFlow` has started, each on its channel's player. | Affect gameplay by its timing. |
| `InputSampler` | `engine/input/` | Producing one `GameInput` of actions per update from the player's devices and bindings, or from a recorded demo. | Let anything else read devices. |
| `ContentLibrary` | `engine/content/` | Loading art, audio, layouts and data files, by their path in the assets tree. | Read the ROM or reference the extractor. |
| `AssetsMaker` (base) | `engine/content/` | How a game's assets are made from a file the player supplies: what the player is asked for, its version (assets made under another are made again), and the making itself, into the installed folder, all of them or none, telling how far it has come (`MakingProgress`: the step, how much is done, and a count that moves with the work). A `GameDefinition` gives one, or none. | Show anything, or choose the file. |
| `AssetsSetup` | `engine/core/` | The start of an exported game whose assets are missing or another version's, when it has a maker for them: asking for the file with the system's own file chooser the first time, keeping it beside the assets' folder, running the maker off the main thread, showing its progress as a bar, the step and its count, noting the version made, and then letting the `GameLoop` begin the game. A failure is logged in full and said on the screen, and the player may choose again. | Know what the file is or how assets are made from it. |
| `RomAssets` | `game/` | This game's `AssetsMaker`, in its Android build alone: it hands the ROM the player chose to the asset extractor, which the Android game carries, takes its version from the extractor's own, and turns the extractor's counts into the bar's length. It is the one place the game touches the extractor. | Exist in any other build, or use anything else of the extractor or the old code. |
| `Recolouring`, `IRecoloured` | `engine/ui/` | Showing something drawn with some of its colours changed. A thing that can be recoloured (`ActorVisual`, `Terrain`, `Backdrop`) is told by whoever changes colours which colours to show as which (`ColourSwap`; the colour shown may be see-through, wholly or in part, which is how a thing fades in a colour at a time without changing where it stands: this game's Ridley shows himself so), under that one's own name (an `Enemy` shows its hurt flash so, in the colours its data gives for it under `hurt_colours`, and brighter only where its data gives none), so several may change different colours of the same pictures at once and each takes back only its own; where two change the same colour, the one that asked last is shown, as the last to write a colour of a palette is. A change of colours is read from data by one reader (`ColourChange`: colours listed place by place, each thing shown those of the places it shows pictures from; `ColourChange.ReadSwaps` reads one place's). Something that changes colours in its own turn, or as the room is set up, shows the change itself, at once: on its own visuals (`ColourChange.ShowOn`, `Recolour`) and on the room's terrain and backdrops (`Room.Recolour`), so it is seen on the update it is made and through a door's passage; the room's terrain and backdrops are set up before anything enters the room, so what enters may change their colours as it does (Ridley's arena is hidden in black so, and comes out of the dark by his own count). The change is made by a shader where the pictures are drawn, which swaps exact colours: no picture is kept twice for it. A swap may hold in one region of a picture only, which the shader reads from the picture's map of regions; and each thing says which places it shows pictures from, so a change written for one place's pictures is shown only there. | Decide which colours change, or when. |

**Assets.** Everything of this game that can live in an external file does. They come from the player's ROM, are
never committed and never shipped, and producing them is the asset extractor's job and no part of the Godot
project. (The Android game carries the extractor and calls it once, to make them on the device: `RomAssets`, and
the manifesto's section on what the owner has settled.)

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
- **Colours that change are changed by a shader.** A glow, a flicker, a flash or a fade that changes some colours
  of a picture and leaves its shapes is a list of colours each shown as another (`ColourSwap`), handed to what
  draws the picture (`IRecoloured`); its data names each colour as the picture has it and as it is shown, place
  by place, since two sets of pictures may have one colour and change it differently. Where one colour of a
  picture has to become two (the cartridge drew the two parts from different rows of its palette), the picture
  has a map of regions beside it (`name.regions.png`: greys, a region as far from white as its number), which
  `ContentLibrary.LoadTexture` draws the picture with and the shader reads, and the swap names the region. No
  picture is kept again in other colours for a change of colours.
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
  share this game's own flight (`GradualFlight`, in `game/compatibility/`), whose speed changes by the cartridge's
  seeking steps one at a time (`SeekingSteps`), as Ridley's does.
- The husks of Tourian are enemies, not scenery, because shots and bombs set them crumbling: `Corpse` (the Zoomers,
  Rippers and Skree, one scene with a data file each), `DeadSidehopper`, and `DeadTorizo` with its heap of sand.
- Ki-Hunters: green, yellow, red. Wings are part of the Ki-Hunter; shed wings are an `Effect`.
- Space Pirates: wall, walking, ninja. Three scenes, their colours data files. What Samus and her shots meet of
  one is the boxes of the picture it shows, as on the cartridge: the three share a class that takes a
  `ColliderPoses` pose with every step of their timelines, which places two or three hurtboxes.
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
  hurt it is, the flash of a hit in the mouth and its dying are changes of the colours of its one set of pictures,
  which its spikes and nails show with its arm and foot, and its eyes' own colours are shown over them; what it does
  to its room, the ceiling it breaks, the platforms and spikes that crumble, are `PatchSequence`s; the three spikes of its belly and
  its two nails are enemies of their own the room places, there until it is beaten; its rocks are `Projectile`s; the
  screens it keeps the view from until it has grown are `CameraZone`s it opens); Crocomire
  (its tongue is a visual; the pieces of it a shot or Samus meets are ten hurtboxes its `ColliderPoses` place for
  each picture, each pose saying what every piece does and how far its mouth is open; what it does in the acid is a
  script in its data; the bridge's blocks, the walls that hold Samus in while it dies and the opened spike wall are
  patches; the pieces of the spike wall are a `Projectile` of their own class, the bridge's pieces and its fire
  plain ones; the screens of its room its fight and death shut and open it writes to the room's scroll cells
  itself; a room where it has been beaten places the same scene as its skeleton); Phantoon (eye, tentacles, mouth are parts, and so is the one pixel at its middle, a hurtbox that alone takes and spends shots on the update between its eye beginning to track and opening to harm; while its fight lasts the door of its room is wall, a patch of the room's terrain, and the door's cap is out of sight); Botwoon
  (its head is the enemy, sized by a cue for the way it points, and the thirteen segments of its body are visuals
  with hurtboxes that stop shots and are not hurt; how hurt it is is a set of its pictures; its wall falls as a
  `PatchSequence` the room goes on playing after it is gone, and the `CameraZone` its wall kept shut is opened as
  the wall begins to fall); Draygon (eye, tail, arms are parts; its body's pictures are kept once for each band of its health; its tail and arms
  are recoloured, in its opening's colours until its first hurt and in its fight's from then on; its data, not
  its cannons, says which cannon fires on which of its turns and from where, and each cannon in the room says which
  it is);
  Ridley and Ceres Ridley (one scene, two setups: one enemy written across three files, what both fights share and
  a file for each fight, chosen by its `OnCeres` setting; breakup pieces are `Effect`s; it has one set of pictures,
  and the colours it is in, the dark it waits in, each step of its eyes' and body's showing and each look of its
  health, are changes of their colours shown on all its visuals at once, each written with its arrival's colours
  and the rest laid together; its data says which parts of it can be touched and shot in each pose); Mother Brain (her brain, the joints of her neck
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
- `RoomExit`: every way out, each a `Door` of this game's.
- `Elevator` (this game's, `game/objects/elevator/`): every elevator.
- The two Chozo statues that come to life are one enemy
  scene (`ChozoStatue`) with two setups: the one walks through terrain by its timeline's strides with
  Samus in its hand, and the other lowers the room's liquid.
- The game has no platform that is not an enemy, and the engine has no such kind. The Tripper is an enemy (it can
  be frozen and shot), and the suspensor platform is the
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
- `Hazard`: Ceres steam (its reach follows its plume by its timeline's cues, and each vent waits a different,
  chance length before it first blows). The plants that seize Samus are not hazards: they are blocks of the
  room that a sequence of its own plays (`SamusEaters`).
- `Creature`: the Etecoons and the Dachora, during play (`Etecoon`, `Dachora`) and during the escape
  (`EscapeEtecoon`, `EscapeDachora`), in `game/objects/etecoon/` and `game/objects/dachora/`. The Dachora's
  glows, of speed and of the shine, are frames of changes of its pictures' colours, which its data lists
  (`glow_colours`).
- The Tourian entrance statue is a plain node holding the pictures its hall's sequence
  shows and moves. Bomb Torizo's hand (`BombTorizoHand`) and the morph ball eye
  (`MorphBallEye`, in `game/objects/morph_ball_eye/`) are room objects of their own, each with a state
  machine of its own; the face blocks and the corpses are enemies.

### Terrain

- `BlockType`s: shot, bomb, power bomb, super missile and speed blocks, regrowing or not; crumble blocks, regrowing
  or not, in each size; blocks hidden until shot, bombed or seen by X-ray; spikes; conveyors and treadmills; sand;
  grapple blocks, lasting and breaking.
- Patches: everything the old code's scripts wrote into the tile grid during play (a collapsing bridge, a boss's
  room changing, a wall opening).

### Room conditions

- `Liquid`: water, lava, acid, including the rising acid of the escape.
- `RoomEffect`: heat, rain, fog, spores and Ceres haze are the engine's `OverlayEffect`, each with a look (heat with
  ongoing damage as well); the earthquake is the engine's. An overlay is laid over the view by its room whenever
  the room shows itself (`OverlayEffect.Follow`), so it is there from the moment the room is seen, before the room's
  effects have had a turn; its pictures and its drift move on as it is stepped. The
  Fireflea rooms' dark is this game's own (`FirefleaDarkness`, in `game/effects/`), told to darken by each Fireflea
  that dies; it looks at the deaths as it is stepped, so a death shows from the update after.
- `Backdrop`: room backgrounds, the scrolling sky (a backdrop is drawn behind everything else in its room); the Ceres rotating floor and the tilting picture of its
  elevator shaft are `TurningFloor`s (a turning floor lies where its `LiesAt` says, level or turned: the shaft's lies
  a row above the room's own rows, where the cartridge shows a room's picture against what stands in it). What a level draws behind itself is one picture, with the part of it the
  cartridge draws in front of the room's things beside it: Ridley's room on Ceres is all that part, over tiles
  that are black all over. The floor he flies off over is put away until he flies, and while he does `CeresGetaway`
  gives it the depth in front of the room's tiles (`RoomDepth.BehindThings`), where the cartridge shows it in
  their place.
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

Ceres: the colours its doors take once Ridley has gone (`CeresDoorColours`: given to a room's doors as the room is
set up, as a change of colours written by the extractor, so a door there while he left keeps its own until the room
is entered again); the elevator arrival (`CeresArrival`: the pad that sets Samus down); Ridley's getaway
(`CeresGetaway`: he flies off over the turning floor and throws Samus aside, then the self-destruct's warning comes
up, is typed out and stays up flashing, and the countdown begins); the station shaking once the self-destruct has
begun (`CeresShaking`); the falling debris (`CeresDebris`); the elevator shaft tilting and the elevator taking Samus
(`CeresShaft`, which tells the `Escape` service she has left). Crateria: the gunship landing. Brinstar and
Norfair: the Dachora shaft camera (`DachoraShaftCamera`: once the shaft's top is open to the view, the zone beside
it is closed), Crocomire's room shaking (`CrocomireRoomShaking`: the terrain is shown shifted by as much as he
rattles his wall). Maridia: the transport tube ride (`TubeRide`: Samus held as she came in and moved by the room at a speed that gains). Tourian: the entrance
statue's hall (`TourianStatueHall`: the guardians' eyes, each one's release once its boss is beaten, and the
statue's descent, which is its part of an update's opening (`IOpeningStep`), played before the update's random
number is drawn; the statue is the room's backdrop, which the hall sinks, with a picture over it for each
guardian's eyes and for each turned grey (those of the guardians that are part of the backdrop's picture lie behind
the room's tiles with it, at `RoomDepth.BehindRoom`), and its floor crumbles away as a change to the terrain its data gives), and the rising acid, which is a `Trigger` telling the room's `Liquid` to rise. A watch on a
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
- `GameMode`s: section 4. `Cinematic`s: the intro, Ceres destruction, the ending and the credits.
  The departure from Ceres (`CeresDeparture`) is a mode laid over play, which goes on beneath it: Samus stands
  held on the elevator for sixty updates, the screen darkens over the tilting shaft a level an update, and on the
  update it reaches black her leaving is saved, the countdown stopped and the destruction (`CeresDestruction`)
  begun, which is also where time running out on Ceres leads, with Samus still aboard. Time running out
  (`TimeUpMode`) is laid over play too: play goes on for the eleven updates of its white-out, which is the
  countdown's own (`Escape.StepWhiteOut`) and is shown by the HUD, so that a door or a refill that takes play from
  beneath it leaves the screen as white as it was and the time up that comes back, as on Zebes it does on the
  next update of play, goes on from there; then nothing plays while the white screen goes black a level an update,
  and on the update it is black the countdown stops and what follows begins. The intro
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
item containers as types of their own; room "states" as separate rooms; debugger save states
and input recording tools.

Doors and elevators were first built from the general kinds alone. Full parity with the cartridge needs the passage
through a door and an elevator's ride update for update, which those kinds cannot say, so each is now a kind of this
game's own (`Door` with its `DoorTransitionMode`, and `Elevator` with its `ElevatorJourney`). The engine's `Carrier`, which
the elevators and the tube were first built from, was removed with its player state once nothing used it: the engine
keeps no kind that nothing uses. For the same reason the movements of following a path, homing in and flying in a
wave, the wrapping counter, the tint effect, the scenery object and the player's pushed and scripted states were
removed: a kind is built when something needs it.

**Why, and extending it.** This section is the worked example of sections 0 and 2 applied to the whole game, so
that placing a thing is looking it up, not deciding. It was built from the old code's lists of names, not from
reading every behaviour, so expect to find things it misses or misplaces. When that happens, run section 2 on the
thing, apply move 1, place it, and correct this section. If a thing listed here turns out, once understood, to be
two things or to be an existing type with a setting, the list is wrong, not the rule.
