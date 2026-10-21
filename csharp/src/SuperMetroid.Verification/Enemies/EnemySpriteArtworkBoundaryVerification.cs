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
        int enemyCases = 0, projectileCases = 0, spriteObjectCases = 0;

        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        foreach ((ushort x, ushort y, bool clip) in new VerifyEnemySpriteArtworkBoundaryCase[]
            { new VerifyEnemySpriteArtworkBoundaryCase(128, 96, false), new VerifyEnemySpriteArtworkBoundaryCase(511, 1, true), new VerifyEnemySpriteArtworkBoundaryCase(0, ushort.MaxValue, true) })
        {
            var expected = new OamBuffer(); var actual = new OamBuffer();
            DrawImportedEnemySpritemap(source, expected, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(x, y), PaletteBits = 0x0a00, BaseTileIndex = 0x01ff, ClipVerticalWrap = clip, OriginYIsOnScreen = (SnesLittleEndian.HighByteInPlace(y)) == 0 });
            enemies.DrawEnemySpritemap(actual, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new Position16<OamPixels>(x, y), PaletteBits = 0x0a00, BaseTileIndex = 0x01ff, ClipVerticalWrap = clip, OriginYIsOnScreen = (SnesLittleEndian.HighByteInPlace(y)) == 0 });
            Equal(expected, actual, $"enemy {frame.Name}");
            enemyCases++;

            if (frame.Bank != EnemySpritemapDefinitions.RoomSpriteObjectBank || clip) continue;
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[31];
            sprite.Cursor = AnimationProgramCursor<RoomSpriteObjectProgram>.Start(RoomSpriteObjectProgram.SmallExplosion);
            sprite.Frame = RoomSpriteObjectVisualDefinitions.FrameOf(frame.Pointer);
            sprite.Position = sprite.Position.WithWhole(new(x, y));
            sprite.Attributes = new SnesObjAttributeWord(0x0a04);
            actual = new OamBuffer(); expected = new OamBuffer();
            enemies.SpriteObjects.DrawRoomSpriteObjectsAsOam(actual, Position16<RoomPixels>.Zero);
            DrawImportedEnemySpritemap(source, expected, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(x, y), PaletteBits = 0x0a00, BaseTileIndex = 4 });
            Equal(expected, actual, $"room sprite object {frame.Name}");
            sprite.Clear();
            spriteObjectCases++;
        }

        RoomEnemyProjectileSlot roomSlot = enemies.Projectiles.EnemyProjectiles[17];
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameSets.All)
        foreach ((ushort x, ushort y) in new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase[] { new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase(128, 96), new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase(255, 252), new EnemySpriteArtworkBoundaryVerifyEnemySpriteArtworkBoundaryCase(0, ushort.MaxValue) })
        {
            ushort pointer = source.ReadCartridgeWordFixedBank(0x860000 | frame.OperandAddress);
            roomSlot.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
            roomSlot.DrawPriority = EnemyProjectileDrawPriority.High;
            roomSlot.Frame = EnemyProjectileSlotFrameAccess.ProgramFrameAt(frame.OperandAddress);
            roomSlot.Position = roomSlot.Position.WithWhole(new(x, y)); roomSlot.GraphicsIndex = 0x0a04;
            var expected = new OamBuffer(); var actual = new OamBuffer();
            DrawImportedEnemyProjectileSpritemap(source, expected, pointer, new(x, y), 0x0a04, (SnesLittleEndian.HighByteInPlace(y)) == 0);
            enemies.Projectiles.DrawEnemyProjectilePassAsOam(actual, Position16<RoomPixels>.Zero, EnemyProjectileDrawPriority.High, false);
            Equal(expected, actual, $"room projectile {frame.Name}");

            projectileCases++;
        }
        roomSlot.Clear();
        AssertEqual(0, projectileArtwork.Get(EnemyProjectileSpritemapDefinitions.BlankSpritemap).Length,
            "native initial blank projectile is compiled as a zero-part frame");
        foreach (EnemyProjectileFrameId composition in Enum.GetValues<EnemyProjectileFrameId>().Where(EnemyProjectileFrames.IsComposition))
        {
            ushort pointer = EnemyProjectileFrames.Composition(composition);
            string name = composition.ToString();
            roomSlot.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
            roomSlot.DrawPriority = EnemyProjectileDrawPriority.High;
            roomSlot.Position = roomSlot.Position.WithWhole(new(128, 96));
            roomSlot.GraphicsIndex = 0x0a04; roomSlot.Frame = composition;
            var actual = new OamBuffer(); var expected = new OamBuffer();
            enemies.Projectiles.DrawEnemyProjectilePassAsOam(actual, Position16<RoomPixels>.Zero, EnemyProjectileDrawPriority.High, false);
            DrawImportedEnemyProjectileSpritemap(source, expected, pointer, new(128, 96), 0x0a04, true);
            Equal(expected, actual, $"direct projectile {name}");
        }
        roomSlot.Clear();

        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 100)) };
        var elevator = new CeresElevatorArrivalState(samus, projectileArtwork);
        elevator.Step(samus);
        var elevatorActual = new OamBuffer(); var elevatorExpected = new OamBuffer();
        elevator.DrawAsOam(elevatorActual, Position16<RoomPixels>.Zero);
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
        Console.WriteLine($"Enemy OAM boundary: {enemyCases} enemy cases, {spriteObjectCases} room-object frames and {projectileCases} program-frame cases pass through ROM-free owners; Ceres and synthetic arithmetic/timed-projectile checks pass.");

        static void Equal(OamBuffer expected, OamBuffer actual, string context)
        {
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, context + " count");
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
