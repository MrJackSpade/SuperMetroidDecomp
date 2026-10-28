namespace SuperMetroid.Core.Rendering;

/// <summary>How a layer drawn on its own combines with the screen behind it.</summary>
public enum BlendOperation
{
    /// <summary>Adds the layer's colour to the screen's, each eight-bit channel clamped at 255, keeping the screen's opacity.</summary>
    Add,
    /// <summary>Subtracts the layer's colour from the screen's, each eight-bit channel clamped at zero, keeping the screen's opacity.</summary>
    Subtract
}
