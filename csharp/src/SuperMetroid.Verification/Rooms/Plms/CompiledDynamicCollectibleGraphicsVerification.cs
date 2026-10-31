using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using static SuperMetroid.Verification.Rooms.Plms.LookupDynamicCollectiblePalettesVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupDynamicCollectibleSourcesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled item graphics and the collectible a retail room population shows over its graphics slot.</summary>
internal static class CompiledDynamicCollectibleGraphicsVerification
{
    /// <summary>
    /// Checks the compiled item graphics against the cartridge, then that a real room load names the Bombs item over its
    /// graphics slot, which selects the slot blocks' pictures, while blocking compiled graphics ROM reads (#1466).
    /// </summary>
    internal static void VerifyCompiledDynamicCollectibleGraphics(
        SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDynamicCollectibleSourcePointers), () => VerifyDynamicCollectibleSourcePointers(rom));
        Suite(nameof(VerifyDynamicCollectibleKindIdentity), () => VerifyDynamicCollectibleKindIdentity());
        Suite(nameof(VerifyDynamicCollectiblePaletteSelectors), () => VerifyDynamicCollectiblePaletteSelectors(rom));

        // Retail population $8F:83FE contains a Chozo-orb Bombs item. Ban every
        // compiled graphics payload and every retail item-list upload from the bus,
        // then exercise the real sequential room loader and assert what it binds.
        var guarded = NewRetailCollectibleGraphicsReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, 64), new RoomLevelPlanes(new RoomTerrainBlock[width * 64], new RoomBlockBehavior[width * 64], new RoomBlockAppearance[width * 64]));
        RoomAppearance appearance = BgArtworkFixtures.For(new SnesVram());
        var plms = PlmFixtures.Unbound();
        PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, appearance, new SharedGameState(), AreaId.Crateria), RoomPlmPopulationDefinitions.Get(0x83fe), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation)), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        CollectiblePlmSnapshot bombs = plms.CollectiblePlms.Collectibles.Single(item => item.Kind == InWorldCollectibleKind.Bombs);
        // The slot's blocks name their pictures by the collectible shown in the slot (#1431, #1453, #1466); nothing is
        // uploaded.
        AssertEqual<InWorldCollectibleKind?>(InWorldCollectibleKind.Bombs, appearance.CollectibleIn(bombs.GraphicsSlot),
            "retail Bombs item shows in its graphics slot");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail collectible loader does not reread compiled character or palette bytes");
    }

    /// <summary>Reads a bank-$84 native graphics instruction word for the independent oracle.</summary>
    internal static ushort ReadCollectibleGraphicsWord(
        ISnesAddressSpace bus, int address) => CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(0x840000 | address);

    /// <summary>Rejects live reads of compiled collectible tiles and item-list upload records.</summary>
    private static CartridgeReadGuard NewRetailCollectibleGraphicsReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => DynamicCollectibleGraphicsConstants.All.Any(graphic =>
                {
                    int first = 0x890000 | graphic.GraphicsPointer;
                    if (address >= first && address < first + graphic.Tiles.Length)
                        return true;
                    int kind = (int)graphic.Kind;
                    return new ushort[]
                        { PlmNativeWords.Word(PlmHeaderId.ExposedEnergyTank),
                          PlmNativeWords.Word(PlmHeaderId.ChozoEnergyTank),
                          PlmNativeWords.Word(PlmHeaderId.ShotBlockEnergyTank) }.Any(baseHeader =>
                    {
                        ushort header = checked((ushort)(baseHeader + kind * 4));
                        int instructionAddress = 0x840000 | ReadCollectibleGraphicsWord(source, header + 2);
                        return address >= instructionAddress && address < instructionAddress + 12;
                    });
                }),
                address => $"Retail collectible reread compiled graphics byte ${address:X6}.");
}
