using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of the two Ridley bodies, Lower Norfair Ridley (<see cref="NorfairRidleyEntity"/>) or Ceres Ridley
/// (<see cref="CeresRidleyEntity"/>): each holds its encounter from its initialization (#2231) and runs the parts of his
/// body both share (#2229): the body animation, whose lift-off and fireball cut it handles (#2230), the drawing of his tail
/// and wings, his hand-authored extended body and tail tip, his shot reaction and his Power Bomb reaction.
/// </summary>
/// <param name="body">The parts of Ridley's body both encounters run.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class RidleyEntity(RidleyBody body, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation,
        IEnemyFrameHitboxes, IEnemyContactHooks, IEnemyAfterSamusContact, IEnemyDrawHooks, IEnemyPrivateShotReaction, IRidleyBodyOwner
{
    /// <summary>The parts of Ridley's body both encounters run.</summary>
    protected RidleyBody Body => body;

    /// <summary>The encounter the body holds from its initialization (#2231).</summary>
    /// <exception cref="InvalidOperationException">The body has begun no encounter.</exception>
    internal abstract RidleyEncounterState Encounter { get; }

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Body.Animation.Advance(Place, Encounter, context.Samus, this);

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

    /// <summary>Resolves Samus against his extended body after the ordinary touch pass.</summary>
    /// <param name="samus">Samus.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    public virtual void ResolveAfterSamusContact(SamusState samus, PseudoScrewCharge pseudoScrew) =>
        _ = Body.Contact.ResolveSamusContact(Place, samus, pseudoScrew);

    /// <summary>Ridley's AI publishes the common writer's shading: its body, wings and tail share one latched phase (#1512).</summary>
    public PictureShading? CommonDrawShading() => Encounter.CommonDrawShading;

    /// <summary>Both Ridley mains draw the tail and wings before the common writer emits the body.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="camera">The camera.</param>
    public virtual void DrawBeforeBody(SpriteFrameBuffer sprites, Position16<RoomPixels> camera) =>
        Body.Drawing.DrawTailAndWings(sprites, Place, Encounter, camera);

    /// <summary>Both Ridley bodies' shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction == RidleyReactions.Shot;
}
