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
            Pogo(0x58, 0x1a0, 0xa, 0x10),
            Pogo(0x70, 0x220, 0x10, 0x20),
            Pogo(0xa0, 0x320, 0x20, 0x40),
            Pogo(0xa8, 0x3e0, 0x30, 0x80),
            Pogo(0xb0, 0x580, 0x40, 0x400),
            Pogo(0xb8, 0x680, 0x50, 0x500),
        ],
        [
            Pogo(0x78, 0x200, 0xa, 0x10),
            Pogo(0x90, 0x280, 0x10, 0x20),
            Pogo(0xc0, 0x380, 0x20, 0x40),
            Pogo(0xc8, 0x440, 0x30, 0x80),
            Pogo(0xd0, 0x5a0, 0x40, 0x400),
            Pogo(0xd8, 0x6a0, 0x50, 0x500),
        ],
        [
            Pogo(0x98, 0x220, 0xa, 0x10),
            Pogo(0xb0, 0x2a0, 0x10, 0x20),
            Pogo(0xe0, 0x3a0, 0x20, 0x40),
            Pogo(0xe8, 0x480, 0x30, 0x80),
            Pogo(0xf0, 0x5c0, 0x40, 0x400),
            Pogo(0xf8, 0x6c0, 0x50, 0x500),
        ],
        [
            Pogo(0xb8, 0x240, 0xa, 0x10),
            Pogo(0xd0, 0x2c0, 0x10, 0x20),
            Pogo(0x100, 0x3c0, 0x20, 0x40),
            Pogo(0x108, 0x4a0, 0x30, 0x80),
            Pogo(0x110, 0x5e0, 0x40, 0x400),
            Pogo(0x118, 0x6e0, 0x50, 0x500),
        ],
    ];

    /// <summary>One launch: its rightward speed and upward speed, then its upward and downward accelerations.</summary>
    private static RidleyPogoDefinition Pogo(int horizontal, int upwardSpeed, int upward, int downward) =>
        new(new EightEightVector<RoomPixels>(new SignedEightEight((ushort)horizontal), new SignedEightEight(unchecked((ushort)-upwardSpeed))),
            upward, downward);
}
