# Grapple release pose-input reference (#338)

## Ceres haze lifecycle reference (#358)

`audit.exe "Super Metroid.smc" ceres-haze` executes the bank-$88 blue/red
pre-instructions using the HDMA dispatcher's eight-bit index convention. Both
branches wait for door function `$E737`, write sixteen progressively brighter
tables, retain the selected channel in their hold phase, and enter fade-out on
door function `$E2DB`. The completed sixteen-byte table contains channel bits
ORed with values 0..15. The fixture tests ROM instructions, not the C# gradient.

This is diagnostic evidence, not a completed #358 fix. The managed renderer still
chooses its color from the live boss bit each frame and uses a constant gradient.
The room-spawn selector at `$88:DDC7` instead chooses the channel once. The next
implementation must preserve that room ownership and the transition-driven fade
lifecycle in both legacy rendering and captured display packets.

## Wall-jump dust reference (#356)

`audit.exe "Super Metroid.smc" wall-jump-dust` executes cartridge routine
`$91:FA76` through the same 65816 interpreter. Ten cases cover both facing
directions, dry air, submerged water/lava, water's interaction-disable option,
and exact surface equality. Native slot three receives type/frame `$0600`,
timer 3, X six pixels behind Samus, and the final occupied foot pixel. A
suppressed spawn preserves the old slot's type, timer, and coordinates.

The managed production regression covers ordinary and grapple wall jumps with
the same output words. Its ordinary launch assertion failed with an empty slot
before the missing pose-entry producer was added. Existing atmospheric rendering
owns the type-six sprite animation; no separate particle renderer is introduced.

From an x64 Visual Studio Native Tools command prompt in the repository root:

```bat
csharp\native\GrapplePoseAudit\build.cmd
csharp\native\GrapplePoseAudit\audit.exe "Super Metroid.smc"
```

This console-only fixture executes original `$91:8000` ROM bytes through the
upstream 65816 interpreter. It seeds the ordinary airborne pose/input state
observed after the first grapple release, with retail controller bindings.
It does not substitute a C implementation of the game's input handler.

Verified output:

```text
Native released-right held aim: prospective pose=0069
Native released-left held aim: prospective pose=006A
```

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Interpreter: upstream-sm `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.

## Report reproduction

The unmodified basic attract demo is set 0, scene 4, room `$AFFB`, 444 frames.
`DebugRunner --grapple-demo-trace "Super Metroid.smc"` traces this and the
advanced demo using their own normal runtime input handoff. Before the fix:

- Frame 162 releases the first grapple.
- Frame 163 installs forward-jump pose `$51`.
- Frame 164 holds Right + aim-up (`$0110`), but remains `$51` instead of `$69`.
- Frame 168 fires the next grapple horizontally and misses the ceiling anchor.

The runtime discarded a valid pending pose transition whenever grapple owned
beta movement. Native `$90:946E` release movement remains independent of normal
alpha pose input: `$91:806E` still performs the ordinary jump transition lookup.
Release cleanup `$9B:CB8B` queues the initial jump body; it does not lock that
pose throughout the subsequent flight.

The shared runtime now retains ordinary pose input during release flight, while
still discarding lookups sampled from a grapple body that was replaced during
connection/cleanup. No demo timing, input, position, or physics constant changed.

The managed regression first failed at frame 164 (`$51` instead of `$69`). After
the correction, it asserts that pose, the second shot's diagonal direction,
all six ceiling attachments in room-data order, and final release beyond the
last anchor. Full native execution here proves the **input-handler expectation**,
not a frame-by-frame native reference for the entire demo or swing physics.
