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
    /// <summary>Stores presentation.</summary>
    [NonSerialized] private CreditsPresentation? presentation;
    /// <summary>Stores tilemap.</summary>
    private readonly ushort[] tilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];
    /// <summary>Stores scroll subposition.</summary>
    private ushort scrollSubposition;
    /// <summary>Stores previous copied scroll.</summary>
    private ushort previousCopiedScroll;
    /// <summary>Stores destination row.</summary>
    private int destinationRow;
    /// <summary>Stores source row.</summary>
    private int sourceRow;

    /// <summary>Initializes a new credits object state instance.</summary>
    public CreditsObjectState(CreditsPresentation presentation)
    {
        this.presentation = Ensure.NotNull(presentation);
        Array.Fill(tilemap, EndingCreditsRomData.Rendering.BlankTile);
    }

    /// <summary>Gets or sets enabled.</summary>
    public bool Enabled { get; private set; } = true;
    /// <summary>Gets or sets finished.</summary>
    public bool Finished { get; private set; }
    /// <summary>Gets or sets vertical scroll.</summary>
    public ushort VerticalScroll { get; private set; }

    /// <summary>Bind presentation.</summary>
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

/// <summary>Represents credits object step result.</summary>
internal readonly record struct CreditsObjectStepResult(
    bool CopiedRow,
    bool Finished);
