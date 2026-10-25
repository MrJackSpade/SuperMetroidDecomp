using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Zebetites' typed health-tier programs (#641).</summary>
internal static class ZebetiteAnimations
{
    /// <summary>The health-tier programs in the order their frames are numbered.</summary>
    private static readonly IdentitySequence<ZebetiteProgram> FrameOrder = new(
        ZebetiteProgram.LargeAtLeast800, ZebetiteProgram.LargeAtLeast600, ZebetiteProgram.LargeAtLeast400,
        ZebetiteProgram.LargeAtLeast200, ZebetiteProgram.LargeBelow200, ZebetiteProgram.SplitAtLeast800,
        ZebetiteProgram.SplitAtLeast600, ZebetiteProgram.SplitAtLeast400, ZebetiteProgram.SplitAtLeast200,
        ZebetiteProgram.SplitBelow200);

    /// <summary>Every health-tier program: its pose for one update, then the native sleep.</summary>
    internal static AnimationProgramSet<ZebetiteProgram, ZebetiteFrame, NoAnimationCue> Programs { get; } = new(
        Enum.GetValues<ZebetiteProgram>().ToDictionary(program => program, program =>
            new[]
            {
                AnimationProgramStep<ZebetiteProgram, ZebetiteFrame, NoAnimationCue>.Show(new ZebetiteFrame(FrameOrder.PositionOf(program)), 1),
                AnimationProgramStep<ZebetiteProgram, ZebetiteFrame, NoAnimationCue>.Hold,
            }));

    /// <summary>
    /// The program for a barrier's form and health: large or split, then one tier for each 200 health below 1000,
    /// the last covering everything under 200.
    /// </summary>
    internal static ZebetiteProgram HealthProgram(bool linkedPair, int health)
    {
        int tier = health < 200 ? 4 :
            health < 400 ? 3 :
            health < 600 ? 2 :
            health < 800 ? 1 : 0;
        return (linkedPair ? ZebetiteProgram.SplitAtLeast800 : ZebetiteProgram.LargeAtLeast800) + tier;
    }
}
