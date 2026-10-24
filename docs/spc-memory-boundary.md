# SPC700 memory boundary

The SPC700 driver exists only in the export pipeline: `ManagedSpcPlayer` and its views live in
`SuperMetroid.AssetExtraction/Audio/Synthesis` and render clips at export. The export pipeline may know the
hardware; the simulation does not.

What the runtime keeps is a **stub**: the acknowledgement timing the driver imposed on gameplay (which update a
sound is accepted on, when a command port reads back as free) is gameplay-observable, so the session advances a
`SoundAcceptanceClock` (a compatibility service, `docs/compatibility-layer.md`) every update and surfaces its outcomes
as semantic `AudioEvent`s. The clock holds requests and acknowledgements per sound library as typed state; it models
no port by number.

Within the renderer, the driver port never indexes APU RAM or uses raw port numbers in its music, sound-effect,
instrument, echo or upload logic. APU RAM addresses, byte order and 16-bit wrapping live in the typed
driver-memory views in `ManagedSpcPlayer.Memory.cs`. The game never names a port: it asks for music and sounds by
identity (`AudioRequest`, #1410), and only the renderer turns a request into a port write.

## Boundary and enforcement

- `[SpcMemoryAccess]`: the player's APU RAM array and the verification reader `ReadApuByteForVerification`. The
  attribute and `[SpcMemoryBoundary]` are declared in `SuperMetroid.AssetExtraction/Audio/Synthesis`, beside the
  driver they mark (#1410).
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
| CPU→APU / APU→CPU ports | none in the game: each `AudioRequest` names a sound library; requests, acknowledgements and the check timing owned by `SoundAcceptanceClock`, which the game session's `AudioSession` advances every update. The player receives only the music command (`WriteMusicPort`) and the tick schedule, as semantic `AudioEvent`s through `ManagedSpcAudioBackend` | $2140-$2143 |

**Alias:** pan indices from 21 read past the 21 pan samples ($1E1D + 21 = $1E32) into the echo FIR
preset table. `ReadPanVolumePair` and `ReadFirCoefficient` are separate views of those bytes.

## Checks

`--spc-memory-boundary` (in the full run) checks upload wrap across $FFFF, pointer-word wrap,
sequence cursor read-then-advance and wrap, instrument record round trip and byte order, the
pan/FIR alias, and typed port bounds. Audio-output equivalence is covered by the audio suites and
the SMV replays.
