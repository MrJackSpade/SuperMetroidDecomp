using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Value-only render input for the power bomb's windowed fixed-color addition: the window
/// table's producing phase and radii, and its colour. Taking it never advances the explosion.
/// </summary>
/// <param name="IsActive">Whether the bank-$88 color-math animation is running.</param>
/// <param name="Position">Room-pixel centre of the explosion.</param>
/// <param name="RenderedPhase">Phase whose per-line window the current update produced.</param>
/// <param name="RenderedShape">Pre-scaled shape the current window was built from, or null when it used none.</param>
/// <param name="RenderedPreExplosionRadius">Pre-explosion 8.8 radius the current window was built from.</param>
/// <param name="RenderedExplosionRadius">Explosion 8.8 radius the current window was built from.</param>
/// <param name="FixedColor">The colour added inside the window.</param>
public readonly record struct PowerBombRenderSnapshot(
    bool IsActive,
    Position16<RoomPixels> Position,
    PowerBombExplosionPhase RenderedPhase,
    PowerBombShapeCursor? RenderedShape,
    PowerBombRadius RenderedPreExplosionRadius,
    PowerBombRadius RenderedExplosionRadius,
    RgbColor FixedColor)
{
    /// <summary>Copies the explosion's rendered window inputs and its current colour.</summary>
    public static PowerBombRenderSnapshot Capture(SamusPowerBombExplosionState explosion)
    {
        Ensure.NotNull(explosion);
        return new(explosion.IsActive, explosion.Position, explosion.RenderedPhase,
            explosion.RenderedShape, explosion.RenderedPreExplosionRadius,
            explosion.RenderedExplosionRadius, explosion.FixedColor);
    }
}
