using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of the two Ridley bodies, Lower Norfair Ridley (<see cref="NorfairRidleyEntity"/>) or Ceres Ridley
/// (<see cref="CeresRidleyEntity"/>): both share the body animation and drawing, his hand-authored extended body and
/// tail tip, his shot reaction and his Power Bomb reaction.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="ceres">The Ceres Ridley owner.</param>
/// <param name="encounter">The shared Ridley encounter.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class RidleyEntity(
    RidleyBehavior ridley, CeresRidleyBehavior ceres, RidleyEncounter encounter, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation,
        IEnemyFrameHitboxes, IEnemyContactHooks, IEnemyAfterSamusContact, IEnemyDrawHooks, IEnemyPrivateShotReaction, IRidleyBodyOwner
{
    /// <summary>The Lower Norfair Ridley owner.</summary>
    protected RidleyBehavior NorfairRidley => ridley;

    /// <summary>The Ceres Ridley owner.</summary>
    protected CeresRidleyBehavior Ceres => ceres;

    /// <summary>The shared Ridley encounter.</summary>
    protected RidleyEncounter Encounter => encounter;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        NorfairRidley.Animation.AdvanceRidleyBodyTypedAnimation(Place, context, this);

    /// <inheritdoc/>
    public abstract void LiftOff();

    /// <inheritdoc/>
    public abstract RidleyBodySegment? CutFireballingShort(SamusState samus);

    /// <summary>None: both Ridley bodies' definitions name the ordinary touch.</summary>
    public EnemyReaction? SpeciesTouch => null;

    /// <summary>Both Ridley bodies' shot reaction, which their definitions leave to them.</summary>
    public EnemyReaction? SpeciesShot => RidleyReactions.Shot;

    /// <summary>Both Ridley bodies' own Power Bomb reaction where their definitions leave the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        LeavesPowerBombToSpecies ? RidleyInteractions.RidleyPowerBombReaction : base.PowerBombReaction;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        RidleyInteractions.FindRidleyHitboxReaction(Place, probe, out reaction);

    /// <summary>
    /// Both Ridley bodies use a hand-authored extended body plus a separately solved tail tip, resolved by their own
    /// contact after the ordinary pass; the ordinary touch pass must not flatten either to the definition's 8x8 radius.
    /// </summary>
    public bool OwnsSamusTouch => true;

    /// <inheritdoc/>
    public void ResolveBeforeOrdinaryHits(RoomEnemyFrame frame)
    {
    }

    /// <inheritdoc/>
    public void ResolveAfterSamusContact(SamusState samus, PseudoScrewCharge pseudoScrew) =>
        _ = Ceres.Hits.ResolveRidleyBodySamusContact(samus, pseudoScrew);

    /// <summary>Ridley's AI publishes the common writer's shading: its body, wings and tail share one latched phase (#1512).</summary>
    public PictureShading? CommonDrawShading() => encounter.Ridley?.CommonDrawShading;

    /// <summary>Both Ridley mains draw the tail and wings before the common writer emits the body.</summary>
    public void DrawBeforeBody(SpriteFrameBuffer sprites, Position16<RoomPixels> camera) =>
        Ceres.BabyScene.DrawRidleySupplementalSprites(sprites, Place, camera);

    /// <summary>Both Ridley bodies' shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction == RidleyReactions.Shot;
}
