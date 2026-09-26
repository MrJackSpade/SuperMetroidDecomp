using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyInstalledTourianStatueColors(ISnesAddressSpace bus,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        TourianStatueColorCatalog colors = stock.TourianStatueColors ??
            throw new InvalidDataException("Installed enemy artwork lacks Tourian statue colors.");
        var forbidden = new HashSet<int>();
        for (int color = 0; color < TourianStatuePaletteRomData.BaseColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.BaseColors + color * sizeof(ushort),
                colors.ResolveBase(color));
        for (int color = 0; color < TourianStatuePaletteRomData.StatueColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.StatueColors + color * sizeof(ushort),
                colors.ResolveStatue(color));
        for (int row = 0; row < TourianStatuePaletteRomData.EyeRowCount; row++)
        for (int color = 0; color < TourianStatuePaletteRomData.EyeColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.EyeColors +
                (row * TourianStatuePaletteRomData.EyeColorCount + color) * sizeof(ushort),
                colors.ResolveEye(row, color));
        for (int color = 0; color < TourianStatuePaletteRomData.GreyColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.GreyColors + color * sizeof(ushort),
                colors.ResolveGrey(color));

        var guard = new TourianStatueColorReadGuard(bus, forbidden);
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            (RoomEnemySystem native, SnesCgram nativeCgram) = CreateEnemy(bus, null);
            (RoomEnemySystem installed, SnesCgram installedCgram) = CreateEnemy(guard, stock);
            Initialize(native, parameter);
            Initialize(installed, parameter);
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"Tourian statue entry parameter {parameter} preserves full native CGRAM");
            AssertEqual(native.Slots[0].CurrentInstruction,
                installed.Slots[0].CurrentInstruction,
                $"Tourian statue entry parameter {parameter} keeps instruction selection");
            AssertEqual(native.EnemyProjectiles.Count(x => x.IsActive),
                installed.EnemyProjectiles.Count(x => x.IsActive),
                $"Tourian statue entry parameter {parameter} keeps spawned actors");
        }
        foreach (ushort parameter in new ushort[] { 0, 2, 4, 6 })
        {
            (RoomEnemySystem native, SnesCgram nativeCgram) = CreateEnemy(bus, null);
            (RoomEnemySystem installed, SnesCgram installedCgram) = CreateEnemy(guard, stock);
            native.SpawnTourianUnlockEffect(parameter, soul: false);
            installed.SpawnTourianUnlockEffect(parameter, soul: false);
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"Tourian eye parameter {parameter} preserves full native CGRAM");
            AssertEqual(native.EnemyProjectiles.Single(x => x.IsActive).XPosition,
                installed.EnemyProjectiles.Single(x => x.IsActive).XPosition,
                $"Tourian eye parameter {parameter} keeps physical X");
            AssertEqual(native.EnemyProjectiles.Single(x => x.IsActive).YPosition,
                installed.EnemyProjectiles.Single(x => x.IsActive).YPosition,
                $"Tourian eye parameter {parameter} keeps physical Y");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Tourian entry and eye actors do not read palette ROM ranges");

        string stockPath = Path.Combine(stockDirectory, TourianStatueColorFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "tourian-statue-color-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, TourianStatueColorFormat.FileName);
        JsonNode changed = JsonNode.Parse(stockBytes)!;
        changed["base"]![1]!["red"] = changed["base"]![1]!["red"]!.GetValue<int>() ^ 1;
        changed["statue"]![1]!["green"] = changed["statue"]![1]!["green"]!.GetValue<int>() ^ 1;
        changed["eye"]![2]![1]!["blue"] = changed["eye"]![2]![1]!["blue"]!.GetValue<int>() ^ 1;
        File.WriteAllBytes(overridePath, Encoding.UTF8.GetBytes(changed.ToJsonString()));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        (RoomEnemySystem nativeEntry, SnesCgram nativeEntryCgram) = CreateEnemy(bus, null);
        (RoomEnemySystem editedEntry, SnesCgram editedEntryCgram) = CreateEnemy(guard, edited);
        Initialize(nativeEntry, 0);
        Initialize(editedEntry, 0);
        CheckOnlyDifferences(nativeEntryCgram, editedEntryCgram,
            [(TourianStatuePaletteRomData.BaseCgramIndex + 1, 1),
             (TourianStatuePaletteRomData.StatueCgramIndex + 1, 1 << 5)],
            "Tourian entrance edit");
        (RoomEnemySystem nativeEye, SnesCgram nativeEyeCgram) = CreateEnemy(bus, null);
        (RoomEnemySystem editedEye, SnesCgram editedEyeCgram) = CreateEnemy(guard, edited);
        nativeEye.SpawnTourianUnlockEffect(4, soul: false);
        editedEye.SpawnTourianUnlockEffect(4, soul: false);
        CheckOnlyDifferences(nativeEyeCgram, editedEyeCgram,
            [(TourianStatuePaletteRomData.EyeCgramIndex + 1, 1 << 10)],
            "Tourian eye edit");

        // The $87:837F grey instruction is reached on re-entry after each
        // defeated-boss statue's grey event. Exercise the actual room program,
        // including its four compiled destination operands.
        JsonNode greyOnly = JsonNode.Parse(stockBytes)!;
        greyOnly["grey"]![1]!["blue"] =
            greyOnly["grey"]![1]!["blue"]!.GetValue<int>() ^ 1;
        byte[] greyBytes = Encoding.UTF8.GetBytes(greyOnly.ToJsonString());
        File.WriteAllBytes(overridePath, greyBytes);
        EnemyTileArtworkCatalog editedGrey = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var nativeRoom = CreateGreyRoom(SuperMetroidAddressSpace.LoadRetailRom(romPath), null);
        var installedRoom = CreateGreyRoom(
            new TourianStatueColorReadGuard(
                SuperMetroidAddressSpace.LoadRetailRom(romPath), forbidden), stock);
        var editedRoom = CreateGreyRoom(
            new TourianStatueColorReadGuard(
                SuperMetroidAddressSpace.LoadRetailRom(romPath), forbidden), editedGrey);
        nativeRoom.TourianStatues.StepTiles(nativeRoom);
        installedRoom.TourianStatues.StepTiles(installedRoom);
        editedRoom.TourianStatues.StepTiles(editedRoom);
        AssertTrue(nativeRoom.Cgram.Colors.SequenceEqual(installedRoom.Cgram.Colors),
            "Tourian grey room instruction preserves full native CGRAM");
        var differences = TourianStatueAnimatedTileMechanicsDefinitions.All
            .Select(definition => (Index: definition.TargetPaletteByteIndex / 2 + 1,
                Mask: 1 << 10)).ToArray();
        CheckOnlyDifferences(nativeRoom.Cgram, editedRoom.Cgram, differences,
            "Tourian grey room instruction edit");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Tourian statue installed paths avoid migrated ROM colors");

        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
        AssertTrue(greyBytes.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Stock re-extraction preserves Tourian statue override");
        EnemyTileArtworkCatalog reloaded = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        AssertEqual(editedGrey.TourianStatueColors!.ResolveGrey(1),
            reloaded.TourianStatueColors!.ResolveGrey(1),
            "Tourian statue grey edit survives reload");
        File.WriteAllText(overridePath, "broken");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Corrupt Tourian statue override fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1}");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Duplicate Tourian statue property fails loudly");
        JsonNode invalid = JsonNode.Parse(stockBytes)!;
        invalid["eye"]![0]![0]!["red"] = 32;
        AssertThrows<InvalidDataException>(() => TourianStatueColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Tourian statue RGB5 channel rejected");
        File.Delete(overridePath);
        File.WriteAllText(stockPath, "broken stock");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "Corrupt Tourian statue stock colors fail manifest validation");
        File.WriteAllBytes(stockPath, stockBytes);
        Console.WriteLine("  Tourian statue colors: 56 native RGB5 words, real entrance/eye/grey CGRAM parity, isolated edits, ROM guard, override persistence and strict failures pass.");

        void CheckSource(int address, ushort expected)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), expected,
                $"Tourian statue palette source ${address:X6}");
        }

        static (RoomEnemySystem, SnesCgram) CreateEnemy(
            ISnesAddressSpace source, EnemyTileArtworkCatalog? artwork)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemy = new RoomEnemySystem { TileArtwork = artwork };
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, (ushort)(color * 31));
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemy, source);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemy, cgram);
            return (enemy, cgram);
        }

        static void Initialize(RoomEnemySystem enemy, ushort parameter)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            RoomEnemySlot slot = enemy.Slots[0];
            slot.Parameter1 = parameter;
            typeof(RoomEnemySystem).GetMethod("InitializeTourianEntranceStatue", flags)!
                .Invoke(enemy, [slot]);
        }

        static SuperMetroidRuntime CreateGreyRoom(
            ISnesAddressSpace source, EnemyTileArtworkCatalog? artwork)
        {
            var runtime = new SuperMetroidRuntime(source, playerInvincibilityEnabled: true);
            runtime.Enemies.TileArtwork = artwork;
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            for (int area = 1; area <= 4; area++)
                runtime.System.SetBossBits(area, BossBits.AreaBoss);
            for (int greyEvent = 6; greyEvent <= 9; greyEvent++)
                runtime.System.SetEventRaw(greyEvent);
            runtime.LoadCartridgeRoomForDebug(0xa66a);
            AssertTrue(runtime.TourianStatues.Enabled,
                "retail Tourian statue room enables grey animation programs");
            return runtime;
        }
    }

    private static void CheckOnlyDifferences(SnesCgram reference, SnesCgram edited,
        IReadOnlyList<(int Index, int Mask)> differences, string context)
    {
        for (int color = 0; color < SnesCgram.ColorCount; color++)
        {
            int mask = differences.Where(change => change.Index == color)
                .Aggregate(0, (current, change) => current ^ change.Mask);
            AssertEqual((ushort)(reference.Colors[color] ^ mask), edited.Colors[color],
                $"{context} CGRAM {color}");
        }
    }

    private sealed class TourianStatueColorReadGuard(ISnesAddressSpace source,
        HashSet<int> forbidden) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Installed Tourian statue color reread ROM byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
