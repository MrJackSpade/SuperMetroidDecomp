using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Room-owned $88:DDC7/$DE10-$DED2 haze selection and direct pre-instruction cases.
/// Waiting falls through into the first fade-in write ($88:DE27..DE2D); counter16
/// selects holding without writing. Holding selects fade-out and returns ($88:DE95),
/// so its first write is on the next call. Fade-in writes counters0..15 before
/// incrementing; fade-out writes16..1 before decrementing and retains1 at zero.
/// These mutually exclusive control phases use scalar state, not a sampled timing
/// table. Immediate viewport loading starts at the completed fade-in state.
/// </summary>
[RoomLifetime]
public sealed class CeresHazeState
{
    /// <summary>Mutually exclusive native pre-instruction phases of the room-owned haze.</summary>
    private enum Phase
    {
        /// <summary>Waits for the room fade-in gate without changing the published haze strength.</summary>
        Waiting,
        /// <summary>Writes strengths zero through fifteen before each counter increment.</summary>
        FadingIn,
        /// <summary>Retains full haze until the room fade-out gate selects the next phase.</summary>
        Holding,
        /// <summary>Writes strengths sixteen through one before decrementing, retaining one at zero.</summary>
        FadingOut,
    }
    /// <summary>Current haze pre-instruction phase, independent of its selected color channel.</summary>
    private Phase _phase;
    /// <summary>Fade position used before incrementing on entry and before decrementing on exit.</summary>
    private int _counter;
    /// <summary>Whether the loaded room setup owns a haze effect; false makes updates inert and suppresses the renderer's layer, independently of the stored fade phase.</summary>
    public bool Enabled { get; private set; }
    /// <summary>Channel selection sampled once by $88:DDC7: true chooses the Ridley-dead red ramp ($20), false the Ridley-alive blue ramp ($80); later boss-bit changes do not retarget it.</summary>
    public bool IsRed { get; private set; }
    /// <summary>Last published native ramp strength in RGB5 channel units, not a frame count: fade-in writes 0..15, fade-out writes 16..1, and zero counter retains the final strength one.</summary>
    public int Intensity { get; private set; }

    /// <summary>Samples the boss bit once, as the HDMA object's instruction list is selected.</summary>
    /// <param name="enabled">Whether the room's setup callback spawns the Ceres haze owner.</param>
    /// <param name="ridleyIsDead">Loaded area's boss-bit result selecting red rather than blue for this owner's lifetime.</param>
    /// <param name="immediateViewport">True for immediate viewport installation: starts holding at counter 16 with published strength 15; false starts waiting at zero for the door fade-in phase.</param>
    [IndependentFlag("independent facts of the loaded room: whether it spawns the haze, Ridley's boss bit, and immediate viewport installation")]
    public CeresHazeState(bool enabled, bool ridleyIsDead, bool immediateViewport)
    {
        Enabled = enabled;
        IsRed = ridleyIsDead;
        _phase = immediateViewport ? Phase.Holding : Phase.Waiting;
        _counter = immediateViewport ? CeresHazeDefinitions.FadeSteps : 0;
        Intensity = immediateViewport ? CeresHazeDefinitions.FadeSteps - 1 : 0;
    }

    /// <summary>One native pre-instruction call. Transition changes do not rewrite the selected channel.</summary>
    /// <param name="roomFade">
    /// The door fade: on fade-in a waiting owner enters fade-in and writes its first strength on this same update; on
    /// fade-out a holding owner changes phase now but publishes its first fade-out strength on the following update.
    /// </param>
    public void Step(CeresHazeRoomFade roomFade = CeresHazeRoomFade.None)
    {
        if (!Enabled) return;
        if (_phase == Phase.Waiting)
        {
            if (roomFade != CeresHazeRoomFade.FadeIn) return;
            _phase = Phase.FadingIn;
        }
        switch (_phase)
        {
            case Phase.FadingIn:
                if (_counter == CeresHazeDefinitions.FadeSteps) _phase = Phase.Holding;
                else Intensity = _counter++;
                break;
            case Phase.Holding:
                if (roomFade == CeresHazeRoomFade.FadeOut) _phase = Phase.FadingOut;
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
