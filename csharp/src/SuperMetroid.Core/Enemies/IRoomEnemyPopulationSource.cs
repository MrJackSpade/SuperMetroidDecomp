namespace SuperMetroid.Core.Enemies;

/// <summary>Where a room's enemy population and its graphics come from.</summary>
public interface IRoomEnemyPopulationSource
{
    /// <summary>The source of the population, its enemy headers and its graphics set.</summary>
    IRoomEnemyDefinitionSource Definitions { get; }

    /// <summary>The room state's enemy population pointer.</summary>
    ushort PopulationPointer { get; }

    /// <summary>The room state's enemy graphics-set pointer.</summary>
    ushort TilesetPointer { get; }

    /// <summary>VRAM the enemy graphics load into.</summary>
    SnesVram Vram { get; }

    /// <summary>CGRAM the enemy palettes load into.</summary>
    SnesCgram Cgram { get; }
}
