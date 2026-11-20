namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// One ninja Space Pirate occupant: its own AI, with the shared reactions. The silver Ninja is this entity; the gold
/// Ninja's armour is its own (<see cref="GoldNinjaSpacePirateEntity"/>, #2179).
/// </summary>
/// <param name="species">The ninja Space Pirate species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal class NinjaSpacePirateEntity(NinjaSpacePirateBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <summary>The ninja Space Pirate species owner.</summary>
    protected NinjaSpacePirateBehavior Species => species;

    /// <inheritdoc/>
    public void Initialize() => NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNinjaSpacePirateMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceNinjaSpacePirateTypedAnimation(Occupant, Place, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => SpacePirateHitboxes.Find(Place, probe, out reaction);

    /// <summary>Pirate contact and shots both meet the frame hitboxes the game defines for the Pirate.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Every ordinary Pirate's touch reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesTouch => SpacePirateReactions.Touch;

    /// <summary>Every ordinary Pirate's shot reaction, which its definition leaves to it.</summary>
    public EnemyReaction? SpeciesShot => SpacePirateReactions.Shot;

    /// <summary>Whether the Pirates' own touch, where the definition leaves Samus's touch to them, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Pirates' own touch where the definition leaves Samus's touch to them, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            species.Walking.ReactToSpacePirateTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Pirates' own shot reaction where the definition leaves shots to them, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => LeavesShotToSpecies ? species.Walking.SpacePirateShotReaction : base.ShotReaction;

    /// <summary>The Pirates' own stages around the common bomb damage every bomb that reaches an ordinary Pirate runs.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        WalkingSpacePirateBehavior.SpacePirateBombReaction;

    /// <summary>The Pirate shot reactions, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        SpacePirateHitboxes.OwnsShotReaction(reaction);
}
