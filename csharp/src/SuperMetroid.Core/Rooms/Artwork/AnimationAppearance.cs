using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The frame an animation shows (#1406, #1466): a room's lava, a treadmill, a statue or a boss's melt. The blocks, cells
/// and words it paints have a picture for each of its frames, and for the room's load before it selects one.
/// </summary>
/// <param name="Animation">The animation.</param>
public sealed record AnimationAppearance(ArtworkAnimation Animation) : AppearanceInput
{
    /// <inheritdoc />
    public override string Token => AppearanceInputTokens.AnimationPrefix + Animation.Name;

    /// <summary>How a picture's name spells <paramref name="frame"/> showing.</summary>
    public static string ValueOf(ArtworkFrame frame) => frame.Name;

    /// <inheritdoc />
    internal override string ValueIn(RoomAppearanceView appearance) =>
        appearance.Frames.TryShown(Animation, out ArtworkFrame frame) ? ValueOf(frame) : RoomAppearanceValues.Loaded;
}
