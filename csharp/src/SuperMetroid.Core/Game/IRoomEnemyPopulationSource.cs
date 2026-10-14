namespace SuperMetroid.Core.Game;

/// <summary>Where a room's enemy population and its graphics come from.</summary>
public interface IRoomEnemyPopulationSource
{
    /// <summary>The bus the population, definitions and tile graphics are read through.</summary>
    ISnesAddressSpace Bus { get; }

    /// <summary>The room state's enemy population pointer.</summary>
    ushort PopulationPointer { get; }

    /// <summary>The room state's enemy graphics-set pointer.</summary>
    ushort TilesetPointer { get; }

    /// <summary>VRAM the enemy graphics load into.</summary>
    SnesVram Vram { get; }

    /// <summary>CGRAM the enemy palettes load into.</summary>
    SnesCgram Cgram { get; }
}
