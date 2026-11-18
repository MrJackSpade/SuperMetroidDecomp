using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Shitroid's membrane pulse (#2259) as a shade sequence played through the shared animation player (#2404): its
/// eight phases in a loop, each held for its pace's updates, the pulse telling its owner as it reaches its cry phase. A
/// change of pace goes on from the phase showing, which keeps the updates it has left.
/// </summary>
internal static class ShitroidMembraneAnimations
{
    /// <summary>The pulse's phases at each pace.</summary>
    internal static AnimationSegments<ShitroidMembranePace, ShitroidMembranePulse, ShitroidMembraneCue> Segments { get; } = new(
        new Dictionary<ShitroidMembranePace, AnimationBeat<ShitroidMembranePulse, ShitroidMembraneCue>[]>
        {
            [ShitroidMembranePace.Calm] = Pulse(ShitroidFightDefinitions.CalmPulsePhaseUpdates),
            [ShitroidMembranePace.Feeding] = Pulse(ShitroidFightDefinitions.FeedingPulsePhaseUpdates),
        });

    /// <summary>The pulse at each pace, looping its phases for good.</summary>
    private static Dictionary<ShitroidMembranePace, Animation<ShitroidMembranePace>> Pulses { get; } = Enum.GetValues<ShitroidMembranePace>()
        .ToDictionary(pace => pace, pace => Animation<ShitroidMembranePace>.Looping([Segments.Whole(pace)]));

    /// <summary>The pulse at <paramref name="pace"/>.</summary>
    internal static Animation<ShitroidMembranePace> At(ShitroidMembranePace pace) => Pulses[pace];

    /// <summary>
    /// The pulse at <paramref name="pace"/> standing on <paramref name="phase"/>, which it moves on from after
    /// <paramref name="updates"/> more updates.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
    internal static AnimationPlayer<ShitroidMembranePace, ShitroidMembranePulse, ShitroidMembraneCue> StandingOn(
        ShitroidMembranePace pace, ShitroidMembranePulse phase, int updates)
    {
        Animation<ShitroidMembranePace> pulse = At(pace);
        AnimationPlayer<ShitroidMembranePace, ShitroidMembranePulse, ShitroidMembraneCue> player =
            AnimationPlayer<ShitroidMembranePace, ShitroidMembranePulse, ShitroidMembraneCue>.Starting(Segments, pulse);
        player.ShowAt(pulse, Segments.PlaceOfFrame(pulse, pace, phase), updates);
        return player;
    }

    /// <summary>The eight phases, each <paramref name="phaseUpdates"/> updates, telling the owner as the cry phase comes.</summary>
    private static AnimationBeat<ShitroidMembranePulse, ShitroidMembraneCue>[] Pulse(int phaseUpdates) =>
    [
        .. ShitroidMembranePulse.All.SelectMany(phase => phase == ShitroidMembranePulse.CryPhase
            ? new AnimationBeat<ShitroidMembranePulse, ShitroidMembraneCue>[]
            {
                new AnimationBeat<ShitroidMembranePulse, ShitroidMembraneCue>.Event(ShitroidMembraneCue.ReachesCryPhase),
                new AnimationBeat<ShitroidMembranePulse, ShitroidMembraneCue>.Frame(phase, phaseUpdates),
            }
            : [new AnimationBeat<ShitroidMembranePulse, ShitroidMembraneCue>.Frame(phase, phaseUpdates)]),
    ];
}
