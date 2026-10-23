using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies;

/// <summary>Where a room's enemy population and its graphics come from.</summary>
public interface IRoomEnemyPopulationSource
{
    /// <summary>The room state's enemy population.</summary>
    RoomEnemyPopulationDefinition Population { get; }

    /// <summary>The room state's enemy graphics set, loaded only when the population is not empty.</summary>
    RoomEnemyGraphicsSetDefinition GraphicsSet { get; }

    /// <summary>VRAM the enemy graphics load into.</summary>
    SnesVram Vram { get; }

    /// <summary>CGRAM the enemy palettes load into.</summary>
    GameplayColours Cgram { get; }
}
