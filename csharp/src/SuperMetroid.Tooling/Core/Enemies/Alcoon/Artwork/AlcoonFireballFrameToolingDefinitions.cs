using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Alcoon.Artwork;

/// <summary>Development-tool members of <see cref="AlcoonFireballFrameDefinitions"/>; never linked by player hosts.</summary>
internal static class AlcoonFireballFrameToolingDefinitions
{
    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(AlcoonFireballFrame frame) => frame.Pose switch
    {
        0 => 0x8404,
        1 => 0x840b,
        2 => 0x8412,
        3 => 0x8419,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Alcoon fireball pose."),
    };

    /// <summary>The pose whose program frame is stored at bank-$86 operand <paramref name="key"/>, if one is.</summary>
    internal static bool TryFrame(ushort key, out AlcoonFireballFrame frame)
    {
        for (int pose = 0; pose < AlcoonFireballFrameDefinitions.PoseCount; pose++)
        {
            frame = new(pose);
            if (EnemyProjectileProgramFrameSources.Operand(AlcoonFireballFrameDefinitions.ShownProjectileFrames[frame]) == key)
                return true;
        }
        frame = default;
        return false;
    }
}
