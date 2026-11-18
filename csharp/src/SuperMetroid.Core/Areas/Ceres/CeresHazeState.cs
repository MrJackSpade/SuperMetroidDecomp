using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres room's haze: the colour it takes and how it fades in and out with the room. A waiting haze starts fading in on
/// the update the room fades in, showing its first strength on that same update; fading in, it shows strengths 0 through
/// 15, one an update, and on the update after 15 it holds at full strength without changing what it shows. Holding, it
/// starts fading out on the update the room fades out and shows its first fade-out strength on the next update; fading
/// out, it shows strengths 16 down to 1, one an update, and then stays at 1. A haze shown as the room appears at once
/// starts holding, at the strength a finished fade-in shows.
/// </summary>
[RoomLifetime]
public sealed class CeresHazeState
{
    /// <summary>Mutually exclusive per-frame phases of the room-owned haze.</summary>
    private enum Phase
    {
        /// <summary>Waits for the room fade-in gate without changing the published haze strength.</summary>
        Waiting,
        /// <summary>Shows strengths zero through fifteen, one an update, rising after each.</summary>
        FadingIn,
        /// <summary>Retains full haze until the room fade-out gate selects the next phase.</summary>
        Holding,
        /// <summary>Shows strengths sixteen down to one, one an update, falling after each, then stays at one.</summary>
        FadingOut,
    }
    /// <summary>Current haze phase, independent of its selected color channel.</summary>
    private Phase _phase;
    /// <summary>How far the haze has faded: the strength fading in shows next, or the strength fading out shows next.</summary>
    private int _counter;
    /// <summary>Channel selection sampled once when the haze starts: true chooses the Ridley-dead red ramp, false the Ridley-alive blue ramp; later defeats do not retarget it.</summary>
    public bool IsRed { get; private set; }
    /// <summary>The strength the haze shows, in steps of its fade: 0 through 15 as it fades in, 16 down to 1 as it fades out, and 1 once it has faded out.</summary>
    public int Intensity { get; private set; }

    /// <summary>Samples Ridley's defeat once, as the room's haze starts; only a room whose setup spawns the haze builds one.</summary>
    /// <param name="ridleyIsDead">Whether the loaded area's boss is defeated, selecting red rather than blue for this owner's lifetime.</param>
    /// <param name="immediateViewport">True for immediate viewport installation: starts holding at counter 16 with published strength 15; false starts waiting at zero for the door fade-in phase.</param>
    [IndependentFlag("independent facts of the loaded room: whether Ridley is defeated, and immediate viewport installation")]
    public CeresHazeState(bool ridleyIsDead, bool immediateViewport)
    {
        IsRed = ridleyIsDead;
        _phase = immediateViewport ? Phase.Holding : Phase.Waiting;
        _counter = immediateViewport ? CeresHazeDefinitions.FadeSteps : 0;
        Intensity = immediateViewport ? CeresHazeDefinitions.FadeSteps - 1 : 0;
    }

    /// <summary>One per-frame haze step while no door fade runs. Transition changes do not rewrite the selected channel.</summary>
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

    /// <summary>One haze step under <paramref name="roomFade"/>.</summary>
    private void Step(RoomFade roomFade)
    {
        if (_phase == Phase.Waiting)
        {
            if (roomFade != RoomFade.FadingIn) return;
            _phase = Phase.FadingIn;
        }
        switch (_phase)
        {
            case Phase.FadingIn:
                if (_counter == CeresHazeDefinitions.FadeSteps) _phase = Phase.Holding;
                else Intensity = _counter++;
                break;
            case Phase.Holding:
                if (roomFade == RoomFade.FadingOut) _phase = Phase.FadingOut;
                break;
            case Phase.FadingOut:
                if (_counter != 0) Intensity = _counter--;
                break;
            case Phase.Waiting:
                throw new InvalidOperationException("Waiting either returns or starts the fade-in above.");
            default:
                throw new InvalidOperationException($"Undefined haze phase {_phase}.");
        }
    }
}
