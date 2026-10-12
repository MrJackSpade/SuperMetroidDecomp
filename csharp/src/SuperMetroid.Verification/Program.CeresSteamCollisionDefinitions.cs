using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class CeresSteamCollisionDefinitionsVerification
{
    internal static void VerifyInstalledCeresSteamInstructionFrames(
        EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        int selected = 0;
        foreach (CeresSteamVariant variant in new[]
                 {
                     CeresSteamVariant.Up, CeresSteamVariant.Left,
                     CeresSteamVariant.Down, CeresSteamVariant.Right,
                 })
        {
            var denied = NewCeresSteamCollisionNoReadBus();
            var enemies = new RoomEnemySystem { TileArtwork = stock };
            enemies.BindFixtureBus(denied);
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Steam;
            slot.Definition = default(RoomEnemyDefinition) with
            { Bank = CeresSteamCollisionDefinitions.Bank };
            slot.Parameter1 = (ushort)variant;
            enemies.CeresSteamBehavior.InitializeCeresSteam(slot);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < 90; frame++)
            {
                EnemyInstructionRunner.Process(enemies, arguments);
                if ((slot.SpritemapPointer & 0x8000) == 0)
                    continue;
                AssertTrue(stock.ExtendedFrames!.TryGetDisplay(
                        CeresSteamCollisionDefinitions.Bank, slot.SpritemapPointer,
                        out _),
                    $"installed Ceres steam {variant} frame {frame} has editable art");
                selected++;
            }
            AssertEqual(0, denied.ForbiddenReadAttempts,
                $"installed Ceres steam {variant} instruction cycle reads no ROM bytes");
        }
        AssertTrue(selected >= CeresSteamCollisionDefinitions.FramePointers.Length,
            "installed Ceres steam cycles select every directional visual family");
        Console.WriteLine($"Installed Ceres steam: four 90-frame directional cycles, " +
            $"{selected} compiled visual selections, no ROM reads.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres steam collision definitions")]
    internal static void VerifyCeresSteamCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = NewCeresSteamCollisionNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var compiled = new RoomEnemySystem();
        compiled.BindFixtureBus(denied);
        var nativeSlot = new RoomEnemySlot(0);
        RoomEnemySlot compiledSlot = compiled.SlotTable.Slots[0];
        compiledSlot.Definition =
            default(RoomEnemyDefinition) with { Bank = CeresSteamCollisionDefinitions.Bank };

        compiledSlot.EnemyDefinitionPointer = EnemyDefinitionId.Steam;
        AssertEqual(28, CeresSteamCollisionDefinitions.FramePointers.Length,
            "all Ceres steam extended frames have compiled collision");
        AssertEqual(21, CeresSteamCollisionDefinitions.HitboxPointers.Count(),
            "all distinct Ceres steam hitbox lists have compiled collision");

        var seenLists = new HashSet<ushort>();
        int probes = 0;
        foreach (ushort frame in CeresSteamCollisionDefinitions.FramePointers)
        {
            CeresSteamCollisionDefinitions.ComponentSequence components =
                CeresSteamCollisionDefinitions.ComponentsAt(frame);
            AssertEqual(1, components.Length,
                $"Ceres steam $A6:{frame:X4} has one physical component");
            AssertEqual((ushort)0x1001, rom.ReadCartridgeWord((0xa60000 | frame)),
                $"Ceres steam $A6:{frame:X4} preserves the native $1001 header");
            CeresSteamCollisionComponent component = components[0];
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((0xa60000 | frame + 2))), component.Offset.X,
                "Ceres steam native component X");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((0xa60000 | frame + 4))), component.Offset.Y,
                "Ceres steam native component Y");
            AssertEqual(rom.ReadCartridgeWord((0xa60000 | frame + 8)), component.HitboxPointer,
                "Ceres steam native hitbox-list pointer");
            seenLists.Add(component.HitboxPointer);

            compiledSlot.SpritemapPointer = frame;
            foreach ((ushort originX, ushort originY) in
                     new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                     })
            {
                compiledSlot.Position = compiledSlot.Position.WithWhole(new(originX, originY));
                // Probe every rectangle boundary and the fully hidden frames.
                // Shot and touch use different strict/inclusive comparisons.
                ReadOnlySpan<CeresSteamCollisionHitbox> boxes =
                    CeresSteamCollisionDefinitions.HitboxesAt(component.HitboxPointer);
                ushort[] xs = boxes.IsEmpty
                    ? [originX, unchecked((ushort)(originX + 12))]
                    : BoundaryPoints(originX, boxes[0].Left, boxes[0].Right);
                ushort[] ys = boxes.IsEmpty
                    ? [originY, unchecked((ushort)(originY + 12))]
                    : BoundaryPoints(originY, boxes[0].Top, boxes[0].Bottom);
                foreach (ushort x in xs)
                foreach (ushort y in ys)
                for (int shot = 0; shot <= 1; shot++)
                {
                    object?[] compiledArguments =
                        [compiledSlot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>((ushort)0, (ushort)0), shot != 0, (ushort)0];
                    bool nativeHit = NativeHit(frame, originX, originY, x, y, shot != 0, out ushort nativeCallback);
                    bool compiledHit = ExtendedHitboxWalker.Find(compiled, compiledArguments);
                    AssertEqual(nativeHit, compiledHit,
                        $"Ceres steam $A6:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                    AssertEqual(nativeCallback,
                        (ushort)compiledArguments[^1]!,
                        $"Ceres steam $A6:{frame:X4} callback {x:X4},{y:X4}, shot={shot}");
                    probes++;
                }
            }
        }
        AssertTrue(seenLists.SetEquals(CeresSteamCollisionDefinitions.HitboxPointers),
            "every compiled Ceres steam hitbox list belongs to a selected frame");
        foreach (ushort list in CeresSteamCollisionDefinitions.HitboxPointers)
        {
            ReadOnlySpan<CeresSteamCollisionHitbox> boxes =
                CeresSteamCollisionDefinitions.HitboxesAt(list);
            AssertEqual(rom.ReadCartridgeWord((0xa60000 | list)), (ushort)boxes.Length,
                $"Ceres steam $A6:{list:X4} native hitbox count");
            for (int index = 0; index < boxes.Length; index++)
            {
                CeresSteamCollisionHitbox box = boxes[index];
                int record = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(record)), box.Left, "steam left");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 2))), box.Top, "steam top");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 4))), box.Right, "steam right");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 6))), box.Bottom, "steam bottom");
                AssertEqual(rom.ReadCartridgeWord((record + 8)), box.TouchAi, "steam touch callback");
                AssertEqual(rom.ReadCartridgeWord((record + 10)), box.ShotAi, "steam shot callback");
            }
        }
        AssertThrows<InvalidDataException>(
            () => CeresSteamCollisionDefinitions.ComponentsAt(0x8000),
            "unknown Ceres steam frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => CeresSteamCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Ceres steam hitbox list fails loudly");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Ceres steam touch and shot walkers never read ROM bytes");
        Console.WriteLine($"Ceres steam collision: 28 frames, 21 lists, {probes} " +
            "native-equivalent touch/shot boundary probes; installed path forbids ROM reads.");

        // Decode the original cartridge independently: the production ROM fallback no longer exists.
        bool NativeHit(ushort frame, ushort originX, ushort originY, ushort x, ushort y,
            bool shot, out ushort callback)
        {
            callback = 0;
            int component = 0xa60000 | frame + 2;
            ushort componentX = unchecked((ushort)(originX + (short)rom.ReadCartridgeWord(component)));
            ushort componentY = unchecked((ushort)(originY + (short)rom.ReadCartridgeWord((component + 2))));
            int list = 0xa60000 | rom.ReadCartridgeWord((component + 6));
            for (int index = 0; index < rom.ReadCartridgeWord(list); index++)
            {
                int record = list + 2 + 12 * index;
                ushort left = unchecked((ushort)(componentX + (short)rom.ReadCartridgeWord(record)));
                ushort top = unchecked((ushort)(componentY + (short)rom.ReadCartridgeWord((record + 2))));
                ushort right = unchecked((ushort)(componentX + (short)rom.ReadCartridgeWord((record + 4))));
                ushort bottom = unchecked((ushort)(componentY + (short)rom.ReadCartridgeWord((record + 6))));
                bool hit = shot
                    ? unchecked((short)(x - left)) >= 0 && unchecked((short)(x - right)) < 0 &&
                      unchecked((short)(y - top)) >= 0 && unchecked((short)(y - bottom)) < 0
                    : unchecked((short)(left - x)) < 0 && unchecked((short)(right - x)) >= 0 &&
                      unchecked((short)(top - y)) < 0 && unchecked((short)(bottom - y)) >= 0;
                if (!hit)
                    continue;
                callback = rom.ReadCartridgeWord((record + (shot ? 10 : 8)));
                return true;
            }
            return false;
        }
        static ushort[] BoundaryPoints(ushort origin, short low, short high) =>
        [
            unchecked((ushort)(origin + low - 1)),
            unchecked((ushort)(origin + low)),
            unchecked((ushort)(origin + low + 1)),
            unchecked((ushort)(origin + high - 1)),
            unchecked((ushort)(origin + high)),
            unchecked((ushort)(origin + high + 1)),
        ];
    }

    private static CartridgeReadGuard NewCeresSteamCollisionNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Ceres steam collision read ROM byte ${address:X6}.")
            .RejectWrites(address => $"Installed Ceres steam collision wrote ROM byte ${address:X6}.");

    [VerificationCommand("--verify-ceres-steam-collision")]
    internal static int VerifyCeresSteamCollisionCommand()
    {
        Suite(nameof(VerifyCeresSteamCollisionDefinitions), () => VerifyCeresSteamCollisionDefinitions());
        return 0;
    }
}
