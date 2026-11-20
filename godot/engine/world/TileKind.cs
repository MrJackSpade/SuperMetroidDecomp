namespace Engine.World;

/// <summary>What a tile of terrain is.</summary>
public enum TileKind
{
    /// <summary>Nothing: open space.</summary>
    Empty,

    /// <summary>Solid all through.</summary>
    Solid,

    /// <summary>Sloped ground, solid below or above its surface.</summary>
    Slope,

    /// <summary>A reactive block, whose rules are its block type's.</summary>
    Block,
}
