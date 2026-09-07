# Issue 321 acceptance audit

Audit checkpoint: `78a6d1d`, September 7, 2026. This is a completion checklist,
not a replacement for the full issue specification. A passing sampled fixture
does not prove untested paths. The goal remains incomplete.

## Definition-of-done evidence

Current-build aggregate revalidation at `e2b74b7`: Release `--retail-all` completed
with exit code 0, all 20 suites on both hardware and WARP. This includes the newly
fixed independent startup reference, blocked left/right doors, both elevator
directions, native Ridley escape branches and the save/death/reserve/ending paths.
It remains exact GPU/software coverage, not a claim that every scene matches an
independent emulator image. The separate title-pan reference diagnostic tracks #336.

The attempted Linux rerun after #335 did not execute: the permission checker
rejected exposing the private repository to the container. No workaround was used.
The previous successful Linux run at `d4aa15c` remains the recorded evidence, not a
new pass at this checkpoint. Any repeat of that container access needs approval.

Scope amendment (September 7, 2026): the user explicitly deferred the RDP
disconnect/reconnect test because it is not a standard use case for them. That
test is not a completion blocker for issue 321. Recovery across an actual RDP
reconnect remains unverified, not passed. This deferral does not waive the other
lifecycle, correctness, performance or hardware-default requirements except as
amended next: the user subsequently also explicitly deferred actual cross-monitor
testing. That external transition test is unverified and non-blocking. The enabled
PerMonitorV2 policy and automated resize/lifecycle checks remain in scope.

| Required gate | Current evidence | Outstanding proof/action |
| --- | --- | --- |
| Inventory and ownership | `RENDERER_MIGRATION.md`, `SuperMetroidGame.RenderCapture.cs`, ordered `RetailSceneTests` registry; publication routing audit below completed at `75ecdc1` | Review final changes against this inventory; do not count the registry length as exhaustive proof. |
| Portable contract/reference | Core targets plain `net10.0`; `PORTABLE_RENDER_TESTS.md` records isolated Linux build and execution, most recently at `16d33cd` | Latest Core changes pass. Recheck if further portable code changes land; this does not require a second hardware backend. |
| GPU coverage without raster/readback | Captured menu/intro/gameplay/ending publication, integer shaders, explicit readback API; implicit intro/gameplay raster fallbacks removed in `47239b2` | Complete publication audit; retain explicit legacy software diagnostics, not implicit GPU scene fallback. |
| Ownership and lifetimes | Owned packet/codec tests, retained replay, generation gate, blocked consumer, device recreation; close-during-load race fixed in `6d0881c` | Review final changes together; distinguish immutable packet proof from merely matching one frame. |
| Simulation/input/audio independence | Eight cases at `399f666`: two rooms, normal/blocked, hardware/WARP; software/GPU/headless runtime graphs and 974,400 PCM samples per case | The exact graph is the runtime graph, not the whole frontend graph. Confirm required frontend/save transitions through their separate state/audio tests; do not claim every field in every scene was compared. |
| Exact images and regression scenes | Full 20-suite Release aggregate at `399f666`; 18-suite Debug aggregate at `bc2c5ae` plus later individual tests; blocked doors added at `3c201b3` | Index retained independent cartridge/emulator evidence for historical regressions. GPU/legacy equality alone is not that evidence. |
| Selection and lifecycle | Auto now defaults to hardware with logged startup fallback; explicit Software/Direct3D11 retained; injected device-loss/reset, bounded retries, resize/suspension, shutdown tests; visible minimize/restore passed at `07b8e40` | Monitor/DPI checks remain unperformed. RDP reconnect testing is explicitly deferred by the user and non-blocking. Default selection is implemented; final qualification remains incomplete. |
| Performance | Visible Debug and Release five-minute gameplay/pause reports meet measured CPU/GPU budgets; full-history Debug and hidden Release memory/late-frame evidence | Preserve separate sampling scopes: older visible Release GPU percentiles are rolling; Debug visible uses whole post-warmup history. Do not claim 60 delivered RDP FPS. |
| Clean packaging/tools/save boundary | Clean game publication revalidated at `75ecdc1`, including minimized host wiring; four rebuilt shaders, matching assemblies/assets, desktop lifecycle and short audio smoke. Failure artifacts and packet replay verified in `fb34cc7` | Recheck if production host changes. Final docs must distinguish framework-dependent Windows app from portable Core. |
| Commit/push and handoff | Scoped changes pushed; evidence attached to issue | Final requirement audit, remaining unrelated-bug summary, usage/validation directions, then awaiting-player-validation label. Do not close without player confirmation. |

