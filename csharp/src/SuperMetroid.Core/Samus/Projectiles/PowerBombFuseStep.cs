namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What one update of a Power Bomb's fuse changes and asks for.</summary>
/// <param name="UpdatesLeft">The updates the fuse has left after the step: unchanged once burned out, otherwise one fewer, zero on the update that starts the explosion.</param>
/// <param name="FastAnimation">Whether the animation switches to its fast counterpart.</param>
/// <param name="SpawnExplosion">Whether the fuse burned out this update, so the explosion starts and spawns.</param>
/// <param name="DeleteProjectile">Whether the bomb is deleted.</param>
[IndependentFlag("independent outcomes of one fuse step")]
public readonly record struct PowerBombFuseStep(
    int UpdatesLeft, bool FastAnimation, bool SpawnExplosion, bool DeleteProjectile)
{
    /// <summary>
    /// Whether the Power Bomb's boundary reactions follow this step: only from a fuse that had already burned out. A fuse
    /// still burning reacts with nothing, and so does the step that burns it out (#2468; the evidence is
    /// <see cref="BeamFiredOverLastShot"/>'s).
    /// </summary>
    public bool BoundaryReacts => UpdatesLeft == 0 && !SpawnExplosion;
}
