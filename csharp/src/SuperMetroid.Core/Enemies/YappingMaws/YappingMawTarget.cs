namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The distance and bearing from a Yapping Maw to its target.</summary>
/// <param name="Distance">The distance.</param>
/// <param name="Bearing">The bearing.</param>
internal readonly record struct YappingMawTarget(int Distance, SnesBearing Bearing);
