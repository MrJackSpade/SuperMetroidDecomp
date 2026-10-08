# Shared verification source

These MSBuild shared-source groups own fixtures used by multiple developer runners.
They are deliberately not player dependencies and are not separate executables.

- `RenderFixtures.projitems`: cartridge/render scenarios for RenderVerification.
- `StateFixtures.projitems`: private fixture loading for IntegrationVerification and DesktopVerification.
- `WindowsConsole.projitems`: non-interactive native error handling for Windows verification runners and the portable integration runner when hosted on Windows.
- `VerificationAccess.Core.projitems`, `.Desktop.projitems`, `.Rendering.projitems`: views of
  private production state, test drivers and oracle-only ROM constants that tests use and
  production does not. Production carries no test-only members (#1273); these C# 14 extension
  members reach private state through `PrivateState` reflection, by name, in one place.
- `VerificationImporters.projitems`, `RoomHeaderImport.projitems`: cartridge importers that only verification fixtures read.
- `RepositoryInstallation.projitems` and `WindowsConsole.projitems` are also linked into developer
  tools. Their verifier-only parts live in `RepositoryInstallation.Verification.projitems` and
  `WindowsConsole.Verification.projitems`, which only verification projects import, so a tool
  compiles nothing it does not use (the reachability gate enforces this).
- Code that tools need but players never run belongs in `src/SuperMetroid.Tooling`, not here:
  tests may use it, but it must have a tool consumer.

Each consumer explicitly imports its group. These internal fixture types compile into each
consumer; no runner reaches into another executable's source tree. The source audit retains
its existing common owner under `csharp/tools`.

Production code is consumed through its owning assembly: portable Android session, import,
policy and preference services live in Diagnostics; GameForm and desktop audio adapters live
in Desktop. Existing namespaces are retained. Platform Activity/View/AudioTrack implementations
remain in Android. The portable integration suite requires no Android workload.
