using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's membrane pulse (#2259) as a shade sequence played through the shared animation player (#2404): its
/// eight phases in a loop, each held for its pace's updates, the pulse telling its owner as it reaches its cry phase. A
/// change of pace goes on from the phase showing, which keeps the updates it has left.
/// </summary>
internal static partial class BabyMetroidMembraneAnimations
{
    /// <summary>The pulse's phases at each pace.</summary>
    internal static AnimationSegments<BabyMetroidMembranePace, BabyMetroidMembranePulse, BabyMetroidMembraneCue> Segments { get; } = new(
        new Dictionary<BabyMetroidMembranePace, AnimationBeat<BabyMetroidMembranePulse, BabyMetroidMembraneCue>[]>
        {
            [BabyMetroidMembranePace.Calm] = Pulse(BabyMetroidFightDefinitions.CalmPulsePhaseUpdates),
            [BabyMetroidMembranePace.Feeding] = Pulse(BabyMetroidFightDefinitions.FeedingPulsePhaseUpdates),
        });

    /// <summary>The pulse at each pace, looping its phases for good.</summary>
    private static Dictionary<BabyMetroidMembranePace, Animation<BabyMetroidMembranePace>> Pulses { get; } = Enum.GetValues<BabyMetroidMembranePace>()
        .ToDictionary(pace => pace, pace => Animation<BabyMetroidMembranePace>.Looping([Segments.Whole(pace)]));

    /// <summary>The pulse at <paramref name="pace"/>.</summary>
    internal static Animation<BabyMetroidMembranePace> At(BabyMetroidMembranePace pace) => Pulses[pace];

    /// <summary>
    /// The pulse at <paramref name="pace"/> standing on <paramref name="phase"/>, which it moves on from after
    /// <paramref name="updates"/> more updates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal static AnimationPlayer<BabyMetroidMembranePace, BabyMetroidMembranePulse, BabyMetroidMembraneCue> StandingOn(
        BabyMetroidMembranePace pace, BabyMetroidMembranePulse phase, int updates)
    {
        Animation<BabyMetroidMembranePace> pulse = At(pace);
        AnimationPlayer<BabyMetroidMembranePace, BabyMetroidMembranePulse, BabyMetroidMembraneCue> player =
            AnimationPlayer<BabyMetroidMembranePace, BabyMetroidMembranePulse, BabyMetroidMembraneCue>.Starting(Segments, pulse);
        player.ShowAt(pulse, Segments.PlaceOfFrame(pulse, pace, phase), updates);
        return player;
    }

    /// <summary>The eight phases, each <paramref name="phaseUpdates"/> updates, telling the owner as the cry phase comes.</summary>
    private static AnimationBeat<BabyMetroidMembranePulse, BabyMetroidMembraneCue>[] Pulse(int phaseUpdates) =>
    [
        .. BabyMetroidMembranePulse.All.SelectMany(phase => phase == BabyMetroidMembranePulse.CryPhase
            ? new AnimationBeat<BabyMetroidMembranePulse, BabyMetroidMembraneCue>[]
            {
                new AnimationBeat<BabyMetroidMembranePulse, BabyMetroidMembraneCue>.Event(BabyMetroidMembraneCue.ReachesCryPhase),
                new AnimationBeat<BabyMetroidMembranePulse, BabyMetroidMembraneCue>.Frame(phase, phaseUpdates),
            }
            : [new AnimationBeat<BabyMetroidMembranePulse, BabyMetroidMembraneCue>.Frame(phase, phaseUpdates)]),
    ];
}
