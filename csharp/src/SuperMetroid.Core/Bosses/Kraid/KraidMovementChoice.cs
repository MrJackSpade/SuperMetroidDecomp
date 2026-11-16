namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's chosen horizontal target and the time before he chooses again.</summary>
/// <param name="TargetX">The room column Kraid walks towards.</param>
/// <param name="ThinkTimer">The updates before he chooses again.</param>
public readonly record struct KraidMovementChoice(int TargetX, int ThinkTimer);
