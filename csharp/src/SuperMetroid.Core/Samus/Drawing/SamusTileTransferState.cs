using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Bank-$92 Samus animation-tile selection plus bank-$80's dedicated NMI DMA path.
/// </summary>
/// <remarks>
/// Samus is not stored as a convenient sheet of finished frames. Every animation frame
/// chooses one top-half and (usually) one bottom-half seven-byte transfer definition. NMI
/// then copies each definition in two pieces into fixed OBJ character slots. Modeling that
/// indirection is essential: it lets a debugger expose the same pose, animation record,
/// graphics source, size, and VRAM destination that the cartridge uses.
/// </remarks>
public sealed class SamusTileTransferState
{
    /// <summary>Currently bound external visual presentation; never part of save state.</summary>
    [field: NonSerialized]
    public SamusBodyArtworkCatalog? Artwork { get; private set; }

    /// <summary>Rebind installed visual data without replacing pending native DMA state.</summary>
    public void BindArtwork(SamusBodyArtworkCatalog? value) => Artwork = value;

    /// <summary>The installed standard OBJ page; never part of save state.</summary>
    [field: NonSerialized]
    public SpriteArtwork? StandardObjects { get; private set; }

    /// <summary>Rebind the installed standard OBJ page.</summary>
    public void BindStandardObjects(SpriteArtwork? value) => StandardObjects = value;

    /// <summary>The body characters the selected transfers make visible; a half never selected is blank.</summary>
    public SpriteArtwork BodyArtwork => (Artwork ?? throw new InvalidOperationException(
            "Samus body artwork requires installed body artwork."))
        .BodyArtwork(TopTransferEnabled ? TopDefinitionAddress : null, BottomTransferEnabled ? BottomDefinitionAddress : null);

    /// <summary>
    /// The standard OBJ page with the body characters over its first 32, for spritemaps whose parts may name any
    /// OBJ character (the zero-pointer WRAM spritemap). Composed on each use; only that rare spritemap needs it.
    /// </summary>
    public SpriteArtwork BodyOnStandardObjects
    {
        get
        {
            SpriteArtwork page = StandardObjects ?? throw new InvalidOperationException(
                "Samus's WRAM spritemap requires the installed standard OBJ page.");
            SpriteArtwork body = BodyArtwork;
            return SpriteArtwork.Overlay(new SpriteArtworkId(SpriteArtworkKind.SamusBodyOnStandardObjects, body.Id.Variant), page, body, 0);
        }
    }

    /// <summary>Bank-$92 address of the selected seven-byte top-half DMA definition.</summary>
    public int TopDefinitionAddress { get; internal set; }

    /// <summary>Bank-$92 address of the selected seven-byte bottom-half DMA definition.</summary>
    public int BottomDefinitionAddress { get; internal set; }

    /// <summary>Low byte of WRAM <c>$071D</c>; nonzero enables top-half NMI DMA.</summary>
    public bool TopTransferEnabled { get; internal set; }

    /// <summary>Low byte of WRAM <c>$071E</c>; nonzero enables bottom-half NMI DMA.</summary>
    public bool BottomTransferEnabled { get; internal set; }

    /// <summary>
    /// Ports <c>Set_SamusTilesDefinitions_ForCurrentAnimation</c> at <c>$92:8000</c>.
    /// </summary>
    public void SelectForPoseFrame(SamusPoseId pose, int animationFrame)
    {

        SamusBodyArtworkCatalog installed = Artwork ?? throw new InvalidOperationException(
            "Samus tile transfer requires installed body artwork.");
        SamusBodyFrameSelection frame = installed.Frame(pose, animationFrame);
        TopDefinitionAddress = installed.DefinitionAddress(true, frame.TopSet, frame.TopPosition);
        TopTransferEnabled = true;
        if (frame.BottomSet != SamusRenderingRomData.TileTransfers.NoBottomTransferSet)
        {
            BottomDefinitionAddress = installed.DefinitionAddress(false, frame.BottomSet,
                frame.BottomPosition);
            BottomTransferEnabled = true;
        }
    }

    /// <summary>
    /// Ports <c>TransferSamusTilesToVRAM</c> at <c>$80:9376</c> for one accepted NMI.
    /// </summary>
    public void TransferToVram(SnesVram vram)
    {
        Ensure.NotNull(vram);

        // Samus has a dedicated DMA path instead of using the ordinary seven-byte VRAM
        // queue. The four destinations form two interleaved character regions selected by
        // the pose spritemaps' tile numbers under gameplay OBSEL=$03.
        SamusBodyArtworkCatalog installed = Artwork ?? throw new InvalidOperationException(
            "Samus tile transfer requires installed body artwork.");
        var samusVram = new SamusVram(vram);
        if (TopTransferEnabled)
            ExecuteInstalledDefinition(installed.DefinitionAt(TopDefinitionAddress),
                samusVram.BodyTopFirst, samusVram.BodyTopSecond);
        if (BottomTransferEnabled)
            ExecuteInstalledDefinition(installed.DefinitionAt(BottomDefinitionAddress),
                samusVram.BodyBottomFirst, samusVram.BodyBottomSecond);

        // These flags are intentionally not cleared. The original NMI routine leaves them
        // set, and Samus_Draw refreshes the selected definitions during each main-loop pass.
    }

    /// <summary>Splits one installed planar definition across Samus's paired dedicated OBJ VRAM regions.</summary>
    private static void ExecuteInstalledDefinition(SamusBodyTileDefinition definition,
        VramRegion first, VramRegion second)
    {
        ReadOnlySpan<byte> planar = definition.Planar.Span;
        first.Load(planar[..definition.FirstSize]);
        if (definition.SecondSize != 0)
            second.Load(planar[definition.FirstSize..]);
    }

}
