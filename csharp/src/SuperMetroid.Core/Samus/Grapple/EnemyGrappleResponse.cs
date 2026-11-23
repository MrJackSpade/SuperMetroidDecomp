namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// What an enemy's definition says about the grapple beam (#2553): it reacts in one of the beam's ways, or, like the
/// elevator, it has no reaction to the beam and its own behaviour carries on. Only a reacting enemy's reaction is one the
/// grapple's reaction table answers.
/// </summary>
public abstract partial record EnemyGrappleResponse
{
    /// <summary>Closes the hierarchy to the responses declared here.</summary>
    private EnemyGrappleResponse()
    {
    }

    /// <summary>How the beam that meets the enemy reacts: the enemy's own reaction, or none.</summary>
    public abstract GrappleEnemyReaction BeamReaction { get; }

    /// <summary>An enemy that reacts to the beam in <paramref name="Reaction"/>'s way.</summary>
    /// <param name="Reaction">How the enemy reacts to the beam.</param>
    public sealed partial record Reacting(GrappleEnemyReaction Reaction) : EnemyGrappleResponse
    {
        /// <inheritdoc/>
        public override GrappleEnemyReaction BeamReaction => Reaction;
    }

    /// <summary>
    /// The enemy has no reaction of the beam's own to the grapple beam (#2283, #2558): the elevator, whose grapple behaviour is
    /// its main behaviour. The beam meets it as the game's endpoint test does: the enemy is held grappled,
    /// and the beam, finding none of the shared grapple behaviours, reads it as it reads an enemy with no interaction; the
    /// held enemy's own behaviour carries on, and nothing releases it.
    /// </summary>
    public sealed partial record Unreactive : EnemyGrappleResponse
    {
        /// <inheritdoc/>
        public override GrappleEnemyReaction BeamReaction => GrappleEnemyReaction.None;

        /// <summary>The one unreactive response.</summary>
        public static Unreactive Instance { get; } = new();

        /// <summary>Creates the one unreactive response.</summary>
        private Unreactive()
        {
        }
    }

    /// <summary>The response of an enemy that reacts to the beam in <paramref name="reaction"/>'s way.</summary>
    public static implicit operator EnemyGrappleResponse(GrappleEnemyReaction reaction) => new Reacting(reaction);
}
