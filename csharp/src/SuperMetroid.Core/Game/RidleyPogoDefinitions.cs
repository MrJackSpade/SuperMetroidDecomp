namespace SuperMetroid.Core.Game;

/// <summary>Native pogo launch velocities and asymmetric accelerations, not presentation data.</summary>
public static class RidleyPogoDefinitions
{
    /// <summary>
    /// $A6:B94D..B9D4 SetRidleyPogoSpeeds: pattern selects four increasing launch
    /// magnitudes; stage selects acceleration and bounce height. The first two
    /// stages remain addressable although ordinary health selects stages two to five.
    /// </summary>
    public static (ushort X, ushort Y, ushort UpwardAcceleration, ushort DownwardAcceleration) Read(int pattern, int stage)
    {
        if ((uint)pattern >= 4) throw new ArgumentOutOfRangeException(nameof(pattern));
        if ((uint)stage >= 6) throw new ArgumentOutOfRangeException(nameof(stage));

        // Each faster pattern adds 1/8 pixel per frame horizontally. The ordinary
        // four health stages add 1/32 per stage; the two unused lead-in stages
        // begin below that progression.
        int horizontal = stage < 2 ? 0x58 + 0x18 * stage : 0x90 + 8 * stage;
        horizontal += 0x20 * pattern;

        // Later bounce stages use a one-pixel rise per stage and uniform pattern
        // increments. Earlier stages add a larger initial jump between the slowest
        // and other patterns; stage three also raises the two fastest patterns.
        int verticalMagnitude;
        if (stage >= 4)
            verticalMagnitude = 0x180 + 0x100 * stage + 0x20 * pattern;
        else
        {
            verticalMagnitude = stage switch
            {
                0 => 0x1a0,
                3 => 0x3e0,
                _ => 0x120 + 0x100 * stage,
            };
            verticalMagnitude += 0x20 * pattern + (pattern > 0 ? 0x40 : 0);
            if (stage == 3 && pattern >= 2) verticalMagnitude += 0x20;
        }
        int upward = stage == 0 ? 0x0a : 0x10 * stage;
        int downward = stage < 4 ? 0x10 << stage : 0x100 * stage;
        return ((ushort)horizontal, unchecked((ushort)-verticalMagnitude), (ushort)upward, (ushort)downward);
    }
}
