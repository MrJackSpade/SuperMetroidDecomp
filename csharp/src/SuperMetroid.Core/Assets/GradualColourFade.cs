using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// A gradual fade between the colour an ink shows and one fixed colour (#1512, #1544), as the game's shared gradual colour
/// change moves colours: each step moves every fade level (<see cref="FadeLevelColour"/>) by its remaining distance over
/// the steps left, in 256ths, step zero changes nothing and the step after the last reaches the far colour
/// exactly. Every colour between is the game's own step, the colour of the levels it reaches. The fade keeps the step
/// counter's value at each step it took, as a fade that shares the counter with another sees the values that fade leaves it.
/// </summary>
public sealed record GradualColourFade : ColourEffect
{
    /// <summary>Keeps a fade's fixed colour, direction, length and the steps it has taken.</summary>
    /// <param name="endpoint">The fixed colour: the one faded toward, or the one faded in from.</param>
    /// <param name="direction">Whether the colour shown fades toward the fixed colour or in from it.</param>
    /// <param name="denominator">The fade's length: its last moving step is this one, one through <see cref="LongestDenominator"/>.</param>
    /// <param name="steps">The step counter's value at each step taken, in order.</param>
    public GradualColourFade(RgbColor endpoint, GradualFadeDirection direction, int denominator, ImmutableArray<int> steps)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined gradual fade direction.");
        Denominator = Ensure.BetweenInclusive(1, LongestDenominator, denominator);
        Endpoint = endpoint;
        Direction = direction;
        Steps = steps.IsDefault ? [] : steps;
    }

    /// <summary>The fixed colour: the one faded toward, or the one faded in from.</summary>
    public RgbColor Endpoint { get; }

    /// <summary>Whether the colour shown fades toward <see cref="Endpoint"/> or in from it.</summary>
    public GradualFadeDirection Direction { get; }

    /// <summary>
    /// The longest fade (#1627): any length whose step after the last, the one that reaches the far colour, is still a
    /// count of steps.
    /// </summary>
    public const int LongestDenominator = int.MaxValue - 1;

    /// <summary>The fade's length: its last moving step is this one.</summary>
    public int Denominator { get; }

    /// <summary>The step counter's value at each step taken, in order.</summary>
    public ImmutableArray<int> Steps { get; }

    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown)
    {
        FadeLevelColour current = (Direction == GradualFadeDirection.Toward ? shown : Endpoint).FadeLevels;
        FadeLevelColour goal = (Direction == GradualFadeDirection.Toward ? Endpoint : shown).FadeLevels;
        foreach (int step in Steps)
            current = Step(step, current, goal, Denominator);
        return current.ToColour();
    }

    /// <summary>Moves <paramref name="current"/>'s levels one step of a fade of length <paramref name="denominator"/> toward <paramref name="goal"/>'s.</summary>
    /// <param name="step">The step counter's value at this step.</param>
    /// <param name="current">The levels before the step.</param>
    /// <param name="goal">The levels the fade ends in.</param>
    /// <param name="denominator">The fade's length.</param>
    public static FadeLevelColour Step(int step, FadeLevelColour current, FadeLevelColour goal, int denominator) =>
        new(Component(step, current.Red, goal.Red, denominator), Component(step, current.Green, goal.Green, denominator),
            Component(step, current.Blue, goal.Blue, denominator));

    /// <summary>
    /// Moves one fade level one step toward its goal: step zero keeps it, the step after the last reaches the goal, and
    /// every other step moves it by its distance in 256ths over the steps left, truncated to whole 256ths.
    /// </summary>
    /// <param name="step">The step counter's value at this step.</param>
    /// <param name="current">The level before the step.</param>
    /// <param name="destination">The level the fade ends in.</param>
    /// <param name="denominator">The fade's length.</param>
    public static int Component(int step, int current, int destination, int denominator)
    {
        if (step == 0)
            return current;
        int zeroBasedStep = step - 1;
        if (zeroBasedStep == denominator)
            return destination;
        int fixedDelta = Math.Abs(destination - current) * 256 / (denominator - zeroBasedStep);
        if (destination < current)
            fixedDelta = -fixedDelta;
        return EightEight.WholeOf(EightEight.FromWhole(current) + fixedDelta);
    }
}
