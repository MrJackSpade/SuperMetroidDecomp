# Standalone Ceres Ridley audit repair (#587)

The standalone audit failed before reveal because its room fixture omitted the
required area-boss service. Fresh projectile subfixtures also omitted this
service (and one omitted the current-RNG reader). Explicit fresh-Ceres services
now satisfy those dependencies; production required-service checks are unchanged.

Advancing the repaired fixture exposed obsolete assertions at three boundaries:

- Contact publishes a pending hit. It does not immediately install bank-$90
  knockback. Fireball checks now admit the pending request before inspecting hurt
  pose/OAM; body/tail checks use the shared exact publication/admission helper.
- Ejection is requested at getaway byte index $D0, then admitted on the following
  frame. It owns a movement handler, not the host-wide `InputLocked` flag. The
  fixture now invokes the actual admission boundary and asserts pending at frame
  104, active at 105, with host input lock still false.
- Warning setup has fifteen transfer-list records plus `DrawEmergencyText`
  ($A6:C0AA calls $A6:C136). The last write is checked as 18 bytes from
  $A6:C164 to VRAM $50CB. After the 128-frame hold, English enters phase 10
  ($A6:C0E3-$C0F1); it must finish typing before escape starts, not skip directly
  to the final phase.

No production files changed. The complete command now passes:

`SuperMetroid.DebugRunner --ceres-ridley-audit "Super Metroid.smc"`

Observed full run: battle entry 306 frames; fireball evidence 190 frames;
runtime hurt OAM 59 entries; 100 real bomb/beam impacts; retreat 474 frames;
Mode 7 113 calls; warning setup 16 writes over 14 frames; 128-frame hold;
typewriter 174 frames. Fresh projectile lifecycle and Power Bomb subaudits also
complete. These observations are not claims of new player-visible fixes.

Build: DebugRunner Release, zero warnings/errors. Pinned local disassembly and
existing shared contact assertions were used to preserve native scheduling.
