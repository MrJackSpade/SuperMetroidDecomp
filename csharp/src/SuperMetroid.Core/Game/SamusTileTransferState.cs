using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

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
    /// <summary>Installed visual catalog supplying pose-frame selections and immutable transfer definitions.</summary>
    [NonSerialized] private SamusBodyArtworkCatalog? artwork;
    /// <summary>Currently bound external visual presentation; never part of save state.</summary>
    public SamusBodyArtworkCatalog? Artwork => artwork;

    /// <summary>Rebind installed visual data without replacing pending native DMA state.</summary>
    public void BindArtwork(SamusBodyArtworkCatalog? value) => artwork = value;

    /// <summary>Bank-$92 address of the selected seven-byte top-half DMA definition.</summary>
    public int TopDefinitionAddress { get; private set; }

    /// <summary>Bank-$92 address of the selected seven-byte bottom-half DMA definition.</summary>
    public int BottomDefinitionAddress { get; private set; }

    /// <summary>Low byte of WRAM <c>$071D</c>; nonzero enables top-half NMI DMA.</summary>
    public bool TopTransferEnabled { get; private set; }

    /// <summary>Low byte of WRAM <c>$071E</c>; nonzero enables bottom-half NMI DMA.</summary>
    public bool BottomTransferEnabled { get; private set; }

    /// <summary>
    /// Ports <c>Set_SamusTilesDefinitions_ForCurrentAnimation</c> at <c>$92:8000</c>.
    /// </summary>
    public void SelectForPoseFrame(ISnesAddressSpace bus, byte pose, ushort animationFrame)
    {
        ArgumentNullException.ThrowIfNull(bus);

        SamusBodyArtworkCatalog installed = artwork ?? throw new InvalidOperationException(
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
    public void TransferToVram(ISnesAddressSpace bus, SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);

        // Samus has a dedicated DMA path instead of using the ordinary seven-byte VRAM
        // queue. The four destinations form two interleaved character regions selected by
        // the pose spritemaps' tile numbers under gameplay OBSEL=$03.
        SamusBodyArtworkCatalog installed = artwork ?? throw new InvalidOperationException(
            "Samus tile transfer requires installed body artwork.");
        if (TopTransferEnabled)
            ExecuteInstalledDefinition(vram, installed.DefinitionAt(true, TopDefinitionAddress),
                SamusRenderingRomData.TileTransfers.TopDestinations);
        if (BottomTransferEnabled)
            ExecuteInstalledDefinition(vram, installed.DefinitionAt(false, BottomDefinitionAddress),
                SamusRenderingRomData.TileTransfers.BottomDestinations);

        // These flags are intentionally not cleared. The original NMI routine leaves them
        // set, and Samus_Draw refreshes the selected definitions during each main-loop pass.
    }

    /// <summary>Copies one installed planar tile definition to the two VRAM regions assigned to its body half.</summary>
    /// <param name="vram">VRAM receiving the definition's first and optional second byte ranges.</param>
    /// <param name="definition">Planar tile bytes and split sizes selected from the installed artwork catalog.</param>
    /// <param name="destinations">Character destinations for each split range, in VRAM word addresses.</param>
    private static void ExecuteInstalledDefinition(SnesVram vram,
        SamusBodyTileDefinition definition,
        SamusRenderingRomData.TileTransfers.SplitVramDestinations destinations)
    {
        ReadOnlySpan<byte> planar = definition.Planar.Span;
        vram.LoadBytes(destinations.First * 2, planar[..definition.FirstSize]);
        if (definition.SecondSize != 0)
            vram.LoadBytes(destinations.Second * 2, planar[definition.FirstSize..]);
    }

}
