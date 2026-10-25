namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Bounded transitions of the hand-beam attack dispatcher.</summary>
public static class MotherBrainHandBeamPhases
{
    /// <summary>The phase the body's increment-death-beam-attack-phase opcode advances to.</summary>
    /// <exception cref="InvalidOperationException">The finishing phase has no successor.</exception>
    public static MotherBrainHandBeamPhase Next(MotherBrainHandBeamPhase phase) => phase switch
    {
        MotherBrainHandBeamPhase.BackUp
            or MotherBrainHandBeamPhase.WaitForBombs
            or MotherBrainHandBeamPhase.Firing => EnumOrder.Next(phase),
        MotherBrainHandBeamPhase.Finish => throw new InvalidOperationException(
            "Mother Brain's hand-beam phase cannot advance past finishing."),
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainHandBeamPhase)} {phase}."),
    };
}
