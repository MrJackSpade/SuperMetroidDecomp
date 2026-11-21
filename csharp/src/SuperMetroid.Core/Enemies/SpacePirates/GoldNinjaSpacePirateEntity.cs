namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The gold ninja Space Pirate (#2179): the ninja's AI with its own armour. Its two body reactions reflect or ignore what
/// does it no damage, bombs neither damage nor reflect from its authored regions, its own reaction shows no hit burst,
/// and its death scatters drops.
/// </summary>
/// <param name="species">The ninja Space Pirate species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class GoldNinjaSpacePirateEntity(NinjaSpacePirateBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : NinjaSpacePirateEntity(species, occupant, place, common)
{
    /// <summary>The gold Ninja's own shot reaction where the definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == SpacePirateReactions.Shot ? Species.Walking.GoldNinjaShotReaction : base.ShotReaction;

    /// <summary>
    /// Runs the gold Ninja's own reaction to a bomb its frames' hitboxes meet with one of the Pirates' own reactions, before
    /// the ignoring check: its body reactions answer only a Power Bomb, and the bomb's overlap with its hitbox has already
    /// been counted, so a normal bomb touches it without damage, an impact, a reflection or its special-drop death.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context) =>
        context.UsesFrameHitboxes && context.SelectedShot != EnemyReaction.OrdinaryShot || base.TryReactToBomb(context);

    /// <summary>The gold Ninja's own stages around the common bomb damage every bomb that reaches it runs.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        WalkingSpacePirateBehavior.GoldNinjaBombReaction;
}
