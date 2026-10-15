# Shared verification source

These MSBuild shared-source groups own fixtures used by multiple developer runners.
They are deliberately not player dependencies and are not separate executables.

Each group is one module: `X.projitems` compiles exactly the `X/` directory beside it
(`X/**/*.cs`), so a new file belongs to a group by where it is placed, and no file sits outside one.

- `VerificationAccess.Core`, `.Desktop`, `.Rendering.Direct3D11`: views of private production state, test
  drivers and oracle-only ROM constants that tests use and production does not. Production carries
  no test-only members (#1273); these C# 14 extension members reach private state through
  `PrivateState` reflection, by name, in one place. `VerificationAccess.<Assembly>/<Module>/` mirrors the
  module of the type each view reaches and shares its namespace (`VerificationAccess.Core/Bosses/Kraid`
  is `SuperMetroid.Core.Bosses.Kraid`); the root holds the shared access infrastructure (`CoreAccess`,
  `CatalogFields`, `MetadataShape`).
- Every other module's files are in `SuperMetroid.TestSupport.<Module>`.
- `RenderFixtures`: cartridge/render scenarios for RenderVerification.
- `FrameCapture`: Direct3D frame readback and pixel comparison for render captures.
- `SaveMapFixtures`: the save-map presentation fixture shared by the save, desktop and render verifiers.
- `ScenarioFixtures`, `CatalogInventories`: gameplay scenarios and retail catalog inventories that
  DebugRunner records or prints and the verifier checks.
- `VerificationOracles`: reference oracles and audits only the main verifier uses.
- `CpuBusTestDoubles`: exact CPU-bus test doubles (#629).
- `VerificationImporters`, `RoomHeaderImport`: cartridge importers that only verification fixtures read.
- `WindowsConsole`: non-interactive native error handling for Windows verification runners and the
  portable integration runner when hosted on Windows.
- `RepositoryInstallation` and `WindowsConsole` are also linked into developer tools. Their
  verifier-only parts live in `RepositoryInstallation.Verification` and `WindowsConsole.Verification`,
  which only verification projects import, so a tool compiles nothing it does not use (the
  reachability gate enforces this).
- Code that tools need but players never run belongs in `src/SuperMetroid.Tooling`, not here:
  tests may use it, but it must have a tool consumer.

Each consumer explicitly imports its groups. These internal fixture types compile into each
consumer; no runner reaches into another executable's source tree.

Production code is consumed through its owning assembly: portable Android session, import,
policy and preference services live in Diagnostics; GameForm and desktop audio adapters live
in Desktop. Existing namespaces are retained. Platform Activity/View/AudioTrack implementations
remain in Android. The portable integration suite requires no Android workload.
