using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The lava rises of the Speed Booster escape, in the order Samus reaches them while escaping left.</summary>
internal static partial class SpeedBoosterEscapeStageDefinitions
{
    /// <summary>Eastern checkpoint action, reached first while escaping left.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition EasternCheckpoint = new(1835, 447, SignedEightEight.FromPixelFraction(-11, 16));

    /// <summary>Central checkpoint action, reached after the eastern rise.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition CentralCheckpoint = new(1290, 359, SignedEightEight.FromPixelFraction(-7, 8));

    /// <summary>Western checkpoint action, the final rise before completion.</summary>
    internal static readonly SpeedBoosterEscapeStageDefinition WesternCheckpoint = new(580, 256, SignedEightEight.FromPixelFraction(-7, 8));

    /// <summary>
    /// The escape's rises in the order they trigger; the escape is over once Samus has passed the last
    /// (<see cref="SpeedBoosterEscapeObject.IsEscaped"/>).
    /// </summary>
    internal static readonly ImmutableArray<SpeedBoosterEscapeStageDefinition> Stages =
        [EasternCheckpoint, CentralCheckpoint, WesternCheckpoint];
}
