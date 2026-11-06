namespace SuperMetroid.Core.Rendering;

/// <summary>Whether a blend brightens what it covers by adding a colour or darkens it by subtracting one.</summary>
public enum BlendOperation
{
    /// <summary>Adds the colour, brightening what it covers, each channel clamped at full.</summary>
    Add,
    /// <summary>Subtracts the colour, darkening what it covers, each channel clamped at zero.</summary>
    Subtract
}
