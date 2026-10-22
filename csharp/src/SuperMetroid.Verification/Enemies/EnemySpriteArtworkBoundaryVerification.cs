using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.HardwareVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Programs;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>Checks that enemy OAM and projectile sprite owners use installed artwork after cartridge readers are removed.</summary>
internal static class EnemySpriteArtworkBoundaryVerification
{
    /// <summary>Verifies the statically identified enemy OAM owners after deleting their address readers.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for enemy projectile presentation spritemap pointers")]
    internal static void VerifyEnemySpriteArtworkBoundary(string sourceRom)
    {
        using var temporary = new TestScratchDirectory("map-catalog");
        var installation = GameAssetInstaller.Install(sourceRom, temporary.Root);
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        InstalledEnemyArtwork artwork = installation.LoadEnemyTiles();
        EnemyProjectileSpritemapCatalog projectileArtwork = (artwork.Enemies.ProjectileSpritemaps ?? throw new InvalidOperationException("Verification requires artwork.ProjectileSpritemaps."));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        int enemyCases = 0, projectileCases = 0;

        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        foreach ((ushort x, ushort y, bool clip) in new VerifyEnemySpriteArtworkBoundaryCase[]
            { new VerifyEnemySpriteArtworkBoundaryCase(128, 96, false), new VerifyEnemySpriteArtworkBoundaryCase(511, 1, true), new VerifyEnemySpriteArtworkBoundaryCase(0, ushort.MaxValue, true) })
        {
            var expected = new OamBuffer(); var actual = new OamBuffer();
            DrawImportedEnemySpritemap(source, expected, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(x, y), PaletteBits = 0x0a00, BaseTileIndex = 0x01ff, ClipVerticalWrap = clip, OriginYIsOnScreen = (SnesLittleEndian.HighByteInPlace(y)) == 0 });
            enemies.DrawEnemySpritemap(actual, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new Position16<OamPixels>(x, y), PaletteBits = 0x0a00, BaseTileIndex = 0x01ff, ClipVerticalWrap = clip, OriginYIsOnScreen = (SnesLittleEndian.HighByteInPlace(y)) == 0 });
            Equal(expected, actual, $"enemy {frame.Name}");
            enemyCases++;

        }

        // Spawned objects draw from the sheets the extraction composes from these compositions (#1392): each installed
        // composition draws the cartridge's OAM.
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameSets.All)
        foreach ((ushort x, ushort y) in new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase[] { new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase(128, 96), new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase(255, 252), new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase(0, ushort.MaxValue) })
        {
            ushort pointer = source.ReadCartridgeWordFixedBank(0x860000 | frame.OperandAddress);
            var expected = new OamBuffer(); var actual = new OamBuffer();
            // The sprite list places every part at its signed position and leaves clipping at the screen edges to the
            // backend; native OAM instead parks a part that crosses the top or bottom edge at Y=$F0. The oracle therefore
            // places the native parts without that clip.
            ImportedSpritemapOracle.DrawEnemy(CartridgeImportSource.Require(source), expected, new NativeEnemySpritemapDraw
            {
                Bank = 0x8d, Pointer = pointer, Origin = new(x, y),
                PaletteBits = new SnesObjAttributeWord(0x0a04).PaletteBits, BaseTileIndex = 0x04, ClipVerticalWrap = false,
            });
            actual.AddEnemySpritemap(projectileArtwork.GetProgramFrame(frame.OperandAddress).Span, new Position16<OamPixels>(x, y),
                new SnesObjAttributeWord(0x0a04).PaletteBits, 0x04);
            Equal(expected, actual, $"room projectile {frame.Name} at {x},{y}");

            projectileCases++;
        }
        AssertEqual(0, projectileArtwork.Get(EnemyProjectileSpritemapDefinitions.BlankSpritemap).Length,
            "native initial blank projectile is compiled as a zero-part frame");
        foreach (EnemyProjectileFrameId composition in Enum.GetValues<EnemyProjectileFrameId>().Where(EnemyProjectileFrames.IsComposition))
        {
            ushort pointer = EnemyProjectileFrames.Composition(composition);
            var actual = new OamBuffer(); var expected = new OamBuffer();
            actual.AddEnemySpritemap(projectileArtwork.Get(pointer).Span, new Position16<OamPixels>(128, 96), 0x0a00, 0x04,
                clipVerticalWrap: true, originYIsOnScreen: true);
            DrawImportedEnemyProjectileSpritemap(source, expected, pointer, new(128, 96), 0x0a04, true);
            Equal(expected, actual, $"direct projectile {composition}");
        }

        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 100)) };
        var elevator = new CeresElevatorArrivalState(samus);
        elevator.Step(samus);
        var elevatorActual = new OamBuffer(); var elevatorExpected = new OamBuffer();
        elevator.DrawAsOam(elevatorActual, SuperMetroid.TestSupport.RepositoryInstallation.Verification.SpawnedObjectFixtures.Presentation(SuperMetroid.Core.World.RoomStates.CeresRoomStateDefinitions.CeresElev0), Position16<RoomPixels>.Zero);
        foreach (var actor in new[] { elevator.pad, elevator.platform })
        {
            var position = actor.Position;
            DrawImportedEnemyProjectileSpritemap(source, elevatorExpected, actor.SpritemapPointer,
                position.ToScreen(Position16<RoomPixels>.Zero).ToOam(), checked((ushort)actor.GraphicsIndex), (SnesLittleEndian.HighByteInPlace(position.Y)) == 0);
        }
        Equal(elevatorExpected, elevatorActual, "Ceres elevator owner ordering and placements");

        Suite(nameof(VerifyOamSpritemapPacking), () => VerifyOamSpritemapPacking());
        AssertTrue(!MetadataShape.DeclaresMethod(typeof(OamBuffer), "AddEnemyProjectileSpritemap") &&
            !MetadataShape.DeclaresMethod(typeof(OamBuffer), "ReadSpritemapByte", BindingFlags.Static | BindingFlags.NonPublic),
            "Core OAM cannot recover a generic spritemap address reader");
        Console.WriteLine($"Enemy OAM boundary: {enemyCases} enemy and room-object cases and {projectileCases} program-frame cases pass through ROM-free owners; Ceres and synthetic arithmetic/timed-projectile checks pass.");

        static void Equal(OamBuffer expected, OamBuffer actual, string context)
        {
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, context + " count");
            for (int sprite = 0; sprite < expected.NextByteOffset / OamBuffer.LowRecordByteCount; sprite++)
                AssertEqual(expected.GetEntry(sprite), actual.GetEntry(sprite), $"{context} OAM sprite {sprite}");
            AssertTrue(expected.ImageEquals(actual), context + " packed OAM");
        }
    }

    /// <summary>Runs installed enemy, projectile, and room-object OAM comparisons against native spritemap drawing.</summary>
    [VerificationCommand("--enemy-sprite-artwork-boundary")]
    internal static int EnemySpriteArtworkBoundaryCommand()
    {
        string enemySpriteRom = RepositoryRomPath;
        Suite(nameof(VerifyEnemySpriteArtworkBoundary), () => VerifyEnemySpriteArtworkBoundary(enemySpriteRom));
        return 0;
    }
}
