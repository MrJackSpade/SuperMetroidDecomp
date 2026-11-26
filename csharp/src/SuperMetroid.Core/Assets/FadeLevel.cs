namespace SuperMetroid.Core.Assets;

/// <summary>
/// How far a fade has come (#2563): <see cref="Step"/> of its <see cref="Steps"/> steps, from where it starts (none) to
/// where it ends (every step). A fade, a crossfade or a colour shown partway between black and itself names its level so;
/// the backend shows it, in the steps the game shows (<see cref="IColourBlend"/>).
/// </summary>
public readonly partial record struct FadeLevel
{
    /// <summary>A fade <paramref name="step"/> steps of <paramref name="steps"/> along.</summary>
    /// <param name="step">How many of its steps the fade has taken, zero through <paramref name="steps"/>.</param>
    /// <param name="steps">How many steps the fade takes from its start to its end, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The fade takes no step, or the step lies outside it.</exception>
    public FadeLevel(int step, int steps)
    {
        Steps = Ensure.BetweenInclusive(1, int.MaxValue, steps);
        Step = Ensure.BetweenInclusive(0, Steps, step);
    }

    /// <summary>How many of its steps the fade has taken.</summary>
    public int Step { get; }

    /// <summary>How many steps the fade takes from its start to its end.</summary>
    public int Steps { get; }

    /// <summary>How the fade's colours move as its steps are taken (#3036): its share of the way unless it names the cartridge's gradual colour change.</summary>
    public FadeArithmetic Arithmetic { get; init; }

    /// <summary>Whether the fade has taken every step: it shows where it ends.</summary>
    public bool IsComplete => Step == Steps;

    /// <inheritdoc />
    public override string ToString() => Arithmetic == FadeArithmetic.Proportional ? $"{Step}/{Steps}" : $"{Step}/{Steps} {Arithmetic}";
}
