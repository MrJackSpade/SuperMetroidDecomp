namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Definition data of the game's shared random sequence: the value it starts from at power-on and at a soft reset, from
/// which the whole sequence every consumer draws follows. Read by <see cref="SharedRandom"/> when it is created and when
/// a reset reseeds it. Evidence: the original's boot and reset seed the random value with 97.
/// </summary>
[CompatibilityService("the shared random sequence's power-on and reset seed")]
internal static class SharedRandomDefinitions
{
    /// <summary>The seed the generator holds when the game is powered on or reset: 97.</summary>
    internal static SharedRandomState PowerOnState { get; } = new(97);
}
