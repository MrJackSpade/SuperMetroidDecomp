using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One Ridley occupant: Lower Norfair Ridley, Ceres Ridley or the breakup explosion actor.</summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="ceres">The Ceres Ridley owner.</param>
/// <param name="encounter">The shared Ridley encounter.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RidleyEntity(
    RidleyBehavior ridley, CeresRidleyBehavior ceres, RidleyEncounter encounter, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation,
        IEnemyFrameHitboxes, IEnemyContactHooks, IEnemyShotAdmission, IEnemyDrawHooks, IEnemyPrivateShotReaction
{
    /// <summary>Whether the occupant is one of the two Ridley bodies rather than the breakup actor.</summary>
    private bool IsBody => Slot.DefinitionId is EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres;

    /// <inheritdoc/>
    public void Initialize()
    {
        switch (RidleyPartDefinitions.Parts[Slot.DefinitionId])
        {
            case RidleyPart.Ridley:
                ridley.Combat.InitializeNorfairRidley(Slot);
                break;
            case RidleyPart.Ceres:
                ceres.Flight.InitializeCeresRidley(Slot);
                break;
            case RidleyPart.Explosion:
                ridley.Death.RunRidleyExplosionInitializationAi(Slot);
                break;
            default:
                throw new InvalidOperationException($"Ridley entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (RidleyPartDefinitions.Parts[Slot.DefinitionId])
        {
            case RidleyPart.Ridley:
                ridley.RunRidleyMainAi(Slot, frame);
                break;
            case RidleyPart.Ceres:
                ceres.RunCeresRidleyMainAi(Slot, frame);
                break;
            case RidleyPart.Explosion:
                ridley.Death.RunNorfairRidleyExplosionMain(Slot);
                break;
            default:
                throw new InvalidOperationException($"Ridley entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (RidleyPartDefinitions.Parts[Slot.DefinitionId])
        {
            case RidleyPart.Ridley or RidleyPart.Ceres:
                ridley.Animation.AdvanceRidleyBodyTypedAnimation(Slot, context);
                break;
            case RidleyPart.Explosion:
                RidleyBodyAnimation.AdvanceRidleyBreakupAnimation(Slot);
                break;
            default:
                throw new InvalidOperationException($"Ridley entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <summary>None: both Ridley bodies' definitions name the ordinary touch, and the breakup actor's ignores it.</summary>
    public EnemyReaction? SpeciesTouch => null;

    /// <summary>Both Ridley bodies' shot reaction, which their definitions leave to them; the breakup actor's ignores shots.</summary>
    public EnemyReaction? SpeciesShot => IsBody ? RidleyReactions.Shot : null;

    /// <summary>
    /// Lower Norfair Ridley's hurt behaviour for the post-hit window, which owns hurt frames instead of falling back to
    /// ordinary main AI: movement and phase work run only on even actor frames, while tail projectile armour and hurt
    /// palettes stay live on every frame. Ceres Ridley and the breakup actor take the shared hurt their definitions name.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        if (Slot.DefinitionId != EnemyDefinitionId.Ridley)
            return base.RunHurt(frame);
        ridley.RunRidleyHurt(Slot, frame);
        return true;
    }

    /// <summary>Lower Norfair Ridley's own shot reaction where his definition leaves shots to him, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction =>
        Slot.DefinitionId == EnemyDefinitionId.Ridley && LeavesShotToSpecies ? RidleyInteractions.RidleyShotReaction : base.ShotReaction;

    /// <summary>Runs Ceres Ridley's own reaction to a bomb that selects his shot reaction, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (Slot.DefinitionId != EnemyDefinitionId.RidleyCeres || context.SelectedShot != RidleyReactions.Shot)
            return base.TryReactToBomb(context);
        ceres.Hits.ReactToCeresRidleyBomb(context);
        return true;
    }

    /// <summary>Lower Norfair Ridley's own stages around common bomb damage, for a bomb that selects his shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        Slot.DefinitionId == EnemyDefinitionId.Ridley && context.SelectedShot == RidleyReactions.Shot
            ? RidleyInteractions.RidleyBombReaction
            : base.CommonBombDamageReaction(context);

    /// <summary>Both Ridley bodies' own Power Bomb reaction where their definitions leave the blast to them, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        IsBody && LeavesPowerBombToSpecies ? RidleyInteractions.RidleyPowerBombReaction : base.PowerBombReaction;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        RidleyInteractions.FindRidleyHitboxReaction(Slot, probe, out reaction);

    /// <summary>
    /// Both Ridley bodies use a hand-authored extended body plus a separately solved tail tip, resolved by their own
    /// contact after the ordinary pass; the ordinary touch pass must not flatten either to the definition's 8x8 radius.
    /// </summary>
    public bool OwnsSamusTouch => IsBody;

    /// <inheritdoc/>
    public void ResolveBeforeOrdinaryHits(RoomEnemyFrame frame)
    {
    }

    /// <inheritdoc/>
    public void ResolveAfterSamusContact(SamusState samus)
    {
        if (IsBody)
            _ = ceres.Hits.ResolveRidleyBodySamusContact(samus);
    }

    /// <summary>Ceres Ridley never enters the ordinary shot pass; its encounter resolves its own shots.</summary>
    public bool AdmitsShots() => Slot.DefinitionId != EnemyDefinitionId.RidleyCeres;

    /// <summary>Ridley's AI publishes the common writer's shading: its body, wings and tail share one latched phase (#1512).</summary>
    public PictureShading? CommonDrawShading() => IsBody ? encounter.Ridley?.CommonDrawShading : null;

    /// <summary>Both Ridley mains draw the tail and wings before the common writer emits the body.</summary>
    public void DrawBeforeBody(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        if (IsBody)
            ceres.BabyScene.DrawRidleySupplementalSprites(sprites, Slot, camera);
    }

    /// <summary>
    /// Only shots meet Lower Norfair Ridley's frames' hitboxes; its contact, Ceres Ridley's collision and the
    /// breakup actor's take neither.
    /// </summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId == EnemyDefinitionId.Ridley ? EnemyCapabilities.ProjectileFrameHitboxes : EnemyCapabilities.None;

    /// <summary>Both Ridley bodies' shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        IsBody && reaction == RidleyReactions.Shot;
}
