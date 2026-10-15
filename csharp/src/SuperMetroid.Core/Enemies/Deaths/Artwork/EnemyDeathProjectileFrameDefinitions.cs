namespace SuperMetroid.Core.Enemies.Deaths.Artwork;

/// <summary>
/// The presentation boundary of an enemy's death explosion and drop programs (#641): the installed artwork key of each
/// <see cref="EnemyDeathProjectileFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class EnemyDeathProjectileFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 47;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(EnemyDeathProjectileFrame frame) => frame.Pose switch
    {
        0 => 0xeca5,
        1 => 0xecb9,
        2 => 0xecd7,
        3 => 0xed4d,
        4 => 0xed51,
        5 => 0xed55,
        6 => 0xed5b,
        7 => 0xed5f,
        8 => 0xed63,
        9 => 0xed6b,
        10 => 0xed6f,
        11 => 0xed73,
        12 => 0xed79,
        13 => 0xed7d,
        14 => 0xed81,
        15 => 0xee01,
        16 => 0xee05,
        17 => 0xee09,
        18 => 0xee0d,
        19 => 0xee11,
        20 => 0xee15,
        21 => 0xee19,
        22 => 0xee1f,
        23 => 0xee23,
        24 => 0xee27,
        25 => 0xee2b,
        26 => 0xee2f,
        27 => 0xee33,
        28 => 0xee37,
        29 => 0xee3b,
        30 => 0xee3f,
        31 => 0xed8f,
        32 => 0xed93,
        33 => 0xed97,
        34 => 0xed9b,
        35 => 0xeda5,
        36 => 0xeda9,
        37 => 0xedad,
        38 => 0xedb1,
        39 => 0xedbb,
        40 => 0xedbf,
        41 => 0xeddf,
        42 => 0xede3,
        43 => 0xeded,
        44 => 0xedf1,
        45 => 0xedf5,
        46 => 0xedf9,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined enemy death and drop pose."),
    };
}
