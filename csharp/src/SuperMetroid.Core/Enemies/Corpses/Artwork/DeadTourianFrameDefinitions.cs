using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>
/// The presentation boundary of the dead Sidehopper's and Tourian corpses' programs (#641): the bank-$A9 spritemap each
/// <see cref="DeadTourianFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class DeadTourianFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(DeadTourianFrame frame) => frame.Pose switch
    {
        0 => 0xed25,
        1 => 0xed4f,
        2 => 0xed79,
        3 => 0xed85,
        4 => 0xed91,
        5 => 0xed9d,
        6 => 0xeda9,
        7 => 0xedb5,
        8 => 0xedcb,
        9 => 0xede1,
        10 => 0xee3c,
        11 => 0xee61,
        12 => 0xee86,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined dead Tourian enemy pose."),
    };

    /// <summary>The installed frame each <see cref="DeadTourianFrame"/> shows.</summary>
    internal static EnemyFrameTable<DeadTourianFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.DeadSidehopperCorpse },
        { new(Pose: 1), EnemyFrameId.DeadSidehopperInitiallyDead },
        { new(Pose: 10), EnemyFrameId.DeadSidehopperHopBody },
        { new(Pose: 11), EnemyFrameId.DeadSidehopperHopAlt },
        { new(Pose: 12), EnemyFrameId.DeadSidehopperIdle },
        { new(Pose: 2), EnemyFrameId.DeadZoomerCorpse0 },
        { new(Pose: 3), EnemyFrameId.DeadZoomerCorpse2 },
        { new(Pose: 4), EnemyFrameId.DeadZoomerCorpse4 },
        { new(Pose: 5), EnemyFrameId.DeadRipperCorpse0 },
        { new(Pose: 6), EnemyFrameId.DeadRipperCorpse2 },
        { new(Pose: 7), EnemyFrameId.DeadSkreeCorpse0 },
        { new(Pose: 8), EnemyFrameId.DeadSkreeCorpse2 },
        { new(Pose: 9), EnemyFrameId.DeadSkreeCorpse4 },
    };
}
