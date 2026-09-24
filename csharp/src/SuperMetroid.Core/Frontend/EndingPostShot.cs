using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Func139–142: rotate the shot graphic, fade Samus, and stage the final logo.</summary>
internal sealed class EndingPostShot
{
    private readonly ISnesAddressSpace bus;
    private readonly ushort[] sourcePalette;
    private byte[] tiles = [];
    private byte[] map = [];
    private readonly byte[] font;
    private int calls;
    private int hold;
    public int Scale { get; private set; } = EndingPostShotDefinitions.InitialScale;
    public byte Angle { get; private set; }
    public int Uploads { get; private set; }
    public bool RotationFinished { get; private set; }
    public bool ReadyForWhiteFlash { get; private set; }

    public EndingPostShot(ISnesAddressSpace bus, SnesCgram cgram, EndingFontAtlas fontAtlas,
        EndingObjectArtworkCatalog? artwork = null)
    {
        ArgumentNullException.ThrowIfNull(fontAtlas);
        this.bus = bus;
        sourcePalette = cgram.Colors.ToArray();
        BindArtwork(artwork);
        font = fontAtlas.Transfer.ToArray();
    }

    /// <summary>
    /// Rebinds presentation after installation changes or a debugger-state restore,
    /// without restarting the rotation, fade, hold, or six-step upload cursor.
    /// </summary>
    internal void BindArtwork(EndingObjectArtworkCatalog? artwork, SnesVram? vram = null)
    {
        tiles = artwork?.PostShotLogoTiles.Transfer.ToArray() ??
            Decode(EndingPostShotDefinitions.LogoTiles);
        map = artwork?.PostShotLogoMap.Transfer.ToArray() ??
            Decode(EndingPostShotDefinitions.LogoMap);
        if (vram is not null)
        {
            for (int index = 0; index < Uploads; index++)
                Upload(vram, index);
        }
    }

    /// <summary>
    /// Reinstalls all five logo-art transfers after the post-shot owner has
    /// handed off to the white flash or assembling-logo actors. The subtitle
    /// font transfer is unchanged and is intentionally excluded.
    /// </summary>
    internal static void RebindCompletedLogoArtwork(SnesVram vram,
        EndingObjectArtworkCatalog artwork)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(artwork);
        ReadOnlySpan<byte> tiles = artwork.PostShotLogoTiles.Transfer.Span;
        ReadOnlySpan<byte> map = artwork.PostShotLogoMap.Transfer.Span;
        for (int index = 1; index < EndingPostShotUploadDefinitions.Count; index++)
        {
            EndingPostShotUploadDefinition transfer = EndingPostShotUploadDefinitions.Get(index);
            ReadOnlySpan<byte> source = transfer.SourceAddress == EndingPostShotDefinitions.LogoMapSource
                ? map[..transfer.Length]
                : tiles.Slice(transfer.SourceAddress - EndingPostShotDefinitions.LogoTileSource,
                    transfer.Length);
            vram.LoadBytes(transfer.DestinationWord * sizeof(ushort), source);
        }
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
            if (samusFade == EndingPostShotDefinitions.FadeFrames && Uploads < EndingPostShotUploadDefinitions.Count)
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
        EndingPostShotUploadDefinition transfer = EndingPostShotUploadDefinitions.Get(index);
        int length = transfer.Length;
        int source = transfer.SourceAddress;
        int destination = transfer.DestinationWord;
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
