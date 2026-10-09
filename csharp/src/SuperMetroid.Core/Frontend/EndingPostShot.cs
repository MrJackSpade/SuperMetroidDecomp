using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Func139–142: rotate the shot graphic, fade Samus, and stage the final logo.</summary>
internal sealed class EndingPostShot
{
    /// <summary>Address space retained for the post-shot scene's cartridge-backed resources.</summary>
    private readonly ISnesAddressSpace bus;

    /// <summary>Palette snapshot from the scene's start, used as the source for each fade step.</summary>
    private readonly ushort[] sourcePalette;

    /// <summary>Transfer bytes for the staged logo tiles installed as the sequence advances.</summary>
    private byte[] tiles = [];

    /// <summary>Transfer bytes for the logo tilemap uploaded at the final artwork stage.</summary>
    private byte[] map = [];

    /// <summary>Subtitle font transfer retained for its scheduled VRAM upload.</summary>
    private readonly byte[] font;

    /// <summary>Number of updates processed since the post-shot sequence began.</summary>
    private int calls;

    /// <summary>Remaining hold updates after the shot rotation reaches its minimum scale.</summary>
    private int hold;

    /// <summary>Gets the current fixed-point scale of the rotating shot graphic.</summary>
    public int Scale { get; private set; } = EndingPostShotDefinitions.InitialScale;

    /// <summary>Gets the current rotation angle in the cartridge's byte-sized angle representation.</summary>
    public byte Angle { get; private set; }

    /// <summary>Gets the number of staged graphics transfers already uploaded to VRAM.</summary>
    public int Uploads { get; private set; }

    /// <summary>Gets whether rotation has reached its terminal scale and the hold phase has begun.</summary>
    public bool RotationFinished { get; private set; }

    /// <summary>Gets whether the hold phase is complete and ownership can pass to the white-flash scene.</summary>
    public bool ReadyForWhiteFlash { get; private set; }

    /// <summary>Starts the ending post-shot rotation, palette fades, and staged logo transfer sequence.</summary>
    /// <param name="bus">Address space retained by this scene for cartridge-backed resources.</param>
    /// <param name="cgram">Current color RAM, whose initial colors are saved as the fade source.</param>
    /// <param name="fontAtlas">Installed ending font containing the subtitle transfer.</param>
    /// <param name="artwork">Artwork used for the staged logo tile and map transfers, when already available.</param>
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
        EndingObjectArtworkCatalog installed = artwork ?? throw new InvalidOperationException(
            "Post-shot sequence requires installed object artwork.");
        tiles = installed.PostShotLogoTiles.Transfer.ToArray();
        map = installed.PostShotLogoMap.Transfer.ToArray();
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

    /// <summary>Advances rotation and fades, uploads scheduled graphics, and signals when the white-flash handoff is ready.</summary>
    /// <param name="vram">Video memory receiving each due artwork transfer.</param>
    /// <param name="cgram">Color RAM updated by the shooting and Samus fade sequences.</param>
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

    /// <summary>Applies a proportional fade from the captured source colors to a contiguous palette range.</summary>
    /// <param name="cgram">Color RAM receiving the faded colors.</param>
    /// <param name="start">First color index in the palette range.</param>
    /// <param name="elapsed">Elapsed fade updates, clamped by the caller to the sequence duration.</param>
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

    /// <summary>Copies one indexed subtitle, logo tile, or logo map transfer into its cartridge-defined VRAM destination.</summary>
    /// <param name="vram">Video memory receiving the transfer bytes.</param>
    /// <param name="index">Ordinal of the upload in the post-shot transfer table.</param>
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

}
