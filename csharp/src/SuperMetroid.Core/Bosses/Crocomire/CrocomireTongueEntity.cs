using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's tongue (#2146): it follows the body through its own animation, and its ordinary touch and shot lead to
/// Crocomire's component contact. Its frames carry no hitboxes (#1870, #1889).
/// </summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="place">The place the tongue occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CrocomireTongueEntity(CrocomireBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CrocomireEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => Species.Fight.InitializeCrocomireTongue(Place);

    /// <summary>The encounter the tongue joined as it set up.</summary>
    internal override CrocomireEnemyState Encounter => Place.RequireSpeciesState<CrocomireTongueState>().Crocomire;

    /// <summary>Takes no action of its own; the tongue follows the body through its animation and frames.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Fight.AdvanceCrocomireTongueAnimation(Place);

    /// <summary>No probe overlaps the tongue: its frames carry no hitboxes.</summary>
    public override bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return false;
    }

    /// <summary>The tongue's ordinary touch leads to Crocomire's component contact.</summary>
    protected override bool OwnsTouch => Place.Definition.Touch == EnemyReaction.NormalEnemyTouch;

    /// <summary>The tongue's ordinary shot leads to his hitboxes' reactions.</summary>
    protected override bool OwnsShot => Place.Definition.Shot == EnemyReaction.NormalEnemyShot;
}
