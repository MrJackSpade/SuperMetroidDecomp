# Issue #1230: Tourian statue release sound

Report: a missing statue sound in $00/$33 ($8F:A66A), affected build carried forward as 0.4.14-smoke1228 / equivalent gameplay tag v0.4.15.

The eye-glow projectile script reaches $86:B7DB after 110 ticks: QueueSound_Lib2_Max6 with the byte operand $19, followed by the unlocking earthquake and particle spawns. The port's generic projectile interpreter advanced over packed sound instructions without publishing audio. The Tourian unlock interpreter now handles this script's sound command through the existing enemy sound queue, retaining its three-byte length, library, Max6 limit, and captured suppression behavior. The separate descending-statue earthquake sound path is unchanged.

Reference: pinned InsaneFirebat NTSC J/U 1.0 disassembly revision 362be646929cf8e483f692b73a6561cfc2dc1d0d, bank_86.asm, InstList_EnemyProj_TourianStatueEyeGlow $B7B3-$B7E8.

The focused frontend fixture loads the actual statue room, spawns its native eye-glow effect, and runs 130 frames with the audio renderer and real acknowledgements. Before: zero requests/zero sends. After: one library-2 $19 Max6 request and one port write, both at frame 110; no duplicate in the remaining tail. Release build passes.

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll --statue-release-sound-audit 'C:/Users/Service Account/AppData/Local/SuperMetroid'
```

The reporter did not identify the specific missing cue. This restores a confirmed omission in that room; audible player confirmation remains pending.
