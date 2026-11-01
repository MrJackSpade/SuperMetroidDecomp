using System.Reflection;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Forces a power-bomb explosion's activity so a check can test what an explosion rejects (#1483).</summary>
internal static class SamusPowerBombExplosionStateAccess
{
    /// <summary>Marks the room's explosion active or inactive without an animation phase or an armed bomb.</summary>
    extension(SamusPowerBombExplosionState explosion)
    {
        /// <summary>
        /// Sets <see cref="SamusPowerBombExplosionState.IsActive"/>; clearing it also returns the phase to
        /// <see cref="PowerBombExplosionPhase.Inactive"/>, as a finished explosion leaves it.
        /// </summary>
        internal void ForceActive(bool active)
        {
            SetPrivate(explosion, nameof(SamusPowerBombExplosionState.IsActive), active);
            if (!active && explosion.Phase != PowerBombExplosionPhase.Inactive)
                SetPrivate(explosion, nameof(SamusPowerBombExplosionState.Phase), PowerBombExplosionPhase.Inactive);
        }
    }

    /// <summary>Writes a property through its private setter.</summary>
    private static void SetPrivate(SamusPowerBombExplosionState explosion, string property, object value) =>
        (typeof(SamusPowerBombExplosionState).GetProperty(property, BindingFlags.Instance | BindingFlags.Public)?.SetMethod
            ?? throw new MissingMemberException(nameof(SamusPowerBombExplosionState), property))
        .Invoke(explosion, [value]);
}
