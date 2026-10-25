namespace SuperMetroid.Core.Rendering;

/// <summary>The exclusive colour blends a turning floor takes in the renderer shader contract.</summary>
public enum FloorColorBlend
{
    /// <summary>Insert the background without color arithmetic.</summary>
    None = 0,
    /// <summary>The floor minus the sprites.</summary>
    SubtractSprites = 1,
    /// <summary>BG1/OBJ main plus BG1 subscreen; only eligible layers/palettes participate.</summary>
    AddBg1 = 2,
}
