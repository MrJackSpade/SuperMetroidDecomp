namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Standalone horizontal speed records selected directly rather than by movement type.</summary>
public enum SamusStandaloneSpeedRecord
{
    /// <summary>The horizontal speeds of a diagonal bomb jump.</summary>
    DiagonalBombJump,
    /// <summary>The horizontal speeds after a grapple release in air.</summary>
    GrappleReleaseAir,
    /// <summary>The horizontal speeds after a grapple release in water.</summary>
    GrappleReleaseWater,
    /// <summary>The horizontal speeds after a grapple release in lava or acid.</summary>
    GrappleReleaseLavaAcid,
}
