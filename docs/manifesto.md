# The project manifesto

This document is the reference for every design decision in this repository. Every agent, primary or sub, reads it
before changing code, and every change is judged against it. It explains what the project is, where it is going,
and what a correct change looks like.

It describes the destination and the principles, not the current state. Current state lives in the tree, the
tickets and the build. If a shorter rule elsewhere (AGENTS.md, a `docs/*.md` note, a ticket) seems to permit
something this document forbids, this document wins, and the shorter rule is wrong and gets fixed.

## 1. What this project is

This is a C# port of Super Metroid that plays the retail game from beginning to end with the cartridge's gameplay
reproduced exactly: update order, state changes, collision, RNG, input consumption. The player owns the ROM; the
repository owns the game logic. Nothing extracted from the ROM is ever committed.

It is **not** an emulator, and it is not meant to look like one. The port began as a translation of 65C816
routines into C#, so it inherited the machine's shape: a flat address space, WRAM cells, 16-bit words standing in
for every quantity, code pointers standing in for behaviour, hex literals standing in for meaning. That shape got
the game running. It is also the thing the project exists to remove.

## 2. Where it is going

The destination is a Godot game that plays as the cartridge does, written new
(`docs/v1.0.0-manifesto.md`, `docs/godot-architecture.md`).

The path there is deliberate and ordered:

1. **Make it play.** Functionally exact cartridge gameplay with hardware lag eliminated.
2. **Remove the machine.** Replace emulated storage, hardware layouts, native words, cartridge addresses and
   address-valued dispatch with owned state, typed domains, semantic identities and behaviour objects, inside the
   C# product, with the existing hosts consuming the modernised simulation.
3. **Write it in Godot.** A new program, designed for Godot, with this code as its reference for what the game does
   ([the v1.0.0 manifesto](v1.0.0-manifesto.md)). This code is not moved or reused, and this manifesto and its
   rules stop at the Godot project's boundary.

Stage two exists to make this code a clear reference for stage three. It does not define stage three's design.

## 3. The standard of cleanliness

The finished simulation is code that a competent reviewer could read without being able to tell it was ported
from a cartridge. Not because the history is hidden (it is in git and in these documents), but because nothing in
the simulation refers to the machine: no addresses, no banks, no register widths, no pointers, no hex, no
"native" anything. An enemy is a type with a name. A room is a named identity. A timer is a timer. A behaviour is
an object. The cartridge is a thing the import pipeline reads once; the simulation has never heard of it.

That standard is what every principle below serves, and it is the test to apply to any change: does this leave
the simulation knowing less about the machine than before, or the same, or more? Only the first is progress.

## 4. The principles

### Intermediate steps are not designs

Removing the machine happens in steps, and every step leaves a shape in the tree that is better than what it
replaced and worse than the destination: an address catalog where addresses used to be scattered, a typed view
over a bus that used to be read directly, a slot word with a typed property over it, an enum whose names are
semantic but whose values are still addresses. These shapes are **scaffolding**. They exist so the next step can
be taken; they are not the thing being built.

The repository's recurring failure is codifying scaffolding as design: a step gets written into a `docs` note or
AGENTS.md as "the pattern", the next agent copies it faithfully because that is what the documentation says, a
tool is built to maintain it, and the project locks itself into a temporary state that nobody chose. A pattern
that was a sensible first move during one migration becomes the thing every later change preserves.

So:

- A step toward the destination is always described as a step: what it replaced, what replaces it, and what must
  be true before it can be replaced. A document or pattern that describes a step without saying what it is on the
  way to is wrong.
- Before copying an existing pattern, an agent asks whether that pattern is the destination or scaffolding. The
  destination is section 3 and the principles below. If the pattern falls short of them, it is scaffolding, and
  the agent does not add a new instance of it. A new case is written to the destination shape even when its
  neighbours are not there yet.
- Scaffolding is never extended, tooled, inventoried or verified as if it were permanent. A tool that maintains a
  scaffold (an address inventory, a baseline, a reviews file for shapes the design forbids, an accepted-cycles
  list) is a sign the scaffold is being mistaken for a design.
- When the last consumer of a scaffold goes, the scaffold goes in the same change. Nothing is kept "in case".

### Load-bearing behaviour versus incidental representation

The cartridge's behaviour is a mix of two things that look identical in a translated source:

- **Load-bearing machine behaviour.** A timer that wraps below zero and is tested by its sign. An 8-bit decrement
  of the low byte of a word whose high byte is independent state. A health value whose borrow into its subunit
  changes what the player sees. The order two enemies' routines run in. These produce the gameplay the player
  experiences. They are preserved **exactly**, and they are preserved **explicitly**: a type or a named operation
  says "this wraps, and here is the branch that follows."
