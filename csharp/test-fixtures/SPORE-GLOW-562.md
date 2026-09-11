# Spore Spawn background glow (#562)

Affected version: 0.1.1. Closed at player direction after battle glow confirmation.
No production change or confirmed mismatch.

## Cartridge definition

Pinned disassembly bank 83 FX record 826E selects palette-FX bit 08 for the
Spore Spawn room. Brinstar's object F779 starts at 8D:EE2D; EE31 selects byte
destination E2 (CGRAM words 113..115, background palette 7 colors 1..3).
EE35..EEC4 contains fourteen ten-frame color records followed by a loop.
Thus the intended blue-spore glow cycle is 140 frames, including before combat.
Setup F730 suppresses spawning after the area mini-boss bit is set; pre-instruction
EEC5 deletes the active effect when that bit becomes set during the fight.
This is distinct from the boss's health palette and death palette transitions.

## Live-room evidence

Run `--spore-glow-audit ROM OUTPUT_DIRECTORY` through SuperMetroid.DebugRunner.
It loads the real room through its upward incoming door, runs the full runtime,
and captures the production gameplay render packet for 980 consecutive frames.
The observer places Samus at the bottom encounter position, locks input and
provides 1499 health. It does not force boss AI phases or palette writes.

All 980 displayed-palette samples match an independent transcription of the
disassembly's three-color records and ten-frame cadence. There are 596 frames
in the moving-fight phase; the other frames include initial idle/descent.
The initial 420-frame run failed the active-fight coverage assertion
(only 36 moving frames); extending the observation fixed the test setup, not gameplay.

Each frame is rendered twice: unchanged, then with only CGRAM words 113..115
zeroed in a detached snapshot. At least 633 visible non-HUD pixels differ on
every frame. Boss movement cannot substitute for this comparison, since all
other memory/layer inputs are identical. This establishes actual displayed glow
contribution, not merely an active palette slot or advancing timer.

Bright/dim combat captures at frames 560 and 630 were visually inspected: the
blue background spores visibly change intensity behind the live boss. Images and
CSV remain private under ignored `csharp/test-temp/spore-glow-562`; none published.

## Evidence limits and closure

The expected cadence comes from pinned source/data, not yet an independent
original-CPU palette handler capture. The player explicitly directed closure once
battle glow was confirmed and rejected death/re-entry animation checks as outside
the requested behavior. Those extra checks are not completion requirements.
The counterfactual render verifies palette propagation and visible contribution;
it is not a native-emulator screenshot comparison or independent rasterizer oracle.
Issue #562 is closed on that basis; no native-handler replay is claimed.
