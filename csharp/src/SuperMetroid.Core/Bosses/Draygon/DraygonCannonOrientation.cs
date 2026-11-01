namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Horizontal orientation selected by one of the three retail cannon headers.</summary>
public enum DraygonCannonOrientation
{
    /// <summary>Left-facing cannon header, whose damage step uses the left draw family and control word.</summary>
    Left,
    /// <summary>Right-facing cannon headers, whose live and destroyed forms use the right draw family and control word.</summary>
    Right,
}
