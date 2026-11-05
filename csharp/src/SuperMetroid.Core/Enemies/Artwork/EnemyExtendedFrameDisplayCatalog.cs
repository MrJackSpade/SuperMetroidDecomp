namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed extended enemy frames as gameplay draws them (#1463, #1513): which frames are installed, which of them
/// have a sprite picture, and the frame displayed for each frame an enemy shows. Each pictured frame is one whole picture
/// on the installed enemy sheets, composed by the import pipeline from the frame's pieces; hitbox records stay
/// application-owned.
/// </summary>
public sealed class EnemyExtendedFrameDisplayCatalog
{
    /// <summary>The installed frames, the pictured frames and the display bindings.</summary>
    internal readonly EnemyExtendedFrameDisplaySelection selection;

    /// <summary>Retains the installed frames, the frames with a picture and the same-family display bindings the import pipeline validated.</summary>
    internal EnemyExtendedFrameDisplayCatalog(
        HashSet<EnemyFrameId> frames,
        HashSet<EnemyFrameId> pictured,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames) =>
        selection = new(frames, pictured, displayFrames);

    /// <summary>
    /// Whether the frame gameplay shows is displayed through an installed extended frame, and whether the displayed frame
    /// has a sprite picture: a frame drawn only on the boss body layer, and the shared empty extended frame, have none.
    /// </summary>
    internal bool TryGetDisplay(EnemyFrameId frame, out bool pictured) =>
        EnemyExtendedFrameDisplays.TryGetDisplay(this, frame, out pictured);

    /// <summary>The frame displayed for <paramref name="frame"/>, whose boss body half accompanies the displayed picture.</summary>
    internal EnemyFrameId GetDisplayFrame(EnemyFrameId frame) => EnemyExtendedFrameDisplays.GetDisplayFrame(this, frame);
}
