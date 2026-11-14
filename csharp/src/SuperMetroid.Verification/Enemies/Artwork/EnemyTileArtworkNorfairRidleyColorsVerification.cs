using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Bosses.Ridley;

using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
/// <summary>Checks installed Norfair Ridley palette rows against native CGRAM writes and reveal cadence.</summary>
internal static class EnemyTileArtworkNorfairRidleyColorsVerification
{
    /// <summary>Compares all initial and reveal colors and guarded production updates, and rejects out-of-range channels.</summary>
    internal static void VerifyInstalledNorfairRidleyColors(ISnesAddressSpace bus,
        string stockDirectory, ImportEnemyArtwork stock)
    {
        if (stock.Bosses.NorfairRidleyColors is null)
            throw new InvalidDataException("Installed enemy artwork lacks Norfair Ridley colors.");
        var forbidden = new HashSet<int>();
        for (int color = 0; color < NorfairRidleyColorFormat.InitialColorCount; color++)
            CheckSource(EnemySourceDefinitions.NorfairRidleyInitialColors + color * sizeof(ushort),
                stock.Bosses.NorfairRidleyColors.ResolveInitial(color).ToWord());
        for (int row = 0; row < NorfairRidleyRevealDefinitions.RevealStepCount; row++)
        {
            int pointerAddress = (int)EnemySourceDefinitions.RevealSourcePointers + row * sizeof(ushort);
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), pointerAddress);
            forbidden.Add(pointerAddress);
            forbidden.Add(pointerAddress + 1);
            AssertTrue(pointer >= 0x8000, $"Norfair Ridley reveal row {row} has a bank-$A6 source");
            for (int color = 0; color < NorfairRidleyColorFormat.RevealColorCount; color++)
                CheckSource(0xa60000 + pointer + color * sizeof(ushort),
                    (stock.Bosses.NorfairRidleyColors.ResolveReveal(row, color)).ToWord());
        }
        int terminatorAddress = (int)EnemySourceDefinitions.RevealSourcePointers +
            NorfairRidleyRevealDefinitions.RevealStepCount * sizeof(ushort);
        AssertEqual((ushort)0, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), terminatorAddress),
            "Norfair Ridley reveal pointer table ends after fifteen color rows");
        forbidden.Add(terminatorAddress);
        forbidden.Add(terminatorAddress + 1);
        CartridgeReadGuard guard = new CartridgeReadGuard(bus).Forbid(forbidden.Contains,
            address => $"Installed Norfair Ridley palette read migrated ROM byte ${address:X6}.");
        (SnesCgram nativeCgram, RidleyEnemyState nativeState) = CreateReference();
        (RoomEnemySystem installed, RoomEnemySlot installedSlot,
            RidleyEnemyState installedState) = Create(guard, stock);
        // The reveal's terminator hands the battle liquid height to the room's liquid.
        var installedLiquid = new RoomFxOverlayState(new RoomFxSession());
        installed.RoomFx.BindRoomFx(installedLiquid);
        AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(Shown(installedState, stock))),
            "Norfair Ridley initial palette matches all native CGRAM entries");
        for (int call = 0; call <= NorfairRidleyRevealDefinitions.RevealStepCount * 3; call++)
        {
            TickReference(nativeCgram, nativeState);
            Tick(installed, installedSlot, installedState);
            AssertEqual(nativeState.FadeStep, installedState.FadeStep,
                $"Norfair Ridley reveal call {call} row/terminator position");
            AssertEqual(nativeState.PhaseTimer, installedState.PhaseTimer,
                $"Norfair Ridley reveal call {call} three-call cadence");
            AssertEqual(nativeState.Phase, installedState.Phase,
                $"Norfair Ridley reveal call {call} terminal function");
            AssertTrue(new CgramPalettes(nativeCgram).ImageEquals(new CgramPalettes(Shown(installedState, stock))),
                $"Norfair Ridley reveal call {call} preserves full native CGRAM");
        }
        AssertEqual(RidleyPhase.ClearVelocity, installedState.Phase,
            "Norfair Ridley reveal zero terminator enters the native movement handoff");
        AssertEqual<int?>(RidleyLiquidDefinitions.BattleHeight, installedLiquid.TargetSurface,
            "Norfair Ridley reveal terminator publishes the battle liquid height");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Installed Norfair Ridley reveal never rereads color or pointer ROM data");

        byte[] stockBytes = File.ReadAllBytes(Path.Combine(stockDirectory, NorfairRidleyColorFormat.FileName));
        JsonNode invalid = RequiredJson.Parse(stockBytes);
        invalid.Required("reveal").Required(0).Required(0)["green"] = 32;
        AssertThrows<InvalidDataException>(() => NorfairRidleyColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Norfair Ridley RGB5 component rejected");
        Console.WriteLine("  Norfair Ridley colors: 242 native RGB5 words, initial/reveal full-CGRAM parity, three-call cadence and terminator, ROM guard, and channel rejection pass.");

        // Matches one installed RGB5 word to its native address and marks both source bytes forbidden.
        void CheckSource(int address, ushort expected)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), expected,
                $"Norfair Ridley color source ${address:X6}");
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
            return new CreateReferenceResult(cgram, new RidleyEnemyState { Phase = RidleyPhase.WaitBeforeLiftoff });
        }

        // Applies one native reveal update, including the three-call timer and terminating pointer.
        void TickReference(SnesCgram cgram, RidleyEnemyState state)
        {
            if (--state.PhaseTimer >= 0)
                return;
            state.PhaseTimer = 2;
            ushort pointer = ReadReferenceWord(0xa6a4eb + state.FadeStep * 2);
            state.FadeStep++;
            if (pointer != 0)
            {
                for (int color = 0; color < 14; color++)
                    cgram.SetColor(113 + color, RgbColor.FromWord(checked(ReadReferenceWord(0xa60000 + pointer + color * 2))));
                return;
            }
            state.FadeStep = 0;
            state.Phase = RidleyPhase.ClearVelocity;
        }

        // Reads a raw cartridge word for the independent palette oracle.
        ushort ReadReferenceWord(int address) =>
            LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, address), CpuBusRegionRead.Read(bus, address + 1));

        // Initializes the production Norfair Ridley actor with selected artwork and seeded CGRAM.
        static EnemyTileArtworkNorfairRidleyColorsCreateResult Create(
            ISnesAddressSpace source, ImportEnemyArtwork artwork)
        {
            var enemy = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = artwork.Bosses };
            enemy.VerificationServices().IsAreaBossDefeated = () => false;
            RoomEnemySlot slot = enemy.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Ridley, slot.Definition);
            enemy.RidleyBehavior.Combat.InitializeNorfairRidley(slot);
            RidleyEnemyState state = enemy.RidleyEncounter.Ridley ??
                throw new InvalidOperationException("Norfair Ridley initialization did not publish its state.");
            state.Phase = RidleyPhase.WaitBeforeLiftoff;
            state.PhaseTimer = 0;
            return new EnemyTileArtworkNorfairRidleyColorsCreateResult(enemy, slot, state);
        }

        // The colours the production encounter shows over the seeded CGRAM image: the import's shades of the installed
        // colours, laid as its AI laid them (#1512).
        static SnesCgram Shown(RidleyEnemyState state, ImportEnemyArtwork artwork)
        {
            NorfairRidleyColorCatalog colours = artwork.Bosses.NorfairRidleyColors
                ?? throw new InvalidOperationException("Norfair Ridley's fixture installs his colours.");
            ushort[] words = RidleyLaidColours.Over(state, RidleySheetShades.NorfairSprites(colours), RidleySheetShades.NorfairRoom(colours),
                [.. Enumerable.Range(0, SnesCgram.ColorCount).Select(color => (ushort)(color * 31))]);
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, RgbColor.FromWord(words[color]));
            return cgram;
        }

        // Executes one production arena-reveal callback.
        static void Tick(RoomEnemySystem enemy, RoomEnemySlot slot, RidleyEnemyState state)
        {
            enemy.RidleyBehavior.Combat.TickNorfairRidleyArenaReveal(slot, state);
        }
    }
}
