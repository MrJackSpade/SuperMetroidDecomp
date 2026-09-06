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
