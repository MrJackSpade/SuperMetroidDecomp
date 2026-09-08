namespace SuperMetroid.Core.Game;

/// <summary>Room-owned $88:DDC7/$DE10-$DECA haze selection and pre-instruction lifecycle.</summary>
public sealed class CeresHazeState
{
    private enum Phase { Waiting, FadingIn, Holding, FadingOut }
    private Phase _phase;
    private int _counter;
    public bool Enabled { get; private set; }
    public bool IsRed { get; private set; }
    public int Intensity { get; private set; }

    /// <summary>Samples the boss bit once, as the HDMA object's instruction list is selected.</summary>
    public void Load(bool enabled, bool ridleyIsDead, bool immediateViewport)
    {
        Enabled = enabled;
        IsRed = ridleyIsDead;
        _phase = immediateViewport ? Phase.Holding : Phase.Waiting;
        _counter = immediateViewport ? CeresHazeDefinitions.FadeSteps : 0;
        Intensity = immediateViewport ? CeresHazeDefinitions.FadeSteps - 1 : 0;
    }

    /// <summary>One native pre-instruction call. Transition changes do not rewrite the selected channel.</summary>
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
