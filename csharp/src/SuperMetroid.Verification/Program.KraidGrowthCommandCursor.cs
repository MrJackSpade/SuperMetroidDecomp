using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidGrowthCommandCursor()
    {
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var guard = new KraidHeadProgramReadGuard(rom);
        var referenceArt = new EnemyIdentityFixture().Build().KraidBackground!;
        var head = KraidHeadTilemapAtlas.Load(new MemoryStream(KraidHeadTilemapAtlas.Encode(
            new byte[KraidBackgroundRomData.HeadTilemapWords * 2])));
        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
                kraidBackground: new KraidBackgroundArtwork(referenceArt.Upper, referenceArt.Lower,
                    new Dictionary<ushort, KraidHeadTilemapAtlas> { [0x9dc8] = head }, referenceArt.RoomBackgroundTiles)),
        };
        var state = new KraidEnemyState();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, new SnesVram());
        typeof(RoomEnemySystem).GetField("_kraidState", flags)!.SetValue(enemies, state);
        var execute = typeof(RoomEnemySystem).GetMethod("ExecuteKraidHeadInstruction", flags)!
            .CreateDelegate<Func<RoomEnemySlot, KraidEnemyState, ushort>>(enemies);
        var footMain = typeof(RoomEnemySystem).GetMethod("RunKraidFootMain", flags)!
            .CreateDelegate<Action<RoomEnemySlot, ushort>>(enemies);
        var body = enemies.Slots[0];
        var foot = enemies.Slots[5];
        body.EnemyDefinitionPointer = RoomEnemySystem.KraidDefinition;
        state.HealthEighthThresholds[6] = 875;
        body.Health = 874;
        body.VariableB = 0x96e2;
        execute(body, state);
        AssertEqual((ushort)0x96ea, body.VariableB, "head interpreter leaves the next cursor on the roar command");
        ushort displayed = state.CurrentHeadTilemap;
        foot.VariableA = (ushort)KraidAiFunction.FootPrepareFirstPhaseLunge;
        footMain(foot, 0);
        AssertEqual((ushort)0x96f4, body.VariableB, "native command-cursor growth resumes at the closing-mouth frame");
        AssertEqual((ushort)64, body.VariableC, "native quick-kill growth delay is preserved");
        AssertEqual(displayed, state.CurrentHeadTilemap, "growth selection does not advance the displayed head");
        AssertEqual(KraidAiFunction.GrowReleaseCamera, state.Parts[0].NextFunction, "growth schedules camera release");
        AssertEqual((ushort)180, body.VariableF, "growth function timer");
        AssertEqual((ushort)KraidAiFunction.NoOperation, foot.VariableA, "growth stops the foot lunge");
        AssertEqual(0, guard.ForbiddenReadAttempts, "growth selection does not read compiled cartridge programs");

        // Confirm this offset-two conversion for every declared command form,
        // including sound words and terminal words adjacent to the next list.
        foreach (var definition in KraidHeadInstructionDefinitions.All)
        {
            int address = 0xa70000 | (definition.Pointer + 2);
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(guard, definition.Pointer),
                $"native growth selection word at ${definition.Pointer:X4}");
        }
        Console.WriteLine("Kraid growth command cursor: reported foot-lunge transition, native 64-frame delay, retained display, and offset-two words for all declared commands pass.");
    }
}
