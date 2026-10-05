using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledCeresSteamInstructionFrames(
        EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod(
            "ProcessInstructions", flags)!;
        int selected = 0;
        foreach (CeresSteamVariant variant in new[]
                 {
                     CeresSteamVariant.Up, CeresSteamVariant.Left,
                     CeresSteamVariant.Down, CeresSteamVariant.Right,
                 })
        {
            var denied = new CeresSteamCollisionNoReadBus();
            var enemies = new RoomEnemySystem { TileArtwork = stock };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, denied);
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(
                enemies, (Func<ushort>)(() => 0));
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = CeresSteamDefinitions.EnemyDefinition;
            slot.Definition = default(RoomEnemyDefinition) with
            { Bank = CeresSteamCollisionDefinitions.Bank };
            slot.Parameter1 = (ushort)variant;
            typeof(RoomEnemySystem).GetMethod("InitializeCeresSteam", flags)!
                .Invoke(enemies, [slot]);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            for (int frame = 0; frame < 90; frame++)
            {
                process.Invoke(enemies, arguments);
                if ((slot.SpritemapPointer & 0x8000) == 0)
                    continue;
                AssertTrue(stock.ExtendedFrames!.TryGetDisplay(
                        CeresSteamCollisionDefinitions.Bank, slot.SpritemapPointer,
                        out _),
                    $"installed Ceres steam {variant} frame {frame} has editable art");
                selected++;
            }
            AssertEqual(0, denied.ReadAttempts,
                $"installed Ceres steam {variant} instruction cycle reads no ROM bytes");
        }
        AssertTrue(selected >= CeresSteamCollisionDefinitions.FramePointers.Length,
            "installed Ceres steam cycles select every directional visual family");
        Console.WriteLine($"Installed Ceres steam: four 90-frame directional cycles, " +
            $"{selected} compiled visual selections, no ROM reads.");
    }

    private static void VerifyCeresSteamCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = new CeresSteamCollisionNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;

        var compiled = new RoomEnemySystem();

        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(compiled, denied);

        RoomEnemySlot compiledSlot = compiled.Slots[0];
        compiledSlot.Definition =
            default(RoomEnemyDefinition) with { Bank = CeresSteamCollisionDefinitions.Bank };

        compiledSlot.EnemyDefinitionPointer = CeresSteamDefinitions.EnemyDefinition;
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
            AssertEqual((ushort)0x1001, ReadWord(0xa60000 | frame),
                $"Ceres steam $A6:{frame:X4} preserves the native $1001 header");
            CeresSteamCollisionComponent component = components[0];
            AssertEqual(unchecked((short)ReadWord(0xa60000 | frame + 2)), component.X,
                "Ceres steam native component X");
            AssertEqual(unchecked((short)ReadWord(0xa60000 | frame + 4)), component.Y,
                "Ceres steam native component Y");
            AssertEqual(ReadWord(0xa60000 | frame + 8), component.HitboxPointer,
                "Ceres steam native hitbox-list pointer");
            seenLists.Add(component.HitboxPointer);

            compiledSlot.SpritemapPointer = frame;
            foreach ((ushort originX, ushort originY) in
                     new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                     })
            {
                compiledSlot.XPosition = originX;
                compiledSlot.YPosition = originY;
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
                        [compiledSlot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                    bool nativeHit = NativeHit(frame, originX, originY, x, y, shot != 0, out ushort nativeCallback);
                    bool compiledHit = (bool)walker.Invoke(compiled, compiledArguments)!;
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
            AssertEqual(ReadWord(0xa60000 | list), (ushort)boxes.Length,
                $"Ceres steam $A6:{list:X4} native hitbox count");
            for (int index = 0; index < boxes.Length; index++)
            {
                CeresSteamCollisionHitbox box = boxes[index];
                int record = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)ReadWord(record)), box.Left, "steam left");
                AssertEqual(unchecked((short)ReadWord(record + 2)), box.Top, "steam top");
                AssertEqual(unchecked((short)ReadWord(record + 4)), box.Right, "steam right");
                AssertEqual(unchecked((short)ReadWord(record + 6)), box.Bottom, "steam bottom");
                AssertEqual(ReadWord(record + 8), box.TouchAi, "steam touch callback");
                AssertEqual(ReadWord(record + 10), box.ShotAi, "steam shot callback");
            }
        }
        AssertThrows<InvalidDataException>(
            () => CeresSteamCollisionDefinitions.ComponentsAt(0x8000),
            "unknown Ceres steam frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => CeresSteamCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Ceres steam hitbox list fails loudly");
        AssertEqual(0, denied.ReadAttempts,
            "installed Ceres steam touch and shot walkers never read ROM bytes");
        Console.WriteLine($"Ceres steam collision: 28 frames, 21 lists, {probes} " +
            "native-equivalent touch/shot boundary probes; installed path forbids ROM reads.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        // Decode the original cartridge independently: the production ROM fallback no longer exists.
        bool NativeHit(ushort frame, ushort originX, ushort originY, ushort x, ushort y,
            bool shot, out ushort callback)
        {
            callback = 0;
            int component = 0xa60000 | frame + 2;
            ushort componentX = unchecked((ushort)(originX + (short)ReadWord(component)));
            ushort componentY = unchecked((ushort)(originY + (short)ReadWord(component + 2)));
            int list = 0xa60000 | ReadWord(component + 6);
            for (int index = 0; index < ReadWord(list); index++)
            {
                int record = list + 2 + 12 * index;
                ushort left = unchecked((ushort)(componentX + (short)ReadWord(record)));
                ushort top = unchecked((ushort)(componentY + (short)ReadWord(record + 2)));
                ushort right = unchecked((ushort)(componentX + (short)ReadWord(record + 4)));
                ushort bottom = unchecked((ushort)(componentY + (short)ReadWord(record + 6)));
                bool hit = shot
                    ? unchecked((short)(x - left)) >= 0 && unchecked((short)(x - right)) < 0 &&
                      unchecked((short)(y - top)) >= 0 && unchecked((short)(y - bottom)) < 0
                    : unchecked((short)(left - x)) < 0 && unchecked((short)(right - x)) >= 0 &&
                      unchecked((short)(top - y)) < 0 && unchecked((short)(bottom - y)) >= 0;
                if (!hit)
                    continue;
                callback = ReadWord(record + (shot ? 10 : 8));
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

    private sealed class CeresSteamCollisionNoReadBus : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Ceres steam collision read ROM byte ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed Ceres steam collision wrote ROM byte ${address:X6}.");
    }
}
