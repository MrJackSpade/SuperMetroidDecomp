using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Nuclear.Artwork;

/// <summary>
/// The presentation boundary of Nuclear Waffle's programs (#641): the bank-$A6 spritemap each
/// <see cref="NuclearWaffleFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class NuclearWaffleFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(NuclearWaffleFrame frame) => frame.Pose switch
    {
        0 => 0x9954,
        1 => 0x995b,
        2 => 0x9962,
        3 => 0x9969,
        4 => 0x9970,
        5 => 0x9977,
        6 => 0x997e,
        7 => 0x9985,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Nuclear Waffle pose."),
    };

    /// <summary>The installed frame <paramref name="frame"/> shows.</summary>
    internal static EnemyFrameId Frame(NuclearWaffleFrame frame) => frame switch
    {
        { Pose: 0 } => EnemyFrameId.NuclearWaffle00,
        { Pose: 1 } => EnemyFrameId.NuclearWaffle01,
        { Pose: 2 } => EnemyFrameId.NuclearWaffle03,
        { Pose: 3 } => EnemyFrameId.NuclearWaffle04,
        { Pose: 4 } => EnemyFrameId.NuclearWaffle02,
        { Pose: 5 } => EnemyFrameId.NuclearWaffle05,
        { Pose: 6 } => EnemyFrameId.NuclearWaffle06,
        { Pose: 7 } => EnemyFrameId.NuclearWaffle07,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a frame the NuclearWaffleFrame programs show."),
    };
}
