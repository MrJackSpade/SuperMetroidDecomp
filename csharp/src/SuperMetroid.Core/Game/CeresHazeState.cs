namespace SuperMetroid.Core.Game;

/// <summary>
/// Room-owned $88:DDC7/$DE10-$DED2 haze selection and direct pre-instruction cases.
/// Waiting falls through into the first fade-in write ($88:DE27..DE2D); counter16
/// selects holding without writing. Holding selects fade-out and returns ($88:DE95),
/// so its first write is on the next call. Fade-in writes counters0..15 before
/// incrementing; fade-out writes16..1 before decrementing and retains1 at zero.
/// These mutually exclusive control phases use scalar state, not a sampled timing
/// table. Immediate viewport loading starts at the completed fade-in state.
/// </summary>
public sealed class CeresHazeState
{
    /// <summary>Mutually exclusive control stages for the room-owned haze ramp.</summary>
    private enum Phase
    {
        /// <summary>Waits for the room's door fade-in gate before beginning the ramp.</summary>
        Waiting,

        /// <summary>Publishes fade-in strengths from zero through the final ramp step.</summary>
        FadingIn,

        /// <summary>Retains the completed fade-in strength until the room fade-out gate arrives.</summary>
        Holding,

        /// <summary>Publishes fade-out strengths on successive pre-instruction updates.</summary>
        FadingOut
    }

    /// <summary>Current stage controlling when the next haze intensity update is published.</summary>
    private Phase _phase;

    /// <summary>Per-phase step index used by fade-in and fade-out, including their distinct endpoint rules.</summary>
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
    public void Load(bool enabled, bool ridleyIsDead, bool immediateViewport)
    {
        Enabled = enabled;
        IsRed = ridleyIsDead;
        _phase = immediateViewport ? Phase.Holding : Phase.Waiting;
        _counter = immediateViewport ? CeresHazeDefinitions.FadeSteps : 0;
        Intensity = immediateViewport ? CeresHazeDefinitions.FadeSteps - 1 : 0;
    }

    /// <summary>One native pre-instruction call. Transition changes do not rewrite the selected channel.</summary>
    /// <param name="roomFadeIn">Door-transition fade-in gate; a waiting owner enters fade-in and writes its first strength on this same update.</param>
    /// <param name="roomFadeOut">Door-transition fade-out gate; a holding owner changes phase now but publishes its first fade-out strength on the following update.</param>
    public void Step(bool roomFadeIn = false, bool roomFadeOut = false)
    {
        if (!Enabled) return;
        if (_phase == Phase.Waiting)
        {
            if (!roomFadeIn) return;
            _phase = Phase.FadingIn;
        }
        switch (_phase)
        {
            case Phase.FadingIn:
                if (_counter == CeresHazeDefinitions.FadeSteps) _phase = Phase.Holding;
                else Intensity = _counter++;
                break;
            case Phase.Holding:
                if (roomFadeOut) _phase = Phase.FadingOut;
                break;
            case Phase.FadingOut:
                if (_counter != 0) Intensity = _counter--;
                break;
        }
    }
}
