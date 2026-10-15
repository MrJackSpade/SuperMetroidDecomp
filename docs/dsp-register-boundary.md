# S-DSP register boundary (#631)

Audio code never addresses S-DSP registers by number. Register numbers, the sixteen-byte voice
stride, field masks and register encodings live in `SnesDspRegisterMap`, `ManagedSnesDsp` and the
typed controls in `SnesDspControls.cs`; the sound-driver port (`ManagedSpcPlayer`) and everything else
use `SnesDspControls.Global` and `SnesDspControls.Voice(index)`.

Since #1305 the S-DSP and its typed controls are part of the offline clip renderer in
`SuperMetroid.AssetExtraction/Audio/Synthesis`; nothing at runtime writes DSP registers.

## Boundary and enforcement

- `[DspRegisterAccess]`: `ManagedSnesDsp.WriteRegister`, `SnesDspRegisterMap`, `DspGlobalRegister`,
  `DspVoiceRegister`, and the verification readers `ReadRegister` / `ReadDspRegisterForVerification`.
- `[DspRegisterBoundary("reason")]`: the DSP implementation, the typed controls, the sound-effect
  ADSR native-quirk adapter, and exact register tests.
- **SME6310** (raw register access outside a boundary) and **SME6311** (raw contract implemented
  outside a boundary) are build errors, on the shared `LayoutBoundaryAnalyzer`.

## Inventory

| Register | Address | Typed member | Notes |
|---|---|---|---|
| VxVOLL / VxVOLR | $x0 / $x1 | `Voice(x).WriteVolume(side, sbyte)` | signed |
| VxPITCHL / VxPITCHH | $x2 / $x3 | `Voice(x).WritePitch(ushort)` | low byte first, as the driver writes |
| VxSRCN | $x4 | `Voice(x).WriteSource(byte)` | |
| VxADSR1 / VxADSR2 | $x5 / $x6 | `Voice(x).WriteAdsr(adsr1, adsr2)` | ADSR1 first |
| VxGAIN | $x7 | `Voice(x).WriteGain(byte)` | |
| VxENVX / VxOUTX | $x8 / $x9 | read by exact register tests | published by the DSP |
| MVOLL / MVOLR | $0C / $1C | `Global.WriteMasterVolume(side, sbyte)` | signed |
| EVOLL / EVOLR | $2C / $3C | `Global.WriteEchoVolume(side, sbyte)` | signed |
| KON | $4C | `Global.KeyOn(DspVoiceMask)` | starts voices |
| KOFF | $5C | `Global.KeyOff(DspVoiceMask)` | releases voices |
| FLG | $6C | `Global.WriteFlags(DspFlags)` | **alias:** `DspFlags.Control` (bits 5-7) and `DspFlags.NoiseRate` (bits 0-4) |
| ENDX | $7C | read by exact register tests | voice end flags |
| EFB | $0D | `Global.WriteEchoFeedback(sbyte)` | signed |
| PMON | $2D | `Global.WritePitchModulation(DspVoiceMask)` | |
| NON | $3D | `Global.WriteNoiseEnable(DspVoiceMask)` | |
| EON | $4D | `Global.WriteEchoEnable(DspVoiceMask)` | |
| DIR | $5D | `Global.WriteSourceDirectory(byte page)` | |
| ESA | $6D | `Global.WriteEchoBufferPage(byte page)` | |
| EDL | $7D | `Global.WriteEchoDelay(byte)`; `SnesDspControls.EchoDelayOf` | low four bits used |
| FIR C0-C7 | $0F + 16 × tap | `Global.WriteFirCoefficient(tap, sbyte)` | signed |

**Native quirk:** the cartridge's sound-effect ADSR update forms its address from voice × 8 rather
than the voice stride, so for odd voices it writes a different register. `ManagedSpcPlayer.WriteSoundEffectAdsr`
preserves that exactly and is the only raw-address writer outside the DSP.

## Checks

`--dsp-register-boundary` (also in the full run) writes every typed control for all eight voices
and FIR taps and checks the exact register and value, the pitch and ADSR byte order, the FLG
alias, voice/tap bounds, and that typed writes allocate nothing. Audio-output equivalence is
covered by the existing audio suites and the SMV replays.
