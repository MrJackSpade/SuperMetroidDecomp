# Display scene (SME6449)

Gameplay publishes what the screen shows as typed scene state at the frame-end latch. The backend draws that scene with
shaders (#1408). The SNES encodes the same scene in PPU registers, written during vertical blank and per scanline by HDMA,
and that encoding is the hardware's, not the game's (v0.9.0 manifesto, section 2).

## Target shape

- The frame-end latch publishes a scene: camera scroll per layer, layer visibility, window geometry, blend mode per
  layer, brightness and the perspective-floor transform.
- Per-line scroll and distortion are effect descriptions: the liquid surface, heat haze, Phantoon's wave, the Power
  Bomb and X-ray shapes and the rainbow beam.
- The per-line effect phase that the cartridge runs as HDMA object pre-instructions keeps its order exactly. Some of its
  work advances the random number, and that participation is documented compatibility behaviour.
- Which update makes each scene change visible is the latch's rule, unchanged.

## The rule

`DisplayRegisterAnalyzer` reports, in the `SuperMetroid.Core` assembly only, a field or property that:

- names a display register in its identifier, as one camel-case word or a run of up to three (`Bg34Nba`, `W12Sel`,
  `M7A`): `BG1SC`..`BG4SC`, `BG12NBA`, `BG34NBA`, `BGMODE`, `TM`, `TS`, `W12SEL`, `W34SEL`, `WOBJSEL`, `WH0`..`WH3`,
  `WBGLOG`, `WOBJLOG`, `TMW`, `TSW`, `COLDATA`, `CGADSUB`, `CGWSEL`, `INIDISP`, `M7SEL`, `M7A`..`M7D`, `M7X`, `M7Y`;
- names one of those registers as an upper-case token in its documentation;
- is a byte buffer (an array, span or memory of bytes) named for registers, or declared by a type named for them.

`Bg1Scroll` is a camera value, not `BG1SC`, so it is not reported.

## Exemplar

```csharp
// Before, in Core: a shadow of the register the cartridge writes during vertical blank.
public byte Bg34Nba { get; set; }

// After: nothing. The register chose where layer 3 read its characters; layer 3 shows pre-coloured pictures (#1466),
// so the scene has no character selection to name, and Kraid's room no longer moves the HUD's characters.
```

A register shadow is first renamed for what it selects only when the selection itself survives as a gameplay or display
fact; when the selection existed only to address video memory, the shadow is deleted with its last reader.

## Allowance

There is none. A register shadow found in Core becomes the scene value it encodes. No baseline, allowlist, count,
severity change or scope change (project manifesto, section 6).
