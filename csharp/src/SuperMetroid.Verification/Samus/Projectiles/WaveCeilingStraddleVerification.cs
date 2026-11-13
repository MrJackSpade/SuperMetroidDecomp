using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Rooms.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Confirms #1650's reading of the cartridge: a Wave shot whose top edge lies above the room's ceiling, in a room eight
/// screens wide, breaks a respawning shot block over terrain installed as solid, as the 100% replay showed in Crocomire's
/// arena at (99, 1). The original horizontal Wave scan (<c>$94:A352</c>) multiplies the top edge's row, 255 for a top edge
/// just above the ceiling, by the room's width as eight-bit operands into an offset two bytes a block; stepping a row adds
/// the width twice, the second addition taking the first's carry, so in a 128-block-wide room the walk wraps past the end
/// of its cycle one byte further on for a leading edge in the room's right half. The block read there (<c>$94:A1B5</c>)
/// is the level word straddling the solid block and the next, which collides as shootable, while the block index is the
/// solid block's: its reaction (<c>$94:9E55</c>) spawns the respawning shot block its behaviour selects there, whose setup
/// (<c>$84:CE6B</c>) keeps the solid block's own collision kind to restore. The port's scan reaches the same block.
/// </summary>
internal static class WaveCeilingStraddleVerification
{
    /// <summary>The room's width in blocks: eight screens, as Crocomire's arena is.</summary>
    private const int RoomWidth = 128;

    /// <summary>The room's height in blocks: one screen.</summary>
    private const int RoomHeight = 16;

    /// <summary>The block Crocomire's arena holds at (99, 1): a solid block.</summary>
    private const int SolidBlockNumber = 0x321;

    /// <summary>The shot's position: eight pixels below the room's top.</summary>
    private const int ShotY = 8;

    /// <summary>The shot's radius down from its position, which puts its top edge four pixels above the ceiling.</summary>
    private const int ShotYRadius = 12;

    /// <summary>The shot's radius along its flight.</summary>
    private const int ShotXRadius = 8;

    /// <summary>
    /// A rightward uncharged Spazer-Ice-Wave shot, as the replay fired, with its top edge above the ceiling and its leading
    /// edge in column 99 of a room eight screens wide, reacts at the solid block (99, 1), where the read straddling it and
    /// (100, 1) collides as shootable air: one respawning shot block spawns there, restoring the solid block's collision
    /// kind. The same pair of blocks at (63, 1) and (64, 1), under a leading edge in column 63, sets off nothing: in the
    /// room's left half the walk wraps on the second addition and reads whole blocks.
    /// </summary>
    internal static void VerifyShotAboveCeilingBreaksOverSolidTerrain()
    {
        StraddledTerrain<RoomTerrainBlock> straddled = InstalledFixtures.SamusOverreads.StraddledTerrain;
        var solid = RoomTerrainBlock.Of(SolidBlockNumber, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomTerrainBlock tail = RoomBlockInventoryVerification.ShootableAirTail(straddled, solid);

        Scan arenaScan = ScanAt(column: 99, solid, tail, straddled);
        RoomLevelData arena = arenaScan.Level;
        RoomObjectSystem arenaObjects = arenaScan.RoomObjects;
        var reacted = new Position16<RoomBlocks>(99, 1);
        RoomObjectSlot[] active = [.. arenaObjects.SlotTable.InProcessingOrder.Where(slot => slot.Active)];
        AssertEqual(1, active.Length, "the shot above the ceiling sets off one reaction in the room's right half");
        AssertEqual(reacted, active[0].Block, "the reaction spawns at the solid block the straddled read starts in");
        AssertEqual(RoomCollisionType.SolidBlock, active[0].RestoreTerrain?.Collision,
            "the respawning shot block restores the solid block's own collision kind, as its setup reads the block it spawned at");
        AssertTrue(arena.GetCollisionBlock(reacted).Terrain != solid, "the solid block is replaced while the shot block is broken");
        AssertEqual(tail, arena.GetCollisionBlock(new(100, 1)).Terrain, "the block after it is left as it was");

        RoomObjectSystem leftObjects = ScanAt(column: 63, solid, tail, straddled).RoomObjects;
        AssertEqual(0, leftObjects.SlotTable.ActiveCount, "the same pair in the room's left half sets off nothing, as the walk reads whole blocks there");
        Console.WriteLine("PASS a Wave shot above the ceiling breaks a respawning shot block over solid terrain where the cartridge's walk straddles it.");
    }

    /// <summary>
    /// Scans a rightward Wave shot above the ceiling whose leading edge lies in <paramref name="column"/>, in an eight-screen
    /// room of air holding <paramref name="solid"/>, selecting the respawning shot block, in that column of row 1 and
    /// <paramref name="tail"/> after it.
    /// </summary>
    private static Scan ScanAt(int column, RoomTerrainBlock solid, RoomTerrainBlock tail,
        StraddledTerrain<RoomTerrainBlock> straddled)
    {
        var terrain = new RoomTerrainBlock[RoomWidth * RoomHeight];
        var behaviors = new RoomBlockBehavior[terrain.Length];
        int index = RoomWidth + column;
        terrain[index] = solid;
        terrain[index + 1] = tail;
        int entry = ShotBlockReactionDefinitions.LeadingEntries.ToList().IndexOf(ShotBlockReaction.RespawningBreak);
        behaviors[index] = RoomBlockBehavior.Reacting(BlockReaction.Common(entry));
        RoomLevelData level = CreateRoom(new(RoomWidth, RoomHeight), terrain, behaviors);
        RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
        var shot = new SamusProjectile(0)
        {
            Position = new(new((ushort)((column * 16) + ShotXRadius), 0), new(ShotY, 0)),
            Radius = new(ShotXRadius, ShotYRadius),
            Velocity = new(1, 0),
            Type = SamusProjectileType.Beam(SamusBeamCombination.SpazerIceWave, charged: false),
        };
        ProjectileTerrainCollision.ScanHorizontalWaveShotReactions(level, shot, roomObjects, straddled);
        return new Scan(level, roomObjects);
    }

    /// <summary>A scanned room and its room objects.</summary>
    /// <param name="Level">The room's terrain after the scan.</param>
    /// <param name="RoomObjects">The room objects the scan's reactions spawned.</param>
    private sealed record Scan(RoomLevelData Level, RoomObjectSystem RoomObjects);

    /// <summary>Runs #1650's Wave-shot-above-the-ceiling check.</summary>
    [VerificationCommand("--wave-ceiling-straddle")]
    internal static int WaveCeilingStraddleCommand()
    {
        Suite(nameof(VerifyShotAboveCeilingBreaksOverSolidTerrain), VerifyShotAboveCeilingBreaksOverSolidTerrain);
        return 0;
    }
}
