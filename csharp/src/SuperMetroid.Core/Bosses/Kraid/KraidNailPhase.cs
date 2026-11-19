namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase one of Kraid's fingernails runs.</summary>
public enum KraidNailPhase
{
    /// <summary>Runs the fingernail's phase wait, then sets its launch up (<see cref="Initialize"/>).</summary>
    WaitBeforeLaunch,
    /// <summary>Selects the fingernail's starting side, animation, and firing transition.</summary>
    Initialize,
    /// <summary>Waits until the top lint's X position reaches at least 256, then fires (<see cref="Fire"/>).</summary>
    WaitForLint,
    /// <summary>Advances the fingernail's independent firing animation.</summary>
    Fire,
}
