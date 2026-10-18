using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian entrance statue species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class TourianEntranceStatueBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.TourianStatue];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new TourianEntranceStatueEntity(this, slot, common);

    /// <summary>Provides installed statue colors and room-owned state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates the three fixed statue decoration actors in native reverse-slot order.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies visible CGRAM for the entrance statue's initial palette rows.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds room state, projectile allocation, and installed presentation to the statue species.</summary>
    internal TourianEntranceStatueBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPresentation presentation)
    {
        _room = room;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>
    /// Native <c>tourian_entrance_statue_finished</c> bit $8000. The translated enemy-
    /// projectile pre-instruction publishes the same completion latch for debugger hosts.
    /// </summary>
    public bool TourianEntranceStatueFinished { get; internal set; }
    /// <summary>Signed BG2 displacement owned by $88:DC69; OBJ follows the same sinking statue.</summary>
    public short TourianEntranceStatueVerticalOffset { get; internal set; }

    /// <summary>
    /// The statues' shared animation state, set and cleared by the statue tile programs and the descent;
    /// <see cref="TourianStatueAnimationState.None"/> is the room-load state.
    /// </summary>
    public TourianStatueAnimationState TourianEntranceStatueAnimationState { get; internal set; }

    /// <summary>Clears the completion, palette-animation, and BG2 displacement latches on room load.</summary>
    internal void ResetTourianEntranceStatueRoomState()
    {
        TourianEntranceStatueFinished = false;
        TourianEntranceStatueAnimationState = TourianStatueAnimationState.None;
        TourianEntranceStatueVerticalOffset = 0;
    }

    /// <summary>The words each physical slot keeps across TourianEntranceStatue occupants for the current room.</summary>
    internal readonly TourianEntranceStatueExtensionWords[] _extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static TourianEntranceStatueExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, RoomEnemySystem.MaximumEnemyCount).Select(_ => new TourianEntranceStatueExtensionWords())];

    /// <summary>Installs one of three animations and lets only the first statue record spawn the fixed decorative actors.</summary>
    internal void InitializeTourianEntranceStatue(RoomEnemySlot statue)
    {
        if (statue.Parameter1 is not (0 or 2 or 4))
        {
            throw new InvalidDataException(
                $"Tourian entrance statue parameter one ${statue.Parameter1:X4} " +
                "is outside the three-entry instruction table.");
        }

        statue.PaletteIndex = 0;
        statue.InstructionTimer = NativeCountdown.Start(1);
        statue.Timer = NativeCountdown.Start(0);
        _extensions[statue.SlotIndex].Program = AnimationProgramCursor<TourianEntranceStatueProgram>.Start(
            TourianEntranceStatueAnimationDefinitions.InitialProgram(checked((ushort)statue.Parameter1)));

        // Only the first of the three enemy records creates the fixed screen actors. The
        // native allocator searches from slot $22 down, so this exact call order gives the
        // decoration, Ridley, and Phantoon their retail OAM ordering.
        if (statue.Parameter1 == 0)
        {
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueBaseDecoration,
                position: new(120, 184));
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueRidley,
                position: new(142, 85));
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatuePhantoon,
                position: new(132, 136));
        }

        // Statue init writes target palette rows $F and $A from $AA:D785/$D765.
        // This runtime exposes the currently visible CGRAM buffer, so install those rows
        // directly while retaining every cartridge color word.
        (_room.Artwork?.TourianStatueColors ?? throw new InvalidDataException(
            "Tourian entrance statues require installed palette artwork.")).ApplyEntrance(_presentation.Cgram);
    }

    /// <summary>Allocates a decorative projectile when a slot is free and retains its authored position in variable words.</summary>
    internal void SpawnTourianEntranceStatueProjectile(
        RoomEnemyProjectileKind kind,
        Position16<RoomPixels> position)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);
        projectile.Position = projectile.Position.WithWhole(position);
        // The projectile's two general words retain the authored spawn coordinates.
        projectile.Variable0 = position.X;
        projectile.Variable1 = position.Y;
    }

    /// <summary>Tracks the BG2 statue's vertical displacement and publishes the base decoration's completion latch.</summary>
    internal void PositionTourianEntranceStatueProjectile(RoomEnemyProjectileSlot projectile)
    {
        if (ClosedNativeWords.Decode<EnemyProjectilePreInstruction>(
                projectile.PreInstruction, "enemy-projectile pre-instruction") ==
                EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess &&
            TourianEntranceStatueAnimationState == TourianStatueAnimationState.None)
        {
            TourianEntranceStatueFinished = true;
        }

        projectile.Position = projectile.Position.WithWholeX(checked((ushort)projectile.Variable0));

        // $86:BA42 adds layer1_y_pos and subtracts HDMA's layer1_y_pos + displacement.
        // The camera terms cancel here; ordinary OBJ drawing subtracts the camera once.
        // This keeps all sprite pieces attached to the descending BG2 statue artwork.
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Variable1 - TourianEntranceStatueVerticalOffset)));
    }

    /// <summary>Registers the shared pre-instruction for base, Ridley, and Phantoon decoration actors.</summary>
    internal void RegisterTourianEntranceStatueProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            PositionTourianEntranceStatueProjectile(context.Projectile);
        }, EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess, EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor);
    }

    /// <summary>Advances TourianEntranceStatue's typed animation.</summary>
    internal void AdvanceTourianEntranceStatueTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<TourianEntranceStatueProgram> cursor = _extensions[slot.SlotIndex].Program ??
            throw new InvalidOperationException($"Tourian entrance statue slot {slot.SlotIndex} has no animation.");
        EnemyProgramAnimation.Run(slot, TourianEntranceStatueAnimations.Programs, ref cursor, TourianEntranceStatueFrameDefinitions.Spritemap);
        _extensions[slot.SlotIndex].Program = cursor;
    }

    /// <summary>Ports TourianEntranceStatue's initialization AI.</summary>
    internal void RunTourianEntranceStatueInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeTourianEntranceStatue(slot);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        for (int slotIndex = 0; slotIndex < _extensions.Length; slotIndex++)
            _extensions[slotIndex] = new TourianEntranceStatueExtensionWords();
        ResetTourianEntranceStatueRoomState();
    }

    /// <summary>Ports TourianEntranceStatue's main AI.</summary>
    internal static void RunTourianEntranceStatueMainAi(RoomEnemySlot _, IEnemyAiFrame _1)
    {
        // $AA:D7C7 is the one-byte RTL immediately before the initializer. The
        // three enemy records animate exclusively through their ROM lists.
    }
}
