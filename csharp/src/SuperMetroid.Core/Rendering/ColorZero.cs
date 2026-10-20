namespace SuperMetroid.Core.Rendering;

/// <summary>How a background treats colour index zero.</summary>
public enum ColorZero
{
    /// <summary>Colour zero shows the layers below.</summary>
    Transparent,

    /// <summary>Colour zero draws its palette colour.</summary>
    Opaque,
}
