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
    [NonSerialized] private CreditsPresentation? presentation;
    private readonly ushort[] tilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];
    private ushort scrollSubposition;
    private ushort previousCopiedScroll;
    private int destinationRow;
    private int sourceRow;

    public CreditsObjectState(CreditsPresentation presentation)
    {
        this.presentation = Ensure.NotNull(presentation);
        Array.Fill(tilemap, EndingCreditsRomData.Rendering.BlankTile);
    }

    public bool Enabled { get; private set; } = true;
    public bool Finished { get; private set; }
    public ushort VerticalScroll { get; private set; }

    public void BindPresentation(CreditsPresentation? value) => presentation = value;

    /// <summary>Runs one <c>CreditsObject_Process</c> call.</summary>
    public CreditsObjectStepResult Step()
    {
        if (!Enabled)
            return new CreditsObjectStepResult(false, Finished);

        // AddToHiLo($198F,$198D,$00008000): half a pixel per accepted frame.
        uint fixedScroll = ((uint)VerticalScroll << 16) | scrollSubposition;
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

internal readonly record struct CreditsObjectStepResult(
    bool CopiedRow,
    bool Finished);
