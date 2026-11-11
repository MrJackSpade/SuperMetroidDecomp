using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native words Ridley's tail state is stored in, for comparing the port's tail against snapshots and the
/// cartridge's own tables. The cartridge counts a tail angle in 256ths of a turn from 64 whole turns, so straight down
/// is $4000 and the word never wraps; a distance is its 8.8 word, and an unaimed whip target or an unset extension
/// target is parked at a sentinel.
/// </summary>
internal static class RidleyTailNativeWords
{
    /// <summary>The native word of straight down: 64 whole turns of 256 steps.</summary>
    private const int StraightDownWord = 0x4000;

    /// <summary>The native word of a tail angle.</summary>
    public static ushort Angle(RidleyTailAngle angle) => checked((ushort)(StraightDownWord + angle.StepsFromStraightDown));

    /// <summary>The tail angle a native angle word holds.</summary>
    public static RidleyTailAngle AngleOf(int word) => RidleyTailAngle.FromStepsFromStraightDown(checked((ushort)word) - StraightDownWord);

    /// <summary>The native word of a whip target: the angle's word, or $FFFF while no whip is aimed.</summary>
    public static ushort WhipTarget(RidleyTailAngle? target) => target is { } angle ? Angle(angle) : ushort.MaxValue;

    /// <summary>The native word of a segment's extension target: its 8.8 word, or zero while it is not extending.</summary>
    public static ushort TargetDistance(SignedEightEight? target) => target is { } distance ? distance.RawValue : (ushort)0;
}
