namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One Metroid occupant: its own AI and reactions, with the shared ones its definition names.</summary>
/// <param name="species">The Metroid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class MetroidEntity(MetroidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, MetroidSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal MetroidSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly MetroidEnemyState state = species.InitializeMetroid(occupant, settings, place);

    /// <summary>Clears the Metroid's outer body before a lethal Power Bomb's generic death.</summary>
    /// <param name="context">The blast meeting the Metroid.</param>
    internal void FinishDeath(EnemyPowerBombContext context) => MetroidBehavior.FinishMetroidDeath(state, Ensure.NotNull(context).Samus);

    /// <summary>Applies frozen missile damage or unfrozen recoil and Ice accumulation once the shot has marked its hit.</summary>
    /// <param name="c">The shot meeting the Metroid.</param>
    /// <returns>Whether the resolution replaces the rest of the shot reaction.</returns>
    internal bool ResolveOrdinaryShot(in EnemyShotContext c) => species.ResolveMetroidOrdinaryShot(c, state);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunMetroidMainAi(Occupant, Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceMetroidTypedAnimation(Place, state);

    /// <summary>The Metroid's own hurt behaviour, ahead of its frozen behaviour when it is both hurt and frozen; the animation still advances.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        MetroidBehavior.ApplyMetroidHurt(Place, state);
        return true;
    }

    /// <summary>The frozen Metroid's behaviour, which retains its outer-body state.</summary>
    public override void RunFrozen() => MetroidBehavior.RunMetroidFrozen(Place, state);

    /// <summary>Whether the Metroid's own touch, its attachment and suit-dependent draining, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == MetroidReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Metroid's own touch where its definition leaves the touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == MetroidReactions.Touch)
            species.ResolveMetroidTouch(context.Place, state, context.Samus);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Metroid's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == MetroidReactions.Shot ? OwnShot : base.ShotReaction;

    /// <summary>The Metroid's own shot reaction, built once for this Metroid with its state (#2554).</summary>
    private EnemyShotReaction OwnShot => field ??= MetroidBehavior.ShotReaction(ResolveOrdinaryShot);

    /// <summary>The Metroid's own Power Bomb reaction, built once for this Metroid with its state (#2554).</summary>
    private EnemyPowerBombReaction OwnPowerBomb => field ??= MetroidBehavior.PowerBombReaction(FinishDeath);

    /// <summary>The Metroid's own bomb reaction, which comes before the ignoring check, where its definition leaves shots to it.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (Occupant.Definition.Shot != MetroidReactions.Shot)
            return base.TryReactToBomb(context);
        species.ReactToMetroidBomb(context, state);
        return true;
    }

    /// <summary>The Metroid's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction =>
        LeavesPowerBombToSpecies ? OwnPowerBomb : base.PowerBombReaction;

    /// <summary>The Metroid's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path selects (none: the definition leaves shots to the Metroid), or the empty frame's.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == MetroidReactions.Shot;
}
