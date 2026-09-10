# Upward Blue Brinstar elevator top-edge reproduction (#516)

Affected version: **0.1.1**. Reproduced, not fixed; no validation label yet.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --elevator-top-edge-audit 'Super Metroid.smc' csharp/test-temp/elevator-top-516
```

The fixture loads the retail Morph Ball room, equips Morph Ball and five
missiles to select the return state, positions Samus on the actual elevator,
and uses Up inputs to ride until the real door collision publishes the Blue
Brinstar elevator destination. It then runs the production door coroutine,
arrival movement, and thirty settled frames. It does not publish a fabricated
door at the elevator's resting position.

The 231-frame trace captures the transition and arrival. On frame 96 Samus's
world Y is 292, camera/display Y are both zero, and her boots are visibly drawn
near the top of the room, below the HUD. `frame-0096.png` was visually inspected.
This is the reported symptom, not an inference from an endpoint assertion.

The regression blackens Samus's dedicated OBJ palette in an immutable copy of
the same display packet, preserving OAM, tile memory, layer ordering and the
rest of CGRAM. It counts actual composite pixel changes in the top 80 rows
while Samus's center is at least 256 pixels below the viewport origin. It
currently **fails**, with 186 pixel-frame differences, first at frame 96.
The check does not alter gameplay or infer failure from camera alignment alone.

Native source review: `DrawSamusSpritemap` ($81:89AE) writes low-byte Y without
the generic actor renderer's vertical-wrap hiding rule. Therefore simply adding
that hiding rule to Samus's renderer is not justified. The upward transition's
camera offset, final nudge, and arrival timing must be compared against original
CPU execution before changing production code. A stable managed camera at zero
does not prove that zero is the correct cartridge camera position.

Local `csharp/test-temp/elevator-top-516-pixels` preserves the CSV and captured
frames, including the visible feet (96), a later arrival frame (120), and
settled state (224). Screenshots were not committed: GitHub reports the current
repository as public, so publication needs separate confirmation. The Release
build passes; the diagnostic's intentional failure remains visible in the CLI.
No gameplay fix or issue closure is claimed.
