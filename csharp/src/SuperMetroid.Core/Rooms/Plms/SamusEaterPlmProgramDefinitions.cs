using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.SamusEaterProgram, SuperMetroid.Core.Rooms.Plms.SamusEaterDraw, SuperMetroid.Core.Rooms.Plms.SamusEaterCue>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The Brinstar plants' PLM programs (#1296), in place of their bank-$84 instruction lists. The floor and ceiling
/// plants run the same chew with their own artwork; the ceiling plant chews twice as many times.
/// </summary>
internal static class SamusEaterPlmProgramDefinitions
{
    /// <summary>How many chews the floor plant makes.</summary>
    internal const int FloorChews = 4;
    /// <summary>How many chews the ceiling plant makes.</summary>
    internal const int CeilingChews = 8;
    /// <summary>How long each chew frame shows.</summary>
    private const int ChewFrameDuration = 5;
    /// <summary>How long the closed mouth shows after the release.</summary>
    private const int ReleaseDuration = 96;
    /// <summary>The chew sound's native queue cap.</summary>
    internal static readonly SoundRequestPriority ChewSoundMaximumQueued = SoundRequestPriority.Normal;

    /// <summary>Every plant program.</summary>
    internal static AnimationProgramSet<SamusEaterProgram, SamusEaterDraw, SamusEaterCue> Programs { get; } = new(
        new Dictionary<SamusEaterProgram, Step[]>
        {
            [SamusEaterProgram.FloorBite] = Bite(FloorChews, SamusEaterProgram.FloorChew),
            [SamusEaterProgram.FloorChew] = Chew(SamusEaterProgram.FloorChew,
                SamusEaterDraw.FloorIdle, SamusEaterDraw.FloorChew1, SamusEaterDraw.FloorChew2, SamusEaterDraw.FloorChew3),
            [SamusEaterProgram.CeilingBite] = Bite(CeilingChews, SamusEaterProgram.CeilingChew),
            [SamusEaterProgram.CeilingChew] = Chew(SamusEaterProgram.CeilingChew,
                SamusEaterDraw.CeilingIdle, SamusEaterDraw.CeilingChew1, SamusEaterDraw.CeilingChew2, SamusEaterDraw.CeilingChew3),
        });

    /// <summary>Holds Samus and seeds the chew counter, then runs into the chew.</summary>
    private static Step[] Bite(int chews, SamusEaterProgram chew) =>
        [Step.Publish(SamusEaterCue.HoldSamus), Step.SetCounter(chews), Step.FallInto(chew)];

    /// <summary>One chew with two bites of damage, repeated while the counter runs; then the release and deletion.</summary>
    private static Step[] Chew(SamusEaterProgram self, SamusEaterDraw idle, SamusEaterDraw open, SamusEaterDraw half, SamusEaterDraw closed) =>
    [
        Step.Show(half, ChewFrameDuration),
        Step.Show(open, ChewFrameDuration),
        Step.Show(half, ChewFrameDuration),
        Step.Publish(SamusEaterCue.QueueChewSound),
        Step.Publish(SamusEaterCue.DamageSamus),
        Step.Show(closed, ChewFrameDuration),
        Step.Show(half, ChewFrameDuration),
        Step.Show(open, ChewFrameDuration),
        Step.Show(half, ChewFrameDuration),
        Step.Publish(SamusEaterCue.DamageSamus),
        Step.Show(closed, ChewFrameDuration),
        Step.RepeatWhileCounting(self),
        Step.Publish(SamusEaterCue.ReleaseSamus),
        Step.Publish(SamusEaterCue.ClearPreInstruction),
        Step.Show(closed, ReleaseDuration),
        Step.Show(idle, 1),
        Step.Stop,
    ];
}
