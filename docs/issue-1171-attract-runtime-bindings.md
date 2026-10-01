# Attract-demo runtime bindings (#1171)

Affected version: `0.4.2+719e01e72669478823123e3fea2926af0691c4f1`.
The supplied session log ends in `StepAttractDemo -> SuperMetroidRuntime` with
`Gameplay requires the installed initial palette catalog` when the title's
attract sequence allocates its private gameplay owner.

## Cause and correction

Normal gameplay passed the installed palette catalog and bound all installed
room, PLM, background, actor, and projectile presentation resources. The attract
path used a separate, older construction block that omitted the initial palette
argument and most room presentation bindings. Both paths are the only explicit
`SuperMetroidRuntime` construction sites in production Core.

Both now call `CreateGameplayRuntime`. This single construction/binding path
supplies initial palettes and the complete installed-resource set. Attract mode
still creates a fresh owner, preserves incoming random state, disables host
cheats, and retains default controller/options behavior. Selected play retains
its configured options. No save or demo-progression logic changes.

## Focused confirmation

`--attract-runtime-bindings` reproduces the reported missing-palette constructor
failure, then exercises the shared allocation/binding method used by attract
loading. It checks all initial CGRAM colors, standard-object and enemy-artwork
ownership, incoming random state, independent runtime instances, disabled demo
cheats, and retained selected-game options. Runtime memory has no cartridge.

This confirms construction and resource binding, not an entire attract sequence.
Verification build and the PLM, queued VRAM DMA, and enemy visual release audit
gates pass. The issue is closed after implementation and push under the player's
explicit session workflow; player demo playback validation is not claimed.
