using SuperMetroid.Core.Enemies;

/// <summary>The enemy instruction interpreter the Shitroid program runs through, with no live Samus, room or input (#633).</summary>
internal readonly record struct ShitroidInterpreter
{
    /// <summary>The enemy system's instruction interpreter.</summary>
    public required EnemyInstructionInterpreter Instructions { get; init; }
}
