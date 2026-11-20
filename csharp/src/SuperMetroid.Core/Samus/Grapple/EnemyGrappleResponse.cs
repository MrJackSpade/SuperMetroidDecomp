namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// What an enemy's definition says about the grapple beam (#2553): it reacts in one of the beam's ways, or, like the
/// elevator, it has no reaction to the beam and its own behaviour carries on. Only a reacting enemy's reaction is one the
/// grapple's reaction table answers.
/// </summary>
public abstract record EnemyGrappleResponse
{
    /// <summary>Closes the hierarchy to the responses declared here.</summary>
    private EnemyGrappleResponse()
    {
    }

    /// <summary>An enemy that reacts to the beam in <paramref name="Reaction"/>'s way.</summary>
    /// <param name="Reaction">How the enemy reacts to the beam.</param>
    public sealed record Reacting(GrappleEnemyReaction Reaction) : EnemyGrappleResponse;

    /// <summary>
    /// The enemy has no reaction to the grapple beam (#2283, #2558): the elevator, whose own behaviour carries on. The beam
    /// never meets it, and reaching it fails.
    /// </summary>
    public sealed record Unreactive : EnemyGrappleResponse
    {
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
