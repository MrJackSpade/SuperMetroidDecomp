using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres room's haze: the colour it takes and how it fades in and out with the room. A waiting haze starts fading in on
/// the update the room fades in, showing its first strength on that same update; fading in, it shows each strength of its
/// fade from none upward, one an update, and on the update after its strongest it holds at full strength without changing
/// what it shows. Holding, it starts fading out on the update the room fades out and shows its first fade-out strength on
/// the next update; fading out, it shows full strength once more and then each strength downward, one an update, to its
/// faintest, and then stays there (#2573). A
/// haze shown as the room appears at once starts holding, at the strength a finished fade-in shows.
/// </summary>
[RoomLifetime]
public sealed partial class CeresHazeState
{
    /// <summary>Mutually exclusive per-frame phases of the room-owned haze.</summary>
    private enum Phase
    {
        /// <summary>Waits for the room fade-in gate without changing the published haze strength.</summary>
        Waiting,
        /// <summary>Shows each strength of the fade from none upward, one an update, rising after each.</summary>
        FadingIn,
        /// <summary>Retains full haze until the room fade-out gate selects the next phase.</summary>
        Holding,
        /// <summary>Shows full strength and then each strength downward, one an update, falling after each, then stays at the faintest.</summary>
        FadingOut,
    }
    /// <summary>Current haze phase.</summary>
    private Phase _phase;
    /// <summary>
    /// The fade running: one update a strength, its age the strength fading in shows next and its updates left the strength
    /// fading out shows next.
    /// </summary>
    private UpdateCountdown fade = UpdateCountdown.Ended();
    /// <summary>The tint the haze adds, chosen once as it starts by whether Ridley is dead; later defeats do not change it.</summary>
    public CeresHazeTint Tint { get; }
    /// <summary>The strength the haze shows, in steps of its fade: rising as it fades in, falling as it fades out, and its faintest once it has faded out.</summary>
    private int _strength;

    /// <summary>The haze's full strength, the last step of its fade.</summary>
    private const int FullStrength = CeresHazeDefinitions.FadeSteps - 1;

    /// <summary>How strongly the haze shows (#2482).</summary>
    public CeresHazeStrength Strength => new(_strength);

    /// <summary>Samples Ridley's defeat once, as the room's haze starts; only a room whose setup spawns the haze builds one.</summary>
    /// <param name="tint">The haze's tint for this owner's lifetime: red once the loaded area's boss is defeated, blue before.</param>
    /// <param name="immediateViewport">True for immediate viewport installation: starts holding at the end of its fade, showing full strength; false starts waiting at zero for the door fade-in phase.</param>
    public CeresHazeState(CeresHazeTint tint, bool immediateViewport)
    {
        Tint = tint;
        _phase = immediateViewport ? Phase.Holding : Phase.Waiting;
        _strength = immediateViewport ? CeresHazeDefinitions.FadeSteps - 1 : 0;
    }

    /// <summary>One per-frame haze step while no door fade runs.</summary>
    public void Step() => Step(RoomFade.None);

    /// <summary>
    /// One haze step while the door's destination fades in: a waiting haze starts fading in and shows its first strength on
    /// this same update; any other runs its ordinary step.
    /// </summary>
    public void StepWhileRoomFadesIn() => Step(RoomFade.FadingIn);

    /// <summary>
    /// One haze step while the door's source fades out: a holding haze starts fading out now and shows its first fade-out
    /// strength on the following update; any other runs its ordinary step.
    /// </summary>
    public void StepWhileRoomFadesOut() => Step(RoomFade.FadingOut);

    /// <summary>The door fade one haze step sees.</summary>
    private enum RoomFade
    {
        /// <summary>No door fade is running.</summary>
        None,

        /// <summary>The destination room's colours are fading in.</summary>
        FadingIn,

        /// <summary>The source room's colours are fading out.</summary>
        FadingOut,
    }

    /// <summary>What <c>Step</c> does for each <see cref="Phase"/> member, each member's own arm.</summary>
    private IdentityTable<Phase, Action<RoomFade>> StepForPhase => field ??= new(new Dictionary<Phase, Action<RoomFade>>
    {
        [Phase.FadingIn] = roomFade =>
        {
            if (fade.IsOver)
                _phase = Phase.Holding;
            else
            {
                _strength = fade.Age;
                fade.Count();
            }
        },
        [Phase.Holding] = roomFade =>
        {
            if (roomFade == RoomFade.FadingOut)
            {
                _phase = Phase.FadingOut;
                fade = UpdateCountdown.Lasting(CeresHazeDefinitions.FadeSteps);
            }
        },
        [Phase.FadingOut] = roomFade =>
        {
            if (fade.IsRunning)
            {
                // The fade-out counts down from one past full, which the haze shows as full (#2573).
                _strength = Math.Min(fade.UpdatesLeft, FullStrength);
                fade.Count();
            }
        },
        [Phase.Waiting] = roomFade =>
        {
            // The room's fade-in starts the haze's, whose first step runs on the same update.
            if (roomFade != RoomFade.FadingIn)
                return;
            _phase = Phase.FadingIn;
            fade = UpdateCountdown.Lasting(CeresHazeDefinitions.FadeSteps);
            StepForPhase[Phase.FadingIn](roomFade);
        },
    });

    /// <summary>One haze step under <paramref name="roomFade"/>.</summary>
    private void Step(RoomFade roomFade) => StepForPhase[_phase](roomFade);
}
