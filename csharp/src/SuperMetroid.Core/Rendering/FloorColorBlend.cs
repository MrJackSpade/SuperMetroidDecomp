namespace SuperMetroid.Core.Rendering;

/// <summary>How a turning floor blends: not at all, or one of two ways.</summary>
public enum FloorColorBlend
{
    /// <summary>The floor is drawn without blending.</summary>
    None = 0,
    /// <summary>The floor minus the sprites.</summary>
    SubtractSprites = 1,
    /// <summary>The floor's colour is added over the floor and the blending sprites; the backdrop and the other sprites are unchanged.</summary>
    AddForeground = 2,
}
