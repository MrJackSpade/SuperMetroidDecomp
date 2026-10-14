namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive horizontal endpoints and expanded-byte RGB addition for one physical scanline.</summary>
public readonly record struct ColorAddWindow(byte Left, byte Right, byte Red, byte Green, byte Blue)
{
    /// <summary>Inverted endpoints represent an empty window, including hidden HUD lines.</summary>
    public static ColorAddWindow Empty => new(1, 0, 0, 0, 0);
}
