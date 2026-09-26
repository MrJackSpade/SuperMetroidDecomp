using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledNorfairRidleyColors(ISnesAddressSpace bus,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        if (stock.NorfairRidleyColors is null)
            throw new InvalidDataException("Installed enemy artwork lacks Norfair Ridley colors.");
        var forbidden = new HashSet<int>();
        for (int color = 0; color < NorfairRidleyPaletteRomData.InitialColorCount; color++)
            CheckSource(NorfairRidleyPaletteRomData.InitialColors + color * sizeof(ushort),
                stock.NorfairRidleyColors.ResolveInitial(color));
        for (int row = 0; row < NorfairRidleyPaletteRomData.RevealRowCount; row++)
        {
            int pointerAddress = NorfairRidleyPaletteRomData.RevealSourcePointers + row * sizeof(ushort);
            ushort pointer = RomDataReader.ReadWordFixedBank(bus, pointerAddress);
            forbidden.Add(pointerAddress);
            forbidden.Add(pointerAddress + 1);
            AssertTrue(pointer >= 0x8000, $"Norfair Ridley reveal row {row} has a bank-$A6 source");
            for (int color = 0; color < NorfairRidleyPaletteRomData.RevealColorCount; color++)
                CheckSource(0xa60000 + pointer + color * sizeof(ushort),
                    stock.NorfairRidleyColors.ResolveReveal(row, color));
        }
        int terminatorAddress = NorfairRidleyPaletteRomData.RevealSourcePointers +
            NorfairRidleyPaletteRomData.RevealRowCount * sizeof(ushort);
        AssertEqual((ushort)0, RomDataReader.ReadWordFixedBank(bus, terminatorAddress),
            "Norfair Ridley reveal pointer table ends after fifteen color rows");
        forbidden.Add(terminatorAddress);
        forbidden.Add(terminatorAddress + 1);
        var guard = new NorfairRidleyColorReadGuard(bus, forbidden);
        (RoomEnemySystem native, SnesCgram nativeCgram, RoomEnemySlot nativeSlot,
            RidleyEnemyState nativeState) = Create(bus, null);
        (RoomEnemySystem installed, SnesCgram installedCgram, RoomEnemySlot installedSlot,
            RidleyEnemyState installedState) = Create(guard, stock);
        AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
            "Norfair Ridley initial palette matches all native CGRAM entries");
        for (int call = 0; call <= NorfairRidleyPaletteRomData.RevealRowCount * 3; call++)
        {
            Tick(native, nativeSlot, nativeState);
            Tick(installed, installedSlot, installedState);
            AssertEqual(nativeState.FadePaletteOffset, installedState.FadePaletteOffset,
                $"Norfair Ridley reveal call {call} row/terminator position");
            AssertEqual(nativeState.FunctionTimer, installedState.FunctionTimer,
                $"Norfair Ridley reveal call {call} three-call cadence");
            AssertEqual(nativeState.Function, installedState.Function,
                $"Norfair Ridley reveal call {call} terminal function");
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                $"Norfair Ridley reveal call {call} preserves full native CGRAM");
        }
        AssertEqual(RidleyAiFunction.ClearVelocity, installedState.Function,
            "Norfair Ridley reveal zero terminator enters the native movement handoff");
        AssertEqual(RidleyLiquidRomData.BattleHeight, installedState.FxTargetYPosition,
            "Norfair Ridley reveal terminator publishes the battle liquid height");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Installed Norfair Ridley reveal never rereads color or pointer ROM data");

        string stockPath = Path.Combine(stockDirectory, NorfairRidleyColorFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "norfair-ridley-color-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, NorfairRidleyColorFormat.FileName);
        JsonNode changed = JsonNode.Parse(stockBytes)!;
        int initialRed = changed["initial"]![1]!["red"]!.GetValue<int>();
        int revealBlue = changed["reveal"]![7]![3]!["blue"]!.GetValue<int>();
        changed["initial"]![1]!["red"] = initialRed ^ 1;
        changed["reveal"]![7]![3]!["blue"] = revealBlue ^ 1;
        byte[] editedBytes = Encoding.UTF8.GetBytes(changed.ToJsonString());
        File.WriteAllBytes(overridePath, editedBytes);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        (RoomEnemySystem editedEnemy, SnesCgram editedCgram, RoomEnemySlot editedSlot,
            RidleyEnemyState editedState) = Create(guard, edited);
        (RoomEnemySystem control, SnesCgram controlCgram, RoomEnemySlot controlSlot,
            RidleyEnemyState controlState) = Create(bus, null);
        CheckEditedCgram("Norfair Ridley edited initial colors", revealRowVisible: false);
        for (int call = 0; call <= 7 * 3; call++)
        {
            Tick(editedEnemy, editedSlot, editedState);
            Tick(control, controlSlot, controlState);
            AssertEqual(controlState.FadePaletteOffset, editedState.FadePaletteOffset,
                $"Norfair Ridley edited reveal call {call} preserves row selection");
            AssertEqual(controlState.FunctionTimer, editedState.FunctionTimer,
                $"Norfair Ridley edited reveal call {call} preserves cadence");
        }
        AssertEqual((ushort)8, editedState.FadePaletteOffset,
            "Norfair Ridley edited reveal row seven is selected");
        CheckEditedCgram("Norfair Ridley edited reveal row seven", revealRowVisible: true);
        Tick(editedEnemy, editedSlot, editedState);
        Tick(control, controlSlot, controlState);
        Tick(editedEnemy, editedSlot, editedState);
        Tick(control, controlSlot, controlState);
        Tick(editedEnemy, editedSlot, editedState);
        Tick(control, controlSlot, controlState);
        AssertEqual((ushort)9, editedState.FadePaletteOffset,
            "Norfair Ridley reveal advances after three calls");
        CheckEditedCgram("Norfair Ridley next row replaces edit", revealRowVisible: false);

        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
        AssertTrue(editedBytes.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Stock re-extraction leaves Norfair Ridley override untouched");
        EnemyTileArtworkCatalog reloaded = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        AssertEqual(edited.NorfairRidleyColors!.ResolveReveal(7, 3),
            reloaded.NorfairRidleyColors!.ResolveReveal(7, 3),
            "Norfair Ridley reveal edit survives restart/re-extraction");
        File.WriteAllText(overridePath, "broken");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Corrupt Norfair Ridley override fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1}");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Duplicate Norfair Ridley JSON property fails loudly");
        JsonNode invalid = JsonNode.Parse(stockBytes)!;
        invalid["reveal"]![0]![0]!["green"] = 32;
        AssertThrows<InvalidDataException>(() => NorfairRidleyColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Norfair Ridley RGB5 component rejected");
        File.Delete(overridePath);
        File.WriteAllText(stockPath, "broken stock");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "Corrupt Norfair Ridley stock colors fail manifest validation");
        File.WriteAllBytes(stockPath, stockBytes);
        Console.WriteLine("  Norfair Ridley colors: 242 native RGB5 words, initial/reveal full-CGRAM parity, three-call cadence and terminator, ROM guard, isolated edits, override persistence and strict failures pass.");

        void CheckSource(int address, ushort expected)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, address), expected,
                $"Norfair Ridley color source ${address:X6}");
        }

        void CheckEditedCgram(string context, bool revealRowVisible)
        {
            for (int color = 0; color < SnesCgram.ColorCount; color++)
            {
                ushort expected = controlCgram.Colors[color];
                if (color == NorfairRidleyPaletteRomData.InitialCgramIndex + 1)
                    expected ^= 1;
                if (revealRowVisible && color == NorfairRidleyPaletteRomData.RevealCgramIndex + 3)
                    expected ^= 1 << 10;
                AssertEqual(expected, editedCgram.Colors[color], $"{context} CGRAM {color}");
            }
        }

        static (RoomEnemySystem, SnesCgram, RoomEnemySlot, RidleyEnemyState) Create(
            ISnesAddressSpace source, EnemyTileArtworkCatalog? artwork)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemy = new RoomEnemySystem { TileArtwork = artwork };
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, (ushort)(color * 31));
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemy, source);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemy, cgram);
            typeof(RoomEnemySystem).GetField("_isAreaBossDefeated", flags)!
                .SetValue(enemy, (Func<bool>)(() => false));
            RoomEnemySlot slot = enemy.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.NorfairRidleyDefinition;
            typeof(RoomEnemySystem).GetMethod("InitializeNorfairRidley", flags)!
                .Invoke(enemy, [slot]);
            RidleyEnemyState state = enemy.Ridley ??
                throw new InvalidOperationException("Norfair Ridley initialization did not publish its state.");
            state.Function = RidleyAiFunction.WaitBeforeLiftoff;
            state.FunctionTimer = 0;
            return (enemy, cgram, slot, state);
        }

        static void Tick(RoomEnemySystem enemy, RoomEnemySlot slot, RidleyEnemyState state)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyArenaReveal", flags)!
                .Invoke(enemy, [slot, state]);
        }
    }

    private sealed class NorfairRidleyColorReadGuard(ISnesAddressSpace source,
        HashSet<int> forbidden) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Installed Norfair Ridley palette read migrated ROM byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
