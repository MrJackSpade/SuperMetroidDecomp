using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Cross-checks compiled Spore Spawn component lists and collision callbacks against cartridge data.</summary>
internal static class EnemyTileArtworkSporeSpawnCollisionVerification
{
    /// <summary>Compares all twelve visual roots, their hitbox records, and live touch/shot overlap results with an independent cartridge walker.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Spore Spawn collision")]
    internal static void VerifyCompiledSporeSpawnCollision(
        SuperMetroidAddressSpace rom)
    {
        // VerifySporeSpawnAnimations confirms each typed Spore Spawn frame's spritemap equals its native operand.
        var selected = SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnAnimationsVerification.BodySpritemaps();

        var installed = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);

        RoomEnemySlot installedSlot = installed.SlotTable.Slots[0];
        installedSlot.Bind(installedSlot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
            {
                Bank = DraygonBg2FrameDefinitions.Bank,
            });
        // The test-only cartridge walker supplies an independent reference.
        installedSlot.Bind(EnemyDefinitionId.SporeSpawn, installedSlot.Definition);
        installedSlot.Position = installedSlot.Position.WithWhole(new(0x100, 0x100));

        var seenLists = new HashSet<ushort>();
        int frameCount = 0;
        int callbackComparisons = 0;
        foreach (EnemyExtendedFrameDefinition frame in
                 EnemyExtendedFrameDefinitions.Frames)
        {
            if (frame.Bank != DraygonBg2FrameDefinitions.Bank ||
                !SporeSpawnCollisionDefinitions.IsFrame(frame.Pointer))
                continue;
            AssertTrue(selected.Contains(frame.Pointer),
                $"Spore Spawn program selects native frame {frame.Pointer:X4}");
            installedSlot.SpritemapPointer =
                frame.Pointer;
            int root = ((int)new SnesAddress(frame.Bank, frame.Pointer));
            ReadOnlySpan<SporeSpawnCollisionComponent> components =
                SporeSpawnCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual(rom.ReadCartridgeByte(root), components.Length,
                $"Spore Spawn frame {frame.Pointer:X4} native component count");
            for (int component = 0; component < components.Length; component++)
            {
                int record = root + 2 + component * 8;
                SporeSpawnCollisionComponent compiled = components[component];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), unchecked((ushort)compiled.Offset.X),
                    $"Spore Spawn frame {frame.Pointer:X4} component {component} X");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), unchecked((ushort)compiled.Offset.Y),
                    $"Spore Spawn frame {frame.Pointer:X4} component {component} Y");
                // The cartridge names each component's list by address; the compiled component holds the list itself.
                ushort listPointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6));
                AssertHitboxListMatchesRom(listPointer, compiled.Hitboxes,
                    $"Spore Spawn frame {frame.Pointer:X4} component {component} list {listPointer:X4}");
                seenLists.Add(listPointer);
            }
            for (int y = -80; y <= 80; y += 16)
            for (int x = -64; x <= 64; x += 16)
            for (int shot = 0; shot <= 1; shot++)
            {
                ushort targetX = unchecked((ushort)(0x100 + x));
                ushort targetY = unchecked((ushort)(0x100 + y));
                object?[] installedArguments =
                    [installedSlot, new Position16<RoomPixels>(targetX, targetY), new Extent16<RoomPixels>(2, 2), shot != 0, (ushort)0];
                bool nativeFound = NativeCallback(root, targetX, targetY, shot != 0, out ushort nativeCallback);
                bool installedFound = ExtendedHitboxWalker.Find(installed, installedArguments);
                AssertEqual(nativeFound, installedFound,
                    $"Spore Spawn frame {frame.Pointer:X4} native overlap at {x},{y}, shot={shot}");
                AssertEqual(nativeCallback,
                    (ushort)(installedArguments[^1] ?? throw new InvalidOperationException("Verification requires installedArguments[^1].")),
                    $"Spore Spawn frame {frame.Pointer:X4} callback at {x},{y}, shot={shot}");
                callbackComparisons++;
            }
            frameCount++;
        }
        AssertEqual(SporeSpawnCollisionDefinitions.FrameCount, frameCount,
            "all compiled Spore Spawn frames are installed visual identities");
        AssertEqual(12, frameCount, "all twelve Spore Spawn OAM frame roots");
        AssertEqual(SporeSpawnCollisionDefinitions.ListCount, seenLists.Count,
            "all compiled Spore Spawn hitbox lists are referenced");
        AssertThrows<InvalidDataException>(
            () => SporeSpawnCollisionDefinitions.ComponentsAt(0xa2df),
            "Draygon frame cannot enter Spore Spawn collision data");
        foreach (ushort invalid in new ushort[] { 0xee64, 0xee66, 0xee70, 0xee7a, 0xeef7, 0xef01, 0xef33, 0xef62 })
        {
            AssertTrue(!SporeSpawnCollisionDefinitions.IsFrame(invalid), "Spore component exact native root domain");
            AssertThrows<InvalidDataException>(() => SporeSpawnCollisionDefinitions.ComponentsAt(invalid),
                "Spore invalid or unused component root rejected");
        }
        AssertEqual(1, SporeSpawnCollisionDefinitions.ComponentsAt(0xee6f).Length, "Spore closed frame has no inner point");
        AssertEqual(2, SporeSpawnCollisionDefinitions.ComponentsAt(0xef3d).Length, "Spore open frame has exactly two components");
        Console.WriteLine($"  Spore Spawn collision: {frameCount} roots, {seenLists.Count} lists and {callbackComparisons} live touch/shot comparisons match the cartridge with installed ROM reads denied.");

        // Compares one compiled hitbox list with the cartridge list its component names.
        void AssertHitboxListMatchesRom(ushort pointer, IReadOnlyList<SporeSpawnCollisionHitbox> hitboxes, string at)
        {
            int list = ((int)new SnesAddress(DraygonBg2FrameDefinitions.Bank, pointer));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(list), hitboxes.Count,
                $"{at} native hitbox count");
            for (int index = 0; index < hitboxes.Count; index++)
            {
                int record = list + 2 + index * 12;
                SporeSpawnCollisionHitbox box = hitboxes[index];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), unchecked((ushort)box.Left),
                    $"{at} box {index} left");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), unchecked((ushort)box.Top),
                    $"{at} box {index} top");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 4)), unchecked((ushort)box.Right),
                    $"{at} box {index} right");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6)), unchecked((ushort)box.Bottom),
                    $"{at} box {index} bottom");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 8)), SporeSpawnHitboxCallbackCodec.ToCallbackWord(box.Touch),
                    $"{at} box {index} touch callback");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 10)), SporeSpawnHitboxCallbackCodec.ToCallbackWord(box.Shot),
                    $"{at} box {index} shot callback");
            }
        }

        // Walks cartridge component and hitbox records to determine the first overlapping callback.
        // Bank A0:9ADF..9B78 orders touch rectangles; 9C43..9D20 supplies the distinct shot
        // boundaries and callback operand. Only this fixture reads original bytes.
        bool NativeCallback(int root, ushort targetX, ushort targetY, bool shot, out ushort callback)
        {
            callback = 0;
            ushort targetLeft = unchecked((ushort)(targetX - 2));
            ushort targetRight = unchecked((ushort)(targetX + 2));
            ushort targetTop = unchecked((ushort)(targetY - 2));
            ushort targetBottom = unchecked((ushort)(targetY + 2));
            int count = rom.ReadCartridgeByte(root);
            for (int component = 0; component < count; component++)
            {
                int record = root + 2 + 8 * component;
                ushort centerX = unchecked((ushort)(0x100 + CartridgeImportSource.Require(rom).ReadCartridgeWord(record)));
                ushort centerY = unchecked((ushort)(0x100 + CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2))));
                int list = 0xa50000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6));
                int rectangles = CartridgeImportSource.Require(rom).ReadCartridgeWord(list);
                for (int rectangle = 0; rectangle < rectangles; rectangle++)
                {
                    int box = list + 2 + 12 * rectangle;
                    ushort left = unchecked((ushort)(centerX + CartridgeImportSource.Require(rom).ReadCartridgeWord(box)));
                    ushort top = unchecked((ushort)(centerY + CartridgeImportSource.Require(rom).ReadCartridgeWord((box + 2))));
                    ushort right = unchecked((ushort)(centerX + CartridgeImportSource.Require(rom).ReadCartridgeWord((box + 4))));
                    ushort bottom = unchecked((ushort)(centerY + CartridgeImportSource.Require(rom).ReadCartridgeWord((box + 6))));
                    bool overlaps = shot
                        ? unchecked((short)(targetRight - left)) >= 0 && unchecked((short)(targetLeft - right)) < 0 &&
                          unchecked((short)(targetBottom - top)) >= 0 && unchecked((short)(targetTop - bottom)) < 0
                        : unchecked((short)(left - targetRight)) < 0 && unchecked((short)(right - targetLeft)) >= 0 &&
                          unchecked((short)(top - targetBottom)) < 0 && unchecked((short)(bottom - targetTop)) >= 0;
                    if (!overlaps) continue;
                    callback = CartridgeImportSource.Require(rom).ReadCartridgeWord((box + (shot ? 10 : 8)));
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>Runs the cartridge comparison for compiled Spore Spawn collision data.</summary>
    [VerificationCommand("--lookup-stream4-spore-collision")]
    internal static int LookupStream4SporeCollisionCommand()
    {
        Suite(nameof(VerifyCompiledSporeSpawnCollision), () => VerifyCompiledSporeSpawnCollision(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
