# Cinematic Crystal Flash interruptions (#432)

This fixture verifies the three cinematic Crystal Flash interruption families described by
the Shinespark Suit technique reference: Mother Brain's rainbow beam, the Super Metroid's
terminal drain, and Ceres Ridley's ejection. The wiki is treated as a technique hypothesis;
the expected handler ownership comes from executing the pinned Japan/USA cartridge and
cross-checking `upstream-sm/src/sm_90.c`, `sm_91.c`, and `sm_a9.c`.

The native matrix enters Crystal Flash through `$90:D5A2`, advances the complete movement,
animation, pose-transition, hurt, and collision sequence, and invokes the real cinematic
entry points. It records the physical Samus movement/input handlers, frame-handler trio,
pose, resources, shine timer, and special-palette handler. No gameplay cheats are enabled.

## Confirmed behavior

- Mother Brain command `$18` changes the drained pose and frame handlers but preserves an
  active Crystal Flash movement pointer. Its locked beta handler executes no movement;
  command one later restores the ordinary beta dispatcher and the retained Flash resumes.
- If the Super Metroid installs drained crouching while Flash is already active, drained
  animation command `$F7` later replaces the Flash movement pointer. Palette handler seven
  and the RTS input pointer survive, yielding the immobile shinespark-suit state.
- In the adjacent failing order, drained crouching exists before Crystal Flash admission.
  Flash replaces that state and completes through ordinary movement/input restoration,
  freeing the stun without retaining the suit.
- Ceres `$90:E119` immediately replaces Flash movement with its ejection no-op. It does not
  replace palette handler seven or the RTS pose-input handler.
- X-Ray admission replaces the retained Flash movement, palette, and input-handler owners.
  Its teardown therefore returns to ordinary input rather than resurrecting immobility.
- An unsuited Super Metroid drain tick subtracts four energy. The earlier C# reduction of
  three shifted the terminal-crouch timing by multiple actor frames.

The active timing samples cover raising (`0`/`5` calls), ammo drain (`12`/`240`), and the
adjacent completed-Flash control (`250`). A separate native row runs 400 calls after the
Super Metroid-before-Flash ordering and confirms ordinary movement/input restoration. The
C# verifier uses the production Mother Brain, drained-Samus, Ceres ejection, Crystal Flash,
X-Ray, and runtime-dispatch paths; it asserts suspension, same-frame resume, retained
immobility, X-Ray recovery, and the successful/failed ordering distinction.

This is a focused handler/state reproduction, not a full controller route through either
boss room and not a claim about every visual cue. The independently verified indefinite
palette/body cue and X-Ray cancellation traces remain documented under
`../issue-430-flash-lifetime/`.

## Reproduction

```powershell
cmd /c '"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" && csharp\native\DraygonCrystalAudit\build.cmd'
csharp\native\DraygonCrystalAudit\audit.exe "Super Metroid.smc" cinematic-flash
dotnet run -c Release --project csharp/src/SuperMetroid.Verification/SuperMetroid.Verification.csproj -- --cinematic-flash csharp/test-fixtures/movement-release/cinematic-flash-432.csv
```

Original ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.

Committed trace SHA-256:
`4AF751016FE4922EAD25AC5043FC4CF40A3B1C8A44A7052758E959A15BBA1556`.
