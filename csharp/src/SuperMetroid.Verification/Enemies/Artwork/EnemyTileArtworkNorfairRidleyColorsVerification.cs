using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Checks installed Norfair Ridley palette rows against native CGRAM writes and reveal cadence.</summary>
internal static class EnemyTileArtworkNorfairRidleyColorsVerification
{
    /// <summary>Compares all initial and reveal colors, guarded production updates, isolated edits, and override validation.</summary>
    internal static void VerifyInstalledNorfairRidleyColors(ISnesAddressSpace bus,
        string stockDirectory, InstalledEnemyArtwork stock)
    {
        if (stock.Bosses.NorfairRidleyColors is null)
            throw new InvalidDataException("Installed enemy artwork lacks Norfair Ridley colors.");
        var forbidden = new HashSet<int>();
        for (int color = 0; color < NorfairRidleyPaletteDefinitions.InitialColorCount; color++)
            CheckSource(EnemySourceDefinitions.NorfairRidleyInitialColors + color * sizeof(ushort),
                stock.Bosses.NorfairRidleyColors.ResolveInitial(color).ToWord());
        for (int row = 0; row < NorfairRidleyPaletteDefinitions.RevealRowCount; row++)
        {
            int pointerAddress = (int)EnemySourceDefinitions.RevealSourcePointers + row * sizeof(ushort);
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), pointerAddress);
            forbidden.Add(pointerAddress);
            forbidden.Add(pointerAddress + 1);
            AssertTrue(pointer >= 0x8000, $"Norfair Ridley reveal row {row} has a bank-$A6 source");
            for (int color = 0; color < NorfairRidleyPaletteDefinitions.RevealColorCount; color++)
                CheckSource(0xa60000 + pointer + color * sizeof(ushort),
                    (stock.Bosses.NorfairRidleyColors.ResolveReveal(row, color)).ToWord());
        }
        int terminatorAddress = (int)EnemySourceDefinitions.RevealSourcePointers +
            NorfairRidleyPaletteDefinitions.RevealRowCount * sizeof(ushort);
        AssertEqual((ushort)0, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), terminatorAddress),
            "Norfair Ridley reveal pointer table ends after fifteen color rows");
        forbidden.Add(terminatorAddress);
        forbidden.Add(terminatorAddress + 1);
        CartridgeReadGuard guard = new CartridgeReadGuard(bus).Forbid(forbidden.Contains,
            address => $"Installed Norfair Ridley palette read migrated ROM byte ${address:X6}.");
        (SnesCgram nativeCgram, RidleyEnemyState nativeState) = CreateReference();
        (RoomEnemySystem installed, SamusColourRig installedCgram, RoomEnemySlot installedSlot,
            RidleyEnemyState installedState) = Create(guard, stock);
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
            "Norfair Ridley initial palette matches all native CGRAM entries");
        for (int call = 0; call <= NorfairRidleyPaletteDefinitions.RevealRowCount * 3; call++)
        {
            TickReference(nativeCgram, nativeState);
            Tick(installed, installedSlot, installedState);
            AssertEqual(nativeState.FadeStep, installedState.FadeStep,
                $"Norfair Ridley reveal call {call} row/terminator position");
            AssertEqual(nativeState.FunctionTimer.RawValue, installedState.FunctionTimer.RawValue,
                $"Norfair Ridley reveal call {call} three-call cadence");
            AssertEqual(nativeState.Function, installedState.Function,
                $"Norfair Ridley reveal call {call} terminal function");
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(installedCgram)),
                $"Norfair Ridley reveal call {call} preserves full native CGRAM");
        }
        AssertEqual(RidleyAiFunction.ClearVelocity, installedState.Function,
            "Norfair Ridley reveal zero terminator enters the native movement handoff");
        AssertEqual(RidleyLiquidDefinitions.BattleHeight, installedState.FxTargetYPosition,
            "Norfair Ridley reveal terminator publishes the battle liquid height");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Installed Norfair Ridley reveal never rereads color or pointer ROM data");

        string stockPath = Path.Combine(stockDirectory, NorfairRidleyColorFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "norfair-ridley-color-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, NorfairRidleyColorFormat.FileName);
        JsonNode changed = RequiredJson.Parse(stockBytes);
        int initialRed = changed.Required("initial").Required(1).Required("red").GetValue<int>();
        int revealBlue = changed.Required("reveal").Required(7).Required(3).Required("blue").GetValue<int>();
        changed.Required("initial").Required(1)["red"] = initialRed ^ 1;
        changed.Required("reveal").Required(7).Required(3)["blue"] = revealBlue ^ 1;
        byte[] editedBytes = Encoding.UTF8.GetBytes(changed.ToJsonString());
        File.WriteAllBytes(overridePath, editedBytes);
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        (RoomEnemySystem editedEnemy, SamusColourRig editedCgram, RoomEnemySlot editedSlot,
            RidleyEnemyState editedState) = Create(guard, edited);
        (SnesCgram controlCgram, RidleyEnemyState controlState) = CreateReference();
        CheckEditedCgram("Norfair Ridley edited initial colors", revealRowVisible: false);
        for (int call = 0; call <= 7 * 3; call++)
        {
            Tick(editedEnemy, editedSlot, editedState);
            TickReference(controlCgram, controlState);
            AssertEqual(controlState.FadeStep, editedState.FadeStep,
                $"Norfair Ridley edited reveal call {call} preserves row selection");
            AssertEqual(controlState.FunctionTimer.RawValue, editedState.FunctionTimer.RawValue,
                $"Norfair Ridley edited reveal call {call} preserves cadence");
        }
        AssertEqual(8, editedState.FadeStep,
            "Norfair Ridley edited reveal row seven is selected");
        CheckEditedCgram("Norfair Ridley edited reveal row seven", revealRowVisible: true);
        Tick(editedEnemy, editedSlot, editedState);
        TickReference(controlCgram, controlState);
        Tick(editedEnemy, editedSlot, editedState);
        TickReference(controlCgram, controlState);
        Tick(editedEnemy, editedSlot, editedState);
        TickReference(controlCgram, controlState);
        AssertEqual(9, editedState.FadeStep,
            "Norfair Ridley reveal advances after three calls");
        CheckEditedCgram("Norfair Ridley next row replaces edit", revealRowVisible: false);

        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
        AssertTrue(editedBytes.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Stock re-extraction leaves Norfair Ridley override untouched");
        InstalledEnemyArtwork reloaded = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        AssertEqual((edited.Bosses.NorfairRidleyColors ?? throw new InvalidOperationException("Verification requires edited.NorfairRidleyColors.")).ResolveReveal(7, 3),
            (reloaded.Bosses.NorfairRidleyColors ?? throw new InvalidOperationException("Verification requires reloaded.NorfairRidleyColors.")).ResolveReveal(7, 3),
            "Norfair Ridley reveal edit survives restart/re-extraction");
        File.WriteAllText(overridePath, "broken");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Corrupt Norfair Ridley override fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1}");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Duplicate Norfair Ridley JSON property fails loudly");
        JsonNode invalid = RequiredJson.Parse(stockBytes);
        invalid.Required("reveal").Required(0).Required(0)["green"] = 32;
        AssertThrows<InvalidDataException>(() => NorfairRidleyColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Norfair Ridley RGB5 component rejected");
        File.Delete(overridePath);
        File.WriteAllText(stockPath, "broken stock");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "Corrupt Norfair Ridley stock colors fail manifest validation");
        File.WriteAllBytes(stockPath, stockBytes);
        Console.WriteLine("  Norfair Ridley colors: 242 native RGB5 words, initial/reveal full-CGRAM parity, three-call cadence and terminator, ROM guard, isolated edits, override persistence and strict failures pass.");

        // Matches one installed RGB5 word to its native address and marks both source bytes forbidden.
        void CheckSource(int address, ushort expected)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), expected,
                $"Norfair Ridley color source ${address:X6}");
        }

        // Checks full CGRAM while allowing only the selected initial red or current reveal blue edit.
        void CheckEditedCgram(string context, bool revealRowVisible)
        {
            for (int color = 0; color < SnesCgram.ColorCount; color++)
            {
                RgbColor expected = controlCgram.Colors[color];
                if (color == NorfairRidleyPaletteDefinitionsConstants.InitialCgramIndex + 1)
                    expected = RgbColor.FromWord((ushort)(expected.ToWord() ^ (1)));
                if (revealRowVisible && color == NorfairRidleyPaletteDefinitionsConstants.RevealCgramIndex + 3)
                    expected = RgbColor.FromWord((ushort)(expected.ToWord() ^ (1 << 10)));
                AssertEqual(expected, editedCgram.Colors[color], $"{context} CGRAM {color}");
            }
        }

        // Seeds the independent native initial CGRAM image and Ridley reveal state using
        // raw $A6:A478 palette words and the terminated $A6:A4D6 pointer table.
        CreateReferenceResult CreateReference()
        {
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, RgbColor.FromWord((ushort)(color * 31)));
            for (int color = 0; color < 32; color++)
                cgram.SetColor(160 + color, RgbColor.FromWord(checked(ReadReferenceWord(0xa6e1cf + color * 2))));
            for (int color = 0; color < 15; color++)
            {
                cgram.SetColor(113 + color, new RgbColor(0, 0, 0));
                cgram.SetColor(241 + color, new RgbColor(0, 0, 0));
            }
            return new CreateReferenceResult(cgram, new RidleyEnemyState { Function = RidleyAiFunction.WaitBeforeLiftoff });
        }

        // Applies one native reveal update, including the three-call timer and terminating pointer.
        void TickReference(SnesCgram cgram, RidleyEnemyState state)
        {
            state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
            if (!state.FunctionTimer.IsNegative)
                return;
            state.FunctionTimer = WrappingCountdown.Start(2);
            ushort pointer = ReadReferenceWord(0xa6a4eb + state.FadeStep * 2);
            state.FadeStep++;
            if (pointer != 0)
            {
                for (int color = 0; color < 14; color++)
                    cgram.SetColor(113 + color, RgbColor.FromWord(checked(ReadReferenceWord(0xa60000 + pointer + color * 2))));
                return;
            }
            state.FadeStep = 0;
            state.Function = RidleyAiFunction.ClearVelocity;
        }

        // Reads a raw cartridge word for the independent palette oracle.
        ushort ReadReferenceWord(int address) =>
            LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, address), CpuBusRegionRead.Read(bus, address + 1));

        // Initializes the production Norfair Ridley actor with selected artwork and seeded CGRAM.
        static EnemyTileArtworkNorfairRidleyColorsCreateResult Create(
            ISnesAddressSpace source, InstalledEnemyArtwork artwork)
        {
            var enemy = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
            var cgram = new SamusColourRig();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, RgbColor.FromWord((ushort)(color * 31)));
            enemy.Presentation.BindColours(cgram.Gameplay);
            enemy.VerificationServices().IsAreaBossDefeated = () => false;
            RoomEnemySlot slot = enemy.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Ridley, slot.Definition);
            enemy.RidleyBehavior.Combat.InitializeNorfairRidley(slot);
            RidleyEnemyState state = enemy.RidleyEncounter.Ridley ??
                throw new InvalidOperationException("Norfair Ridley initialization did not publish its state.");
            state.Function = RidleyAiFunction.WaitBeforeLiftoff;
            state.FunctionTimer = WrappingCountdown.Start(0);
            return new EnemyTileArtworkNorfairRidleyColorsCreateResult(enemy, cgram, slot, state);
        }

        // Executes one production arena-reveal callback.
        static void Tick(RoomEnemySystem enemy, RoomEnemySlot slot, RidleyEnemyState state)
        {
            enemy.RidleyBehavior.Combat.TickNorfairRidleyArenaReveal(slot, state);
        }
    }
}
