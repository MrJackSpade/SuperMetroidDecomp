# Climb landing / first-shot report (#1159)

Affected player build: `smoke-94a75e59`.

The original confirmation mistakenly examined input frame 6106, the later
successful shot. The actual first shot at frame 5977 was reproduced: Samus
starts the frame running at X=475, Y=2187; the projectile explodes behind the
closed cap without creating its opening PLM. She enters the wall-stop pose
later in the same frame. The shot at 6106 uses the shorter wall-stop muzzle
offset and opens the door.

`native-climb-first-shot-probe.h` executes the **original 65816 CPU**, with
the retail bytes restored after the comparison harness's carry patches.
It constructs only the two reported producer states and a minimal Climb
cap/transition-block layout. It is not a native replay of the whole landing
trajectory. No ROM, SRAM, recording, images or extracted artwork are included.

The native results are:

| Producer pose | Explosion anchor | Opening actors |
| --- | --- | --- |
| Running right (`09`) | 498, 2179 | 0 |
| Stopped at wall (`89`) | 494, 2182 | 1 |

Both use initial collision radii 8, 4 and the ordinary uncharged beam.
`$90:BA56` selects running muzzle offset (15, -2), versus ordinary (11, 1),
subtracting the pose's six-pixel Y correction. `$94:A23B` scans the leading
edge's column. Running's probe is column 31, behind the cap at column 30;
the stopped probe is column 30. `$90:AE3A` then moves the explosion anchor
to the leading radius. The running shot's miss is therefore also present
in the native producer for this same position/pose/terrain.

No gameplay change is warranted by these two shot outcomes. In particular,
do not clamp the muzzle to this door or move projectile production after
beta just to make the first shot open it.

## Repeat

Use the existing `native-release-probe.h` integration described in this folder's
README (if not already present), then apply `native-climb-first-shot-entrypoint.patch`
in `upstream-sm`. Build the native Release x64 project and run:

```powershell
./upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-climb-first-shot 'Super Metroid.smc'
```

The entry point bypasses SDL startup, suppresses explicit error/warning dialogs,
and installs the Windows no-fault-dialog policy before loading the cartridge.
The bounded CPU helper fails with a console error/nonzero exit code on exhaustion.
Both exact coordinates, the explosion type and PLM allocation count are asserted.
Reverse only this integration patch afterward; preserve existing native changes.

For the player's private recording, the guarded DebugRunner's
`--confirm-reported-shaft-door RECORDING INSTALLATION CSV` confirms the **actual**
5977 miss, persistence of the closed cap, the 6106 opening actor, and clearance
of all four cap blocks at the 18-tick opening endpoint. It uses installed content,
recording-owned SRAM and a RAM-only gameplay address space; it does not change
the player's saves or installation. Player confirmation remains pending.
