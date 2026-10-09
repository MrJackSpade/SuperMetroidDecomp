using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The single scrolling credits object at <c>$8B:9932-$8B:99FD</c>.
/// </summary>
/// <remarks>
/// Extraction compiles the cartridge's row program into immutable installed content. The
/// runtime still uses the native half-pixel cadence and circular 32-row staging buffer, but
/// never reads cartridge text or bank-$8C instructions.
/// </remarks>
internal sealed class CreditsObjectState
{
    /// <summary>Installed credits content that supplies each successive row for the tilemap.</summary>
    [NonSerialized] private CreditsPresentation? presentation;
    /// <summary>32-row circular BG1 staging tilemap populated as the credits scroll advances.</summary>
    private readonly ushort[] tilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];
    /// <summary>Whole-pixel component of the native half-pixel scroll position.</summary>
    private ushort scrollWhole;
    /// <summary>Fractional component paired with <see cref="scrollWhole"/> in the 16.16 scroll accumulator.</summary>
    private ushort scrollSubposition;
    /// <summary>Whole scroll position at which the most recent source row was copied.</summary>
    private ushort previousCopiedScroll;
    /// <summary>Next circular tilemap row to receive decoded credits text.</summary>
    private int destinationRow;
    /// <summary>Next source row index requested from the installed credits presentation.</summary>
    private int sourceRow;
    /// <summary>Whether the credits object's per-frame scroll and row-copy processing is active.</summary>
    private bool enabled = true;

    /// <summary>Creates the active credits scroller and initializes its staging tilemap with blank tiles.</summary>
    /// <param name="presentation">Installed immutable credits rows consumed as the camera scrolls.</param>
    public CreditsObjectState(CreditsPresentation presentation)
    {
        this.presentation = presentation ?? throw new ArgumentNullException(nameof(presentation));
        Array.Fill(tilemap, EndingCreditsRomData.Rendering.BlankTile);
    }

    /// <summary>Whether this object continues advancing and copying credit rows.</summary>
    public bool Enabled => enabled;
    /// <summary>Whether every source row has been consumed and the scroller has stopped.</summary>
    public bool Finished { get; private set; }
    /// <summary>Current whole-pixel vertical scroll value; fractional half-pixel progress is retained separately.</summary>
    public ushort VerticalScroll => scrollWhole;

    /// <summary>Replaces the installed row source, typically when state restoration rebinds presentation assets.</summary>
    /// <param name="value">Presentation supplying future rows, or <see langword="null"/> until assets are rebound.</param>
    public void BindPresentation(CreditsPresentation? value) => presentation = value;

    /// <summary>Runs one <c>CreditsObject_Process</c> call.</summary>
    public CreditsObjectStepResult Step()
    {
        if (!Enabled)
            return new CreditsObjectStepResult(false, Finished);

        // AddToHiLo($198F,$198D,$00008000): half a pixel per accepted frame.
        uint fixedScroll = ((uint)scrollWhole << 16) | scrollSubposition;
        fixedScroll = unchecked(
            fixedScroll + EndingCreditsRomData.Motion.CreditsScrollDelta16Point16);
        scrollWhole = unchecked((ushort)(fixedScroll >> 16));
        scrollSubposition = unchecked((ushort)fixedScroll);

        // A new source row is interpreted whenever the scroll advances eight whole pixels,
        // i.e. every sixteen NTSC frames. The signed modular comparison is intentional.
        if ((short)unchecked((ushort)(scrollWhole - previousCopiedScroll - 8)) < 0)
            return new CreditsObjectStepResult(false, Finished);

        previousCopiedScroll = scrollWhole;
        CreditsPresentation content = presentation ?? throw new InvalidOperationException(
            "Scrolling credits require installed ending-credits.json content.");
        bool copied;
        if (sourceRow >= content.RowCount)
        {
            Finished = true;
            enabled = false;
            copied = false;
        }
        else
        {
            content.GetRow(sourceRow++).CopyTo(
                tilemap.AsSpan(destinationRow * EndingCreditsRomData.Rendering.TilemapWidth,
                    EndingCreditsRomData.Rendering.TilemapWidth));
            destinationRow = (destinationRow + 1) &
                (EndingCreditsRomData.Rendering.TilemapHeight - 1);
            copied = true;
        }
        return new CreditsObjectStepResult(copied, Finished);
    }

    /// <summary>Uploads the live circular tilemap to native BG1 base word $4800.</summary>
    public void UploadTilemap(SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        vram.ExecuteWordTransfer(
            tilemap,
            EndingCreditsRomData.Rendering.CreditsTilemapWord,
            wordIncrement: 1);
    }

}

/// <summary>Reports the work and completion status produced by one credits-object update.</summary>
/// <param name="CopiedRow">Whether this update copied the next source row into the circular tilemap.</param>
/// <param name="Finished">Whether no source rows remain and the object has disabled itself.</param>
internal readonly record struct CreditsObjectStepResult(
    bool CopiedRow,
    bool Finished);
