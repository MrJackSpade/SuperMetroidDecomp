using SuperMetroid.Core.Assets;

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
    /// <summary>Installed credits rows; this reference is rebound after debugger state restoration.</summary>
    [NonSerialized] private CreditsPresentation? presentation;
    /// <summary>Thirty-two-row circular BG1 staging map updated one source row at a time.</summary>
    internal readonly ushort[] tilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];
    /// <summary>Fractional half-pixel scroll accumulated between whole-pixel advances.</summary>
    private ushort scrollSubposition;
    /// <summary>Last whole scroll position that caused a source row to be copied.</summary>
    private ushort previousCopiedScroll;
    /// <summary>Next circular tilemap row to overwrite.</summary>
    internal int destinationRow;
    /// <summary>Next installed credits row to consume.</summary>
    private int sourceRow;

    /// <summary>Initializes the scrolling object with installed text and a blank BG1 staging map.</summary>
    public CreditsObjectState(CreditsPresentation presentation)
    {
        this.presentation = Ensure.NotNull(presentation);
        Array.Fill(tilemap, EndingCreditsRomData.Rendering.BlankTile);
    }

    /// <summary>Whether the credits object still advances and may copy source rows.</summary>
    public bool Enabled { get; private set; } = true;
    /// <summary>Set when scrolling reaches the end of installed credits content.</summary>
    public bool Finished { get; private set; }
    /// <summary>Whole-pixel component of the native 16.16 credits scroll position.</summary>
    public ushort VerticalScroll { get; private set; }

    /// <summary>Reattaches installed row content after restoring serialized scrolling state.</summary>
    public void BindPresentation(CreditsPresentation? value) => presentation = value;

    /// <summary>Runs one <c>CreditsObject_Process</c> call.</summary>
    public CreditsObjectStepResult Step()
    {
        if (!Enabled)
            return new CreditsObjectStepResult(false, Finished);

        // AddToHiLo($198F,$198D,$00008000): half a pixel per accepted frame.
        uint fixedScroll = SnesSixteenSixteen.Compose(VerticalScroll, scrollSubposition);
        fixedScroll = unchecked(
            fixedScroll + EndingCreditsRomData.Motion.CreditsScrollDelta16Point16);
        VerticalScroll = SnesFixedPosition.WholeOf(unchecked((int)fixedScroll));
        scrollSubposition = unchecked((ushort)fixedScroll);

        // A new source row is interpreted whenever the scroll advances eight whole pixels,
        // i.e. every sixteen NTSC frames. The signed modular comparison is intentional.
        if ((short)unchecked((ushort)(VerticalScroll - previousCopiedScroll - 8)) < 0)
            return new CreditsObjectStepResult(false, Finished);

        previousCopiedScroll = VerticalScroll;
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
        Ensure.NotNull(vram);
        new EndingVram(vram).CreditsTilemap.Write(tilemap);
    }

}
