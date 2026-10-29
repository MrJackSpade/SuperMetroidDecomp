using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// Which pictures Samus's body shows: the top-half and bottom-half pictures the current pose and animation frame
/// select, and the installed artwork they are drawn from.
/// </summary>
/// <remarks>
/// Every animation frame selects one top-half picture, and most select one bottom-half picture too. A frame that
/// selects no bottom half keeps showing the bottom half an earlier frame selected (the suited forward pose's frames do
/// this), and before any frame has selected one the bottom half is blank. That retained-bottom rule is gameplay-visible
/// presentation the body sheets are composed for: each sheet frame is keyed by the pair of pictures it shows.
/// </remarks>
public sealed class SamusBodyFrameState
{
    /// <summary>Currently bound external visual presentation; never part of save state.</summary>
    [field: NonSerialized]
    public InstalledSamusBodyArtwork? Artwork { get; private set; }

    /// <summary>Rebinds the installed body artwork without changing the shown pictures.</summary>
    public void BindArtwork(InstalledSamusBodyArtwork? value) => Artwork = value;

    /// <summary>The installed pre-coloured sheets the body draws from; never part of save state.</summary>
    [field: NonSerialized]
    public InstalledSpriteSheets? Sheets { get; private set; }

    /// <summary>Rebind the installed sheets.</summary>
    public void BindSheets(InstalledSpriteSheets? value) => Sheets = value;

    /// <summary>
    /// The half pictures the body shows: the top half the latest selection named, and the bottom half the latest
    /// selection that named one chose, or null for a half no selection has named yet.
    /// </summary>
    public SamusBodyPictures Shown { get; private set; }

    /// <summary>
    /// Selects the pictures of <paramref name="animationFrame"/> of <paramref name="pose"/>, porting
    /// <c>Set_SamusTilesDefinitions_ForCurrentAnimation</c>: the top half always changes, and the bottom half changes
    /// only when the frame names one, otherwise keeping the bottom half already shown.
    /// </summary>
    public void Select(SamusPoseId pose, int animationFrame)
    {
        InstalledSamusBodyArtwork installed = Artwork ?? throw new InvalidOperationException(
            "Samus body frame selection requires installed body artwork.");
        SamusBodyFramePictures frame = installed.Frame(pose, animationFrame);
        Shown = new SamusBodyPictures(frame.Top, frame.Bottom ?? Shown.Bottom);
    }
}
