namespace SuperMetroid.Core.Game;

/// <summary>The Tourian entrance statue species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class TourianEntranceStatueBehavior : IEnemySpecies
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Stores presentation.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Initializes a new tourian entrance statue behavior instance.</summary>
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
    /// Native <c>tourian_entrance_statue_animstate</c>. Its producers live in the palette-
    /// FX/HDMA unlocking sequence; zero is the retail room-load state.
    /// </summary>
    public ushort TourianEntranceStatueAnimationState { get; internal set; }

    /// <summary>Resets tourian entrance statue room state.</summary>
    internal void ResetTourianEntranceStatueRoomState()
    {
        TourianEntranceStatueFinished = false;
        TourianEntranceStatueAnimationState = 0;
        TourianEntranceStatueVerticalOffset = 0;
    }

    /// <summary>Initializes tourian entrance statue.</summary>
    internal void InitializeTourianEntranceStatue(RoomEnemySlot statue)
    {
        if (statue.Parameter1 is not (0 or 2 or 4))
        {
            throw new InvalidDataException(
                $"Tourian entrance statue parameter one ${statue.Parameter1:X4} " +
                "is outside the three-entry instruction table.");
        }

        statue.PaletteIndex = 0;
        statue.InstructionTimer = 1;
        statue.Timer = 0;
        statue.CurrentInstruction =
            TourianEntranceStatueInstructionProgramDefinitions.GetInitialInstruction(
                statue.Parameter1);

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
            "Tourian entrance statues require installed palette artwork.")).ApplyEntrance(_presentation._cgram!);
    }

    /// <summary>Spawns tourian entrance statue projectile.</summary>
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

    /// <summary>Position tourian entrance statue projectile.</summary>
    internal void PositionTourianEntranceStatueProjectile(RoomEnemyProjectileSlot projectile)
    {
        if (ClosedNativeWords.Decode<EnemyProjectilePreInstruction>(
                projectile.PreInstruction, "enemy-projectile pre-instruction") ==
                EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess &&
            TourianEntranceStatueAnimationState == 0)
        {
            TourianEntranceStatueFinished = true;
        }

        projectile.Position = projectile.Position.WithWholeX(projectile.Variable0);

        // $86:BA42 adds layer1_y_pos and subtracts HDMA's layer1_y_pos + displacement.
        // The camera terms cancel here; ordinary OBJ drawing subtracts the camera once.
        // This keeps all sprite pieces attached to the descending BG2 statue artwork.
        projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(projectile.Variable1 - TourianEntranceStatueVerticalOffset)));
    }

    /// <summary>Registers tourian entrance statue projectiles.</summary>
    internal void RegisterTourianEntranceStatueProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            PositionTourianEntranceStatueProjectile(context.Projectile);
        }, EnemyProjectilePreInstruction.TourianStatueBaseDecoration_AllowProcess, EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor);
    }

    /// <summary>Registers tourian entrance statue initialization ai.</summary>
    internal void RegisterTourianEntranceStatueInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_TourianStatue, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.TourianStatue, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeTourianEntranceStatue(slot);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetTourianEntranceStatueRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }


    /// <summary>Registers tourian statue main ai.</summary>
    internal static void RegisterTourianStatueMainAi(EnemyAiTable table)
    {
        table.RegisterMain(EnemyAiRoutine.MainAI_TourianStatue, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.TourianStatue, (slot, frame, nmiFrameCounter8) =>
        {
            // $AA:D7C7 is the one-byte RTL immediately before the initializer. The
            // three enemy records animate exclusively through their ROM lists.
        });
    }
}
