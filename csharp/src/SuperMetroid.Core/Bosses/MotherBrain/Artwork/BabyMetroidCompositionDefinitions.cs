using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Native Baby Metroid composition identities and record geometry.</summary>
internal static class BabyMetroidCompositionDefinitions
{
    /// <summary>$A9:F9A8/FA40/FAD8: thirty five-byte records in each Baby Metroid pose, arranged as fifteen reflected pairs.</summary>
    internal const int NativePartCount = 30;
    /// <summary>The three Baby Metroid poses, Spritemap_BabyMetroid_0 to _2, each of thirty parts.</summary>
    internal static ImmutableArray<EnemyFrameId> Poses { get; } = [EnemyFrameId.Shitroid02, EnemyFrameId.Shitroid01, EnemyFrameId.Shitroid00];
}
