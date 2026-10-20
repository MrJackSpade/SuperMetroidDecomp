namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>What the area map's colour math takes as its subscreen.</summary>
public enum AreaMapColorMath
{
    /// <summary>The backdrop is part of the colour math.</summary>
    WithBackdrop,

    /// <summary>Only the foreground layer takes part.</summary>
    ForegroundOnly,
}
