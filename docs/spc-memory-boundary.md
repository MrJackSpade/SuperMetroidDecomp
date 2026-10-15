# SPC700 memory boundary (#632)

The sound-driver port (`ManagedSpcPlayer`) never indexes APU RAM or uses raw port numbers in its
music, sound-effect, instrument, echo or upload logic. APU RAM addresses, byte order and 16-bit
wrapping live in the typed driver-memory views in `ManagedSpcPlayer.Memory.cs`; the CPU/APU ports
are the `ApuPort` enum end to end.

## Boundary and enforcement

- `[SpcMemoryAccess]`: the player's APU RAM array, `SpcCommandPorts`' port arrays, the stored debugger byte
  `CartridgeAudioCommand.Port`, and the verification reader `ReadApuByteForVerification`.
- `[SpcMemoryBoundary("reason")]`: the driver-memory views, the DSP's shared view of APU RAM, the
  port-command codec, and exact APU memory tests.
- **SME6320** (raw access outside a boundary) and **SME6321** (raw contract implemented outside a
  boundary) are build errors, on the shared `LayoutBoundaryAnalyzer`.

## Inventory

| Domain | View | Layout |
|---|---|---|
| Music sequence streams | `NextSequenceByte(ref cursor)`, `PeekSequenceByte(cursor)` | byte stream; cursor advances after the read and wraps at 16 bits |
| Sound-effect streams | `ReadSoundByte(channel)` | channel stream pointer + index, 16-bit wrap, index advances after the read |
| Pointer words and tables | `ReadWord(address)`, `ReadPointerEntry(table, index)` | little-endian; high byte wraps at 16 bits; two bytes per entry |
| Music top-level list, pattern tables, sound-effect library tables | `ReadPointerEntry` / `ReadWord` | as above |
| Instrument table | `ReadInstrument` / `WriteInstrument` → `SpcInstrumentRecord` | six bytes: source/noise rate, ADSR1, ADSR2, gain, pitch base **big-endian** |
| Pan volume table ($1E1D) | `ReadPanVolumePair(address)` | entry and next entry |
| Echo FIR presets ($1E32) | `ReadFirCoefficient(index)` | signed bytes |
| Uploads and compiled definitions | `WriteBytes`, `WriteByte` | 16-bit wrapping copy |
| Driver reset work regions | `ClearDriverWorkRegions()` | the regions the native reset clears |
| DSP sample/echo memory | `CreateDsp()` | the DSP shares APU RAM |
| CPU→APU / APU→CPU ports | `SpcCommandPorts.WritePort(ApuPort, byte)`, `CartridgeAudioCommand.InputPort`; latches, sound-effect acknowledgement ports and the driver timer owned by `SpcCommandPorts`, which the game session's `CartridgeAudioSession` advances every update. The player receives only the music command (`WriteMusicPort`) and the tick schedule, as semantic `AudioEvent`s through `ManagedSpcAudioBackend` | $2140-$2143 |

**Alias:** pan indices from 21 read past the 21 pan samples ($1E1D + 21 = $1E32) into the echo FIR
preset table. `ReadPanVolumePair` and `ReadFirCoefficient` are separate views of those bytes.

## Checks

`--spc-memory-boundary` (in the full run) checks upload wrap across $FFFF, pointer-word wrap,
sequence cursor read-then-advance and wrap, instrument record round trip and byte order, the
pan/FIR alias, and typed port bounds. Audio-output equivalence is covered by the audio suites and
the SMV replays.
