using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Plms.DynamicCollectibleArtVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupDynamicCollectiblePalettesVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupDynamicCollectibleSourcesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled item graphics through retail room population loading.</summary>
internal static class CompiledDynamicCollectibleGraphicsVerification
{
    /// <summary>Checks Bombs item VRAM after a real room load while blocking compiled graphics ROM reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyCompiledDynamicCollectibleGraphics(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDynamicCollectibleSourcePointers), () => VerifyDynamicCollectibleSourcePointers(rom));
        Suite(nameof(VerifyDynamicCollectibleKindIdentity), () => VerifyDynamicCollectibleKindIdentity());
        Suite(nameof(VerifyDynamicCollectiblePaletteSelectors), () => VerifyDynamicCollectiblePaletteSelectors(rom));

        // Retail population $8F:83FE contains a Chozo-orb Bombs item. Ban every
        // compiled graphics payload and every retail item-list upload from the bus,
        // then exercise the real sequential room loader and assert its resulting VRAM.
        var guarded = NewRetailCollectibleGraphicsReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, 64), new RoomLevelPlanes(new ushort[width * 64], new byte[width * 64], new ushort[width * 64]), new byte[0x400 * 8]);
        var vram = new SnesVram();
        var plms = new RoomPlmSystem();
        plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, level, level.CreateBackgroundStreamer(), vram, new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationDefinition.FromCompiled(0x83fe), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertTrue(plms.CollectiblePlms.Collectibles.Any(item =>
                item.Kind == InWorldCollectibleKind.Bombs),
            "retail Bombs item remains allocated after compiled graphics upload");
        ReadOnlySpan<byte> expected =
            RoomPlmDynamicCollectibleGraphicsDefinitions.Get(
                InWorldCollectibleKind.Bombs).Tiles.Span;
        for (int offset = 0; offset < expected.Length; offset++)
            AssertEqual(expected[offset], vram.ReadByte(0x3e00 * 2 + offset),
                $"retail Bombs item VRAM character byte {offset}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail collectible loader does not reread compiled character or palette bytes");
        Suite(nameof(VerifyInstalledDynamicCollectibleArt), () => VerifyInstalledDynamicCollectibleArt(rom));
    }

    /// <summary>Reads a bank-$84 native graphics instruction word for the independent oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for collectible graphics words")]
    internal static ushort ReadCollectibleGraphicsWord(
        ISnesAddressSpace bus, int address) => CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(0x840000 | address);

    /// <summary>Rejects live reads of compiled collectible tiles and item-list upload records.</summary>
    private static CartridgeReadGuard NewRetailCollectibleGraphicsReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => RoomPlmDynamicCollectibleGraphicsDefinitions.All.Any(graphic =>
                {
                    int first = 0x890000 | graphic.GraphicsPointer;
                    if (address >= first && address < first + graphic.Tiles.Length)
                        return true;
                    int kind = (int)graphic.Kind;
                    return new ushort[]
                        { (ushort)PlmHeaderId.ExposedEnergyTank,
                          (ushort)PlmHeaderId.ChozoEnergyTank,
                          (ushort)PlmHeaderId.ShotBlockEnergyTank }.Any(baseHeader =>
                    {
                        ushort header = checked((ushort)(baseHeader + kind * 4));
                        int instructionAddress = 0x840000 | ReadCollectibleGraphicsWord(source, header + 2);
                        return address >= instructionAddress && address < instructionAddress + 12;
                    });
                }),
                address => $"Retail collectible reread compiled graphics byte ${address:X6}.");
}
