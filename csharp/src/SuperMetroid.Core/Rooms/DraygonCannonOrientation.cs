namespace SuperMetroid.Core.Rooms;

/// <summary>Horizontal orientation selected by one of the three retail cannon headers.</summary>
public enum DraygonCannonOrientation : byte
{
    /// <summary>Native left-facing header $DF71, whose damage instruction uses the left draw family and bank-$A5 control word.</summary>
    Left,
    /// <summary>Native right-facing headers $DF59/$DF65, whose live and destroyed forms use the right draw family and bank-$A5 control word.</summary>
    Right,
}
