namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Value-only render input for the power bomb's windowed fixed-color addition: the window
/// table's producing phase and radii, and the COLDATA color. Taking it never advances the explosion.
/// </summary>
/// <param name="IsActive">Whether the bank-$88 color-math animation is running.</param>
/// <param name="Position">Room-pixel centre of the explosion.</param>
/// <param name="RenderedPhase">Phase whose HDMA window table the current logic frame produced.</param>
/// <param name="RenderedShape">Pre-scaled shape the current window was built from, or null when it used none.</param>
/// <param name="RenderedPreExplosionRadius">Pre-explosion 8.8 radius the current window was built from.</param>
/// <param name="RenderedExplosionRadius">Explosion 8.8 radius the current window was built from.</param>
/// <param name="FixedColorRed">Raw COLDATA red byte added inside the window, 0 through 255.</param>
/// <param name="FixedColorGreen">Raw COLDATA green byte added inside the window, 0 through 255.</param>
/// <param name="FixedColorBlue">Raw COLDATA blue byte added inside the window, 0 through 255.</param>
public readonly record struct PowerBombRenderSnapshot(
    bool IsActive,
    Position16<RoomPixels> Position,
    PowerBombExplosionPhase RenderedPhase,
    PowerBombShapeCursor? RenderedShape,
    PowerBombRadius RenderedPreExplosionRadius,
    PowerBombRadius RenderedExplosionRadius,
    int FixedColorRed,
    int FixedColorGreen,
    int FixedColorBlue)
{
    /// <summary>Copies the explosion's rendered window inputs and its current COLDATA color.</summary>
    public static PowerBombRenderSnapshot Capture(SamusPowerBombExplosionState explosion)
    {
        Ensure.NotNull(explosion);
        return new(explosion.IsActive, explosion.Position, explosion.RenderedPhase,
            explosion.RenderedShape, explosion.RenderedPreExplosionRadius,
            explosion.RenderedExplosionRadius, explosion.FixedColorRed, explosion.FixedColorGreen,
            explosion.FixedColorBlue);
    }
}
