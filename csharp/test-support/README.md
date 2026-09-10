# Shared verification source

These MSBuild shared-source groups own fixtures used by multiple developer runners.
They are deliberately not player dependencies and are not separate executables.

- `RenderFixtures.projitems`: cartridge/render scenarios for DebugRunner and RenderVerification.
- `StateFixtures.projitems`: private fixture loading for IntegrationVerification and DesktopVerification.
- `WindowsConsole.projitems`: non-interactive native error handling for the two Windows verification runners.

Each consumer explicitly imports its group. These internal fixture types compile into each
consumer; no runner reaches into another executable's source tree. The source audit retains
its existing common owner under `csharp/tools`.

Production code is consumed through its owning assembly: portable Android session, import,
policy and preference services live in Diagnostics; GameForm and desktop audio adapters live
in Desktop. Existing namespaces are retained. Platform Activity/View/AudioTrack implementations
remain in Android. The portable integration suite requires no Android workload.
