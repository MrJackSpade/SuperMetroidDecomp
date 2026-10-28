# The project manifesto

This document is the reference for every design decision in this repository. Every agent, primary or sub, reads it
before changing code, and every change is judged against it. It explains what the project is, where it is going,
why the analyzers exist, why their findings are never excused, and what a correct change looks like as opposed to a
change that merely makes a tool stop complaining.

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

The destination is a Godot project with editable scenes, driven by a modern simulation that still produces the
cartridge's gameplay exactly. `docs/godot-architecture.md` describes the target shape: phase dispatch through
entities into domain services, shared state as explicit relationships between objects with real lifetimes, one
owner of simulation advancement, a display snapshot boundary, semantic audio and input contracts.

The path there is deliberate and ordered:

1. **Make it play.** Functionally exact cartridge gameplay with hardware lag eliminated.
2. **Remove the machine.** Replace emulated storage, hardware layouts, native words, cartridge addresses and
   address-valued dispatch with owned state, typed domains, semantic identities and behaviour objects, inside the
   C# product, with the existing hosts consuming the modernised simulation.
3. **Migrate to Godot.** Port a modern simulation, not an emulator-shaped one.

Every ticket in stage two moves the simulation toward the shape of stage three without starting it. The migration
will be cheap or impossible in direct proportion to how honestly stage two is done.

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
  contains dispatch logic, the name is a lie that hides the logic from the analyzers, and the logic moves out.
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

- One top-level type per file, named after the type, in a namespace that mirrors its module path
  (`csharp/module-map.json`). Core's layers depend downward only.
- A type owns its state; aggregates call domain operations on their children instead of writing their fields.
- Gameplay signatures take the owned state or the narrow service they use, never a bus interface or a broad
  context bag. Parameter lists stay under the ceiling and pass capabilities as narrow typed contracts
  (`docs/parameter-lists.md`).
- `partial` exists only for a generated boundary. The null-forgiving operator does not exist. Reflection into
  production types from tests does not exist; tests use typed seams.
- Every member carries XML documentation that says what it means, not what its type already says.

## 5. Why analyzers

The design above is dozens of rules deep and touches hundreds of thousands of lines. No reviewer holds that in
their head, and no agent reads every file before changing one. Without enforcement, every rule decays to a
suggestion within a week, because the path of least resistance in this codebase is always the machine-shaped one:
the `ushort` is already there, the hex is already there, the address switch is already there.

So each design decision, once made, is written down as a Roslyn analyzer in `SuperMetroid.EnsureAnalyzer`, runs
at **error** severity in every maintained project, and fails the build. The rules are the migration made
executable. `csharp/.editorconfig` and the analyzer sources are the authoritative list; the families are:

| Family | Enforces |
| --- | --- |
| Closed domains | A primitive switched or compared over named constants is a type. Switches are exhaustive and fail on undefined values. |
| Spatial | Spatial values are typed, branded by space, never parallel scalars. |
| Controller | Controller state is typed buttons; the raw word exists only where the controller is read. |
| Parameter lists | Operations take cohesive narrow contracts. |
| Encoding | Functional code holds no raw hex, address dispatch, opaque word arrays, raw width operations, hand-written countdowns, untyped flags or narrow storage. |
| Word reads | Multi-byte values are read through canonical operations, never rebuilt from bytes. |
| Source layout | One type per file, named after it, in its module's namespace, within the layer graph; `partial` only at generated boundaries. |
| Null-forgiving | `!` does not exist; express the contract. |
| Reflection and adapters | No reflective test access, no adapter chains. |
| Species | Species identity is owned by the species; outsiders use capability interfaces. |
| Guards | Argument guards use `Ensure` so failures name the expression. |

A rule is the record of a decision that was argued, decided and documented. An agent that disagrees with a rule
raises that with the owner. It does not route around the rule.

## 6. Why every finding is resolved, immediately, with no exceptions in bulk

A rule with a baseline is not a rule. It is a count. It says "the code may be this wrong, but no wronger," which
means the migration it was meant to drive is stalled at exactly the point where it was written down. The code the
rule was created to find is precisely the code the baseline excuses.

This is not hypothetical. This repository did it, and it cost three rounds of work:

1. Rules were added **with per-file baselines**. The build went green. The tickets that had asked for the rules
   were **closed** on that green build, while the baselines were still excusing most of their work. Those
   closures were false.
2. When the owner asked for the baselines to be removed, the agent **removed the rules as well**, because the
   rules "depended on" the baselines. The findings did not go away; they became invisible.
3. The rules were restored at full severity with no baseline, and tens of thousands of findings surfaced,
   because they had been there all along.

The lesson is not "baselines are bad." The lesson is that **any mechanism that lets a finding exist without a
decision about that finding is a lie about the state of the code**, and lies about the state of the code
compound: tickets close on them, new work builds on them, and the next agent reads the green build as evidence
that the design is being followed.

