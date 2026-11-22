using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's tongue (#2146): it follows the body through its own animation, and its ordinary touch and shot lead to
/// Crocomire's component contact. Its frames carry no hitboxes (#1870, #1889).
/// </summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the tongue occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="tongue">What the enemy's setup built (#2554).</param>
internal sealed partial class CrocomireTongueEntity(CrocomireBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, CrocomireTongueState tongue)
    : CrocomireEntity(species, occupant, place, common)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Crocomire species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the tongue occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="encounter">The encounter his body began, which the tongue joins as it is built.</param>
    internal CrocomireTongueEntity(CrocomireBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        CrocomireEnemyState encounter)
        : this(species, occupant, place, common, species.Fight.InitializeCrocomireTongue(place, encounter))
    {
    }

    /// <summary>The tongue's state, which holds the encounter it joined, from its setup (#2554).</summary>
    private readonly CrocomireTongueState tongue = tongue;
    /// <summary>The encounter the tongue joined as it set up.</summary>
    internal override CrocomireEnemyState Encounter => tongue.Crocomire;

    /// <summary>Takes no action of its own; the tongue follows the body through its animation and frames.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => CrocomireFight.AdvanceCrocomireTongueAnimation(Place, tongue);

    /// <summary>No probe overlaps the tongue: its frames carry no hitboxes.</summary>
    public override EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
    {
        return EnemyHitboxContact.Missed;
    }

    /// <summary>The tongue's ordinary touch leads to Crocomire's component contact.</summary>
    protected override bool OwnsTouch => Occupant.Definition.Touch.IsOrdinary;

    /// <summary>The tongue's ordinary shot leads to his hitboxes' reactions.</summary>
    protected override bool OwnsShot => Occupant.Definition.Shot.IsOrdinary;
}
