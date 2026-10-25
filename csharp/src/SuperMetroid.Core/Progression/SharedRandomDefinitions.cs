namespace SuperMetroid.Core.Progression;

/// <summary>Definition data of the game's shared random-number generator.</summary>
internal static class SharedRandomDefinitions
{
    /// <summary>The seed the generator holds when the game is powered on or reset: 97.</summary>
    internal static SharedRandomState PowerOnState { get; } = new(97);
}