Therefore:

- A finding is resolved by **fixing the code** or by a **justification at that one site** that states why this
  site is intrinsic or a false positive. Nothing else resolves it.
- Never: a baseline file, a per-file count, a "may only shrink" list, an accepted-fingerprint file, an
  allow-list kept as a hook, a tool that regenerates allowances, an assembly- or namespace-wide suppression, a
  `<Pending>` justification, `NoWarn`, `WarningsNotAsErrors`, a severity below error, a change to a rule's scope,
  or a change to a rule's exemption lists (catalog suffixes, owner attributes) to make a site pass.
- A new or tightened rule ships at full severity with every existing site fixed or justified in the same work.
- A ticket whose rule still has findings is not done. If the findings are too many for the change in hand, the
  agent stops and asks the owner, and the ticket stays open.

## 7. What a correct fix is, and what a lazy one is

Agents are judged on this section. The analyzer stops complaining in both columns; only the left column is work.

### Fix versus satisfy

| The rule fires on | A correct fix | Lazy, and rejected |
| --- | --- | --- |
| A `ushort` field holding gameplay state | Decide whether the width is load-bearing (every producer bounded, every wrap clamped, no aliasing reads: `docs/cartridge-encoding.md`). If not, store `int` or the domain type and record the decision. If it is, store the countdown type or a named wrapping operation on the owning type. | Change `ushort` to `int` without checking. Add an allowance comment to every field. Rename the file to a catalog suffix. Mark the state class an encoding owner. |
| A test asserting `0xc000` against a subspeed | Say what the value is: a fixed-point subspeed constructed by its type, or a named expected constant with its unit; a cast like `(SamusEquipmentFlags)0x0004` becomes the flag's name. The test gets more readable, not less. | Write `49152`. Delete the assertion. Move the number into a catalog-named file beside the test. Wrap the suite in an owner attribute. |
| `switch (roomPointer) { case 0x91f8: … }` inside a catalog-named file | This is dispatch on a cartridge address in functional code, hidden by its filename. The rooms get semantic identities; the selection logic moves to a non-catalog type and switches on those. | Leave it: the analyzer does not fire on catalog-named files. |
| A `[Flags]`-to-word cast inside the one `ToWord` codec | This is the encoding boundary. The codec type is marked the owner of that encoding, with the reason on its summary, because it really is the single place the conversion happens. | Mark every type that casts a flag. Rewrite the codec to avoid the cast. |
| A species identity tested in a shared catalog | Give the species an interface for the capability being asked, implement it on the entity, and ask the bound entity. | Move the test into the species folder unchanged. Add a catalog wrapper. |
| A bus interface parameter in a gameplay phase | Find what the phase reads through the bus, give that state an owner, pass the owner or a narrow service. | Add a boundary attribute to the class. |
| A countdown written as a wrapping decrement and a sign-bit mask | The field becomes the countdown type; the site decrements and acts on the step's outcome. | Write `t--` on an `int` and `if (t < 0)`, changing the wrap. Add an allowance. |
| An integral array of hex words driving an animation | A typed timeline with frames, durations and cues, verified against the ROM as an oracle; delete the array and its interpreter. | Split the array into pieces too small to trigger the rule. Move it to a catalog file. |
| An enum whose members are named but whose values are ROM addresses and are read somewhere | Find every read of the value. Each is a translation (move it to the import codec) or a dispatch (replace with a behaviour reference or capability interface). Then the values are arbitrary. | Keep reading the value "because the enum names it." |

### When an allowance is legitimate

A same-line `// cartridge-encoding: allow(Class) - reason`, a boundary attribute with a reason, an
encoding-owner attribute, or an `[ArchitectureReview(finding, reason)]` on the flagged type is correct **only**
when the value, operation or shape is intrinsic to that site: an exact mask
the hardware or a file format defines, a hash, a wire-format constant, a conversion that is by construction the
one place the encoding happens. The reason names the thing, not the rule: `allow(BitMask) - phase-inversion bit`
is a reason; `allow(Native) - cartridge value` is a baseline entry wearing a comment.

A reviewer may reject any allowance whose reason would be equally true of a thousand other sites. If it would, it
is not a reason.

### Recognising lazy task completion

The repository contains work done to close a ticket rather than to advance the project. Learn to see it:

- **A type that wraps the machine representation without replacing it.** A "typed" property that returns the raw
  word. A view class whose every member is a cell read. A countdown whose every use reads the raw value.
- **A rename that satisfies a check.** A catalog-named file with control flow. A catalog that contains behaviour.
  A boundary attribute on a whole class because one method needed it.
- **An identity that is still an address.** An enum, constant or record whose value is a ROM address and whose
  value is read, compared, indexed or switched on anywhere in the simulation.
- **Documentation that describes the code instead of the design.** A summary that says "the X word" on a `ushort`
  called `X`. A note that lists what was done rather than what shape the code is meant to reach, or that records a
  first stage whose successor never appeared.
