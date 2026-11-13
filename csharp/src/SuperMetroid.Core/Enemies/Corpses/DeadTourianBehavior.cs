using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Tourian corpse species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DeadTourianBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. DeadTourianPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DeadTourianEntity(this, slot, common);

    /// <summary>Supplies the room RNG used during decomposition.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies the enemy counter used to pace corpse dust sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Emits dust when a rotting-table row finishes.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Raises each fallen row's dust and its sound request.</summary>
    private readonly CorpseRowDust _rowDust;

    /// <summary>Connects corpse AI to the room's random numbers, frame counter, projectiles and row dust.</summary>
    internal DeadTourianBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, CorpseRowDust rowDust)
    {
        _rowDust = rowDust;
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
    }

    /// <summary>Recognizes the three Tourian corpse enemy definitions handled by this owner.</summary>
    internal static bool IsDeadTourianCorpseDefinition(EnemyDefinitionId definitionId) =>
        definitionId is EnemyDefinitionId.CorpseZoomer or EnemyDefinitionId.CorpseRipper or EnemyDefinitionId.CorpseSkree;

    /// <summary>Selects the species-specific reactions and art layout for a corpse definition.</summary>
    internal static DeadTourianCorpseProfile ProfileForDeadTourianCorpse(EnemyDefinitionId definitionId) =>
        DeadTourianPartDefinitions.Parts[definitionId] switch
        {
            DeadTourianPart.CorpseZoomer => DeadTourianCorpseProfileDefinitions.DeadZoomerProfile,
            DeadTourianPart.CorpseRipper => DeadTourianCorpseProfileDefinitions.DeadRipperProfile,
            DeadTourianPart.CorpseSkree => DeadTourianCorpseProfileDefinitions.DeadSkreeProfile,
            _ => throw new ArgumentOutOfRangeException(nameof(definitionId)),
        };

    /// <summary>Checks that the definition's touch AI points to its species' rot trigger.</summary>
    private static bool HasDeadTourianCorpseTouchOrShotReaction(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.DefinitionId))
            return false;
        return EnemyCollision.TouchAi(slot) == ProfileForDeadTourianCorpse(slot.DefinitionId).TouchAndShot;
    }

    /// <summary>Checks that the definition's shot AI points to its species' rot trigger.</summary>
    internal static bool HasDeadTourianCorpseShotReaction(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.DefinitionId))
            return false;
        return EnemyCollision.ShotAi(slot) == ProfileForDeadTourianCorpse(slot.DefinitionId).TouchAndShot;
    }

    /// <summary>Checks that the definition's power-bomb AI points to its species' rot trigger.</summary>
    private static bool HasDeadTourianCorpsePowerBombReaction(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.DefinitionId))
            return false;
        return EnemyCollision.PowerBombReaction(slot) == ProfileForDeadTourianCorpse(slot.DefinitionId).PowerBomb;
    }

    /// <summary>Initializes a Tourian corpse, checking its corpse is one of its species' before registering state and starting its rot.</summary>
    internal static void InitializeDeadTourianCorpse(RoomEnemySlot slot)
    {
        DeadTourianCorpseProfile profile =
            ProfileForDeadTourianCorpse(slot.DefinitionId);
        DeadTourianCorpseDefinition definition = slot.SpawnSettings<CorpseSpawnSettings>().Corpse;
        int variant = Array.IndexOf(DeadTourianCorpseDefinitions.Variants[profile.Species], definition);
        if (variant < 0)
        {
            throw new InvalidDataException(
                $"Dead {profile.Species} placement names a corpse that is not one of its species'.");
        }
        var state = new DeadTourianCorpseEnemyState(slot, profile, new CorpseRotSchedule(definition.RowCount));
        slot.SetSpeciesState(state);

        slot.Shading = TourianCorpseShades.InDrainedColours;
        state.Phase = DeadTourianCorpsePhase.Waiting;
        state.Playback = DeadSidehopperBehavior.StartDeadTourianAnimation(slot, definition.InitialAnimation);
    }

    /// <summary>Runs the shared corpse behaviour: collision starts a 16-frame delay, then the corpse rots until its last row has fallen.</summary>
    private void RunDeadTourianCorpseMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        switch (state.Phase)
        {
            case DeadTourianCorpsePhase.Waiting:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.SlotIndex) == true)
                    state.Phase = DeadTourianCorpsePhase.PreRotDelay;
                return;
            case DeadTourianCorpsePhase.PreRotDelay:
                state.CountPreRotDelayCall();
                if (state.PreRotDelayCounter >= 16)
                {
                    state.Phase = DeadTourianCorpsePhase.Rotting;
                    slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
                }
                return;
            case DeadTourianCorpsePhase.Rotting:
                RunDeadTourianCorpseRotting(slot, state);
                return;
            case DeadTourianCorpsePhase.Finished:
                return;
            default:
                throw new InvalidDataException(
                    $"Dead {state.Profile.Species} function {state.Phase?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Touch and shot reactions for these three corpse-only actors are identical.</summary>
    internal static void TriggerDeadTourianCorpseRotting(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        state.Phase = DeadTourianCorpsePhase.Rotting;
        slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Power-bomb reactions reject actors whose decomposition already made them intangible.</summary>
    private static void ResolveDeadTourianCorpsePowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTourianCorpseState(slot);
        if (!slot.Contact.HasAny(EnemyContact.IgnoresSamus))
            TriggerDeadTourianCorpseRotting(slot);
    }

    /// <summary>
    /// Runs one rot call, moving the corpse's own rows as the call moves them (#1539), entering inert AI when it completes.
    /// </summary>
    private void RunDeadTourianCorpseRotting(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state)
    {
        bool stillRotting = state.Rot.Step(
            state.Rows.MoveRow,
            _ => FinishDeadTourianCorpseRow(slot));
        if (!stillRotting)
            state.Phase = DeadTourianCorpsePhase.Finished;
    }

    /// <summary>The rows <paramref name="slot"/>'s frames show: its corpse's own, as its rot calls have left them (#1539).</summary>
    internal static CorpseRotRows ShownDeadTourianRows(RoomEnemySlot slot) => RequireDeadTourianCorpseState(slot).Rows.Shown;

    /// <summary>Spawns randomized dust for a completed rot row and gates its sound on the enemy counter.</summary>
    private void FinishDeadTourianCorpseRow(RoomEnemySlot slot) =>
        _rowDust.Spawn(slot, _room.Random.Read(), _projectiles, _frameContext.RandomEnemyCounter);

    /// <summary>Rejects uninitialized slots and slots whose corpse state belongs to another enemy.</summary>
    private static DeadTourianCorpseEnemyState RequireDeadTourianCorpseState(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = slot.SpeciesState<DeadTourianCorpseEnemyState>() ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no dead Tourian corpse state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead Tourian corpse state belongs to another slot.");
        return state;
    }

    /// <summary>Groups one corpse species with its definition's rot-trigger reactions.</summary>
    /// <param name="Species">The corpse species.</param>
    /// <param name="TouchAndShot">The reaction its definition names for touch and shot, which starts its rot.</param>
    /// <param name="PowerBomb">The reaction its definition names for a power bomb, which starts its rot.</param>
    internal sealed record DeadTourianCorpseProfile(
        DeadTourianCorpseSpecies Species,
        EnemyReaction TouchAndShot,
        EnemyReaction PowerBomb);

    /// <summary>Routes bomb hits through the shared shot reaction, which begins decomposition.</summary>
    internal static void AddDeadTourianBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecialAfterNoOp(c => HasDeadTourianCorpseShotReaction(c.Enemy) &&
                                           c.SelectedShotAi == EnemyCollision.ShotAi(c.Enemy),
            c => TriggerDeadTourianCorpseRotting(c.Enemy));
    }

    /// <summary>Advances the Tourian corpses' typed animation.</summary>
    internal static void AdvanceDeadTourianTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        AnimationPlayback<DeadTourianSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, DeadTourianAnimations.Segments, ref playback,
            cue => throw new InvalidDataException($"A corpse pose published {cue.Kind}."));
        state.Playback = playback;
    }

    /// <summary>Runs the Tourian corpses' main AI.</summary>
    internal void RunDeadTourianMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDeadTourianCorpseMain(slot, frame.Samus);
    }

    /// <summary>Registers the power-bomb response, which ignores corpses already marked decomposed.</summary>
    internal static void AddDeadTourianPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(HasDeadTourianCorpsePowerBombReaction)
        {
            ReplacesCommon = (enemy, _) => ResolveDeadTourianCorpsePowerBomb(enemy),
        });
    }

    /// <summary>Registers shot handling that applies the collision prelude and starts decomposition without health damage.</summary>
    internal static void AddDeadTourianShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(HasDeadTourianCorpseShotReaction)
        {
            // The species reactions run after the ordinary projectile prelude, set process-offscreen plus intangibility, and
            // select their species rot reaction. No vulnerability, health damage, or projectile impact follows.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                TriggerDeadTourianCorpseRotting(c.Enemy);
                return true;
            },
        });
    }

    /// <summary>Starts decomposition on physical contact without applying ordinary enemy contact damage.</summary>
    internal static void AddDeadTourianTouch(EnemySharedReactions table)
    {
        // Dead Zoomer/Ripper/Skree definitions intentionally point touch and shot at the same private reaction.
        // Physical contact begins decomposition; these already-dead actors never apply ordinary contact damage.
        table.AddTouch(HasDeadTourianCorpseTouchOrShotReaction,
            context => TriggerDeadTourianCorpseRotting(context.Slot));
    }
}
