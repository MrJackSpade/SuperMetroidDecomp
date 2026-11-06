namespace SuperMetroid.Core.Map;

/// <summary>The shade a map screen shows an area-map cell in (#1513), which its exploration selects.</summary>
public enum MapCellShade
{
    /// <summary>Revealed, by the area's map or a reveal mode, but not yet explored; also every cell nothing reveals.</summary>
    Unexplored,

    /// <summary>Explored by Samus.</summary>
    Explored,
}
