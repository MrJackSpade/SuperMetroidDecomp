# Parameter lists and operation contexts (#633)

Operations take cohesive typed contexts instead of long positional lists. The analyzer in
`SuperMetroid.EnsureAnalyzer/ParameterListAnalyzer.cs` enforces this as build errors in every
maintained project, test and debug code included.

| Rule | Reports |
| --- | --- |
| SME6330 | A method, constructor, delegate or local function with more than **seven** parameters, optional ones included. |
| SME6331 | A signature with three or more nullable delegate parameters that switch capabilities on and off. |
| SME6332 | A parameter typed with a broad context when a narrower capability interface that the context implements covers every member the operation reads. |

## Exemptions

Exemptions are declared on the member, never per project:

- overrides and interface implementations, whose signature belongs to the declaration;
- `extern` members;
- positional record primary constructors and deconstructors, which declare data shape;
- members carrying an attribute named `ParameterListContract("reason")` for an immutable external
  contract (generated code, framework callbacks, P/Invoke or serialization). No member needs one
  today; declare the attribute type in the owning project when the first such boundary appears.

## Designing a context

- Name the context for its domain purpose: `RoomEnemyFrame`, `PlmView`, `SamusTerrain`. Do not
  use a generic `Options`, `Context` or `Parameters` name when the concept is known.
- A parent operation's context is the explicit superset of its children's contracts. Each child
  accepts only its own narrow contract: `RoomEnemyFrame : IEnemyAiFrame`, `IRoomEnemyServices :
  IChozoStatueRoomServices, IMotherBrainRoomServices`, `IRoomPlmEnemyEffects :
  INoobTubeProjectileSpawner, ...`. Where a private child would see only one implementation, compose
  a narrow concrete value (`PaletteFxConditions`, `RuntimeFramePolicy`, `VerticalTerrainProbe`)
  instead of passing the whole parent.
- Required capabilities are non-null and validated at construction (`PlmRoom`,
  `RuntimeFrameAudio.Gameplay`). Optional behaviour is a named scenario or null object
  (a default `RuntimeFrameRequest`, `RuntimeFrameAudio.Deferred`, `HostGameplayOverrides.None`),
  not a nullable delegate.
- A cluster of callbacks that represents one service becomes one interface (`IDemoInputProgram`,
  `IRoomSamusAccess`, `IRoomEarthquakeWriter`).
- Reuse existing domain values before adding new ones (`Mode7RenderRegisters`,
  `OrdinaryGameplayRenderLayer`, `EnemySpritemapDraw`, `PauseMenuEntry`).
- Keep per-invocation input separate from long-lived dependencies: `LoadRoomPopulation(PlmRoom,
  population, RoomPlmServices)`.

`docs/parameter-list-inventory.json` records every occurrence found at the baseline: the
operation, its parameters before and after, the contracts it now takes, and its status. It also
lists the forwarding clusters, the operations that copied parameters into fields, and the test
helpers that mirrored production signatures.

Compile-time contract checks live in `SuperMetroid.EnsureVerification`
(`VerifyParameterContracts`), runtime invariants in the Verification suite
`VerifyOperationContracts`, and analyzer cases in `VerifyParameterListAnalyzer`.
