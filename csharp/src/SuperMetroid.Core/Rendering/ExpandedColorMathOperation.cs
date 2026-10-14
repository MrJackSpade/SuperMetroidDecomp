namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive byte-domain equations used by the current room FX reference.</summary>
public enum ExpandedColorMathOperation : byte
{
    /// <summary>Adds the expanded background RGB channels to the existing output channels, clamping each sum to 255 and preserving output alpha.</summary>
    Add,
    /// <summary>Subtracts the expanded background RGB channels from the existing output channels, clamping each difference to zero and preserving output alpha.</summary>
    Subtract
}
