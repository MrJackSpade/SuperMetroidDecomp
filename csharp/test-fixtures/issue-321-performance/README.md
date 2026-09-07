# Renderer performance evidence (issue #321, incomplete)

Run from the repository root:

```powershell
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --profile-simulation
```

The command emits a uniquely named JSON report under `csharp/test-temp/render-performance`.
It uses the retail ROM, extracted audio assets, production captured frontend stepping,
and the real managed desktop audio adapter. No player save or debugger state is modified.
There are 120 warmup frames followed by 600 measured frames per scene; percentiles use
nearest rank. Audio acknowledgements feed back normally and silent-only results fail.
`CaptureMs` includes simulation plus display snapshot creation, not snapshot creation alone.

The committed Release report records the build MVIDs, CPU identifier, runtime, sample
counts, allocations and timing distributions. The first scene is room-local gameplay
at $8F:A3AE. The second is a paused $8F:CEFB Maridia tube room. Both are entered through
the diagnostic room-load seam after normal new-game startup, not a retail save/load
playthrough; this is not proof of the exact player's inventory or music state.

This is **only an unpaced CPU baseline**. It does not run a GPU, display a window, submit
waveOut buffers, measure underruns, establish cross-backend PCM parity, or meet the
five-minute host-60-Hz gameplay/pause soak requirement. Those gates remain outstanding.
Do not infer whole-game performance from these two fixtures or add their independently
calculated percentiles together.

## Five-minute hidden-HWND paced run

```powershell
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --soak-hidden 300
```

`hidden-soak-release.json` records 300 seconds per scene, 18,000 simulation frames
and 28,800,000 PCM samples per scene. Both scenes had zero native-empty-before-refill
observations, no pending managed audio at the sampled endpoint, and no device recovery.
Producer p95 was 0.1927 ms (gameplay) and 0.1396 ms (Maridia pause).

The HWND was hidden: **zero displayed frames** were reported, with 9,605 occluded
Present calls and 8,395 replaced visual packets per scene. Display/Present p95 was
about 31.7 ms while the producer maintained its 60 Hz schedule. This establishes
independence under that presentation load, not smooth visible output. Renderer
percentiles cover its last 2,048 samples after warmup; producer percentiles cover
the entire measured run. GPU times exclude scaling/Present. The harness uses a
standalone paced producer, not the production WinForms playback timer.

This does not complete the issue's visible/live-production-loop soak, full scene
coverage, whole-run GPU timing, or cross-backend state/PCM parity requirements.

## Five-minute production desktop timer run (hidden HWND)

```powershell
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --soak-desktop-hidden 300
```

`desktop-timer-soak-release.json` exercises the actual PlayableGameControl timer,
input polling, captured stepping, managed audio engine, real silent waveOut output,
and hardware GPU worker. Each scene ran for 300 seconds and 17,999 frames (59.994 FPS).
Producer p95 was 0.2027 ms in gameplay and 0.1383 ms in Maridia pause. Neither run
observed an empty native audio queue before refill; this is not a hardware underrun
duration measurement. Managed queues contained five and three frames at the endpoint.

The HWND remained hidden and every Present was occluded. CPU composition p95 was
0.0327/0.0360 ms; GPU composition p95 was 0.6523/0.6810 ms. The full composition-plus-
display GPU interval was about 31.7 ms, so this run does **not** establish the visible
GPU frame budget. Renderer percentiles retain the last 2,048 samples, whereas producer
percentiles cover the full run. Input was polled by the host, not a prescribed replay.

This establishes production-timer independence under hidden presentation load. Visible
presentation, whole-run GPU timing, complete scene coverage, and full cross-backend
state/PCM parity remain separate gates; no renderer-default change is justified by
these results alone.

## Opt-in visible production timer run

New production-timer soak runs request a bounded 65,536-sample history for each
renderer timing stream and assert that every post-warmup observation is retained.
This permits whole-run percentiles for the five-minute workload without changing
normal desktop workers' 2,048-sample rolling history. Previously committed reports
retain their original sampling scope; this change does not retroactively expand them.

```powershell
dotnet run --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --soak-desktop-visible 300
```

This opens a nonactivating window for each of the same two scenes, with isolated
state files and muted real audio output. Keep it unobscured; do not interact with
the controller during measurement. It requires at least one successful Present
per scene and records adapter, OS/runtime and CPU identity in the report. Successful
Present calls still do not measure frames delivered to an RDP client.

`desktop-visible-soak-release.json` preserves the September 7 Release run on the
interactive RDP desktop: 300 seconds and 17,999 simulation frames per scene.
Gameplay/pause achieved 59.997/59.994 simulation FPS, with zero empty-before-refill
observations and native occupancy between four and six buffers. Producer p95 was
0.2001/0.1442 ms and p99 was 0.3106/0.2031 ms. These measured producer percentiles
leave substantially more than the required 25% CPU deadline headroom.

GPU composition p95 was 3.1908/1.8637 ms; composition-plus-display p95 was
3.2594/1.9333 ms. CPU composition submission p95 was 0.0795/0.0701 ms.
GPU composition p99 was 3.8185/3.2502 ms. Renderer percentiles cover the retained
2,048 samples after 60 warmup samples, not the entire five-minute interval.
Producer percentiles cover the entire interval, including startup work.

There were 9,598/9,601 successful presentations (about 32 FPS), zero occluded
presentations, and 8,402/8,399 replaced visual packets. The bounded mailbox had
no pending frame at either endpoint. This is simulation/audio independence under
RDP presentation limits, **not** a claim of 60 displayed FPS or measured RDP delivery.
Audio queue observations are not an endpoint underrun-duration measurement.

The adapter was an RTX 3090, driver 32.0.15.9636; Microsoft Remote Display Adapter
driver was 10.0.26100.8972. OS, runtime, CPU identifier and build MVIDs are in JSON.
Run from the interactive desktop: the initial restricted-execution run reported
only occluded Presents despite a visible-window flag and failed qualification.
The unchanged harness passed after execution on the interactive desktop. No
production rendering fix was necessary. Debug soaks, lifecycle coverage and
remaining ticket gates are not established by this Release report.
