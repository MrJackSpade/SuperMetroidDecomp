using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledCrocomireColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        CrocomireColorCatalog native = stock.CrocomireColors ??
            throw new InvalidDataException("Installed enemy art has no Crocomire colors.");
        VerifyBand(CrocomirePaletteRomData.FightBodySource,
            CrocomirePaletteRomData.FightBodyCount, native.ResolveFightBody, "fight body");
        VerifyBand(CrocomirePaletteRomData.InitialWallSource,
            CrocomirePaletteRomData.InitialWallCount, native.ResolveInitialWall, "initial wall");
        VerifyBand(CrocomirePaletteRomData.InitialProjectileSource,
            CrocomirePaletteRomData.InitialProjectileCount, native.ResolveInitialProjectile,
            "initial projectile");
        VerifyBand(CrocomirePaletteRomData.SkeletonArmSource,
            CrocomirePaletteRomData.SkeletonArmCount, native.ResolveSkeletonArm, "skeleton arm");
        VerifyBand(CrocomirePaletteRomData.WallSpikesSource,
            CrocomirePaletteRomData.WallSpikesCount, native.ResolveWallSpikes, "wall spikes");

        string file = Path.Combine(stockDirectory, CrocomireColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        CrocomireColorDocument visual = JsonSerializer.Deserialize<CrocomireColorDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Crocomire color JSON is null.");
        visual.FightBody[1] = ChangeRed(visual.FightBody[1]);
        visual.InitialWall[1] = ChangeRed(visual.InitialWall[1]);
        visual.InitialProjectile[1] = ChangeRed(visual.InitialProjectile[1]);
        visual.SkeletonArm[1] = ChangeRed(visual.SkeletonArm[1]);
        visual.WallSpikes[1] = ChangeRed(visual.WallSpikes[1]);
        string overrides = Path.Combine(stockDirectory, "crocomire-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, CrocomireColorFormat.FileName);
        File.WriteAllBytes(overrideFile, CrocomireColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        CrocomireColorCatalog colors = edited.CrocomireColors ??
            throw new InvalidDataException("Edited enemy art has no Crocomire colors.");
        CheckEditedBand(CrocomirePaletteRomData.FightBodyCount,
            native.ResolveFightBody, colors.ResolveFightBody, "fight body");
        CheckEditedBand(CrocomirePaletteRomData.InitialWallCount,
            native.ResolveInitialWall, colors.ResolveInitialWall, "initial wall");
        CheckEditedBand(CrocomirePaletteRomData.InitialProjectileCount,
            native.ResolveInitialProjectile, colors.ResolveInitialProjectile,
            "initial projectile");
        CheckEditedBand(CrocomirePaletteRomData.SkeletonArmCount,
            native.ResolveSkeletonArm, colors.ResolveSkeletonArm, "skeleton arm");
        CheckEditedBand(CrocomirePaletteRomData.WallSpikesCount,
            native.ResolveWallSpikes, colors.ResolveWallSpikes, "wall spikes");
        AssertEqual(colors.ResolveFightBody(1),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .CrocomireColors!.ResolveFightBody(1),
            "Crocomire color override survives catalog reload");

        // Execute the real boss initializer and hurt-flash hook with palette ROM reads
        // forbidden. Their CGRAM writes must come from the installed resource, while the
        // native phase, instruction, and white-flash decisions remain unchanged.
        var guarded = new CrocomirePaletteReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guarded);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        type.GetField("_isAreaMiniBossDefeated", flags)!
            .SetValue(enemies, (Func<bool>)(() => false));
        type.GetField("_setRoomScrollState", flags)!
            .SetValue(enemies, (Action<int, RoomScrollState>)((_, _) => { }));
        RoomEnemySlot body = enemies.Slots[0];
        type.GetMethod("InitializeCrocomire", flags)!.Invoke(enemies, [body]);
        CrocomireEnemyState state = (CrocomireEnemyState)type.GetField("_crocomire", flags)!
            .GetValue(enemies)!;
        AssertEqual((ushort)0, state.DeathSequenceIndex,
            "installed Crocomire colors leave living fight phase unchanged");
        AssertEqual(CrocomireInstructionProgramDefinitions.Initial,
            body.CurrentInstruction,
            "installed Crocomire colors leave initial instruction unchanged");
        AssertBand(CrocomirePaletteRomData.InitialWallDestination,
            CrocomirePaletteRomData.InitialWallCount, colors.ResolveInitialWall,
            "initial wall CGRAM");
        AssertBand(CrocomirePaletteRomData.InitialProjectileDestination,
            CrocomirePaletteRomData.InitialProjectileCount, colors.ResolveInitialProjectile,
            "initial projectile CGRAM");
        MethodInfo hurt = type.GetMethod("ApplyCrocomireHurtPalette", flags)!;
        hurt.Invoke(enemies, [body]);
        AssertBand(CrocomirePaletteRomData.FightBodyDestination,
            CrocomirePaletteRomData.FightBodyCount, colors.ResolveFightBody,
            "restored fight-body CGRAM");
        body.FlashTimer = 1;
        type.GetField("_randomEnemyCounter", flags)!.SetValue(enemies, (ushort)2);
        hurt.Invoke(enemies, [body]);
        for (int color = 0; color < CrocomirePaletteRomData.FightBodyCount; color++)
            AssertEqual((ushort)0x7fff,
                cgram.Colors[CrocomirePaletteRomData.FightBodyDestination + color],
                $"Crocomire native white hurt flash {color}");
        AssertEqual((ushort)0, state.DeathSequenceIndex,
            "white hurt flash leaves fight phase unchanged");
        state.StepCounter = 0;
        type.GetMethod("RunCrocomireSkeletonTileLoadAndWallBreak", flags)!
            .Invoke(enemies, [state]);
        AssertBand(CrocomirePaletteRomData.SkeletonArmDestination,
            CrocomirePaletteRomData.SkeletonArmCount, colors.ResolveSkeletonArm,
            "skeleton-arm CGRAM");
        AssertEqual((ushort)2, state.DeathSequenceIndex,
            "installed skeleton-arm colors leave death-phase handoff unchanged");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Crocomire color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Crocomire color property fails loudly");
        visual.FightBody[1] = visual.FightBody[1] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => CrocomireColorCatalog.Write(visual),
            "Crocomire RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Crocomire colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Crocomire colors: five native RGB5 transfers, live boss CGRAM, white flash, persistent override, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, ushort> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Crocomire {name} color {color} preserves native RGB5");
        }

        void AssertBand(int destination, int count, Func<int, ushort> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), cgram.Colors[destination + color],
                    $"installed Crocomire {name} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckEditedBand(int count, Func<int, ushort> original,
            Func<int, ushort> changed, string name)
        {
            AssertTrue(original(1) != changed(1),
                $"Crocomire {name} edit changes selected color");
            for (int color = 0; color < count; color++)
                if (color != 1)
                    AssertEqual(original(color), changed(color),
                        $"Crocomire {name} edit leaves color {color} unchanged");
        }
    }

    private sealed class CrocomirePaletteReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address >= CrocomirePaletteRomData.FightBodySource &&
            address < CrocomirePaletteRomData.WallSpikesSource +
                CrocomirePaletteRomData.WallSpikesCount * sizeof(ushort)
                ? throw new InvalidOperationException(
                    $"Crocomire accessed migrated palette ROM ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
