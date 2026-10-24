using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Tourian corpse species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DeadTourianBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. DeadTourianPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DeadTourianEntity(this, slot, common);

    /// <summary>Owns the corpse tile scratch area and room RNG used during decomposition.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies the enemy counter used to pace corpse dust sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Emits dust when a rotting-table row finishes.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Provides installed corpse art and queues its VRAM transfers.</summary>
    private readonly DeadMonsterFrame _deadMonsters;

    /// <summary>Connects corpse AI to the room scratch state, projectiles, and installed art.</summary>
    internal DeadTourianBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, DeadMonsterFrame deadMonsters)
    {
        _deadMonsters = deadMonsters;
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
    }

    /// <summary>The words each physical slot keeps across DeadTourian occupants for the current room.</summary>
    internal readonly DeadTourianExtensionWords[] Extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static DeadTourianExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, EnemySlotTable.MaximumEnemyCount).Select(_ => new DeadTourianExtensionWords())];

    /// <summary>Recognizes the three Tourian corpse enemy headers handled by this owner.</summary>
    internal static bool IsDeadTourianCorpseDefinition(EnemyDefinitionId definitionId) =>
        definitionId is EnemyDefinitionId.CorpseZoomer or EnemyDefinitionId.CorpseRipper or EnemyDefinitionId.CorpseSkree;

    /// <summary>Selects the species-specific native callbacks and art layout for a corpse header.</summary>
    internal static DeadTourianCorpseProfile ProfileForDeadTourianCorpse(EnemyDefinitionId definitionId) =>
        DeadTourianPartDefinitions.Parts[definitionId] switch
        {
            DeadTourianPart.CorpseZoomer => DeadTourianCorpseProfileDefinitions.DeadZoomerProfile,
            DeadTourianPart.CorpseRipper => DeadTourianCorpseProfileDefinitions.DeadRipperProfile,
            DeadTourianPart.CorpseSkree => DeadTourianCorpseProfileDefinitions.DeadSkreeProfile,
            _ => throw new ArgumentOutOfRangeException(nameof(definitionId)),
        };

    /// <summary>Checks that the header's touch AI points to its species' rot trigger.</summary>
    private static bool HasDeadTourianCorpseTouchOrShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.DefinitionId))
            return false;
        return EnemyCollision.TouchAi(slot) == ProfileForDeadTourianCorpse(slot.DefinitionId).TouchAndShot;
    }

    /// <summary>Checks that the header's shot AI points to its species' rot trigger.</summary>
    internal static bool HasDeadTourianCorpseShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.DefinitionId))
            return false;
        return EnemyCollision.ShotAi(slot) == ProfileForDeadTourianCorpse(slot.DefinitionId).TouchAndShot;
    }

    /// <summary>Checks that the header's power-bomb AI points to its species' rot trigger.</summary>
    private static bool HasDeadTourianCorpsePowerBombCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.DefinitionId))
            return false;
        return EnemyCollision.PowerBombReaction(slot) == ProfileForDeadTourianCorpse(slot.DefinitionId).PowerBomb;
    }

    /// <summary>Ports initializers <c>$A9:D849/$D876/$D89F</c>, validating the art variant before registering state and copying tiles into work RAM.</summary>
    internal void InitializeDeadTourianCorpse(RoomEnemySlot slot)
    {
        DeadTourianCorpseProfile profile =
            ProfileForDeadTourianCorpse(slot.DefinitionId);
        if ((slot.Spawn.Population.Parameter1 & 1) != 0 || slot.Spawn.Population.Parameter1 / 2 >= profile.VariantCount)
        {
            throw new InvalidDataException(
                $"Dead {profile.Species} parameter 1 ${slot.Spawn.Population.Parameter1:X4} has no retail variant.");
        }

        int variantIndex = slot.Spawn.Population.Parameter1 / 2;
        DeadTourianCorpseDefinition definition =
            DeadTourianCorpseDefinitions.For(profile.Species, variantIndex);
        // Two rows is the smallest corpse whose late-move entry (height less two) is a real entry index.
        if (definition.EntryCount < 2)
            throw new InvalidDataException($"Dead {profile.Species} has fewer than two corpse rows.");

        int yLimit = definition.EntryCount - 1;
        var state = new DeadTourianCorpseEnemyState(slot, profile.Species, variantIndex,
            new CorpseRottingProgram { YLimit = yLimit, LateMoveEntryIndex = yLimit - 1, WrapOffset = definition.WrapOffset },
            profile: profile, rotTable: _room.Staging.RotTable(definition.RotTableOffset, definition.EntryCount))
        {
            Flesh = definition.Kind,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        state.Function = DeadTourianCorpseFunction.Waiting;
        Extensions[slot.SlotIndex].Program = DeadSidehopperBehavior.StartDeadTourianProgram(slot, definition.InitialProgram);
        CorpseRottingTableProcessor.Initialize(state.RotTable);
        _deadMonsters.BeginRot(state.Flesh);
    }

    /// <summary>Runs shared dispatcher <c>$A9:D8DB</c>: collision starts a 16-frame delay, then the corpse rots until its table is exhausted.</summary>
    private void RunDeadTourianCorpseMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        switch (state.Function)
        {
            case DeadTourianCorpseFunction.Waiting:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                    state.Function = DeadTourianCorpseFunction.PreRotDelay;
                return;
            case DeadTourianCorpseFunction.PreRotDelay:
                state.CountPreRotDelayCall();
                if (state.PreRotDelayCounter >= 16)
                {
                    state.Function = DeadTourianCorpseFunction.Rotting;
                    slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                }
                return;
            case DeadTourianCorpseFunction.Rotting:
                RunDeadTourianCorpseRotting(slot, state);
                return;
            case DeadTourianCorpseFunction.Finished:
                return;
            default:
                throw new InvalidDataException(
                    $"Dead {state.Profile.Species} function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Touch and shot callbacks for these three corpse-only actors are identical.</summary>
    internal static void TriggerDeadTourianCorpseRotting(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        state.Function = DeadTourianCorpseFunction.Rotting;
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Power-bomb callbacks reject actors whose decomposition already set $0400.</summary>
    private static void ResolveDeadTourianCorpsePowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTourianCorpseState(slot);
        if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
            TriggerDeadTourianCorpseRotting(slot);
    }

    /// <summary>Advances the rot table once and selects the step as the rot its kind shows, entering inert AI when it completes.</summary>
    private void RunDeadTourianCorpseRotting(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            state.RotTable,
            state.YLimit,
            state.LateMoveEntryIndex,
            entryIndex => FinishDeadTourianCorpseRow(slot, state));
        if (!stillRotting)
            state.Function = DeadTourianCorpseFunction.Finished;
        _deadMonsters.ShowRotStep(state.Flesh);
    }



    /// <summary>The rot <paramref name="slot"/>'s frames show: its kind's (#1406).</summary>
    internal CorpseRotView ShownDeadTourianRot(RoomEnemySlot slot) =>
        _deadMonsters.Shown(RequireDeadTourianCorpseState(slot).Flesh);

    /// <summary>Spawns randomized dust for a completed rot row and gates its sound on the enemy counter.</summary>
    private void FinishDeadTourianCorpseRow(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state)
    {
        state.FinishedEntryCount++;
        _deadMonsters.SpawnCorpseRowDust(slot, _room.Random.Read(), _projectiles, _frameContext.RandomEnemyCounter);
        state.DustSpawnCount++;
    }

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

    /// <summary>Groups one corpse species' header rot-trigger callbacks with the number of its corpse variants.</summary>
    /// <param name="Species">The corpse species.</param>
    /// <param name="TouchAndShot">The callback its header names for touch and shot, which starts its rot.</param>
    /// <param name="PowerBomb">The callback its header names for a power bomb, which starts its rot.</param>
    /// <param name="VariantCount">How many corpse variants the species has; a population parameter selects one.</param>
    internal sealed record DeadTourianCorpseProfile(
        DeadTourianCorpseSpecies Species,
        EnemyInteractionCallback TouchAndShot,
        EnemyInteractionCallback PowerBomb,
        int VariantCount);

    /// <summary>Routes bomb hits through the shared shot callback, which begins decomposition.</summary>
    internal static void RegisterDeadTourianBombReactions(EnemySharedReactions table)
    {
        table.AddBombSpecialAfterNoOp(c => HasDeadTourianCorpseShotCallback(c.Enemy) &&
                                           c.SelectedShotAi == EnemyCollision.ShotAi(c.Enemy),
            c => TriggerDeadTourianCorpseRotting(c.Enemy));
    }

    /// <summary>Advances DeadTourian's typed animation.</summary>
    internal void AdvanceDeadTourianTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<DeadTourianProgram> cursor = Extensions[slot.SlotIndex].Program;
        EnemyProgramAnimation.Run(slot, DeadTourianAnimations.Programs, ref cursor, DeadTourianFrameDefinitions.ShownFrames.Of,
            (cue, _) => throw new InvalidDataException($"A corpse pose published {cue.Kind}."));
        Extensions[slot.SlotIndex].Program = cursor;
    }

    /// <summary>Ports DeadTourian's main AI.</summary>
    internal void RunDeadTourianMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDeadTourianCorpseMain(slot, frame.Samus);
    }

    /// <summary>Registers the power-bomb response, which ignores corpses already marked decomposed.</summary>
    internal static void RegisterDeadTourianPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(HasDeadTourianCorpsePowerBombCallback)
        {
            ReplacesCommon = (enemy, _) => ResolveDeadTourianCorpsePowerBomb(enemy),
        });
    }

    /// <summary>Registers shot handling that applies the collision prelude and starts decomposition without health damage.</summary>
    internal static void RegisterDeadTourianShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(HasDeadTourianCorpseShotCallback)
        {
            // `$DCF8/$DD08/$DD18` run after the ordinary projectile prelude, set process-offscreen plus $0400, and
            // select their species rot callback. No vulnerability, health damage, or projectile impact follows.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                TriggerDeadTourianCorpseRotting(c.Enemy);
                return true;
            },
        });
    }

    /// <summary>Starts decomposition on physical contact without applying ordinary enemy contact damage.</summary>
    internal static void RegisterDeadTourianTouch(EnemySharedReactions table)
    {
        // Dead Zoomer/Ripper/Skree headers intentionally point touch and shot at the same private callback.
        // Physical contact begins decomposition; these already-dead actors never apply ordinary contact damage.
        table.AddTouch(HasDeadTourianCorpseTouchOrShotCallback,
            context => TriggerDeadTourianCorpseRotting(context.Slot));
    }
}
