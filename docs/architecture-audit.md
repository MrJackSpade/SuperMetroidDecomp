# Architecture audit

Every type declared in the repository's source is listed in
[`architecture-inventory.json`](architecture-inventory.json) with its kind, scope (product, test or
tool), assemblies, files, partial status and disposition. The inventory is generated, and CI fails
when it is stale:

```
dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write
```

## Dispositions

Each type's disposition comes from its shape:

- `owner`: a class with instance state;
- `stateless`: a class with no instance state;
- `static-functions`;
- `definition-catalog`: a static class holding only data;
- `value`: an enum or struct;
- `record`;
- `contract`: an interface or delegate;
- `generated-boundary`: one hand-written declaration completed by generated ones.

## Findings

The audit reports five structural findings:

| Flag | Meaning |
| --- | --- |
| `back-reference:<creator>` | The owner is constructed with `this` by the type that stores it, so it can reach back into its creator. |
| `service-locator` | The type implements `IServiceProvider` or has a generic `Get<T>()`/`Resolve<T>()`. |
| `context-bag` | The type carries eight or more service-typed members and has no behaviour. |
| `large` | A product class has more than 1,500 hand-written lines. |
| `wide-dependencies` | A `*Dependencies` record hands its consumer more than 12 collaborators (#1362). |

Every finding fails the audit unless the type itself justifies it:

```csharp
[ArchitectureReview("context-bag", "A record of installed artwork catalogs; its members are data, not services.")]
public sealed record EnemyTileArtworkParts
```

The attribute names the finding kind (`back-reference`, `context-bag`, `service-locator`, `large`, `wide-dependencies`) and a reason
specific to that type. The audit requires one per current finding and fails on a review whose finding is gone, so
justifications cannot outlive the shape they excuse. There is no central list: nothing outside the type can
justify its finding, and the audit never accepts a finding in bulk.

A justification is legitimate only when the site is intrinsic (a framework callback that must reach its host, a
test fixture whose purpose is to carry services) or a false positive of the heuristic (a record of artwork data
counted as services). Every flag names a shape the design forbids (`manifesto.md`: entities and services with
explicit dependencies, no context bags, no reach-backs, no monoliths), so "it is a large boss translation" or
"the Samus aggregate constructs its parts" is not a reason; it is the finding restated. Those types are fixed.

## Large types split along their own structure

A `large` finding is fixed by splitting the type where its content already divides, never by moving an arbitrary
slice of lines out (#1395):

- A catalog splits by the domain key its entries already carry. Room states, doors, enemy populations and enemy
  graphics sets split by room-header region (`CrateriaRoomStateDefinitions`, `NorfairDoorDefinitions`, ...). The
  original catalog keeps the one table that concatenates the regions in native order, so lookups and ordering are
  unchanged. Body programs split by encounter concern (`TorizoFacingLeftPrograms`, `GoldenTorizoPrograms`), sharing
  the step vocabulary (`TorizoProgramSteps`).
- Each part keeps the catalog suffix of its source (`Definitions.cs`), because catalog status, and with it the
  cartridge-encoding analyzer's treatment of the literals it holds, follows the file name.
- A helper type used by only one part moves into that part; a type used by several stays with the source and is
  named through it.
- A static class of operations splits by the situation each operation handles (`SamusAerialPoseTransitions`,
  `SamusGroundedPoseTransitions`, `SamusPosturePoseTransitions`), not by size.
- A behaviour with injected collaborators splits into components that each take only the collaborators they use.
  A component never receives its creator: dependencies between components run one way, passed at construction,
  so the split introduces no `back-reference` finding.

## Composition sits above what it composes

A Core module cycle is broken by moving each dependency to the side it belongs on, never by merging modules or
recording the cycle (#1395). Code that assembles a room from every domain (the enemy system that constructs each
species, room loading, door transitions, the room scope and the per-frame phases that step enemies, PLMs and effects
together) lives in the `World` module, above the domains it composes. A domain below it never names the composition:
it states what it needs as a capability interface in its own module (`IEnemyPlmRequests`, `IRoomPlmFx`,
`IRoomWaterPhysics`), and `World` implements it over the species and owners. A contract only the composition
consumes, such as the display sources the frame boundary reads, belongs to `World` itself. Shared facts a lower domain
needs move to the lowest type that owns them: the enemy slot limits are `EnemySlotTable`'s, and a solid enemy body
carries its slot and whether it blocks Samus instead of the raw enemy words Samus would have to decode.

The seams that broke the cycle follow a few shapes, and a new dependency should reuse them:

- A lower domain sees a higher owner through a view of what it uses: room objects reach Samus through `IRoomSamus`,
  which `SamusRoomView` presents, and rooms and FX read whether a power bomb is active through `IActivePowerBomb`. The
  owner holds the view instead of implementing the interface itself, so its own callers keep the concrete type.
- A pass's results are written by each producer to its own builder: enemies write the elevator and gunship events,
  bosses write their room effects through `BossFrameResultBuilder`, and `World` composes the frame result.
- A family that owns a behaviour starts it itself: the Tourian statue and Mother Brain spawners start their typed
  projectile programs, and a PLM a shot triggers records it through `IShotTriggeredPlm`, rather than a lower table or
  caller naming every family.
- A schema that enumerates every family (the enemy-bank frame definitions, the room-state catalogs) is composition
  and lives in `World`; a lower domain reads it through a lookup the binding carries (`IEnemyFrameWords`).
- A small vocabulary type (a projectile type word, a message identity, a definition record) lives in the lowest
  module that uses it.

Installed artwork follows the same rule. The enemy directory is read once, but it is published as three catalogs, one
per owning domain: `EnemyTileArtworkCatalog` (ordinary enemies), `BossArtworkCatalog` (bosses) and
`AreaArtworkBundle` (the Ceres and Tourian actors). The runtime owns a `BossArtworkBinding` and an
`AreaArtworkBinding` for the session; each room's enemy system hands them to the classes that read them, so a rebind
reaches actors already in the room. The runtime artwork resolves boss and area transfers through the bindings, and
the directory's content identity covers all three catalogs.

## Partial types

The build analyzer enforces partial-type rules in every project:

- **SME6360**: a type's hand-written code is split across declarations.
- **SME6361**: a type is `partial` without a generated declaration.

`partial` is allowed only for a generated boundary: one hand-written declaration plus generated
ones. A generated declaration is one emitted by a source generator (`LibraryImport`,
`GeneratedRegex`), or a checked-in file that follows the compiler's conventions: an
`// <auto-generated>` header, or a `.Generated.cs`, `.g.cs` or `.designer.cs` name. Every
repository generator writes that header.

## Null-forgiving operator

The build analyzer reports every postfix null-forgiving expression (`value!`) as **SME6380**, in
production, verification, tooling and generated code alike, with no baseline or suppression. The
analyzer projects and their verifier, which do not load the analyzer, are checked by
`SuperMetroid.EnsureVerification`. Nullable warnings are build errors.

Express the contract instead: a non-null type, a checked accessor for lifecycle state
(`SuperMetroidGame.GameplayRuntime`, `ActiveRoomState.LoadedLevelData`), flow attributes that state
real behavior (`[NotNullWhen]`, `[MemberNotNull]`, `[NotNull]` on a throwing validator), or an
integration wrapper that fails with context: `RequiredJson`, `RequiredJsonNodes` and
`RequiredReflection` in `SuperMetroid.Tooling`.

## Conversion adapters

The build analyzer reports conversions whose only purpose is to reach another operation:

- **SME6390**: a parameterless `ToArray()` that only feeds a LINQ operator. On a sequence, call the operator on the
  source; on a span or memory, use the `ReadOnlySpanQueries` query that reads it in place (`Any`, `All`, `Count`,
  `First`, `Single`, `Sum`, and the owned-result `SelectToArray`, `WhereToArray`, `ConcatToArray`, `ToDictionary`).
- **SME6391**: a parameterless `AsSpan()` before an array's `SequenceEqual`; C# 14 binds the array to the span
  comparison directly.

A `ToArray()` that is stored, passed, compared through another overload, or that snapshots a reused buffer keeps an
ownership purpose and is not reported. The code fix removes the adapter where that is unambiguous. Domain transforms
belong to their owner type, such as `LayeredScene.Without<TLayer>()` and `LayersOf<TLayer>()`.

## One top-level type per file

The build analyzer reports **SME6400** for every top-level type after the first in a source file: classes,
records, structs, interfaces, enums and delegates, with any accessibility, in file-scoped, block or nested
namespaces, partial or not. A file of top-level statements declares no type. The file is named after its type
(`Type.cs`, or `Type.Purpose.cs` for a deliberate partial), and the domain directory groups related types; an
umbrella file does not. Nested types stay only as implementation details of their enclosing type.

Repository code, including any generator it adds, is analyzed; only the framework generators listed in
`OneTypePerFileAnalyzer.ThirdPartyGeneratorPrefixes` are exempt. The code fix moves a declaration into a sibling
file when its usings, namespace, attributes and documentation carry over unambiguously; a file with preprocessor
directives, statements or file-local types needs manual placement. The analyzer projects and their verifier are
checked by `SuperMetroid.EnsureVerification`.

**SME6401** requires the file to be named after that type: `Type.cs`, or `Type.Purpose.cs` for a deliberate partial
part. Files declaring no type (entry-point statements, assembly attributes) are not named by the rule.

## Comments belong to declarations

**SME6413** reports every `//` or `/* */` comment outside an executable body, and every `#region`/`#endregion`
(#1378). An executable body is the block or expression body of a method, constructor, destructor, operator,
conversion, accessor, local function or lambda, or top-level statements; comments there explain behaviour and stay.
Elsewhere, a comment beside a field, between members, in a data table or initialiser, or after a member is attached
to nothing and drifts when code moves, so the meaning goes into the declaration's XML documentation, a table entry
gets a name that carries it, and native-address notes, group labels, stale notes and banners are deleted.

The only comments a tool requires are accepted, by their exact text: the `// <auto-generated/>` marker, and the
reviewed same-line `// cartridge-encoding: allow(Class) - reason` and `// magic-number-audit: allow(Category) - reason`
allowances. Repository generators emit only that marker and document their provenance on the generated type. Files
the .NET SDK writes into a project's `obj` directory are not repository source and are not analyzed.

## Sequences are collections

**SME6415** reports a type whose fields or properties spell one sequence as numbered siblings (#1369): at least three
members of the same type whose names differ only by one digit run, with ordinals running gap-free from 0 or 1
(`Segment0Distance`..`Segment4Distance`). The type holds one array or indexed collection instead, and its XML summary
says what the index means; an element with its own role gets a named index or a named accessor over the collection.
**SME6416** reports a switch whose consecutive integer arms each return or assign one member of such a family, from
two members up, since it is an indexer written by hand. **SME6417** reports a hand-written list type: an integer
indexer that switches over constant indices or returns the same value for every index, beside a hand-written
`GetEnumerator`. It becomes an array, an `ImmutableArray`, or one value.

A gap-free numbered family that is a set of distinct identities, not a sequence, says so at its site with
`[NumberedIdentities(reason)]` on its members or its type. Gapped or mixed-type numbering (hex sound IDs, sparse pose
IDs) is not reported.

## Behaviour is written once, not once per direction

**SME6418** reports two methods of one type whose names differ only by opposing direction tokens (`Left`/`Right`,
`Up`/`Down`, `Clockwise`/`Counterclockwise`, `Add`/`Subtract`, `First`/`Second`) when their bodies are the same syntax
once mirrored (#1370): identifiers may swap direction tokens, numeric literals may differ in sign, and `+`/`-`,
`+=`/`-=`, `++`/`--` and the ordering comparisons stand for each other. The pair becomes one method taking a typed
direction or a small side descriptor that names what the sides vary; a body that only forwards to one call is already
that shape. Samus's left-facing pose rules and predicates are their right-facing rule applied through
`SamusPoseMirror` and the Left/Right button exchange. A deliberate, behaviourally distinct asymmetry says so at its
site with `[MirroredAsymmetry(reason)]`.

## Helpers are written once, not once per species

**SME6419** reports, in `SuperMetroid.Core`, a method body of at least five statements (nested statements counted,
blocks not) that is the same token sequence as a method in a different type once references to its declaring type's
own members, every type argument and every string literal's contents are normalised (#1371). Each clone group is
reported once and names every member; it becomes one shared implementation that takes what differs as data.
**SME6420** reports positional records with two or more parameters of the same names, types and attributes and no
members of their own: they are one record type. A one-field record is a typed identity, such as each species' pose
type, whose distinct type is its purpose, and is not compared. A copy that must stay separate says why at its site
with `[IntendedDuplicate(reason)]`.

## Modes are named, not numbered

**SME6421** reports, in `SuperMetroid.Core`, an integral field or property whose name says it holds a category (`Mode`,
`State`, `Phase`, `Direction`, `Kind`, `Page`, `Selection`, `SelectedItem`, `Function`, `Stage`, `Status`, `Type`) and
that is compared to a literal (#1372). **SME6422** reports such a member, whatever its name, once at its declaration
when equality comparisons or `is` patterns test it against two or more distinct literals; switch arms are index lookups
and are not counted. The member becomes an enum, or a state object, with semantic members, and the native word is
converted once at the import or snapshot codec. Samus's acceleration mode, the pause page, the game-over and file-select
Yes/No prompts, the file-select rows, Ridley's grab, shutter trigger modes, Yard's airborne facing, door-cap
orientations and the enemy-projectile collision option were converted that way. A genuine count, index, coordinate or
size says so at its declaration with `[IntegerQuantity(reason)]`; a packed native word whose bit fields a typed view
decodes, and whose zero test asks whether it holds anything, says so with `[PackedFields(reason)]`.

## Choices are named, not flagged

**SME6423** reports, in `SuperMetroid.Core`, a by-value bool parameter of a method, constructor or delegate to which
every argument in the compilation is a `true` or `false` literal (#1373). Such a bool chooses behaviour when the code is
written, so the member is split in two or takes an enum naming the cases (`HorizontalDirection`, `Axis`,
`GamePhaseAudio`, `StationSetupGate`, ...). **SME6424** reports a member taking two or more by-value bool parameters,
whose combinations become an enum or a split member; ref and out bools are results, not choices, and are not counted.
**SME6425** reports a field, property or parameter whose delegate type takes a bool input; the fact goes on the
event's context as a named property, or the delegate is split. Overrides, implementations and
method groups that escape to a delegate follow their contract and are not reported. Bools that are independent facts
read from runtime state, such as saved options, equipment, or per-frame conditions, say so at the member, or for a
record at its type, with `[IndependentFlag(reason)]`.

## Contracts are as narrow as their implementations

**SME6426** reports, in `SuperMetroid.Core`, a method whose body never reads two or more of its parameters, discards
and named parameters alike (#1374). **SME6427** reports a method whose whole body is one call to another method of its
own type, passing a strict subset of its parameters unchanged. Both mean a delegate, interface or base shape is wider
than the method needs, so the shape is narrowed and the real method is registered directly. An enemy's initialization
AI takes only its slot; the few species that read the room, Samus, the controller or the camera implement
`IEnemySurroundingsInitialization` and receive the spawn's `EnemySpawnSurroundings`. A main AI or typed animation that
needs only the slot is bound with a lambda at its entity, literal-RTL routines share `EnemyNoOpAi`, and cueless PLM
programs derive from `PlmCuelessProgramAnimation`. A method fulfilling a contract outside the repository, such as a
`System.Text.Json` converter, says so with `[ExternalContract(reason)]`.

## Concepts are defined once

**SME6428** reports, in `SuperMetroid.Core`, an enum whose every member is assigned one constant of the same other type,
and a static class of integral constants whose names and values all appear in one enum (#1375). Each value is written
once: in the enum, with any constant others read derived from it, or in the constant, with the enum gone. **SME6429**
reports a switch over an enum whose every arm maps a member to the member after it; a sequence's order is its enum's
order, stepped with `EnumOrder.Next` or `EnumOrder.NextWrapping`, and a partial map lists the members that have a
successor in one arm. **SME6430** reports a member signature carrying a tuple with three or more named elements, or a
tuple shaped like a type Core declares; the value gets its declared type, held to the same spatial and encoding rules
as any other. Palette-FX colour families read their rows from their programs (`PaletteFxColorRows.Of`), the rainbow-beam
sequence runs on Mother Brain's body functions, and timeline animations run on the one program-set engine.

## Variants are types, not tags

A closed set of variants is a closed hierarchy (#1376): an abstract record with a private constructor and one sealed
record per variant, each holding only its own fields. A record that carries every variant's fields plus a `Kind` enum
saying which of them mean anything lets a reader read a field that is meaningless for the current kind, and makes every
construction a row of placeholders. Consumers switch on the variant type and throw on an unknown one. Operand-less
commands share one `Command` variant whose kind enum lists only those commands; kinds that share one operand shape may
share a variant with their own kind enum. **SME6431** reports a construction of a type with an enum `Kind` member that
passes two or more placeholders beside the kind: `default`, a literal `0`, `false` or `null`, or an optional parameter
left to its default. **SME6432** reports a switch on such a kind-tagged type's kind whose arms read different subsets
of its other members.

## Identities are typed, and state has one writer

An identity is a type, not a string (#1377). A menu page, a sprite, a label, an anchor, a map-screen page or a room is
named by an enum member, a closed record or the definition object itself; the JSON name is a codec at the document
boundary (`PageName`, `FrameName`, `MapScreenPageId.Name`), and the loader keys its compiled dictionaries by the type.
The compiler then proves a selection exists, so the resource audit no longer resolves finite string flows. Text that
is a file format, such as PNG chunk names or player-authored INI values, stays text at its decoder, with a
justification on the decoder. A field another type writes is a field nobody owns: each mutable field is private or a
property whose setter is private, and other types change the state through named operations of its owner
(`ClipLibraryState.Play`, `SamusHandlerFrame.SampleFrameStartPose`, `FrontendSession.ChooseOptions`). Shared frontend
state lives in owners with real lifetimes: one screen at a time behind `IFrontendScreen`, fades in `BrightnessFade`,
the attract schedule in `AttractDemoSchedule`. **SME6433** reports a switch on string constants or a string member
compared with a string constant. **SME6434** reports a string field or property every assignment of which is a
constant of one type. **SME6435** reports an interpolated string of only integral or enum holes used as a dictionary
key. **SME6436** reports a non-private, non-readonly instance field of a class outside a privately nested type.

## Diagnostics are resolved, not disabled

**SME6414** reports every `#pragma warning disable`, whatever diagnostic it names and whatever reason follows it
(#1396). A disable hides a finding for a span of source instead of resolving it, so the code is fixed. A genuine false
positive is justified at its one site with `[SuppressMessage]` and a real `Justification`, which names the rule and the
reason where reports can find them; that attribute is not reported. `#pragma warning restore` on its own changes
nothing and is not reported. SME6414 is the one owner of the rule: the build-policy scan checks only that a gated
diagnostic is not suppressed by attribute or lowered in project configuration.

## Core source layout

Core is organized by gameplay domain first and then by cohesive feature. Each directory is a module whose
namespace matches its path (`Core/Bosses/Kraid` is `SuperMetroid.Core.Bosses.Kraid`):

| Module | Owns |
| --- | --- |
| `Samus` | Samus state, with `Movement`, `Poses`, `Projectiles`, `Grapple`, `Abilities` (X-ray, Crystal Flash, Shinespark, Speed Booster, morph), `Palettes`, `Drawing`, `Death` and `Hud` |
| `Enemies` | the shared enemy engine, one directory per enemy family, `Projectiles` (the enemy-projectile engine), `Drawing` and `Artwork` |
| `Bosses/<Boss>` | one boss encounter: its AI, room objects, projectiles and `Artwork` |
| `Rooms` | room headers, states and loading, with `LevelData`, `Backgrounds`, `Scrolling`, `Plms`, `Doors`, `SpriteObjects`, `Xray` and `Artwork` |
| `Fx` | room FX, animated tiles and HDMA, with `PaletteFx` |
| `Areas/<Area>` | scripted gameplay sequences that belong to one place (Ceres, the escapes, Tourian, Maridia) |
| `Gameplay` | the services every gameplay domain shares within a frame: the NMI clock and handler, time freeze, display state, host options and tester tools, runtime artwork, actor drawing, the game-state word, and the narrow contracts gameplay reads from the frontend (`IGameplayPresentationContent`, `IAttractDemoState`) |
| `Map` | the area map model: layouts, tile words, exploration and explored-bit packing, the minimap, reveal modes and the map views (`IAreaMapSource`) |
| `Hud`, `Saves`, `Progression`, `Programs` | the gameplay HUD and message boxes; save images; boss and event progression; the shared instruction-program machinery |
| `Runtime` | the gameplay frame loop that composes the simulation, with `Attract` (demo playback), door transitions, checkpoint autosaves and the runtime's renderer and audio publication |
| `Frontend/<Screen>` | the game-state dispatcher and host options, and one module per screen or cinematic (title, intro, `CeresCinematics`, file select, pause map, options, game over, ending, recordings) |
| `Assets` | artwork infrastructure every domain shares: PNG and JSON documents (`ArtworkJson`), tile encoders, base palettes |
| `Rendering`, `Audio`, `Spatial`, `Input` | renderers, the SPC driver, spatial and fixed-point types, and controller input. Hardware behaviour the game needs survives only as named compatibility services in `Compatibility` (`manifesto.md`), not as a model of the console |

A domain's installed-artwork layer (catalogs, presentations, formats and documents of extracted artwork) lives in
its `Artwork` module, so gameplay code and the artwork it draws stay separable. The resource audit uses that layer
boundary: files in `Assets` or an `Artwork` module are providers, and code elsewhere consumes them. `Tooling/Core` and
`test-support/VerificationAccess.Core` mirror the module of the type each file extends.

## Module map and dependency rules

`csharp/module-map.json` registers every maintained source root as a module: its path, namespace root, role
(production, tooling, analyzer, verification, verification support, tool source, fixture), its allowed project
references and shared-source imports, and mirror subdirectories (`Tooling/Core` is namespace `SuperMetroid.Core`).
Every project receives the map as an additional file, and the build enforces it:

| Rule | Requires |
| --- | --- |
| **SME6402** | a file declaring a type uses its module's namespace followed by its subdirectories |
| **SME6403** | every compiled source file lies inside a registered module |
| **SME6404** | a Core module references only types of its own or a lower layer |
| **SME6405** | a Verification file lives in the module of the Core domain it exercises, or in the shared `Harness` |
| **SME6406** | every top-level Core module is assigned a layer |

Core's layers, lowest first, are Geometry (`Spatial`), Foundation (`Compatibility`, `Input`, `Audio`, `Programs`,
`Assets`), Simulation (`Gameplay`, `Map` and the gameplay domains), Presentation (`Rendering`), Runtime and Frontend.
Code below the frontend reads frontend-owned content through narrow contracts that the frontend implements, never
through the frontend's types; documentation crefs are not dependencies.

**SME6407** keeps species behind the enemy system. Outside the `Enemies` and `Bosses` modules, Core reads and
commands species only through `RoomEnemySystem`'s capability interfaces (`Elevator`, `Gunship`, `Ceres`,
`RoomMainHooks`, `LoaderHooks`, `GrappleTargets`, `PlmSpawns`, `TourianStatue`, `DisplaySources`), never through a
`*Behavior` or `*Encounter` property.

**SME6351**, **SME6352** and **SME6353** keep Core's numeric vocabulary in its named types (#1326). Outside
`[EncodingOwner]` codec types, a whole value and its fraction are never carried as two primitives
or a tuple (`Speed`/`Subspeed`, `Whole`/`Fraction`): they are one `SignedSixteenSixteen`,
`UnsignedSixteenSixteen` or `FixedCoordinate` (SME6351). An angle, heading or bearing is never a raw byte or
word: it is a `RotationAngle` (8.8 turn), `ByteAngle` (trigonometry-table index) or `Bearing` (the
up-is-zero result of the cartridge angle routine); a name ending in Step, Delta, Mask or Offset is a difference in
table units and is not an angle (SME6352). No member, tuple element or by-reference parameter carries a processor
carry or overflow flag: carry arithmetic is `WordCarry`'s operations, and results name what happened (SME6353).

**SME6354** keeps underscore-prefixed fields and properties private (#1328). An underscore names a type's own
implementation state; another type changes it through a named operation on the owner (`BeginRoomLoad`,
`BindCgram`, `RecordRespawnScratch`) or reads a member named as part of the owner's surface, never the field.

**SME6408** keeps enemy state out of generic slot words. `RoomEnemySlot` declares no multipurpose word
(`VariableA`-`F`, `Parameter1`/`2` or another lettered variable or numbered parameter): each family owns its state as
typed fields, and authored spawn parameters are read from the slot's spawn record.

**SME6409**, **SME6410** and **SME6411** keep room-lifetime state in the room scope (#1329). A type marked
`[RoomLifetime]` is constructed only inside another room-lifetime type or a `[RoomScopeBoundary]` type, which states
its reason (SME6409); a type that is neither holds no field or auto-property of a room-lifetime type (SME6410); and no
`ResetForRoom*`, `Reset*RoomState` or `ResetForRoomLoad` method is declared (SME6411). Each room builds its own scope,
so nothing resets for the next room. See [room-scope.md](room-scope.md).

**SME6412** keeps rooms, room states and doors identified by their definitions (#1333): a field, property, parameter
or method named as a room, room-state or door pointer with a number type is reported. Runtime code names a room by
its `RoomHeaderDefinition`, a state by its `RoomStateSettings` and a door by its `DoorDefinition`; the native
bank-$8F and bank-$83 pointers are the import pipeline's knowledge.

`SuperMetroid.BuildPolicyVerification` checks what an analyzer cannot: every directory under `csharp/src`,
`csharp/test-support` and `csharp/tools` is registered; each project's `ProjectReference` and `.projitems` imports
equal the map's lists; production projects reference only production projects; and no build or tool output sits in
a source root outside each project's own ignored `bin` and `obj` (test fixtures excepted).

The architecture audit also writes `docs/module-inventory.json`: every compiled source file with its module, role,
namespace, assemblies, Core layer and the modules it references, each module's aggregate dependencies, and the
dependency cycles among Core modules. A cycle among Core modules fails the audit, and the map has no list of
accepted cycles; a cycle is broken, never recorded.
