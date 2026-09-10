# Mini-Kraid attack cadence (#518)

Affected player version: 0.1.1. No cadence mismatch found in the matched comparison;
no gameplay timing was changed. Leave open for player confirmation of these findings.

The new audit loads room $8F:A521's actual population and collision data, retaining
Mini-Kraid in slot 3 and deleting the three preceding pirates in both fixtures.
Samus is held on either side (X=$500/$560, Y=0), outside projectile trajectories.
Each side runs 2400 frames with identical prescribed random words `9 + frame*37`
(16-bit wrap), after initialization with 9. This isolates scheduling from unrelated
global RNG consumers; it is not a recording of the player's controller route.

The native probe executes original cartridge initialization $A6:9A58, main AI
$A6:9AC2, instruction interpreter $A0:C26A and projectile processing $86:8104.
It reads collision words/BTS exported from the same room. Both paths retain actual
projectile allocation, movement and terrain collision, not a spawn-count mock.
Samus projectile-contact resolution is omitted in both controlled fixtures.

All 4800 rows match byte-for-byte, including body X, walk/facing words, walk/spit
decision timers, three spike clocks, selector, instruction/map/timer, and newly
allocated spike/spit counts. For each side:

- 36 spikes; first launches at frames 155, 201, 298, 318, 350, 430.
- Observed spike intervals: 20, 32, 46, 64, 80, 97 frames.
- 10 spit pairs, first at frame 168 and then every 240 frames through 2328.

Native main visits one of three spike clocks per frame. A zero clock reloads to
`(random & 63) + 16` and launches its row. The independent rows are not one global
cooldown, and spitting is an independent animation-instruction sequence. The measured
intervals above describe this stimulus, not all possible random streams.

`fake-kraid-cadence-518-v1.zip` contains managed/native CSV and the room collision
fixture. Both CSV SHA-256 values are
`7B86D0E79091CD79A3CCA5E17EFCF6E3B90E9E53503F4610A37ADB63D2187655`.
ROM SHA-256 is `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`;
upstream-sm pin is `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
The harness restores retail ROM bytes, bounds CPU instruction execution, suppresses
dialogs and refuses to overwrite native output. Its temporary entrypoint was removed.

Run DebugRunner with:

```text
--fake-kraid-cadence ROM OUTPUT_DIRECTORY EXTRACTED_NATIVE_CSV
```

The native CSV is SHA-gated and every row is asserted. To regenerate native output,
apply `native-fake-kraid-cadence-entrypoint.patch` in the pinned diagnostic upstream
worktree, build Release x64, then run:

```text
--diagnostic-fake-kraid-cadence ROM EXPORTED_LEVEL_BIN NEW_NATIVE_CSV
```

Reverse the exact entrypoint patch afterward. Clean DebugRunner build and the
4800-frame native comparison pass. The older broad `--brinstar-fake-kraid-audit`
does not currently pass: it expects immediate knockback from the contact producer,
before the common pending-hit interruption runs. Temporarily adapting that assertion
also exposed its immediate-deletion expectation on lethal damage. Those unrelated
combat assertions were left unchanged; do not describe the broad audit as passing.
