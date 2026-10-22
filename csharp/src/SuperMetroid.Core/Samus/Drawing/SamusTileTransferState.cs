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

    /// <summary>The installed pre-coloured sheets the body draws from; never part of save state.</summary>
    [field: NonSerialized]
    public InstalledSpriteSheets? Sheets { get; private set; }

    /// <summary>Rebind the installed sheets.</summary>
    public void BindSheets(InstalledSpriteSheets? value) => Sheets = value;

    /// <summary>The definition records whose characters the body currently shows: each half's, when selected.</summary>
    public SamusBodyRecords SelectedRecords => new(TopTransferEnabled ? TopDefinitionRecord : null,
        BottomTransferEnabled ? BottomDefinitionRecord : null);

    /// <summary>The installed standard OBJ page; never part of save state.</summary>
    [field: NonSerialized]
    public SpriteArtwork? StandardObjects { get; private set; }

    /// <summary>Rebind the installed standard OBJ page.</summary>
    public void BindStandardObjects(SpriteArtwork? value) => StandardObjects = value;

    /// <summary>Record of the selected seven-byte top-half DMA definition.</summary>
    public int TopDefinitionRecord { get; internal set; }

    /// <summary>Record of the selected seven-byte bottom-half DMA definition.</summary>
    public int BottomDefinitionRecord { get; internal set; }

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
        TopDefinitionRecord = installed.DefinitionRecord(SamusBodyHalf.Top, frame.TopSet, frame.TopPosition);
        TopTransferEnabled = true;
        if (frame.BottomSet != SamusRenderingRomData.TileTransfers.NoBottomTransferSet)
        {
            BottomDefinitionRecord = installed.DefinitionRecord(SamusBodyHalf.Bottom, frame.BottomSet,
                frame.BottomPosition);
            BottomTransferEnabled = true;
        }
    }

}
