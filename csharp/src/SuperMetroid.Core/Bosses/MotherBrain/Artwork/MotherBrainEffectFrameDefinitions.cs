namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle programs (#641): the installed artwork key of each
/// <see cref="MotherBrainEffectFrame"/> pose, which is the bank-$86 address of the native frame's spritemap operand.
/// </summary>
internal static class MotherBrainEffectFrameDefinitions
{
    /// <summary>The poses, in installed artwork order.</summary>
    internal const int PoseCount = 62;

    /// <summary>The installed artwork key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(MotherBrainEffectFrame frame) => frame.Pose switch
    {
        0 => 0xc438,
        1 => 0xc440,
        2 => 0xc448,
        3 => 0xc450,
        4 => 0xc458,
        5 => 0xc460,
        6 => 0xc46a,
        7 => 0xc46e,
        8 => 0xc472,
        9 => 0xc476,
        10 => 0xc47a,
        11 => 0xc47e,
        12 => 0xc770,
        13 => 0xc774,
        14 => 0xc778,
        15 => 0xc77c,
        16 => 0xc780,
        17 => 0xc784,
        18 => 0xc788,
        19 => 0xc78c,
        20 => 0xc790,
        21 => 0xc82b,
        22 => 0xc82f,
        23 => 0xc833,
        24 => 0xc837,
        25 => 0xc83b,
        26 => 0xc83f,
        27 => 0xc8b2,
        28 => 0xc8b6,
        29 => 0xc8ba,
        30 => 0xc8be,
        31 => 0xc8c2,
        32 => 0xc8cc,
        33 => 0xc8e5,
        34 => 0xc8e9,
        35 => 0xc8ed,
        36 => 0xc8f1,
        37 => 0xca24,
        38 => 0xca28,
        39 => 0xca2c,
        40 => 0xca30,
        41 => 0xca34,
        42 => 0xca38,
        43 => 0xca3c,
        44 => 0xca40,
        45 => 0xcaa8,
        46 => 0xcaac,
        47 => 0xcab0,
        48 => 0xcab4,
        49 => 0xcab8,
        50 => 0xcabc,
        51 => 0xcac0,
        52 => 0xcac4,
        53 => 0xcacc,
        54 => 0xcad0,
        55 => 0xcad4,
        56 => 0xcad8,
        57 => 0xcadc,
        58 => 0xcae0,
        59 => 0xcae4,
        60 => 0xcae8,
        61 => 0xcb0f,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Mother Brain effect pose."),
    };
}
