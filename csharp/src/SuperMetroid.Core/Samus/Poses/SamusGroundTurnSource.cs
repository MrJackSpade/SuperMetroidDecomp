using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What a pose Samus can turn from on the ground holds for the turn (#2553): a front view, which faces neither way, or the
/// direction an aiming pose aims, which the turn keeps.
/// </summary>
internal abstract record SamusGroundTurnSource
{
    /// <summary>Closes the hierarchy to the sources declared here.</summary>
    private SamusGroundTurnSource()
    {
    }

    /// <summary>The turn Samus takes from this source, given the generic turn her input chose and the turns by aim.</summary>
    /// <param name="chosen">The generic turn her input transitions chose.</param>
    /// <param name="turns">The turns of her posture, standing or crouched, by the direction they keep.</param>
    internal abstract SamusPoseId Turn(SamusPoseId chosen, IdentityTable<SamusProjectileDirection, SamusPoseId> turns);

    /// <summary>A front view, which keeps the generic turn its input chose: the setup reads no aim for it.</summary>
    internal sealed record FacingForward : SamusGroundTurnSource
    {
        /// <summary>The one front-view source.</summary>
        internal static FacingForward Instance { get; } = new();

        /// <summary>Creates the one front-view source.</summary>
        private FacingForward()
        {
        }

        /// <inheritdoc/>
        internal override SamusPoseId Turn(SamusPoseId chosen, IdentityTable<SamusProjectileDirection, SamusPoseId> turns) => chosen;
    }

    /// <summary>An aiming pose, whose turn keeps <paramref name="Direction"/>.</summary>
    /// <param name="Direction">The direction the pose aims.</param>
    internal sealed record Aimed(SamusProjectileDirection Direction) : SamusGroundTurnSource
    {
        /// <inheritdoc/>
        internal override SamusPoseId Turn(SamusPoseId chosen, IdentityTable<SamusProjectileDirection, SamusPoseId> turns) => turns[Direction];
    }
}
