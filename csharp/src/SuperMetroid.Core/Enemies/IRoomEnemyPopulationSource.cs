namespace SuperMetroid.Core.Enemies;

/// <summary>Where a room's enemy population comes from, which also names the artwork its enemies draw from (#1599).</summary>
public interface IRoomEnemyPopulationSource
{
    /// <summary>The room state's enemy population.</summary>
    RoomEnemyPopulationDefinition Population { get; }
}
