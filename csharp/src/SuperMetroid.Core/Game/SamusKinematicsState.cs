namespace SuperMetroid.Core.Game;

/// <summary>
/// Samus position, subposition, radius, and vertical-speed WRAM words consumed by bank-$94
/// room collision.
/// </summary>
/// <remarks>
/// Positions are split high/low words because collision intentionally overwrites individual
/// halves (for example, a right wall sets X subposition to $FFFF). Collapsing this state to
/// floats would erase those observable edge semantics.
/// </remarks>
public sealed class SamusKinematicsState
{
    /// <summary>Whole-pixel world X at WRAM <c>$0AF6</c>.</summary>
    public ushort XPosition { get; set; }

    /// <summary>Fractional world X at WRAM <c>$0AF8</c>.</summary>
    public ushort XSubposition { get; set; }

    /// <summary>Whole-pixel world Y at WRAM <c>$0AFA</c>.</summary>
    public ushort YPosition { get; set; }

    /// <summary>Fractional world Y at WRAM <c>$0AFC</c>.</summary>
    public ushort YSubposition { get; set; }

    /// <summary>Horizontal collision radius; <c>Samus_SetRadius</c> always writes five.</summary>
    public ushort XRadius { get; set; } = 5;

    /// <summary>Pose-defined vertical collision radius at WRAM <c>$0B00</c>.</summary>
    public ushort YRadius { get; set; }

    /// <summary>Whole vertical speed used to suppress grounded horizontal slope scaling.</summary>
    public ushort YSpeed { get; set; }

    /// <summary>Fractional vertical speed used to suppress grounded horizontal slope scaling.</summary>
    public ushort YSubspeed { get; set; }

    /// <summary>Native <c>enable_horiz_slope_coll</c>; bit 1 enables post-X Y alignment.</summary>
    public ushort HorizontalSlopeCollisionEnable { get; set; } = 3;

    /// <summary>Native flag set when square collision or non-square alignment changes Y.</summary>
    public bool PositionAdjustedBySlope { get; set; }

    /// <summary>Current position as an unsigned native 16.16 pair.</summary>
    public uint XFixed => ((uint)XPosition << 16) | XSubposition;

    /// <summary>Current position as an unsigned native 16.16 pair.</summary>
    public uint YFixed => ((uint)YPosition << 16) | YSubposition;

    /// <summary>Current vertical speed as an unsigned native high/low pair.</summary>
    public uint VerticalSpeedFixed => ((uint)YSpeed << 16) | YSubspeed;

    internal void SetXFixed(uint value)
    {
        XPosition = unchecked((ushort)(value >> 16));
        XSubposition = unchecked((ushort)value);
    }

    internal void SetYFixed(uint value)
    {
        YPosition = unchecked((ushort)(value >> 16));
        YSubposition = unchecked((ushort)value);
    }
}
