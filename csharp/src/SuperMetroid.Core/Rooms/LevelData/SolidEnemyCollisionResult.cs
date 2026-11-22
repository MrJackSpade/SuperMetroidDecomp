namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The outcome of one solid-enemy collision probe (#2560): a closed choice of a collision with the enemy met and the
/// whole-pixel gap to it, or no collision with the requested whole distance. No fraction of a pixel is returned.
/// </summary>
public abstract partial record SolidEnemyCollisionResult
{
    /// <summary>Closes the hierarchy to the outcomes declared here.</summary>
    private SolidEnemyCollisionResult()
    {
    }

    /// <summary>Whether the probe met a qualifying body.</summary>
    public bool Collided => this is Collision;

    /// <summary>The whole pixels the move may take: the gap to the enemy met, or the requested whole distance.</summary>
    public abstract int Distance { get; }

    /// <summary>The enemy met, or null when the probe met none.</summary>
    public abstract ISolidEnemy? Enemy { get; }

    /// <summary>
    /// Whether the probe met an enemy Samus was already touching, a zero gap, which drops her fraction of a pixel vertically.
    /// </summary>
    public abstract bool WasTouching { get; }

    /// <summary>
    /// The rounded future box overlaps a qualifying body and the current directional gap is not negative.
    /// </summary>
    /// <param name="Met">The enemy met.</param>
    /// <param name="Gap">The whole-pixel gap, zero or more, between Samus's edge and the body's.</param>
    public sealed partial record Collision(ISolidEnemy Met, int Gap) : SolidEnemyCollisionResult
    {
        /// <inheritdoc/>
        public override int Distance => Gap;

        /// <inheritdoc/>
        public override ISolidEnemy? Enemy => Met;

        /// <inheritdoc/>
        public override bool WasTouching => Gap == 0;
    }

    /// <summary>No qualifying body stops the move.</summary>
    /// <param name="RequestedDistance">The requested distance's whole pixels.</param>
    public sealed partial record NoCollision(int RequestedDistance) : SolidEnemyCollisionResult
    {
        /// <inheritdoc/>
        public override int Distance => RequestedDistance;

        /// <inheritdoc/>
        public override ISolidEnemy? Enemy => null;

        /// <inheritdoc/>
        public override bool WasTouching => false;
    }
}
