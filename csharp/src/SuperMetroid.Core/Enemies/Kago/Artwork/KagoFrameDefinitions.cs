using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kago.Artwork;

/// <summary>
/// The presentation boundary of Kago's programs (#641): the bank-$A8 spritemap each
/// <see cref="KagoFrame"/> pose shows, in ascending spritemap order.
/// </summary>
internal static class KagoFrameDefinitions
{

    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(KagoFrame frame) => frame.Pose switch
    {
        0 => 0xabda,
        1 => 0xabf0,
        2 => 0xac06,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kago pose."),
    };

    /// <summary>The installed frame each <see cref="KagoFrame"/> shows.</summary>
    internal static EnemyFrameTable<KagoFrame> ShownFrames { get; } = new()
    {
        { new(Pose: 0), EnemyFrameId.KagoCycle0 },
        { new(Pose: 1), EnemyFrameId.KagoCycle1 },
        { new(Pose: 2), EnemyFrameId.KagoCycle2 },
    };
}
