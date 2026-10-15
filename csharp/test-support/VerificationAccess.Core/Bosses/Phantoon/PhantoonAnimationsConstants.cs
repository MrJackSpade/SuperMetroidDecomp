using SuperMetroid.Core.Programs;
using C = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartCue;
using F = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartFrameKind;
using P = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Cartridge values and helpers of <see cref="PhantoonAnimations"/> that only verification reads.</summary>
internal static class PhantoonAnimationsConstants
{
    /// <summary>The eye program gazing in clockwise direction <paramref name="ordinal"/> from up.</summary>
    internal static PhantoonPartProgram Gaze(int ordinal) =>
        (uint)ordinal < 8
            ? P.EyeLookingUp + ordinal
            : throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Phantoon gazes in eight directions.");
}