## Interactive work still requiring coordination

### DPI policy implementation

The game now explicitly selects `ApplicationHighDpiMode=PerMonitorV2` instead of
the SDK's SystemAware default. The desktop verifier uses the same policy before
creating HWNDs. The actual game entry point exposes `--dpi-awareness-audit`, also
run by clean publication, to assert the effective mode. Release game audit and the
full hidden desktop suite passed after this change. The existing child-canvas
SizeChanged path queues physical surface dimensions to the renderer; native frame
composition remains 256x224. No Windows monitor settings were modified.

This fixes the missing awareness configuration identified by source inspection;
it does not claim an actual cross-monitor DPI transition was reproduced. That
external test remains outstanding. Microsoft documents the default and per-monitor
configuration in [WinForms automatic scaling](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/forms/autoscale).

### Latest clean-package evidence

Final production DPI-policy package at `78a6d1d` passed the entire clean publish
script with exit code 0, including the effective PerMonitorV2 assertion, rebuilt
shaders, asset hashes, game audits, full hidden desktop lifecycle suite and short
gameplay/pause smoke (59.678/59.670 FPS; producer p95 0.884/0.145 ms; zero audio
drains). The owned `0ef94cc02afb43d2931b926d33de86a6` output was removed afterward.
The most recent portable Linux pass is at `d4aa15c`; no Core change followed it.

Read-only display enumeration on September 7 found one screen, DISPLAY209,
1920x1080. A real cross-monitor transition cannot be performed in that session
without an external setup change. No such change was attempted.

Revalidated after hardware-default selection at `d4aa15c`: the clean publish script
finished with exit code 0, rebuilt all four shaders, checked identical dependencies
and audio assets, passed the game audits and full desktop lifecycle suite (including
successful Auto hardware selection and failure fallback), and completed the short
production-timer smoke. Gameplay/pause measured 59.672/59.636 FPS, producer p95
0.8975/0.1491 ms and zero audio queue drains. This remains a packaging smoke, not a
new five-minute performance qualification. The owned output directory
`42ee6eb2450d42e290faf24dac82e77e` was removed after recording the result.

The isolated publish at `75ecdc1` reached and wrote its final five-second-per-scene
desktop timer report at `2026-09-07T10:32:13Z`. The original terminal handle had
expired when revisited; its exit status was not recovered. The script's preceding
gates require successful game audits and desktop lifecycle checks before this smoke.
The completed report records gameplay/pause respectively: 59.846/59.473 measured
FPS, producer p95 0.9079/0.1552 ms, zero over-deadline producer frames, zero discarded
wall-clock frames and zero audio queue drains. This short packaging smoke is not a
replacement for the five-minute qualification reports.

Game/verifier SHA-256 dependencies were rechecked equal after completion:

- Core: `B7F30F371DF767A223F326436120D9C798095C63AEFE458CFEAF98F0BEC479CD`
- Desktop: `77EA486C304F8069A183A69511E3240E42F4733AAD30CD25CFFA0D445AB84A23`
- Direct3D11: `64086E45363CB22EA41B2AB1BF8D47A90043793A3B53432FD4C3B92DF6056C5B`

The owned publish directory `d8aace6d988a41c3bdee33dcb444a768` was retired after
recording these results. Its builds and synthetic saves are reproducible; no player
save directory or unrelated diagnostic output was removed.

Visible minimize/restore was approved and passed three Release hardware cycles on
September 7, 2026 using `DesktopVerification --visible-minimize-restore`.
The visible HWND delivered native resize events without explicitly raising them:
the worker suspended, accepted twelve publications without presenting during each
250 ms suspension, then resumed presentation with matching canvas dimensions.
The isolated fixture was removed after shutdown; player saves were untouched.
This does not establish monitor/DPI changes or RDP reconnect behavior.
The hidden test explicitly raises a form Resize event and remains a separate fixture.
No RDP disconnect, monitor configuration change,
or physical driver reset has been performed. Injected HRESULT recovery is useful
code-path evidence but does not prove those external lifecycle events.

