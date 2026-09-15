# Crystal Spark interruption: native CPU comparison (#427)

This covers the grounded Crystal Flash interruption of controller-earned spark
windup. It does **not** complete #427's suit-pickup/X-Ray/bomb alternatives or its
other setup heights.

Run the original cartridge instructions, not the translated C functions:

```powershell
cmd /c '"C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat" && csharp\native\TemporaryBlueSuitAudit\build.cmd'
cmd /c 'csharp\native\TemporaryBlueSuitAudit\audit.exe "Super Metroid.smc" crystal-spark > csharp\test-temp\crystal-spark.csv'
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --crystal-spark-audit "Super Metroid.smc" csharp/test-fixtures/issue-427-crystal-spark/native.csv
```

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Numeric trace SHA-256 (LF-normalized UTF-8):
`276DE1B8FB7C586EC14F5DC8A618641DDC3406272A90B8CAF30E5F65D7A791E6`.
No ROM, SRAM, graphics, or player snapshot is included.

## Boundary and controls

The existing flat-room fixture earns Speed Booster, stores a charge, clears run
momentum, and enters windup through controller processing. At frame 152, construct
an aged Power Bomb at its final cleanup boundary. Native executes `$88:8B4E`;
the port advances the real explosion owner to the equivalent boundary and runs a
normal runtime frame. This isolates cleanup/admission, **not** a full player bomb
placement sequence. The native adapter stores HDMA channel registers required by
Flash allocation; it does not execute DMA or verify the bubble rendering.

Eight cases run in each facing, 500 frames each (8,000 observations):

- Mode 0: exact center, 49 health, ten of each ammunition, exact chord; succeeds.
- Modes 1–4: one pixel right/left/down/up; all reject Flash.
- Mode 5: 51 health; rejects.
- Mode 6: nine missiles; rejects.
- Mode 7: extra Jump held; rejects and launches diagonally instead.

Each frame compares position including fractions, pose, animation frame/timer,
base/extra speed, boost counter, contact damage, shared palette/timer words,
vertical speed/direction, health and all three ammunition counts. At frame 460,
the successful case stores a second charge **without running again**, then
launches vertically and drains energy at frame 473. Explicit assertions ensure
this is usable Blue Suit, not merely a retained numeric counter.

## Reproduced differences and corrections

1. Frame 408: after Flash, the old windup movement owner resumed. Flash now
   relinquishes the old movement/palette owner while preserving boost and speed.
2. Frame 181, rejected controls: timed-out windup initialized its launch animation
   before AnimateSamus, consuming the first tick. Timeout now publishes a deferred
   interrupted pose, committed after animation and higher-priority interruptions.
3. Frame 408 after ownership correction: walk-off unconditionally reset upward
   direction. Native `$91:EFEF` preserves ascending velocity; the port now does too.
4. Frame 202 (173 for extra-Jump control): crash entry prematurely cleared contact
   damage. Native EndSuperJump retains it until the next frame's shared clear.

These failures were observed before their corresponding production corrections.
The final full trace, including second-charge reuse, matches exactly.

Regression verification also passed all 161,332 existing temporary-boost and
Draygon comparisons, the complete bank-$80 verification executable, and the
Windows Desktop Release build (zero warnings/errors).
