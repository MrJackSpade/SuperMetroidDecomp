using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Checks Draygon's empty OAM collision frames without claiming Spore Spawn's bank $A5 frames.</summary>
internal static class EnemyTileArtworkDraygonOamCollisionVerification
{
    /// <summary>
    /// Checks native zero-hitbox components and guarded shot and touch lookup. Version 6 grouped
    /// 60 bank $A5 roots under Draygon, but the final eleven belong to Spore Spawn and must be excluded.
    /// </summary>
    internal static void VerifyCompiledDraygonOamCollision(
        CartridgeImportAddressSpace rom)
    {
        // VerifySporeSpawnAnimations confirms each typed Spore Spawn frame's spritemap equals its native operand.
        var sporePointers = SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnAnimationsVerification.BodySpritemaps();

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.DraygonBody, place.Definition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.Position = place.Position.WithWhole(new(0x100, 0x100));

        int draygonFrames = 0;
        int sporeFrames = 0;
        int componentsChecked = 0;
        foreach (EnemyFrameName frame in
                 EnemyExtendedFrameDefinitions.Frames.Take(
                     EnemyExtendedFrameDefinitionsConstants.PreCeresSteamFrameCount))
        {
            if (frame.Bank != DraygonBg2FrameToolingDefinitions.Bank)
                continue;
            if (DraygonBg2FrameDefinitionsConstants.IsFrame(frame.Pointer))
                continue;
            if (sporePointers.Contains(frame.Pointer))
            {
                AssertTrue(frame.Name.StartsWith("spore_spawn_",
                        StringComparison.Ordinal),
                    $"Spore Spawn frame {frame.Pointer:X4} has its own author key");
                AssertTrue(!DraygonCollisionDefinitions.IsEmptyOamFrame(frame.Pointer),
                    $"Spore Spawn frame {frame.Name} is not a Draygon collision frame");
                sporeFrames++;
                continue;
            }

            AssertTrue(DraygonCollisionDefinitions.IsEmptyOamFrame(frame.Pointer),
                $"Draygon OAM frame {frame.Name} has a compiled no-hitbox identity");
            AssertTrue(frame.Name.StartsWith("draygon_",
                    StringComparison.Ordinal),
                $"Draygon frame {frame.Pointer:X4} retains its author family");
            AssertEqual(0,
                DraygonCollision.ComponentsAt(frame.Pointer).Length,
                $"Draygon OAM frame {frame.Name} has no collision components");
            int root = ((int)new SnesAddress(frame.Bank, frame.Pointer));
            int componentCount = rom.ReadCartridgeByte(root);
            AssertTrue(componentCount is > 0 and <= 8,
                $"Draygon OAM frame {frame.Name} has bounded components");
            for (int component = 0; component < componentCount; component++)
            {
                ushort listPointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 2 + component * 8 + 6));
                AssertEqual((ushort)0,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord((((int)new SnesAddress(frame.Bank, listPointer)))),
                    $"Draygon OAM frame {frame.Name} component {component} native hitbox count");
                componentsChecked++;
            }

            place.SpritemapPointer = frame.Pointer;
            object?[] arguments =
                [place, new Position16<RoomPixels>(0x100, 0x100), Extent16<RoomPixels>.Zero, true, default(GameplayAction)];
            AssertTrue(!ExtendedHitboxWalker.Find(arguments),
                $"Draygon OAM frame {frame.Name} has no shot callback without ROM reads");
            arguments[3] = false;
            AssertTrue(!ExtendedHitboxWalker.Find(arguments),
                $"Draygon OAM frame {frame.Name} has no touch callback without ROM reads");
            draygonFrames++;
        }

        AssertEqual(48, draygonFrames,
            "all selected Draygon OAM frames have empty native hitbox lists");
        AssertEqual(11, sporeFrames,
            "remaining bank-$A5 OAM frames are selected by Spore Spawn");
        AssertEqual(DraygonCollisionDefinitions.EmptyOamFrameCount,
            draygonFrames, "compiled Draygon empty-frame set is exact");
        AssertThrows<InvalidDataException>(
            () => DraygonCollision.ComponentsAt(0xee65),
            "Spore Spawn frame cannot enter Draygon collision data");
        Console.WriteLine($"  Draygon OAM collision: {draygonFrames} frames and {componentsChecked} native zero-hitbox components return no shot/touch callback without ROM reads; 11 Spore Spawn frames excluded.");
    }
}
