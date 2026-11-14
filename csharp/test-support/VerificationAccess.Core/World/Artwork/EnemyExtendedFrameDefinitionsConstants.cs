using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Places in the <see cref="EnemyExtendedFrameDefinitions"/> document that only verification reads.</summary>
internal static class EnemyExtendedFrameDefinitionsConstants
{
    /// <summary>Frames the document lists before the Ceres steam extended frames.</summary>
    internal static int PreCeresSteamFrameCount => EnemyExtendedFrameToolingDefinitions.FramesBefore(EnemyFrameId.CeresSteamUp);
}
