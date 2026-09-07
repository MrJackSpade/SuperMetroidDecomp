# Issue 321 acceptance audit

Audit checkpoint: `07b8e40`, September 7, 2026. This is a completion checklist,
not a replacement for the full issue specification. A passing sampled fixture
does not prove untested paths. The goal remains incomplete.

## Definition-of-done evidence

Scope amendment (September 7, 2026): the user explicitly deferred the RDP
disconnect/reconnect test because it is not a standard use case for them. That
test is not a completion blocker for issue 321. Recovery across an actual RDP
reconnect remains unverified, not passed. This deferral does not waive the other
lifecycle, correctness, performance or hardware-default requirements.

| Required gate | Current evidence | Outstanding proof/action |
| --- | --- | --- |
| Inventory and ownership | `RENDERER_MIGRATION.md`, `SuperMetroidGame.RenderCapture.cs`, ordered `RetailSceneTests` registry; publication routing audit below completed at `75ecdc1` | Review final changes against this inventory; do not count the registry length as exhaustive proof. |
| Portable contract/reference | Core targets plain `net10.0`; `PORTABLE_RENDER_TESTS.md` records isolated Linux build and execution, most recently at `16d33cd` | Latest Core changes pass. Recheck if further portable code changes land; this does not require a second hardware backend. |
| GPU coverage without raster/readback | Captured menu/intro/gameplay/ending publication, integer shaders, explicit readback API; implicit intro/gameplay raster fallbacks removed in `47239b2` | Complete publication audit; retain explicit legacy software diagnostics, not implicit GPU scene fallback. |
| Ownership and lifetimes | Owned packet/codec tests, retained replay, generation gate, blocked consumer, device recreation; close-during-load race fixed in `6d0881c` | Review final changes together; distinguish immutable packet proof from merely matching one frame. |
| Simulation/input/audio independence | Eight cases at `399f666`: two rooms, normal/blocked, hardware/WARP; software/GPU/headless runtime graphs and 974,400 PCM samples per case | The exact graph is the runtime graph, not the whole frontend graph. Confirm required frontend/save transitions through their separate state/audio tests; do not claim every field in every scene was compared. |
| Exact images and regression scenes | Full 20-suite Release aggregate at `399f666`; 18-suite Debug aggregate at `bc2c5ae` plus later individual tests; blocked doors added at `3c201b3` | Index retained independent cartridge/emulator evidence for historical regressions. GPU/legacy equality alone is not that evidence. |
| Selection and lifecycle | Explicit Software/Direct3D11/Auto; logged Auto startup fallback; injected device-loss/reset, bounded retries, resize/suspension, shutdown tests; visible minimize/restore passed at `07b8e40` | Monitor/DPI checks remain unperformed. RDP reconnect testing is explicitly deferred by the user and non-blocking. Default is still Software. Qualify and enable the hardware selection policy before completion. |
| Performance | Visible Debug and Release five-minute gameplay/pause reports meet measured CPU/GPU budgets; full-history Debug and hidden Release memory/late-frame evidence | Preserve separate sampling scopes: older visible Release GPU percentiles are rolling; Debug visible uses whole post-warmup history. Do not claim 60 delivered RDP FPS. |
| Clean packaging/tools/save boundary | Clean game publication revalidated at `75ecdc1`, including minimized host wiring; four rebuilt shaders, matching assemblies/assets, desktop lifecycle and short audio smoke. Failure artifacts and packet replay verified in `fb34cc7` | Recheck if production host changes. Final docs must distinguish framework-dependent Windows app from portable Core. |
| Commit/push and handoff | Scoped changes pushed; evidence attached to issue | Final requirement audit, remaining unrelated-bug summary, usage/validation directions, then awaiting-player-validation label. Do not close without player confirmation. |

## Interactive work still requiring coordination

### Latest clean-package evidence

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
