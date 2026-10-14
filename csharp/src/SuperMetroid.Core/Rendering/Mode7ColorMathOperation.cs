namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive Mode 7 color operations in the renderer shader contract.</summary>
public enum Mode7ColorMathOperation
{
    /// <summary>Insert the background without color arithmetic.</summary>
    None = 0,
    /// <summary>BG1 main minus OBJ subscreen.</summary>
    SubtractObj = 1,
    /// <summary>BG1/OBJ main plus BG1 subscreen; only eligible layers/palettes participate.</summary>
    AddBg1 = 2,
}
