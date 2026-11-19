namespace SuperMetroid.Core.Compatibility;

/// <summary>How hard Samus lands (#2555), as her landing speed reads it: the landing's sound and dust follow it.</summary>
public enum LandingImpact
{
    /// <summary>A landing with no sound and no dust.</summary>
    Silent,

    /// <summary>An ordinary landing.</summary>
    Soft,

    /// <summary>A hard landing.</summary>
    Hard,
}
