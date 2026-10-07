# Issue #1258: shallow-water jump in $01/$28

The regression holds Jump for 60 frames at X=104, Y=427.FFFF in retail room
$8F:A408, state $A415, with no equipment. The water surface is Y=446; the
standing pose has radius 21 and occupies bottom pixel 447. The new jump pose
$4B has radius 19, making its bottom pixel 445, although the live collision
radius remains 21 until the next movement pass.

`native.csv` was captured by `probe.cpp` running the original ROM's instructions
on the unchanged pinned `upstream-sm/src/snes/cpu.c`. It uses the retail room's
collision words and BTS exported by the managed fixture, not translated native
movement functions. The probe models WRAM, ROM, and multiply/divide registers;
unexpected accesses fail. Enemy processing, rendering, and audio are excluded.
The managed side runs the real room through `SuperMetroidRuntime.StepFrame`.

Both sides start from the same exported position/subpixels, pose, animation
state, water surface/options and remembered medium, then hold logical A ($0080).
Each native frame executes radius refresh, input, gravity, movement, animation,
pose updates/commands, and held-jump history in their normal order. The CSV checks
pose, 16.16 position and velocity, vertical direction, live radius and remembered
medium on every frame. The apex is Y=339.0000, where the room ceiling stops ascent.

Before the correction, the port launched at 1.C000 and reached only Y=413.1FFF.
Native launches at 4.E000 while retaining medium=water on the contact frame.
Correcting movement alone exposed a three-frame animation delay from the same
stale-radius read in pose-change animation initialization. Both readers now use
the new pose definition; live collision-radius publication remains unchanged.
The complete 60-frame trace matches after both corrections.

References cross-checked against the installed ROM:
- $90:98BC jump initialization calls $90:EC3E, which samples the pose-defined
  radius: https://patrickjohnston.org/bank/90#f98BC
- $91:FB08 independently uses the new pose's radius for animation delay:
  https://github.com/InsaneFirebat/sm_disassembly/blob/master/src/bank_91.asm
- Pinned local translations: `upstream-sm/src/sm_90.c` and `sm_91.c`.

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
CPU source SHA-256: `A5D88B0F2E0798482A2CAE9DDDAF602FEC69A8FD26C55B167417C4C92EEC30A6`.

From repository root, with the supported ROM available:

```powershell
dotnet build csharp/src/SuperMetroid.Verification -c Release --no-restore
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0-windows/SuperMetroid.DebugRunner.dll --export-shallow-water-jump
& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' csharp/test-fixtures/issue-1258-water-jump/probe.vcxproj /p:Configuration=Release /p:Platform=x64 /v:minimal
& csharp/test-temp/issue-1258-water-jump/probe.exe upstream-sm/sm.smc csharp/test-temp/issue-1258-water-jump/room.bin csharp/test-temp/issue-1258-water-jump/seed.txt
dotnet csharp/src/SuperMetroid.Verification/bin/Release/net10.0/SuperMetroid.Verification.dll --shallow-water-jump
```

The probe prints the native CSV; compare that output to the checked-in trace.
Do not regenerate the expected trace from managed output. The managed export
contains room geometry and the initial seed, not expected movement results.