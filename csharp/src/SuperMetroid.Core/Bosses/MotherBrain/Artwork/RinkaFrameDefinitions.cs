using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the Rinkas' programs (#641): the bank-$A2 spritemap each
/// <see cref="RinkaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class RinkaFrameDefinitions
{

    /// <summary>The installed frame each <see cref="RinkaFrame"/> shows.</summary>
    internal static EnemyFrameTable<RinkaFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.RinkaSpinBa38 },
        { new(Pose: 1), EnemyFrameId.RinkaSpinBa3f },
        { new(Pose: 2), EnemyFrameId.RinkaSpinBa46 },
        { new(Pose: 3), EnemyFrameId.RinkaSpinBa4d },
        { new(Pose: 4), EnemyFrameId.RinkaSpinBa54 },
    };
}
