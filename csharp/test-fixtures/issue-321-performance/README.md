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
