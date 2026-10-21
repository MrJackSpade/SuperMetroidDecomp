using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Nuclear.Artwork;

/// <summary>
/// The presentation boundary of Nuclear Waffle's programs (#641): the bank-$A6 spritemap each
/// <see cref="NuclearWaffleFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NuclearWaffleFrameDefinitions
{

    /// <summary>The installed frame each <see cref="NuclearWaffleFrame"/> shows.</summary>
    internal static EnemyFrameTable<NuclearWaffleFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.NuclearWaffle00 },
        { new(Pose: 1), EnemyFrameId.NuclearWaffle01 },
        { new(Pose: 2), EnemyFrameId.NuclearWaffle03 },
        { new(Pose: 3), EnemyFrameId.NuclearWaffle04 },
        { new(Pose: 4), EnemyFrameId.NuclearWaffle02 },
        { new(Pose: 5), EnemyFrameId.NuclearWaffle05 },
        { new(Pose: 6), EnemyFrameId.NuclearWaffle06 },
        { new(Pose: 7), EnemyFrameId.NuclearWaffle07 },
    };
}
