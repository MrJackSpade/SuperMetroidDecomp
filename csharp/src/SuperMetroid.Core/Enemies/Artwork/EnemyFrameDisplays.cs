using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Every installed enemy frame as gameplay draws it (#1463, #1513, #1603): which frames are installed, which of them have a
/// picture on the installed enemy sheets, and the frame displayed for each frame an enemy shows. Each pictured frame is one
/// whole picture composed by the import pipeline from the frame's pieces; hitboxes stay the game's.
/// </summary>
public sealed class EnemyFrameDisplays
{
    /// <summary>Every installed frame.</summary>
    private readonly HashSet<EnemyFrameId> _frames;

    /// <summary>The installed frames with a picture on the enemy sheets.</summary>
    private readonly HashSet<EnemyFrameId> _pictured;

    /// <summary>The frame displayed for each frame an enemy shows, where the installed bindings name one.</summary>
    private readonly Dictionary<EnemyFrameId, EnemyFrameId> _displayFrames;

    /// <summary>
    /// Retains the installed frames, the frames with a picture and the display bindings the import pipeline validated.
    /// Every binding displays an installed frame (#1627): the installation is complete, so no frame falls back to
    /// displaying itself in place of the frame its binding names.
    /// </summary>
    /// <exception cref="InvalidDataException">A binding names a frame the installation does not hold.</exception>
    internal EnemyFrameDisplays(HashSet<EnemyFrameId> frames, HashSet<EnemyFrameId> pictured,
        Dictionary<EnemyFrameId, EnemyFrameId> displayFrames)
    {
        _frames = Ensure.NotNull(frames);
        _pictured = Ensure.NotNull(pictured);
        _displayFrames = Ensure.NotNull(displayFrames);
        foreach ((EnemyFrameId shown, EnemyFrameId displayed) in _displayFrames)
        {
            if (!_frames.Contains(displayed))
                throw new InvalidDataException($"The installed enemy frame binding displays enemy frame {shown} as frame {displayed}, which is not installed.");
        }
    }

    /// <summary>
    /// Whether the frame gameplay shows is displayed through an installed frame, and whether the displayed frame has a
    /// picture: the empty frame, a frame shown only on a boss body, and a frame whose pieces show nothing have none.
    /// </summary>
    internal bool TryGetDisplay(EnemyFrameId frame, out bool pictured)
    {
        // The empty frame draws nothing, so it is displayed without an installed frame.
        if (CommonEnemyEmptyFrames.DrawsNothing(frame))
        {
            pictured = false;
            return true;
        }
        EnemyFrameId displayed = GetDisplayFrame(frame);
        if (!_frames.Contains(displayed))
        {
            pictured = false;
            return false;
        }
        pictured = _pictured.Contains(displayed);
        return true;
    }

    /// <summary>The frame displayed for <paramref name="frame"/>, whose boss body half accompanies the displayed picture.</summary>
    internal EnemyFrameId GetDisplayFrame(EnemyFrameId frame) =>
        _displayFrames.TryGetValue(frame, out EnemyFrameId selected) ? selected : frame;
}
