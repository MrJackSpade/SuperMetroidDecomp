namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>One endpoint sample from the live interactive-enemy list.</summary>
/// <param name="Reaction">The grapple reaction declared by the sampled enemy.</param>
/// <param name="Anchor">Anchor position (the sampled enemy's centre) in room pixels.</param>
/// <param name="EnemyDamage">Contact damage supplied when the reaction hurts Samus.</param>
public readonly record struct GrappleEnemyCollision(
    GrappleEnemyReaction Reaction,
    Position16<RoomPixels> Anchor,
    ushort EnemyDamage)
{
    /// <summary>Gets whether the reaction establishes a persistent grapple connection.</summary>
    public bool Attaches => Reaction is
        GrappleEnemyReaction.Attach or
        GrappleEnemyReaction.AttachWithoutInvincibility or
        GrappleEnemyReaction.AttachAndParalyze;
}
