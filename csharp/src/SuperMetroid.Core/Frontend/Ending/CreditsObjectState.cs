using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

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
    /// <summary>Installed credits rows; this reference is rebound after debugger state restoration.</summary>
    [NonSerialized] private CreditsPresentation? presentation;
    /// <summary>Thirty-two-row circular BG1 staging map updated one source row at a time.</summary>
    internal readonly ushort[] tilemap =
        new ushort[EndingScreenDefinitions.TilemapWords];
    /// <summary>Native 16.16 credits scroll; only the vertical axis moves, half a pixel per accepted update, wrapping at sixteen bits.</summary>
    private FixedPosition<BgPlanePixels> scroll;
    /// <summary>Last whole scroll position that caused a source row to be copied.</summary>
    private Position16<BgPlanePixels> previousCopiedScroll;
    /// <summary>The scroll advance, one eight-pixel tilemap row, after which the next source row is copied.</summary>
    private static readonly Offset16<BgPlanePixels> RowAdvance = new(0, 8);
    /// <summary>Next circular tilemap row to overwrite.</summary>
    internal int NextRow { get; private set; }
    /// <summary>Next installed credits row to consume.</summary>
    private int sourceRow;

    /// <summary>Initializes the scrolling object with installed text and a blank BG1 staging map.</summary>
    public CreditsObjectState(CreditsPresentation presentation)
    {
        this.presentation = Ensure.NotNull(presentation);
        Array.Fill(tilemap, EndingScreenDefinitions.BlankTile);
    }

    /// <summary>Whether the credits object still advances and may copy source rows.</summary>
    public bool Enabled { get; private set; } = true;
    /// <summary>Set when scrolling reaches the end of installed credits content.</summary>
    public bool Finished { get; private set; }
    /// <summary>Whole-pixel credits scroll position the background plane is drawn at.</summary>
    public Position16<BgPlanePixels> ScrollPosition => scroll.Whole;

    /// <summary>Reattaches installed row content after restoring serialized scrolling state.</summary>
    public void BindPresentation(CreditsPresentation? value) => presentation = value;

    /// <summary>Runs one <c>CreditsObject_Process</c> call.</summary>
    public CreditsObjectStepResult Step()
    {
        if (!Enabled)
            return new CreditsObjectStepResult(false, Finished);

        // Half a pixel per accepted frame, carrying into the whole word and wrapping at 32 bits.
        scroll = scroll.AdvanceY(EndingMotionDefinitions.CreditsScrollVelocity);

        // A new source row is interpreted whenever the scroll advances eight whole pixels,
        // i.e. every sixteen NTSC frames. The wrapped signed distance is the cartridge's test.
        if ((ScrollPosition - (previousCopiedScroll + RowAdvance)).Y < 0)
            return new CreditsObjectStepResult(false, Finished);

        previousCopiedScroll = ScrollPosition;
        CreditsPresentation content = presentation ?? throw new InvalidOperationException(
            "Scrolling credits require installed ending-credits.json content.");
        bool copied;
        if (sourceRow >= content.RowCount)
        {
            Finished = true;
            Enabled = false;
            copied = false;
        }
        else
        {
            content.GetRow(sourceRow++).CopyTo(
                tilemap.AsSpan(NextRow * EndingScreenDefinitions.TilemapWidth,
                    EndingScreenDefinitions.TilemapWidth));
            NextRow = (NextRow + 1) &
                (EndingScreenDefinitions.TilemapHeight - 1);
            copied = true;
        }
        return new CreditsObjectStepResult(copied, Finished);
    }

    /// <summary>Uploads the live circular tilemap to native BG1 base word $4800.</summary>
    public void UploadTilemap(EndingScreen screen) =>
        Ensure.NotNull(screen).Record(EndingUploadKind.CreditsTilemap, 0, tilemap);

}
