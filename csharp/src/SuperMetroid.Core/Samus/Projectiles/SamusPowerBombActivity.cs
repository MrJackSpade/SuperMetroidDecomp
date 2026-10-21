using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Whether Samus's power bomb explosion is active, as the room and its PLMs read it to silence sounds (#1395).</summary>
[ArchitectureReview("back-reference", "A stateless view of its owner: the explosion holds the view it presents to rooms and FX, which reads only whether it is active.")]
internal sealed class SamusPowerBombActivity(SamusPowerBombExplosionState explosion) : IActivePowerBomb
{
    /// <inheritdoc/>
    public bool IsActive => explosion.IsActive;
}
