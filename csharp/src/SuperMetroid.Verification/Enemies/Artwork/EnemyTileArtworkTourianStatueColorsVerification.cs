using SuperMetroid.Tooling;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Retail CGRAM and guarded-source checks for editable Tourian entrance statue colors.</summary>
internal static class EnemyTileArtworkTourianStatueColorsVerification
{
    /// <summary>Checks entrance, eye, and grey-room palette writes, isolated edits, override persistence, and invalid file rejection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Tourian statue palette sources")]
    internal static void VerifyInstalledTourianStatueColors(ISnesAddressSpace bus,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        TourianStatueColorCatalog colors = stock.TourianStatueColors ??
            throw new InvalidDataException("Installed enemy artwork lacks Tourian statue colors.");
        var forbidden = new HashSet<int>();
        for (int color = 0; color < TourianStatuePaletteRomData.BaseColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.BaseColors + color * sizeof(ushort),
                colors.ResolveBase(color).ToWord());
        for (int color = 0; color < TourianStatuePaletteRomData.StatueColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.StatueColors + color * sizeof(ushort),
                colors.ResolveStatue(color).ToWord());
        for (int row = 0; row < TourianStatuePaletteRomData.EyeRowCount; row++)
        for (int color = 0; color < TourianStatuePaletteRomData.EyeColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.EyeColors +
                (row * TourianStatuePaletteRomData.EyeColorCount + color) * sizeof(ushort),
                (colors.ResolveEye(row, color)).ToWord());
        for (int color = 0; color < TourianStatuePaletteRomData.GreyColorCount; color++)
            CheckSource(TourianStatuePaletteRomData.GreyColors + color * sizeof(ushort),
                colors.ResolveGrey(color).ToWord());

        var guard = NewTourianStatueColorGuard(bus, forbidden);
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            SnesCgram nativeCgram = SeedColors();
            (RoomEnemySystem installed, SnesCgram installedCgram) = CreateEnemy(guard, stock);
            ReferenceEntrance(nativeCgram);
            Initialize(installed, parameter);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"Tourian statue entry parameter {parameter} preserves full native CGRAM");
            AssertEqual(ReadReferenceWord(0xaad810 + parameter),
                installed.SlotTable.Slots[0].CurrentInstruction,
                $"Tourian statue entry parameter {parameter} keeps instruction selection");
            AssertEqual(parameter == 0 ? 3 : 0,
                installed.Projectiles.EnemyProjectiles.Count(x => x.IsActive),
                $"Tourian statue entry parameter {parameter} keeps spawned actors");
        }
        foreach (ushort parameter in new ushort[] { 0, 2, 4, 6 })
        {
            SnesCgram nativeCgram = SeedColors();
            (RoomEnemySystem installed, SnesCgram installedCgram) = CreateEnemy(guard, stock);
            ReferenceEye(nativeCgram, parameter);
            installed.TourianUnlockBehavior.SpawnTourianUnlockEffect(parameter, soul: false);
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"Tourian eye parameter {parameter} preserves full native CGRAM");
            AssertEqual(new ushort[] { 0x84, 0x7a, 0x9e, 0x68 }[parameter / 2],
                installed.Projectiles.EnemyProjectiles.Single(x => x.IsActive).Position.X.Whole,
                $"Tourian eye parameter {parameter} keeps physical X");
            AssertEqual(new ushort[] { 0x90, 0x51, 0x80, 0x72 }[parameter / 2],
                installed.Projectiles.EnemyProjectiles.Single(x => x.IsActive).Position.Y.Whole,
                $"Tourian eye parameter {parameter} keeps physical Y");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Tourian entry and eye actors do not read palette ROM ranges");

        string stockPath = Path.Combine(stockDirectory, TourianStatueColorFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "tourian-statue-color-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, TourianStatueColorFormat.FileName);
        JsonNode changed = RequiredJson.Parse(stockBytes);
        changed.Required("base").Required(1)["red"] = changed.Required("base").Required(1).Required("red").GetValue<int>() ^ 1;
        changed.Required("statue").Required(1)["green"] = changed.Required("statue").Required(1).Required("green").GetValue<int>() ^ 1;
        changed.Required("eye").Required(2).Required(1)["blue"] = changed.Required("eye").Required(2).Required(1).Required("blue").GetValue<int>() ^ 1;
        File.WriteAllBytes(overridePath, Encoding.UTF8.GetBytes(changed.ToJsonString()));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        SnesCgram nativeEntryCgram = SeedColors();
        (RoomEnemySystem editedEntry, SnesCgram editedEntryCgram) = CreateEnemy(guard, edited);
        ReferenceEntrance(nativeEntryCgram);
        Initialize(editedEntry, 0);
        CheckOnlyDifferences(nativeEntryCgram, editedEntryCgram,
            [(TourianStatuePaletteRomData.BaseCgramIndex + 1, 1),
             (TourianStatuePaletteRomData.StatueCgramIndex + 1, 1 << 5)],
            "Tourian entrance edit");
        SnesCgram nativeEyeCgram = SeedColors();
        (RoomEnemySystem editedEye, SnesCgram editedEyeCgram) = CreateEnemy(guard, edited);
        ReferenceEye(nativeEyeCgram, 4);
        editedEye.TourianUnlockBehavior.SpawnTourianUnlockEffect(4, soul: false);
        CheckOnlyDifferences(nativeEyeCgram, editedEyeCgram,
            [(TourianStatuePaletteRomData.EyeCgramIndex + 1, 1 << 10)],
            "Tourian eye edit");

        // The $87:837F grey instruction is reached on re-entry after each
        // defeated-boss statue's grey event. Exercise the actual room program,
        // including its four compiled destination operands.
        JsonNode greyOnly = RequiredJson.Parse(stockBytes);
        greyOnly.Required("grey").Required(1)["blue"] =
            greyOnly.Required("grey").Required(1).Required("blue").GetValue<int>() ^ 1;
        byte[] greyBytes = Encoding.UTF8.GetBytes(greyOnly.ToJsonString());
        File.WriteAllBytes(overridePath, greyBytes);
        EnemyTileArtworkCatalog editedGrey = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var installedRoom = CreateGreyRoom(
            NewTourianStatueColorGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath), forbidden), stock);
        var editedRoom = CreateGreyRoom(
            NewTourianStatueColorGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath), forbidden), editedGrey);
        // Use the room's pre-step palette as the unchanged background, then apply
        // only the native grey instruction writes read from the four retail programs.
        var nativeGreyCgram = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            nativeGreyCgram.SetColor(color, installedRoom.Cgram.Colors[color]);
        var greyDestinations = new List<int>();
        foreach (int program in new[] { 0x8783ac, 0x878414, 0x87847c, 0x8784e4 })
        {
            AssertEqual((ushort)0x837f, ReadReferenceWord(program + 0x62),
                "native statue program grey-palette instruction");
            int destination = ReadReferenceWord(program + 0x64) / 2;
            greyDestinations.Add(destination);
            CopyReferenceColors(nativeGreyCgram, 0x87839c, destination, 8);
        }
        installedRoom.TourianStatues.StepTiles();
        editedRoom.TourianStatues.StepTiles();
        AssertTrue(new CgramPalettes(nativeGreyCgram).ImageEquals(new CgramPalettes(installedRoom.Cgram)),
            "Tourian grey room instruction preserves full native CGRAM");
        var differences = greyDestinations
            .Select(destination => (Index: destination + 1, Mask: 1 << 10)).ToArray();
        CheckOnlyDifferences(nativeGreyCgram, editedRoom.Cgram, differences,
            "Tourian grey room instruction edit");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Tourian statue installed paths avoid migrated ROM colors");

        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
        AssertTrue(greyBytes.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Stock re-extraction preserves Tourian statue override");
        EnemyTileArtworkCatalog reloaded = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        AssertEqual((editedGrey.TourianStatueColors ?? throw new InvalidOperationException("Verification requires editedGrey.TourianStatueColors.")).ResolveGrey(1),
            (reloaded.TourianStatueColors ?? throw new InvalidOperationException("Verification requires reloaded.TourianStatueColors.")).ResolveGrey(1),
            "Tourian statue grey edit survives reload");
        File.WriteAllText(overridePath, "broken");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Corrupt Tourian statue override fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1}");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Duplicate Tourian statue property fails loudly");
        JsonNode invalid = RequiredJson.Parse(stockBytes);
        invalid.Required("eye").Required(0).Required(0)["red"] = 32;
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
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), expected,
                $"Tourian statue palette source ${address:X6}");
        }

        // Native palette writes from $AA:D7C8 and $86:B88E, independent of the
        // installed catalog and production callbacks used on the actual side.
        void ReferenceEntrance(SnesCgram cgram)
        {
            CopyReferenceColors(cgram, 0xaad785, 240, 16);
            CopyReferenceColors(cgram, 0xaad765, 160, 16);
        }

        void ReferenceEye(SnesCgram cgram, ushort parameter) =>
            CopyReferenceColors(cgram, 0x86b91e + parameter * 4, 249, 4);

        void CopyReferenceColors(SnesCgram cgram, int source, int destination, int count)
        {
            for (int color = 0; color < count; color++)
                cgram.SetColor(destination + color, Bgr555.FromWord(checked((ushort)(ReadReferenceWord(source + color * 2)))));
        }

        ushort ReadReferenceWord(int address) =>
            (ushort)(SnesLittleEndian.Word(CpuBusRegionRead.Read(bus, address), CpuBusRegionRead.Read(bus, address + 1)));

        static SnesCgram SeedColors()
        {
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, Bgr555.FromWord((ushort)(color * 31)));
            return cgram;
        }

        static (RoomEnemySystem, SnesCgram) CreateEnemy(
            ISnesAddressSpace source, EnemyTileArtworkCatalog artwork)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemy = new RoomEnemySystem { TileArtwork = artwork };
            SnesCgram cgram = SeedColors();
            enemy.BindFixtureBus(source);
            enemy.Presentation._cgram = cgram;
            return (enemy, cgram);
        }

        static void Initialize(RoomEnemySystem enemy, ushort parameter)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            RoomEnemySlot slot = enemy.SlotTable.Slots[0];
            slot.Parameter1 = parameter;
            enemy.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
        }

        static SuperMetroidRuntime CreateGreyRoom(
            ISnesAddressSpace source, EnemyTileArtworkCatalog artwork)
        {
            var runtime = CreateRetailRuntimeFixture(source, playerInvincibilityEnabled: true);
            runtime.Enemies.TileArtwork = artwork;
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
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

    /// <summary>Rejects reads of native palette bytes whose colors are now supplied by installed artwork.</summary>
    private static MutableCartridgeReadGuard NewTourianStatueColorGuard(ISnesAddressSpace source,
        HashSet<int> forbidden)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains,
            address => $"Installed Tourian statue color reread ROM byte ${address:X6}.");
        return guard;
    }

    /// <summary>Requires an edited CGRAM image to differ only at the specified RGB5 channel bits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void CheckOnlyDifferences(SnesCgram reference, SnesCgram edited,
        IReadOnlyList<(int Index, int Mask)> differences, string context)
    {
        for (int color = 0; color < SnesCgram.ColorCount; color++)
        {
            int mask = differences.Where(change => change.Index == color)
                .Aggregate(0, (current, change) => current ^ change.Mask);
            AssertEqual((ushort)(reference.Colors[color].ToWord() ^ mask), edited.Colors[color],
                $"{context} CGRAM {color}");
        }
    }
}
