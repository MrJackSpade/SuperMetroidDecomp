namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Power Bomb's blast as the display shows it (#1927): whether it is running, and the oval of added colour it shows.
/// Taking it never advances the explosion.
/// </summary>
/// <param name="IsActive">Whether the blast is running.</param>
/// <param name="Oval">The oval the blast shows this frame.</param>
public readonly partial record struct PowerBombRenderSnapshot(bool IsActive, PowerBombBlastOval Oval)
{
    /// <summary>Copies the blast's shown oval.</summary>
    public static PowerBombRenderSnapshot Capture(SamusPowerBombExplosionState explosion)
    {
        Ensure.NotNull(explosion);
        return new(explosion.IsActive, explosion.ShownOval);
    }
}
