using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupDynamicCollectiblePalettesVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupDynamicCollectibleSourcesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled item graphics and that a retail room population sets its item up from them.</summary>
internal static class CompiledDynamicCollectibleGraphicsVerification
{
    /// <summary>
    /// Checks the compiled item graphics against the cartridge, then that a real room load sets the Bombs item up while
    /// blocking compiled graphics ROM reads (#1466); the item shows its kind's own picture (#1538).
    /// </summary>
    internal static void VerifyCompiledDynamicCollectibleGraphics(
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyDynamicCollectibleSourcePointers), () => VerifyDynamicCollectibleSourcePointers(rom));
        Suite(nameof(VerifyDynamicCollectibleKindIdentity), () => VerifyDynamicCollectibleKindIdentity());
        Suite(nameof(VerifyDynamicCollectiblePaletteSelectors), () => VerifyDynamicCollectiblePaletteSelectors(rom));

        // Retail population $8F:83FE contains a Chozo-orb Bombs item. Ban every
        // compiled graphics payload and every retail item-list upload from the bus,
        // then exercise the real sequential room loader and assert what it binds.
        var guarded = NewRetailCollectibleGraphicsReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, 64), new RoomLevelPlanes(new RoomTerrainBlock[width * 64], new RoomBlockBehavior[width * 64], new RoomBlockAppearance[width * 64]), RoomLevelData.NothingBeyondGrid());
        RoomAppearance appearance = BgArtworkFixtures.For(new SnesVram());
        var roomObjects = RoomObjectFixtures.Unbound();
        RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, appearance, new SharedGameState(new SharedRandom()), AreaId.Crateria), RoomObjectPopulationDefinitions.Get(0x83fe), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
        AssertEqual(1, roomObjects.CollectibleObjects.Collectibles.Count(item => item.Kind == InWorldCollectibleKind.Bombs),
            "retail population sets its Bombs item up");
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
                        { RoomObjectNativeWords.Word(RoomObjectHeaderId.ExposedEnergyTank),
                          RoomObjectNativeWords.Word(RoomObjectHeaderId.ChozoEnergyTank),
                          RoomObjectNativeWords.Word(RoomObjectHeaderId.ShotBlockEnergyTank) }.Any(baseHeader =>
                    {
                        ushort header = checked((ushort)(baseHeader + kind * 4));
                        int instructionAddress = 0x840000 | ReadCollectibleGraphicsWord(source, header + 2);
                        return address >= instructionAddress && address < instructionAddress + 12;
                    });
                }),
                address => $"Retail collectible reread compiled graphics byte ${address:X6}.");
}