- **Incidental machine representation.** That a facing is stored as `$0000` or `$0004`. That an enemy's AI is
  selected by a code pointer. That a quantity which never leaves a small range lives in a `ushort`. That a palette
  lives in CGRAM. These are how the 1994 program happened to be written, and the port sheds them.

Every design rule forces a decision between those two. A `ushort` field is not wrong because it is a `ushort`; it
is wrong because nobody has stated whether its width is load-bearing. Once stated, the field becomes a domain type
(the width was incidental) or becomes a named wrapping type or operation (the width was load-bearing, and now the
code says so in domain terms, without mentioning the processor).

"Preserve gameplay exactly" is never a reason to leave a machine representation in place. It is a reason to
replace it carefully.

### Identity is semantic, never an address

A cartridge address is not an identifier. An enemy whose definition lived at `$D4BF` is not "enemy `0xD4BF`"; it
is a species with a name. A room whose header lived at `$91F8` is a room with a name. A routine at `$A2:88C5` is a
behaviour. Carrying the address around as the thing's identity, even wrapped in a named constant, even as the
backing value of an enum member, keeps the code coupled to the cartridge after the cartridge is gone: the number
means nothing without the ROM, it misleads every reader into thinking the ROM is still in play, and it invites the
next `switch` over addresses.

So:

- Identities are names: enum members, types, instances of behaviour objects. Their backing values are arbitrary
  and nothing reads them.
- Behaviour is dispatched by reference. Where the cartridge stored a code pointer that selected per-frame
  behaviour, the port holds a reference to a behaviour object implementing a typed interface. Identity checks
  compare instances or ask a capability interface. There is no runtime table keyed by a number, and no `switch`
  over addresses.
- The mapping from a ROM address to a semantic identity exists in exactly one place: the import or extraction
  codec that reads the ROM. It is not a catalog the simulation consults; it is the translation layer's private
  knowledge. When the import pipeline is the only code that can name an address, the simulation is clean.
- A catalog of addresses inside the simulation (`*Addresses`, `*Pointers`, `*CodePointers`) is scaffolding: a
  first step that puts every address in one place so it can be seen and removed. It is never the destination,
  and new addresses are not added to the simulation on the grounds that a catalog exists for them.

### Typed domains, not words

- A quantity is an `int` or a unit type, never a native word, unless its wrap is load-bearing and named.
- A mutually exclusive set is an `enum`; a switch over it handles every member and throws on an undefined value.
- Composable bits are `[Flags]`, only where the cartridge demonstrably composes them. The flags stay typed; the
  one place that converts them to a word is a codec at the import or export boundary.
- Spatial values are one typed value in a branded coordinate space, never parallel scalars
  (`docs/spatial-types.md`).
- A countdown is a countdown type that keeps the exact wrapping behaviour and exposes the branch outcomes the
  game takes (`NativeCountdown`); the site reads as "decrement, then act on expiry," not as a mask test.
- Controller state is typed buttons; a raw joypad word exists only where the host reads the controller.

### Catalogs own data; functional code owns behaviour

- Definition data (durations, speeds, shapes, damage, names, frame lists) lives in domain-named catalog types,
  one per domain, with the meaning on each member's XML summary. Call sites read as named domain operations.
- A catalog file holds **data**. It does not hold `if`, `switch`, loops or decisions. If a file named as a catalog
  contains dispatch logic, the name is a lie that hides the logic from review, and the logic moves out.
- Functional code holds no raw hex. A hex literal is one of: a quantity (decimal, with a unit), a domain identity
  (an enum member), composable bits (a flag), a structured instruction (a typed record or timeline), or unknown
  (kept at the boundary under an explicit unresolved name until its meaning is established). "Unknown" is a
  legitimate classification. "Unexamined" is not.

### Behaviour objects and entities

- A species owns its definition, its state, its AI and its identity: a folder with the species' behaviour,
  entity, state and animations. Code outside the folder never tests the species' identity; it asks the bound
  entity through a participation or capability interface.
- Instruction programs (animations, spritemaps, timelines) are typed steps with frames, durations and cues,
  verified against the ROM as an oracle at import or in a verification. The opaque word array and the opcode
  interpreter are deleted when the typed program lands, not kept beside it.
- Shared state is one storage identity injected into every consumer, with lifetimes that match the thing
  (session, room, entity), never a global cell that several systems happen to read.

### The machine is removed, not encapsulated

