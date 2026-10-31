namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Native pogo launch velocities and asymmetric accelerations, not presentation data.</summary>
public static class RidleyPogoDefinitions
{
    /// <summary>
    /// $A6:B94D..B9D4 SetRidleyPogoSpeeds, by pattern then stage: pattern selects four increasing launch magnitudes;
    /// stage selects acceleration and bounce height. The first two stages remain addressable although ordinary health
    /// selects stages two to five. Each faster pattern adds 1/8 pixel per frame horizontally; the ordinary four health
    /// stages add 1/32 per stage, and the two unused lead-in stages begin below that progression. Later bounce stages
    /// use a one-pixel rise per stage and uniform pattern increments; earlier stages add a larger initial jump between
    /// the slowest and other patterns, and stage three also raises the two fastest patterns.
    /// </summary>
    internal static readonly RidleyPogoDefinition[][] Speeds =
    [
        [
            Pogo(88, 416, 10, 16),
            Pogo(112, 544, 16, 32),
            Pogo(160, 800, 32, 64),
            Pogo(168, 992, 48, 128),
            Pogo(176, 1408, 64, 1024),
            Pogo(184, 1664, 80, 1280),
        ],
        [
            Pogo(120, 512, 10, 16),
            Pogo(144, 640, 16, 32),
            Pogo(192, 896, 32, 64),
            Pogo(200, 1088, 48, 128),
            Pogo(208, 1440, 64, 1024),
            Pogo(216, 1696, 80, 1280),
        ],
        [
            Pogo(152, 544, 10, 16),
            Pogo(176, 672, 16, 32),
            Pogo(224, 928, 32, 64),
            Pogo(232, 1152, 48, 128),
            Pogo(240, 1472, 64, 1024),
            Pogo(248, 1728, 80, 1280),
        ],
        [
            Pogo(184, 576, 10, 16),
            Pogo(208, 704, 16, 32),
            Pogo(256, 960, 32, 64),
            Pogo(264, 1184, 48, 128),
            Pogo(272, 1504, 64, 1024),
            Pogo(280, 1760, 80, 1280),
        ],
    ];

    /// <summary>One launch: its rightward speed and upward speed, then its upward and downward accelerations.</summary>
    private static RidleyPogoDefinition Pogo(int horizontal, int upwardSpeed, int upward, int downward) =>
        new(new EightEightVector<RoomPixels>(SignedEightEight.Wrapped(horizontal), SignedEightEight.Wrapped(-upwardSpeed)),
            upward, downward);
}
