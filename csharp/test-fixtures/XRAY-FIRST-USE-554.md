# First-use X-ray diagnostic (#554)

Affected player version: **0.1.1**. The reported intermittent corruption has not
been reproduced by this fixture. This is a negative experiment, not a fix or
grounds for closing the issue / requesting player validation.

## Run locally

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --xray-first-use-audit "Super Metroid.smc" csharp/test-temp/xray-first-use-554
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --compare-sequence csharp/test-temp/xray-first-use-554
```

Outputs contain game assets: keep PNGs, packets, and CSVs local and untracked.
The commands are headless. The second command tests hardware Direct3D11 and WARP.

## Stimulus and evidence

- Load the retail X-ray Scope room ($8F:A2CE), place Samus at (128,139), and
  preserve its origin-aligned loaded viewport. Flush bootstrap VRAM work before
  loading the room; do not reuse a debug camera offset without refilling tiles.
- Select the scope with ordinary controller input. Hold Run for 45 frames,
  release for 30, and repeat. Require active post-setup scanning and completed
  teardown, not merely no exceptions.
- Compare two otherwise identical runtimes with the old $7E:4000 reveal buffer
  filled with zero versus FF before first activation. Compare every rendered
  pixel over 150 frames. The native setup reads/captures BG1 and reconstructs this
  buffer; the experiment checks whether its earlier contents become visible.
- Capture the initial ordinary frame and all 150 subsequent frames. Render them
  in order with one persistent renderer per device to retain GPU resource state.

Result: no stale-buffer-dependent pixel differences across 150 frames. All 151
captured frames match software exactly on RTX 3090 and WARP. The aligned active
scan PNG was visually inspected; Samus stands on the floor with the beam visible.

The capture path reads the reveal map during setup, but keeps the reveal window
closed until scanning begins. Pinned `upstream-sm/src/sm_91.c` stages 2/3 capture
BG1, stage 4 builds the map, stages 6/7 upload it, and stage 8 changes backdrop.
An early reference to the buffer alone is therefore not proof of visible leakage.

## Limits / next reproduction work

This covers one direct room load and one stationary scan trajectory. It does not
cover a player's full frontend/door-entry sequence, other rooms, pre-existing
effects, Android presentation, or dropped display packets. Software/GPU agreement
also cannot establish cartridge parity when both consume the same wrong packet.
Keep #554 open without `awaiting-player-validation` until the reported transient
is reproduced and a matching assertion verifies its fix.

## Sparse display-consumer experiment

`SuperMetroid.RenderVerification --compare-sparse-sequence <capture-directory>`
renders the same complete captured packets while deliberately omitting display
submissions. It always submits the initial ordinary frame, then tests every phase
of strides two through eight. Each schedule uses one persistent renderer, so
resource state carries across first activation, release and subsequent activation.
Software expectations are generated from each retained packet, not from adjacent
frames or a regenerated gameplay run.

Using the existing 151-frame aligned capture, RTX 3090 and WARP each passed all
35 schedules (1,085 retained frames per device) with exact pixel parity. This
rules out skipped setup/display packets at those cadences as a cause in this
fixture. It does not reproduce #554, establish cartridge framebuffer parity,
exercise a real frontend mailbox or asynchronous swapchain, or cover Android.
The next reproduction work remains the actual room-entry/frontend/effect context;
do not label the issue fixed or awaiting player validation based on this result.
