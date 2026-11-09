namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The common reactions the grapple beam's block dispatchers name. Every area reaction rejects the beam.
/// </summary>
public static class GrappleBlockReactionDefinitions
{
    /// <summary>The grapple blocks the beam holds onto without changing them.</summary>
    public static readonly IReadOnlyList<BlockReaction> PersistentAnchors = [BlockReaction.Common(0), BlockReaction.Common(3)];

    /// <summary>The grapple block that breaks under the beam and grows back.</summary>
    public static readonly BlockReaction RespawningBreakableAnchor = BlockReaction.Common(1);

    /// <summary>The grapple block that breaks under the beam for good.</summary>
    public static readonly BlockReaction PermanentBreakableAnchor = BlockReaction.Common(2);

    /// <summary>
    /// The spike block Draygon's destroyed turret leaves: the one spike reaction the beam connects to, costing Samus a
    /// periodic-damage unit.
    /// </summary>
    public static readonly BlockReaction BrokenTurretSpike = BlockReaction.Common(3);
}
