using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The enemy instruction interpreter the Baby Metroid program runs through, with no live Samus, room or input (#633).</summary>
internal readonly record struct BabyMetroidInterpreter
{
    /// <summary>The enemy system's instruction interpreter.</summary>
    public required EnemyAnimationPlayer Instructions { get; init; }
}
