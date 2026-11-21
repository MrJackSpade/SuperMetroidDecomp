namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Where Samus's spin animation stands for the Screw Attack glow (#2560).</summary>
public enum SpinGlowPlace
{
    /// <summary>She is not spin jumping or wall jumping: no spin animation stands anywhere for the glow.</summary>
    NotSpinning,

    /// <summary>
    /// The Screw Attack spin's first frame, or a wall jump before its spin starts: her normal colours show and the glow's
    /// cycle restarts.
    /// </summary>
    Restarting,

    /// <summary>The spin's frames between its start and its first wall-jump-ready frame, which cycle the glow.</summary>
    Glowing,

    /// <summary>A spin jump from the spin's first wall-jump-ready frame on, where she shows her normal colours.</summary>
    ShowsNormalColours,
}
