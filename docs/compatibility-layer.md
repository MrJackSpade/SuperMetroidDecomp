# The compatibility layer

Core is a C# game (v0.9.0 manifesto). The one place behaviour of the original hardware survives in it is the
compatibility layer: a small set of services and value types, in the `SuperMetroid.Core.Compatibility` module, that
reproduce timing or values of the original machine which change what the simulation does next. Everything else that
came from the hardware is incidental representation or a presentation quirk, and is deleted (manifesto, section 2).

## Shape

- **Named for the game.** A service's type, members and parameters say what it does for the game: a sound acceptance
  clock, a room's load progress, a wrapping countdown. Never the chip it imitates; the vocabulary rule (SME6438,
  `docs/core-vocabulary.md`) holds for the layer's identifiers like any other Core code.
- **Marked.** Every type in the module carries `[CompatibilityService("behaviour")]`, whose argument is the gameplay
  behaviour reproduced. Its XML summary states that behaviour, the point in the update at which the service is
  consulted or advanced, and the evidence that established it (an SMV recording's trace, or the original routine).
  The summary may name the hardware behaviour it reproduces; identifiers may not.
- **Owns typed state.** A service holds its own state as domain values (requests, phases, counts). It never holds a
  memory image, a port array, a register byte or an address (SME6439 reports a byte or word array, span or memory
  field on a marked type).
- **Injected.** Session-scoped services are created or selected at the session's composition root
  (`SuperMetroidGameHostServices`, `FrontendSession`) with a lifetime matching the state they own. The game runs against
  one implementation; verification may supply recorded evidence through the same contract.
- **Value types** (a wrapping countdown) live in the module too and are used directly by the entities that own them.
- **Lowest module.** Every other module uses the layer (the wrapping counters are everywhere), so the layer depends on
  no other Core module: a service's contract is written in its own types, and the domain maps onto them (Audio passes
  a library's zero-based index; `SoundEffectLibraries.Count` is the clock's `LibraryCount`).

## What qualifies

Only behaviours the [Godot migration plan](godot-architecture.md) names as gameplay-observable (manifesto, section 4):
the frame-end display latch, draw-time side effects that mutate simulation state, display state gameplay reads back,
sound acceptance and acknowledgement timing, door-transition load progression, wrapping and borrowing counters,
level-data allocation tails and bounded over-reads, and the exact sequence of RNG reads and advances including those
performed by effect work. A behaviour outside that list is not admitted by being hard to remove. Each quirk gets its own
service only when it has its own timing contract; coherent responsibilities share one service.

## What does not qualify

- Anything kept "because the backend reads it": the simulation publishes a display model and the backend learns to
  draw it.
- Anything kept "because the replay needs it": the replay is driven by inputs and these services; if parity depends on
  something, that something is a service here, in domain terms.
- Presentation quirks (sprite limits, priority artifacts, raster timing): deleted, not preserved.

## The services

| Service | Behaviour reproduced | Consulted |
| --- | --- | --- |
| `SoundAcceptanceClock`, with `SoundClockStep`, `SoundRequestCheck(s)` and `SoundAcknowledgements` | when each sound library accepts a request, and what it reports back as acknowledged | every update, after the update's sound requests |
| `ISoundAcknowledgementTiming` / `ClockedSoundAcknowledgements` | the acknowledgements an update's sound handlers read (the clock's own, or a recording's) | at the start of each update |
| `TickAccumulator`, `TickAdvance` | a phase that wraps at a fixed period and carries its remainder, pacing the clock's checks and the music tempo | on every clock step |
| `IDoorMusicLoadFrames` / `InstantDoorMusicLoad` | frames counted while a door's new music loads | once per door transition |
| `IRoomLoadProgress` / `InstantRoomLoad` | how far room loading has progressed when each enemy initializes | each door-scroll update |
| `WrappingCountdown`, `CountdownStep`, `LowByteCountdownStep`, `WrappingCounter` | timers and counters that wrap at their width, tested as the original tested them | where each timer is advanced |
| `WordCarry` | the carry or borrow out of a byte or word that moves the next part of a multi-part value (#1420) | at each chained addition or subtraction |
| `SuitPickupSparkCarryover` | the suit pickup light beam's last progress (lines grown or wiped, or its two edge columns), which a later shinespark windup reads as its vertical acceleration's fraction (#1468) | when the suit pickup publishes its stage, at setup and after each stage call |
| `UnsignedDivision` | the aim-angle division: a word over a byte, a zero divisor giving the largest word (#1420) | when an enemy aims at a point |
| `FloorUpdateResidue`, `FloorUpdate`, `IFloorUpdateResidue` | the descriptors the Ceres Ridley getaway's floor updates leave behind, which Samus's running stage five and the invalid poses read as delays (a bounded over-read, #1338) | recorded at each getaway update that changes the floor; read when Samus's animation reads past its tables |
| `TerrainBeyondRoom<TTerrain, TBehavior>` | what lies past a room's grid: the level data that followed the grid, read as blocks, then solid blocks to the end of the room's level space, where room-object writes persist; and solid terrain for a probe outside the room's width or height (a level-data allocation tail and bounded over-read, #1469) | a room object's setup or draw past the grid, collision probes and extensions crossing the room's edge, enemy terrain reads past the grid; built from the installed level at each room load |
