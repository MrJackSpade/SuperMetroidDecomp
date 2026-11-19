namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The distance and direction from a Yapping Maw to its target.</summary>
/// <param name="Distance">The distance.</param>
/// <param name="Bearing">The direction.</param>
internal readonly record struct YappingMawTarget(int Distance, TrigonometryAngle Bearing);
