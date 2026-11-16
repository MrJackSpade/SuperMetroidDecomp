namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An entity that answers every contact and AI state with the shared responses its definition names (#1312, #1759). A
/// species entity derives from it and overrides the participations it owns, checking its own reaction first and
/// answering with the shared one otherwise. It takes only the common collision paths until the species overrides
/// <see cref="Capabilities"/>, and it owns no private shot reaction unless the species implements
/// <see cref="IEnemyPrivateShotReaction"/>.
/// </summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared responses.</param>
internal abstract class CommonParticipationEnemyEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : IEnemyHurtReaction, IEnemyFrozenAi, IEnemyTouchReaction, IEnemyShotReaction, IEnemyBombReaction, IEnemyPowerBombReaction
{
    /// <summary>The slot the enemy occupies; its definition names the shared responses.</summary>
    protected RoomEnemySlot Slot { get; } = slot;

    /// <summary>The shared responses.</summary>
    protected EnemyCommonParticipation Common { get; } = common;

    /// <inheritdoc/>
    public virtual EnemyCapabilities Capabilities => EnemyCapabilities.None;

    /// <summary>Whether the definition names no shared touch reaction, leaving Samus's touch to the species.</summary>
    protected bool LeavesTouchToSpecies => Slot.Definition.Touch is null;

    /// <summary>Whether the definition names no shared shot reaction, leaving shots and bombs to the species.</summary>
    protected bool LeavesShotToSpecies => Slot.Definition.Shot is null;

    /// <summary>Whether the definition names no shared Power Bomb reaction, leaving the blast to the species or to common damage.</summary>
    protected bool LeavesPowerBombToSpecies => Slot.Definition.PowerBombReaction is null;

    /// <summary>The inert hurt the definition names, which does nothing; false when the definition names the species' own.</summary>
    /// <param name="frame">The enemy frame.</param>
    public virtual bool RunHurt(RoomEnemyFrame frame) => EnemyCommonParticipation.HasInertHurt(Slot.Definition);

    /// <summary>No frozen tail; the common thaw follows.</summary>
    public virtual bool PreemptFrozenTurn() => false;

    /// <summary>No frozen behaviour beyond the common thaw.</summary>
    public virtual void RunFrozen()
    {
    }

    /// <summary>Whether the definition names a shared touch reaction.</summary>
    public virtual bool ReactsToTouch => EnemyCommonParticipation.ReactsToTouch(Slot.Definition);

    /// <summary>Runs the shared touch reaction the definition names.</summary>
    /// <param name="context">The touch.</param>
    public virtual void ReactToTouch(EnemyTouchContext context) => Common.ReactToTouch(context);

    /// <summary>The shared shot reaction the definition names, or null.</summary>
    public virtual EnemyShotReaction? ShotReaction => EnemyCommonParticipation.ShotReaction(Slot.Definition);

    /// <summary>No reaction of its own before the check for the ignoring reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public virtual bool TryReactToBomb(EnemyBombContext context) => false;

    /// <summary>No reaction of its own in place of common bomb damage.</summary>
    /// <param name="context">The bomb contact.</param>
    public virtual bool TryReplaceCommonBombDamage(EnemyBombContext context) => false;

    /// <summary>The plain common bomb damage.</summary>
    /// <param name="context">The bomb contact.</param>
    public virtual EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) => null;

    /// <summary>The shared Power Bomb reaction the definition names, or null for common Power Bomb damage.</summary>
    public virtual EnemyPowerBombReaction? PowerBombReaction => EnemyCommonParticipation.PowerBombReaction(Slot.Definition);
}
