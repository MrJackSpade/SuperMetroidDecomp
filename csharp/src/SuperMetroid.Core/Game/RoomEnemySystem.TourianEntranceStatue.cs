namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA's three inert Tourian entrance statue enemy records. Their visible boss icons
/// and base are bank-$86 actors, while the enemy slots own the ROM instruction lists and
/// the two target-palette rows used by the unlocking sequence.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort TourianEntranceStatueDefinition = 0xefff;

    /// <summary>
    /// Native <c>tourian_entrance_statue_finished</c> bit $8000. The translated enemy-
    /// projectile pre-instruction publishes the same completion latch for debugger hosts.
    /// </summary>
    public bool TourianEntranceStatueFinished { get; private set; }

    /// <summary>
    /// Native <c>tourian_entrance_statue_animstate</c>. Its producers live in the palette-
    /// FX/HDMA unlocking sequence; zero is the retail room-load state.
    /// </summary>
    public ushort TourianEntranceStatueAnimationState { get; internal set; }

    private void ResetTourianEntranceStatueRoomState()
    {
        TourianEntranceStatueFinished = false;
        TourianEntranceStatueAnimationState = 0;
    }

    private void InitializeTourianEntranceStatue(RoomEnemySlot statue)
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
        statue.CurrentInstruction = ReadWord(
            _bus!,
            EnemyRomTablePointers.TourianStatue.InstructionListWords + statue.Parameter1);

        // Only the first of the three enemy records creates the fixed screen actors. The
        // native allocator searches from slot $22 down, so this exact call order gives the
        // decoration, Ridley, and Phantoon their retail OAM ordering.
        if (statue.Parameter1 == 0)
        {
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueBaseDecoration,
                x: 120,
                y: 184);
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatueRidley,
                x: 142,
                y: 85);
            SpawnTourianEntranceStatueProjectile(
                RoomEnemyProjectileKind.TourianStatuePhantoon,
                x: 132,
                y: 136);
        }

        // Statue init writes target palette rows $F and $A from $AA:D785/$D765.
        // This runtime exposes the currently visible CGRAM buffer, so install those rows
        // directly while retaining every cartridge color word.
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(
                240 + color,
                ReadWord(
                    _bus!,
                    EnemyRomTablePointers.TourianStatue.BaseDecorationPaletteWords + color * 2));
            _cgram.SetColor(
                160 + color,
                ReadWord(
                    _bus!,
                    EnemyRomTablePointers.TourianStatue.StatuePaletteWords + color * 2));
        }
    }

    private void SpawnTourianEntranceStatueProjectile(
        RoomEnemyProjectileKind kind,
        ushort x,
        ushort y)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.Variable0 = x;
        projectile.Variable1 = y;
    }

    private void PositionTourianEntranceStatueProjectile(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.PreInstruction ==
                EnemyProjectileCodePointers.PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess &&
            TourianEntranceStatueAnimationState == 0)
        {
            TourianEntranceStatueFinished = true;
        }

        projectile.XPosition = projectile.Variable0;

        // $86:BA42 adds layer1_y_pos and subtracts the live HDMA reveal boundary. Before
        // the unlocking HDMA sequence starts both words are zero, leaving the authored
        // screen-space Y coordinate in variable F. The dedicated HDMA owner can publish a
        // nonzero offset here later without changing projectile identity or list timing.
        projectile.YPosition = projectile.Variable1;
    }
}
