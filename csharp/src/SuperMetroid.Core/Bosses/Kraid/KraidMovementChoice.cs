namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's chosen horizontal target and the time before he chooses again.</summary>
/// <param name="TargetX">The room column Kraid walks towards.</param>
/// <param name="ThinkUpdates">The updates he thinks once he stands at the target, the one he chooses again on included.</param>
public readonly record struct KraidMovementChoice(int TargetX, int ThinkUpdates);
