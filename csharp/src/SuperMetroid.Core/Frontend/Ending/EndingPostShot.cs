using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Func139–142: rotate the shot graphic, fade Samus, and stage the final logo.</summary>
internal sealed class EndingPostShot
{
    /// <summary>Palette captured before the shot and Samus fades begin.</summary>
    private readonly Bgr555[] sourcePalette;
    /// <summary>Installed logo tile bytes used by the timed VRAM transfers.</summary>
    private byte[] tiles = [];
    /// <summary>Installed logo tilemap bytes used by the timed VRAM transfers.</summary>
    private byte[] map = [];
    /// <summary>Subtitle font bytes transferred before the logo sheets.</summary>
    private readonly byte[] font;
    /// <summary>Post-shot update count driving the shooting and Samus fade schedules.</summary>
    private int calls;
    /// <summary>Frames remaining after rotation reaches its minimum scale.</summary>
    private int hold;
    /// <summary>Current size of the rotating shot graphic, clamped at the native minimum.</summary>
    public int Scale { get; private set; } = EndingPostShotDefinitions.InitialScale;
    /// <summary>Eight-bit rotation angle decremented on each active rotation step.</summary>
    public byte Angle { get; private set; }
    /// <summary>Number of subtitle and logo transfers already sent to VRAM.</summary>
    public int Uploads { get; private set; }
    /// <summary>Indicates that the shot graphic has reached its minimum scale.</summary>
    public bool RotationFinished { get; private set; }
    /// <summary>Signals the caller to hand control to the white-flash phase after the hold expires.</summary>
    public bool ReadyForWhiteFlash { get; private set; }

    /// <summary>Captures the starting palette and installed art for the post-shot rotation and logo transfers.</summary>
    public EndingPostShot(SnesCgram cgram, EndingFontAtlas fontAtlas,
        EndingObjectArtworkCatalog? artwork = null)
    {
        Ensure.NotNull(fontAtlas);
        sourcePalette = new CgramPalettes(cgram).Image.ToArray();
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
    [PpuLayoutBoundary("codec: post-shot upload records' VMADD destinations")]
    internal static void RebindCompletedLogoArtwork(SnesVram vram,
        EndingObjectArtworkCatalog artwork)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(artwork);
        ReadOnlySpan<byte> tiles = artwork.PostShotLogoTiles.Transfer.Span;
        ReadOnlySpan<byte> map = artwork.PostShotLogoMap.Transfer.Span;
        for (int index = 1; index < EndingPostShotUploadDefinitions.Count; index++)
        {
            EndingPostShotUploadDefinition transfer = EndingPostShotUploadDefinitions.Get(index);
            ReadOnlySpan<byte> source = transfer.Source switch
            {
                EndingPostShotUploadSource.LogoMap => map[..transfer.Length],
                EndingPostShotUploadSource.LogoTiles => tiles.Slice(
                    transfer.SourceAddress - EndingPostShotDefinitions.LogoTileSource, transfer.Length),
                EndingPostShotUploadSource.Subtitle => throw new InvalidOperationException(
                    "The subtitle transfer is not part of the installed logo."),
                _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
            };
            vram.LoadBytes(transfer.DestinationWord * sizeof(ushort), source);
        }
    }

    /// <summary>Advances the native rotation, both palette fades, timed VRAM uploads, and white-flash handoff by one update.</summary>
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

    /// <summary>Fades one CGRAM palette range from its captured colors using the native 32-step component progression.</summary>
    [PpuLayoutBoundary("codec: native ending fade over its CGRAM index range")]
    private void ApplyFade(SnesCgram cgram, int start, int elapsed)
    {
        int remaining = EndingPostShotDefinitions.FadeFrames - elapsed;
        for (int index = start; index < start + EndingPostShotDefinitions.PaletteColors; index++)
        {
            // The native 8.8 accumulator subtracts component<<3 on every call;
            // taking the high byte is exactly floor(component * remaining / 32).
            new CgramPalettes(cgram).All.Write(index, sourcePalette[index].Map((_, channel) =>
                channel * remaining / EndingPostShotDefinitions.FadeFrames));
        }
    }

    /// <summary>Sends the selected subtitle, tile, or map transfer to its native VRAM word destination.</summary>
    [PpuLayoutBoundary("codec: post-shot upload records' VMADD destinations")]
    private void Upload(SnesVram vram, int index)
    {
        EndingPostShotUploadDefinition transfer = EndingPostShotUploadDefinitions.Get(index);
        int length = transfer.Length;
        int destination = transfer.DestinationWord;
        ReadOnlySpan<byte> bytes = transfer.Source switch
        {
            EndingPostShotUploadSource.Subtitle => font.AsSpan(EndingPostShotDefinitions.SubtitleFontOffset, length),
            EndingPostShotUploadSource.LogoTiles => tiles.AsSpan(
                transfer.SourceAddress - EndingPostShotDefinitions.LogoTileSource, length),
            EndingPostShotUploadSource.LogoMap => map.AsSpan(0, length),
            _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
        };
        vram.LoadBytes(destination * sizeof(ushort), bytes);
    }

}
