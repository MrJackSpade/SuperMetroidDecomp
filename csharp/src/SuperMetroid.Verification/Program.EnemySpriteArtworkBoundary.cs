using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the statically identified enemy OAM owners after deleting their address readers.</summary>
    private static void VerifyEnemySpriteArtworkBoundary(string sourceRom)
    {
        using var temporary = new MapCatalogTestDirectory();
        var installation = GameAssetInstaller.Install(sourceRom, temporary.Root);
        var source = CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        EnemyTileArtworkCatalog artwork = installation.LoadEnemyTiles();
        EnemyProjectileSpritemapCatalog projectileArtwork = artwork.ProjectileSpritemaps!;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, memory);
        MethodInfo drawEnemy = typeof(RoomEnemySystem).GetMethod("DrawEnemySpritemap", flags)!;
        MethodInfo drawProjectile = typeof(RoomEnemySystem).GetMethod("DrawEnemyProjectilePass", flags)!;
        MethodInfo drawSpriteObjects = typeof(RoomEnemySystem).GetMethod("DrawRoomSpriteObjects", flags)!;
        int enemyCases = 0, projectileCases = 0, spriteObjectCases = 0;

        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        foreach ((ushort x, ushort y, bool clip) in new (ushort, ushort, bool)[]
            { (128, 96, false), (511, 1, true), (0, ushort.MaxValue, true) })
        {
            var expected = new OamBuffer(); var actual = new OamBuffer();
            DrawImportedEnemySpritemap(source, expected, frame.Bank, frame.Pointer,
                x, y, 0x0a00, 0x01ff, clip, (y & 0xff00) == 0);
            drawEnemy.Invoke(enemies, [actual, frame.Bank, frame.Pointer, x, y,
                (ushort)0x0a00, (ushort)0x01ff, clip, (y & 0xff00) == 0]);
            Equal(expected, actual, $"enemy {frame.Name}");
            enemyCases++;

            if (frame.Bank != EnemySpritemapDefinitions.RoomSpriteObjectBank || clip) continue;
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[31];
            sprite.InstructionPointer = 1;
            sprite.SpritemapPointer = frame.Pointer;
            sprite.XPosition = x; sprite.YPosition = y;
            sprite.GraphicsIndex = 0x0a04;
            actual = new OamBuffer(); expected = new OamBuffer();
            drawSpriteObjects.Invoke(enemies, [actual, (ushort)0, (ushort)0]);
            DrawImportedEnemySpritemap(source, expected, frame.Bank, frame.Pointer, x, y, 0x0a00, 4);
            Equal(expected, actual, $"room sprite object {frame.Name}");
            sprite.Clear();
            spriteObjectCases++;
        }

        var motherBrain = new MotherBrainEnemyProjectileSystem();
        RoomEnemyProjectileSlot roomSlot = enemies.EnemyProjectiles[17];
        MotherBrainEnemyProjectileSlot brainSlot = motherBrain.Slots[17];
        foreach (EnemyProjectilePresentationFrameDefinition frame in EnemyProjectilePresentationFrameDefinitions.All)
        foreach ((ushort x, ushort y) in new (ushort, ushort)[] { (128, 96), (255, 252), (0, ushort.MaxValue) })
        {
            ushort pointer = (ushort)(source.ReadCartridgeByte(0x860000 | frame.OperandAddress) |
                source.ReadCartridgeByte(0x860000 | (ushort)(frame.OperandAddress + 1)) << 8);
            roomSlot.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
            roomSlot.DrawPriority = EnemyProjectileDrawPriority.High;
            roomSlot.PresentationOperandAddress = frame.OperandAddress;
            roomSlot.SpritemapPointer = EnemyProjectileSpritemapDefinitions.BlankSpritemap;
            roomSlot.XPosition = x; roomSlot.YPosition = y; roomSlot.GraphicsIndex = 0x0a04;
            var expected = new OamBuffer(); var actual = new OamBuffer();
            DrawImportedEnemyProjectileSpritemap(source, expected, pointer, x, y, 0x0a04, (y & 0xff00) == 0);
            drawProjectile.Invoke(enemies, [actual, (ushort)0, (ushort)0, EnemyProjectileDrawPriority.High, false]);
            Equal(expected, actual, $"room projectile {frame.Name}");

            brainSlot.ProjectileId = MotherBrainEnemyProjectileSystem.ProjectileDefinition;
            brainSlot.Properties = 0x1000;
            brainSlot.PresentationOperandAddress = frame.OperandAddress;
            brainSlot.XPosition = x; brainSlot.YPosition = y; brainSlot.GraphicsIndex = 0x0a04;
            actual = new OamBuffer();
            motherBrain.DrawHighPriority(actual, projectileArtwork, 0, 0);
            Equal(expected, actual, $"Mother Brain projectile {frame.Name}");
            brainSlot.Properties = 0;
            actual = new OamBuffer();
            motherBrain.DrawLowPriority(actual, projectileArtwork, 0, 0);
            Equal(expected, actual, $"Mother Brain low projectile {frame.Name}");
            projectileCases++;
        }
        roomSlot.Clear(); brainSlot.Clear();
        AssertEqual(0, projectileArtwork.Get(EnemyProjectileSpritemapDefinitions.BlankSpritemap).Length,
            "native initial blank projectile is compiled as a zero-part frame");
        foreach ((ushort pointer, string name) in EnemyProjectileSpritemapDefinitions.Frames)
        {
            roomSlot.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
            roomSlot.DrawPriority = EnemyProjectileDrawPriority.High;
            roomSlot.XPosition = 128; roomSlot.YPosition = 96;
            roomSlot.GraphicsIndex = 0x0a04; roomSlot.SpritemapPointer = pointer;
            var actual = new OamBuffer(); var expected = new OamBuffer();
            drawProjectile.Invoke(enemies, [actual, (ushort)0, (ushort)0, EnemyProjectileDrawPriority.High, false]);
            DrawImportedEnemyProjectileSpritemap(source, expected, pointer, 128, 96, 0x0a04, true);
            Equal(expected, actual, $"direct projectile {name}");
        }
        roomSlot.Clear();

        var samus = new SamusState { XPosition = 128, YPosition = 100 };
        var elevator = new CeresElevatorArrivalState(memory, samus, projectileArtwork);
        elevator.Step(samus);
        var elevatorActual = new OamBuffer(); var elevatorExpected = new OamBuffer();
        elevator.Draw(elevatorActual, 0, 0);
        foreach (string name in new[] { "pad", "platform" })
        {
            object actor = typeof(CeresElevatorArrivalState).GetField(name, flags)!.GetValue(elevator)!;
            ushort Read(string property) => (ushort)actor.GetType().GetProperty(property)!.GetValue(actor)!;
            ushort y = Read("YPosition");
            DrawImportedEnemyProjectileSpritemap(source, elevatorExpected, Read("SpritemapPointer"),
                Read("XPosition"), y, Read("GraphicsIndex"), (y & 0xff00) == 0);
        }
        Equal(elevatorExpected, elevatorActual, "Ceres elevator owner ordering and placements");

        VerifyOamSpritemapPacking();
        VerifyMotherBrainProjectileRendering();
        VerifyMiscDustProjectiles();
        AssertTrue(typeof(OamBuffer).GetMethod("AddEnemyProjectileSpritemap") is null &&
            typeof(OamBuffer).GetMethod("ReadSpritemapByte", BindingFlags.Static | BindingFlags.NonPublic) is null,
            "Core OAM cannot recover a generic spritemap address reader");
        Console.WriteLine($"Enemy OAM boundary: {enemyCases} enemy cases, {spriteObjectCases} room-object frames and {projectileCases} program-frame cases pass through ROM-free owners; Ceres and synthetic arithmetic/timed-projectile checks pass.");

        static void Equal(OamBuffer expected, OamBuffer actual, string context)
        {
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, context + " count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) &&
                expected.HighTable.SequenceEqual(actual.HighTable), context + " packed OAM");
        }
    }
}