The end state has no address space, no WRAM, no PPU memory, no DSP and no APU RAM in the simulation. What remains
of the machine is a small set of **stubs**: named types and operations that reproduce a specific timing or
idiosyncrasy the player can observe (a wrapping countdown, a borrow between two counters, a frame-phase order, an
acknowledgement delay). A stub is expressed in domain terms, documents the behaviour it preserves, and does not
mention the hardware it came from.

On the way there, hardware access is **isolated** behind declared boundaries (`[CpuBusBoundary]` and its PPU, DSP
and SPC equivalents, each with a reason naming the layer). Understand what a boundary is for:

- A boundary makes a remaining reach-in visible so that it can be removed. It is debt with a label on it.
- A boundary is narrow: a type or a method, never a project or a test family. A wide boundary hides debt instead
  of labelling it.
- A typed view over emulated hardware is a step, not a design. The next step is always the owning system holding
  that state itself, with the hardware layout surviving only in the import and export codec.
- Nothing new is placed behind a boundary. A change that adds a bus read, a cell, a register access or a memory
  view to the simulation moves in the wrong direction, whatever label it carries.

### Ownership and layering

- One top-level type per file, named after the type, in a namespace that mirrors its module
  path. Core's layers depend downward only.
- A type owns its state; aggregates call domain operations on their children instead of writing their fields.
- Gameplay signatures take the owned state or the narrow service they use, never a bus interface or a broad
  context bag. Parameter lists stay short and pass capabilities as narrow typed contracts.
- `partial` exists only for a generated boundary. The null-forgiving operator does not exist. Reflection into
  production types from tests does not exist; tests use typed seams.
- Every member carries XML documentation that says what it means, not what its type already says.

## 5. Code analysis

The SNES port's Roslyn analyzers, audits and asynchronous cleanup (everything under `csharp/` that only policed or
cleaned up the code) were removed on 2026-11-22. The v0.9.0 bugs are still fixed in `csharp/`; once they are, that
code is frozen as the reference version and new work happens only in the Godot project (`godot/`, also C#), which
gets its own code analysis and rules later. Until then the principles above are kept by review.

## 6. Rules for agents

These are binding for a primary agent and for every subagent it spawns. A subagent prompt that omits them is
incomplete.

**Never add machine knowledge to the simulation.** No new address, cell, register, bank, pointer, hex identity or
bus read in gameplay code, whatever label it would carry. New knowledge of the ROM goes in the import pipeline.

**Decide per site, record per decision.** A widened field cites the conditions it satisfies. A field that stays
narrow says why its wrap is load-bearing. A behaviour-object conversion names the pointer it replaced. A test that
stops asserting a raw word says what it asserts instead.

**Gameplay parity is checked by the change's own focused test, not by replays run to look for trouble.** A fix to
a wrap-sensitive field gets a test that exercises the wrap. A behaviour conversion gets its ROM-oracle check.
Nobody runs the full movie replays to "see if anything broke"; the player tests the build.

**One clean build and the focused checks, then commit.** Commits are per change per subsystem, each with its
tests, each leaving the tree no redder than it found it. Nothing is batched into a thousand-file commit.

**Report in these terms.** For each batch: the sites changed and why, anything found that is lazy completion
from earlier work, and anything that needs the owner's decision.

**Ask when the right fix is a design decision the ticket does not settle.** Whether a width is load-bearing is
investigated, not asked. Whether a capability deserves a new interface or an existing one is a pattern question:
find the existing pattern and follow it. A change to a whole subsystem's representation that the destination
requires is made, not asked about: the size of a change is never a reason to pause.

## 7. On documentation

A document in this repository exists to tell the next agent what the design **is**, so that their change follows
it. It is not a changelog, a ticket summary, a status board or a record of effort. A doc that cannot be used to
judge whether a new change is correct is not documentation.

Each `docs/*.md` note states the target shape and the exemplar in the tree. It is updated in the same commit as the design change it describes. A note that describes a
shape the tree no longer has, or a step whose successor never came, is wrong and is fixed or deleted. A note that
describes a point in time (counts, stages, what is in progress) belongs in a ticket, not in `docs`.

## 8. In one paragraph

This is a port of Super Metroid being turned from an emulator-shaped translation into a modern simulation so that
it can be moved into Godot without carrying the SNES with it. The gameplay is preserved exactly; the machine's
representation is removed wherever it is not load-bearing and expressed in domain terms wherever it is. Identities
are names, never addresses; behaviour is an object, never a pointer; the ROM is known only to the import pipeline.
Hardware boundaries are scaffolding for removal, not a design. Work that makes a check pass without moving the simulation further from the machine is not
progress and is recorded as debt. An agent that cannot do a fix correctly within its ticket stops and says so.
