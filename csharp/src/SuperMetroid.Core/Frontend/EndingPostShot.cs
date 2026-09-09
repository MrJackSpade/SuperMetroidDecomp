using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Func139–142: rotate the shot graphic, fade Samus, and stage the final logo.</summary>
internal sealed class EndingPostShot
{
    private readonly ISnesAddressSpace bus;
    private readonly ushort[] sourcePalette;
    private readonly byte[] tiles;
    private readonly byte[] map;
    private readonly byte[] font;
    private int calls;
    private int hold;
    public int Scale { get; private set; } = EndingPostShotDefinitions.InitialScale;
    public byte Angle { get; private set; }
    public int Uploads { get; private set; }
    public bool RotationFinished { get; private set; }
    public bool ReadyForWhiteFlash { get; private set; }

    public EndingPostShot(ISnesAddressSpace bus, SnesCgram cgram)
    {
        this.bus = bus;
        sourcePalette = cgram.Colors.ToArray();
        tiles = Decode(EndingPostShotDefinitions.LogoTiles);
        map = Decode(EndingPostShotDefinitions.LogoMap);
        font = Decode(EndingCreditsRomData.Assets.EndingFontCharacters);
    }

    public void Step(SnesVram vram, SnesCgram cgram)
    {
        if (ReadyForWhiteFlash) throw new InvalidOperationException("Post-shot white-flash handoff must be consumed.");
        calls++;
        ApplyFade(cgram, EndingPostShotDefinitions.ShootingPaletteStart,
            Math.Min(calls, EndingPostShotDefinitions.FadeFrames));
        int samusFade = Math.Clamp(calls - EndingPostShotDefinitions.SamusFadeDelay, 0,
            EndingPostShotDefinitions.FadeFrames);
        ApplyFade(cgram, EndingPostShotDefinitions.SamusPaletteStart, samusFade);
        if (!RotationFinished)
        {
            Angle = unchecked((byte)(Angle - EndingPostShotDefinitions.AngleStep));
            Scale -= EndingPostShotDefinitions.ScaleStep;
            if (Scale < EndingPostShotDefinitions.MinimumScale)
            {
                Scale = EndingPostShotDefinitions.MinimumScale;
                RotationFinished = true;
                hold = EndingPostShotDefinitions.HoldFrames;
            }
        }
        else
        {
            // Func141 calls the fade before the upload helper: its final fade call
            // permits the first sheet replacement on that very same frame.
            if (samusFade == EndingPostShotDefinitions.FadeFrames && Uploads < EndingPostShotDefinitions.UploadCount)
                Upload(vram, Uploads++);
            ReadyForWhiteFlash = --hold == 0;
        }
    }

    private void ApplyFade(SnesCgram cgram, int start, int elapsed)
    {
        int remaining = EndingPostShotDefinitions.FadeFrames - elapsed;
        for (int index = start; index < start + EndingPostShotDefinitions.PaletteColors; index++)
        {
            ushort source = sourcePalette[index];
            // The native 8.8 accumulator subtracts component<<3 on every call;
            // taking the high byte is exactly floor(component * remaining / 32).
            int red = (source & 31) * remaining / EndingPostShotDefinitions.FadeFrames;
            int green = (source >> 5 & 31) * remaining / EndingPostShotDefinitions.FadeFrames;
            int blue = (source >> 10 & 31) * remaining / EndingPostShotDefinitions.FadeFrames;
            cgram.SetColor(index, (ushort)(red | green << 5 | blue << 10));
        }
    }

    private void Upload(SnesVram vram, int index)
    {
        int entry = EndingPostShotDefinitions.UploadTable + index * EndingPostShotDefinitions.UploadRecordBytes;
        int length = RomDataReader.ReadWordFixedBank(bus, entry);
        int source = RomDataReader.ReadWordFixedBank(bus, entry + 2) | bus.ReadByte(entry + 4) << 16;
        int destination = RomDataReader.ReadWordFixedBank(bus, entry + 6);
        ReadOnlySpan<byte> bytes;
        if (source == EndingPostShotDefinitions.SubtitleSource)
            bytes = font.AsSpan(EndingPostShotDefinitions.SubtitleFontOffset, length);
        else if (source >= EndingPostShotDefinitions.LogoTileSource && source < EndingPostShotDefinitions.LogoMapSource)
            bytes = tiles.AsSpan(source - EndingPostShotDefinitions.LogoTileSource, length);
        else if (source == EndingPostShotDefinitions.LogoMapSource)
            bytes = map.AsSpan(0, length);
        else
            throw new InvalidDataException($"Unmapped post-shot graphics source ${source:X6}.");
        vram.LoadBytes(destination * sizeof(ushort), bytes);
    }

    private byte[] Decode(int address) => RomDataReader.Decompress(bus, address,
        EndingCreditsRomData.Rendering.DecompressionLimit);
}
