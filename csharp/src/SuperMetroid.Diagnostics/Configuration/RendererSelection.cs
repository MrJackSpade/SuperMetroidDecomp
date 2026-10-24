namespace SuperMetroid.Diagnostics.Configuration;

/// <summary>The desktop host's backend request from its options file; the game never sees it.</summary>
public enum RendererSelection
{
    /// <summary>Use the portable software renderer explicitly.</summary>
    Software,
    /// <summary>Require the hardware Direct3D11 path; initialization failures must remain errors.</summary>
    Direct3D11,
    /// <summary>Try hardware Direct3D11, allowing a clearly reported software fallback at initialization.</summary>
    Auto
}
