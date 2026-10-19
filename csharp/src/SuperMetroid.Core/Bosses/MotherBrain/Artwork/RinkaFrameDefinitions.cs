using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the Rinkas' programs (#641): the bank-$A2 spritemap each
/// <see cref="RinkaFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class RinkaFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(RinkaFrame frame) => frame.Pose switch
    {
        0 => 0xba38,
        1 => 0xba3f,
        2 => 0xba46,
        3 => 0xba4d,
        4 => 0xba54,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Rinka pose."),
    };

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