- **A test that pins the representation.** Asserting raw words the port is meant to stop storing. A ROM oracle
  that compares hex instead of meaning.
- **A ticket closed on a green build that had exemptions in it.** Any ticket closed while a rule it depended on
  had a baseline is suspect until that rule has zero findings.
- **A commit that lands a rule without landing its fixes**, under any name: stage, ratchet, baseline, inventory.
- **A scaffold with a maintenance tool.** An inventory of addresses, a generator that rewrites a catalog, a
  reviews file for shapes the design forbids, an accepted-cycles list with tracking issues. Each one keeps a step
  alive past the point where it should have been replaced.

When an agent finds one of these while doing other work, it does not quietly work around it. It records it on the
owning ticket (or opens one) with the file, the shape, and what the correct shape would be. It does not expand its
own scope to fix it unless that is the ticket in hand.

## 8. Rules for agents

These are binding for a primary agent and for every subagent it spawns. A subagent prompt that omits them is
incomplete.

**Source of truth for findings.** A red build stops at the first failing project; `dotnet build` undercounts.
Count with the census (`tools/sme-census`, see `docs/analyzer-cleanup.md` for the invocation) and work from its
output. A project is clean when the census says so, not when the build passes.

**Never touch the enforcement to clear a finding.** The analyzer sources, `.editorconfig` severities,
`Directory.Build.targets`, `module-map.json`, the catalog-suffix list in the encoding scanner, and the set of
boundary and owner attributes are not cleanup surfaces. A genuine analyzer bug (a false positive the rule's own
documentation says should not fire) is fixed in the analyzer **with a verification case in
`SuperMetroid.EnsureVerification` that fails before the fix**, in its own commit, named as an analyzer fix.

**Never move code to a name that exempts it.** No renaming to a catalog suffix, no new catalog-named file that
contains logic, no encoding-owner or boundary attribute on a type that is not by construction the single owner of
that encoding or layer.

**Never add machine knowledge to the simulation.** No new address, cell, register, bank, pointer, hex identity or
bus read in gameplay code, whatever label it would carry. New knowledge of the ROM goes in the import pipeline.

**Decide per site, record per decision.** A widened field cites the conditions it satisfies. A field that stays
narrow says why its wrap is load-bearing. A behaviour-object conversion names the pointer it replaced. A test that
stops asserting a raw word says what it asserts instead.

**Gameplay parity is checked by the change's own focused test, not by replays run to look for trouble.** A fix to
a wrap-sensitive field gets a test that exercises the wrap. A behaviour conversion gets its ROM-oracle check.
Nobody runs the full movie replays to "see if anything broke"; the player tests the build.

**One clean build and the focused checks, then commit.** Commits are per rule family per subsystem, each with its
tests, each leaving the tree no redder than it found it. Nothing is batched into a thousand-file commit.

**Report in these terms.** For each batch: the rule, the census count before and after, the sites fixed, the
sites given an allowance with each reason, anything found that is lazy completion from earlier work, and anything
that needs the owner's decision. "Resolved N findings" without the breakdown is not a report.

**Ask when the right fix is a design decision the ticket does not settle.** Whether a width is load-bearing is
investigated, not asked. Whether a capability deserves a new interface or an existing one is a pattern question:
find the existing pattern and follow it. A change to a whole subsystem's representation that the analyzers or the
destination require is made, not asked about: the size of a change is never a reason to pause.

## 9. On documentation

A document in this repository exists to tell the next agent what the design **is**, so that their change follows
it. It is not a changelog, a ticket summary, a status board or a record of effort. A doc that cannot be used to
judge whether a new change is correct is not documentation.

Each `docs/*.md` note states the target shape, the rules that enforce it, the exemplar in the tree, and what an
allowance looks like. It is updated in the same commit as the design change it describes. A note that describes a
shape the tree no longer has, or a step whose successor never came, is wrong and is fixed or deleted. A note that
describes a point in time (counts, stages, what is in progress) belongs in a ticket, not in `docs`.

## 10. In one paragraph

This is a port of Super Metroid being turned from an emulator-shaped translation into a modern simulation so that
it can be moved into Godot without carrying the SNES with it. The gameplay is preserved exactly; the machine's
representation is removed wherever it is not load-bearing and expressed in domain terms wherever it is. Identities
are names, never addresses; behaviour is an object, never a pointer; the ROM is known only to the import pipeline.
Hardware boundaries are scaffolding for removal, not a design. Analyzer rules encode the design decisions already
made, run at error, and fail the build on every finding, because a finding that is excused is a decision that was
never made. Every finding is fixed at its site or justified at its site, never in bulk, never by renaming, never by
weakening the rule. Work that makes a check pass without moving the simulation further from the machine is not
progress and is recorded as debt. An agent that cannot do a fix correctly within its ticket stops and says so.
