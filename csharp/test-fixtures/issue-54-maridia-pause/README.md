# Player's Maridia pause state (#54)

`slot-0.smstate` preserves the player's in-menu debugger state captured September
6, 2026. It must be resumed from its existing frontend and managed audio graph,
not restarted from SRAM. Exact-build assemblies are preserved beside it because
the normal state loader checks their module identities.

The source slot in `debug-states` must not be overwritten by diagnostics.

Verified load: frame 2101, room $CEFB, state PausedB. `resumed-pause.wav` contains
600 consecutive input-zero frames from the saved audio graph (ten seconds of
48-kHz stereo PCM16). No SRAM restart, track restart, or sample-bank replacement
was performed. The state SHA-256 is
`C61287F8191367DBC9DB95DB21F0321B1E9E07E5CB77B7B82C0FAFBEABEAB4FB`.

The saved audio bank is Music_RedBrinstar, upload $D3E812, despite Samus being in
Maridia. All 38 saved source mappings have identical PCM and loop points to the
current extracted bank; stale sample WAVs are not the explanation for this state.
No audible-fix claim is made.

The tracked exact-state harness accepts `pause-audio` (creates the WAV exclusively)
or `pause-audio-inspect` (checks embedded samples without playback). Its assembly
references can be pinned to this fixture's DLLs with MSBuild properties
`ExactStateCoreAssembly` and `ExactStateDesktopAssembly`, using absolute paths.
The loader's normal MVID and ROM digest checks remain enabled. Do not rebuild
game assemblies merely to load this state.

## Playback deadline investigation

The player confirms this resumed WAV sounds correct, but live playback reports
EMU 50, Paint 13.7, Step 19/19, Late 9.8. At 50 produced video frames/second,
the fixed 800-frame audio blocks provide only 40,000 stereo frames/second to a
48,000-Hz device. A finite preroll cannot cover that sustained deficit.

`pause-profile` resumes the same pinned state without writing a WAV and measures
Step and audio generation separately. Before the renderer change, 600 saved-state
frames averaged 15.906 ms Step and 0.521 ms audio on this host, excluding GUI paint.

Composite4BppViewport now reuses each visible tile row's map word, palette and
four bitplanes, instead of reloading them per pixel. No gameplay frames or audio
samples are dropped. A frozen scalar renderer verifies exact output in 120 cases
with priorities, flips, per-line scroll, wrapping, and clipped tile runs. The
Debug-build 256x224 plane benchmark measured 6.502 ms scalar versus 2.806 ms
optimized. These are component measurements, not a claim of live end-to-end FPS.
Full Debug/Release verification and both Game builds pass; live audio and FPS
still require player confirmation after restarting the updated executable.