Do not replace these tests with repeated unrelated scene tests or silently lower
the gate. Likewise, pending interactive work does not prevent the remaining safe
source/fixture audit, Linux gate, or preparation of a reproducible lifecycle harness.

## Reference-evidence distinction

New controlled independent Snes9x captures are retained in
`test-fixtures/issue-321-snes9x-startup` and `issue-321-snes9x-title-pan`, with
ROM/executable/artifact hashes and isolated capture procedure. Startup freeze reload
reproduces its native PNG exactly; the managed startup color difference is separately
tracked as #335. The pan fixture is a native scene reference, not yet a matched-frame
comparison. Neither substitutes for other historical scene references.

The startup reference defect #335 is fixed and passes exact native/managed/GPU
comparison. The pan difference #336 is traced to missing native COLDATA/CGADSUB
HDMA streams, also absent in the pre-migration title renderer at `8e0b7b7`.
It is tracked as a pre-existing shared reference omission, not hidden by declaring
GPU/software parity to be cartridge correctness. See the pan fixture's native
source diagnosis for the exact routines and implementation constraints.

Follow-up inspection located the player's two original Maridia clipboard images
outside the repository. They are now preserved unchanged with SHA-256 and provenance
in `test-fixtures/issue-321-player-references/README.md`. The Snes9x window title and
the original report identify an independent emulator image, but this is not a matched
frame/state golden and cannot qualify the other historical regression scenes.

Repository search at `2225f92` found emulator setup notes in
`test-fixtures/issue-307-underwater-jump/README.md`, but no indexed independent
emulator captures for the renderer regression matrix. Files named
`pause-map-retail*.png` under test-temp are not evidence of emulator provenance
merely because their filenames say retail. This search does not establish that
no screenshots exist elsewhere on the player's machine. Their source, matching
ROM/state and correspondence to the tested render property still need confirmation.

The checked-in issue-321 packet fixture proves a renderer byte-selection defect
and its reproduction. Performance JSON proves only the stated measured workload.
Neither is an emulator capture. The current scene matrix explicitly identifies
GPU/software comparisons as such. A final audit must locate and link any retained
known-correct cartridge captures for the historically reported regressions, or
record the missing evidence and obtain it; do not relabel legacy output as ROM proof.

## Frontend publication routing audit

Inspected publication call sites in `SuperMetroidGame.cs`,
`SuperMetroidGame.AttractDemo.cs` and `SuperMetroidGame.RenderCapture.cs`.

| Implemented dispatcher family | Publication owner | Current verification family |
| --- | --- | --- |
| Reset/title, file select/options | Typed PublishMenu overloads | frontend/title, file/options |
| Intro and Ceres flight | PublishIntro | intro/transition fixtures |
| New-game/load fade, Ceres arrival, ordinary gameplay | PublishGameplay/PublishBlack | room publications, elevator, saved-file load |
| Pause and unpause | PublishGameplay, pause PublishMenu, ordered brightness/black | pause map/equipment/fade and slow-consumer slice |
| Reserve recovery | PublishGameplay | automatic reserve frontend fixture |
| Death/game over | PublishGameplay, brightness, game-over PublishMenu | death PCM/frontend and game-over fixtures |
| File map/load | PublishFileMap/PublishBlack | file-map and saved-file appearance fixtures |
| Door transition | Held source display then PublishGameplay | left/right, elevators, blocked-door fixtures |
| Ceres departure/destruction | PublishGameplay, PublishCeresDestruction, black/fade | Ceres/Zebes transition fixtures; runtime escape fixtures are separate |
| Zebes escape/ending | PublishGameplay, brightness/black, PublishEnding | native-event handoff and all ending reward branches |
| Attract demo states | PublishGameplay, title PublishMenu, PublishBlack/retained display | attract fixture |

All explicit raster calls in these frontend partials are behind the legacy branch
of a publication helper or the explicitly requested CurrentFrame pixel adapter.
Intro/gameplay missing captures throw in captured mode. `renderGameplayFrames=false`
is an intentional diagnostic retained-display mode, not the normal GPU desktop.
This routing audit identifies no additional implemented frontend scene family
requiring a new GPU compositor. It is not proof of every state combination or of
cartridge correctness; those remain bounded by the listed fixtures and reference
evidence. Unsupported enum values still fail in the dispatcher's default branch.
