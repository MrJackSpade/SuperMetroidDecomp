using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Which pictures Samus's body shows: the top-half and bottom-half definitions the current pose and animation frame
/// select, and the installed artwork they are drawn from.
/// </summary>
/// <remarks>
/// Every animation frame names one top-half definition, and most name one bottom-half definition too. A frame that
/// names no bottom half keeps showing the bottom half an earlier frame selected (the suited forward pose's frames do
/// this), and before any frame has named one the bottom half is blank. That retained-bottom rule is gameplay-visible
/// presentation the body sheets are composed for: each sheet frame is keyed by the pair of definitions it shows.
/// </remarks>
public sealed class SamusBodyFrameState
{
    /// <summary>Currently bound external visual presentation; never part of save state.</summary>
    [field: NonSerialized]
    public SamusBodyArtworkCatalog? Artwork { get; private set; }

    /// <summary>Rebinds the installed body artwork without changing the shown definitions.</summary>
    public void BindArtwork(SamusBodyArtworkCatalog? value) => Artwork = value;

    /// <summary>The installed pre-coloured sheets the body draws from; never part of save state.</summary>
    [field: NonSerialized]
    public InstalledSpriteSheets? Sheets { get; private set; }

    /// <summary>Rebind the installed sheets.</summary>
    public void BindSheets(InstalledSpriteSheets? value) => Sheets = value;

    /// <summary>The installed standard sprite page; never part of save state.</summary>
    [field: NonSerialized]
    public SpriteArtwork? StandardObjects { get; private set; }

    /// <summary>Rebind the installed standard sprite page.</summary>
    public void BindStandardObjects(SpriteArtwork? value) => StandardObjects = value;

    /// <summary>
    /// The definition records whose pictures the body shows: the top half the latest selection named, and the bottom
    /// half the latest selection that named one chose, or null for a half no selection has named yet.
    /// </summary>
    public SamusBodyRecords Shown { get; private set; }

    /// <summary>
    /// Selects the pictures of <paramref name="animationFrame"/> of <paramref name="pose"/>, porting
    /// <c>Set_SamusTilesDefinitions_ForCurrentAnimation</c>: the top half always changes, and the bottom half changes
    /// only when the frame names one, otherwise keeping the bottom half already shown.
    /// </summary>
    public void Select(SamusPoseId pose, int animationFrame)
    {
        SamusBodyArtworkCatalog installed = Artwork ?? throw new InvalidOperationException(
            "Samus body frame selection requires installed body artwork.");
        SamusBodyFrameSelection frame = installed.Frame(pose, animationFrame);
        int top = installed.DefinitionRecord(SamusBodyHalf.Top, frame.TopSet, frame.TopPosition);
        int? bottom = frame.NamesBottom
            ? installed.DefinitionRecord(SamusBodyHalf.Bottom, frame.BottomSet, frame.BottomPosition)
            : Shown.Bottom;
        Shown = new SamusBodyRecords(top, bottom);
    }
}
