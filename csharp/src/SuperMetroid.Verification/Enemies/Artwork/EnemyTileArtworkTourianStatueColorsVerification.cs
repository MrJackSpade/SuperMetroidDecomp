using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Native colour-write and guarded-source checks for installed Tourian entrance statue colors (#1512).</summary>
internal static class EnemyTileArtworkTourianStatueColorsVerification
{
    /// <summary>Checks entrance, eye, and grey-room palette writes and out-of-range channel rejection.</summary>
    internal static void VerifyInstalledTourianStatueColors(ISnesAddressSpace bus,
        string stockDirectory, ImportEnemyArtwork stock)
    {
        TourianStatuePalettes colors = stock.Areas.TourianStatueColors ??
            throw new InvalidDataException("Installed enemy artwork lacks Tourian statue colors.");
        var forbidden = new HashSet<int>();
        for (int color = 0; color < TourianStatueColorFormat.BaseColorCount; color++)
            CheckSource(EnemySourceDefinitions.TourianStatueBaseColors + color * sizeof(ushort),
                colors.ResolveBase(color).ToWord());
        for (int color = 0; color < TourianStatueColorFormat.StatueColorCount; color++)
            CheckSource(EnemySourceDefinitions.TourianStatueColors + color * sizeof(ushort),
                colors.ResolveStatue(color).ToWord());
        for (int row = 0; row < TourianStatueColorDefinitions.EyeGlowCount; row++)
        for (int color = 0; color < TourianStatueColorDefinitions.EyeColorCount; color++)
            CheckSource(EnemySourceDefinitions.TourianStatueEyeColors +
                (row * TourianStatueColorDefinitions.EyeColorCount + color) * sizeof(ushort),
                (colors.ResolveEye(row, color)).ToWord());
        for (int color = 0; color < TourianStatueColorDefinitions.GreyColorCount; color++)
            CheckSource(EnemySourceDefinitions.TourianStatueGreyColors + color * sizeof(ushort),
                colors.ResolveGrey(color).ToWord());

        // The statue scene's pictures are the statues' spawned objects', which spawn with the shared sheet's graphics, on
        // the set's shared sheets (#1512, #1513, #1534): the statue enemy itself pictures nothing, so it has no sheet. The
        // decorations, the sprite statues and the released particles and tails are on one of those sheets; the colours a
        // published shading shows are that sheet's inks under it, at the places of the native image the import coloured
        // them from.
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate =>
            RoomEnemyGraphicsSetLists.MembersOf(candidate).Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.TourianStatue));
        SpawnedObjectSheet scenePictures = InstalledFixtures.EnemySheets.Spawned(set, SpawnedObjectOwner.Shared);
        SpriteImage statueSheet = scenePictures.Frame(EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.TourianStatueBaseDecoration)).Sheet;
        foreach (EnemyProjectileFrameId scene in new[]
        {
            EnemyProjectileFrameId.TourianStatueParticleFirst, EnemyProjectileFrameId.TourianStatueTailFirst,
            EnemyProjectileFrameId.TourianStatueBaseDecorationLoop, EnemyProjectileFrameId.TourianStatueRidley, EnemyProjectileFrameId.TourianStatuePhantoon,
        })
        {
            AssertEqual(statueSheet.Name, scenePictures.Frame(EnemySheetDefinitions.ProjectileFrameName(scene)).Sheet.Name,
                $"the statue scene's {scene} is on the decoration's installed sheet");
        }
        // The scene sheet composed in-process, which knows the palette index each pixel was composed as (#1633).
        SpriteImage composedStatueSheet = EnemySheetFixtures.ComposedSpawnedSheet(set, SpawnedObjectOwner.Shared)
            .Frame(EnemySheetDefinitions.ProjectileFrameName(EnemyProjectileFrameId.TourianStatueBaseDecoration)).Sheet;
        var guard = NewTourianStatueColorGuard(bus, forbidden);
        // The places of the native image the scene sheet holds a colour for (#1544): a sheet holds only the colours its
        // pictures show, so a native write to any other place shows nothing in the port.
        HashSet<int> sceneHeld = [.. composedStatueSheet.ColourKeys
            .SelectMany(key => SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.CartridgeColoursOf(composedStatueSheet, key))];
        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            RoomEnemySystem installed = CreateEnemy(stock);
            Initialize(installed, parameter);
            RgbColor[] shown = NativeColouredPictures.ShownIn(composedStatueSheet, installed.SlotTable.Slots[0].Shading);
            AssertNativeWrites(shown, sceneHeld, 0xaad785, TourianStatuePaletteConstants.BaseCgramIndex, TourianStatueColorFormat.BaseColorCount,
                $"Tourian statue entry parameter {parameter} shows the native base colours");
            AssertNativeWrites(shown, sceneHeld, 0xaad765, TourianStatuePaletteConstants.StatueCgramIndex, TourianStatueColorFormat.StatueColorCount,
                $"Tourian statue entry parameter {parameter} shows the native statue colours");
            AssertEqual(ReadReferenceWord(0xaad810 + parameter),
                NativeEnemyAnimationOracle.EntryOf(TourianEntranceStatueNativeListAddresses.Entries,
                    TourianEntranceStatueAnimations.StartingAt.Single(pair => ReferenceEquals(pair.Value,
                        installed.SlotTable.Slots[0].RequireSpeciesState<TourianEntranceStatueState>().Playback.Animation)).Key),
                $"Tourian statue entry parameter {parameter} keeps instruction selection");
            AssertEqual(parameter == 0 ? 3 : 0,
                installed.Projectiles.EnemyProjectiles.Count(x => x.IsActive),
                $"Tourian statue entry parameter {parameter} keeps spawned actors");
        }
        foreach (ushort parameter in new ushort[] { 0, 2, 4, 6 })
        {
            RoomEnemySystem installed = CreateEnemy(stock);
            installed.TourianUnlockBehavior.SpawnTourianEyeGlow(TourianStatueNativeWords.Eye(parameter));
            AssertNativeWrites(NativeColouredPictures.ShownIn(composedStatueSheet, installed.TourianEntranceStatueBehavior.SceneShading), sceneHeld,
                0x86b91e + parameter * 4, TourianStatuePaletteConstants.EyeCgramIndex, TourianStatueColorDefinitions.EyeColorCount,
                $"Tourian eye parameter {parameter} shows the native eye glow colours");
            AssertEqual(new ushort[] { 0x84, 0x7a, 0x9e, 0x68 }[parameter / 2],
                installed.Projectiles.EnemyProjectiles.Single(x => x.IsActive).Position.X.Whole,
                $"Tourian eye parameter {parameter} keeps physical X");
            AssertEqual(new ushort[] { 0x90, 0x51, 0x80, 0x72 }[parameter / 2],
                installed.Projectiles.EnemyProjectiles.Single(x => x.IsActive).Position.Y.Whole,
                $"Tourian eye parameter {parameter} keeps physical Y");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Tourian entry and eye actors do not read palette ROM ranges");

        byte[] stockBytes = File.ReadAllBytes(Path.Combine(stockDirectory, TourianStatueColorFormat.FileName));

        // The $87:837F grey instruction is reached on re-entry after each
        // defeated-boss statue's grey event. Exercise the actual room program,
        // including its four compiled destination operands.
        string romPath = Path.GetFullPath("Super Metroid.smc");
        var installedRoom = CreateGreyRoom(
            NewTourianStatueColorGuard(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath), forbidden), stock);
        installedRoom.TourianStatues.StepBlockAnimations();
        // After the step, the room's pictures under the room's shading and the statue scene's pictures under the scene's
        // shading show, at each of the four retail programs' grey destinations, the native grey instruction's colours
        // (#1512): the room-tile statues grey over the room's pictures, the sprite statues over the scene's.
        RgbColor[] greyRoom = GameplayShownColours.Of(installedRoom, InstalledFixtures.SamusShadeSources);
        RgbColor[] greyScene = NativeColouredPictures.ShownIn(composedStatueSheet, installedRoom.Enemies.TourianEntranceStatueBehavior.SceneShading);
        HashSet<int> roomHeld = [.. GameplayShownColours.ComparedIn(installedRoom)];
        foreach (int program in new[] { 0x8783ac, 0x878414, 0x87847c, 0x8784e4 })
        {
            AssertEqual((ushort)0x837f, ReadReferenceWord(program + 0x62),
                "native statue program grey-palette instruction");
            int destination = ReadReferenceWord(program + 0x64) / 2;
            bool room = destination < SnesCgram.ColorCount / 2;
            AssertNativeWrites(room ? greyRoom : greyScene, room ? roomHeld : sceneHeld, 0x87839c, destination,
                TourianStatueColorDefinitions.GreyColorCount, $"Tourian grey room instruction at colour {destination} shows the native grey");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Tourian statue installed paths avoid migrated ROM colors");

        JsonNode invalid = RequiredJson.Parse(stockBytes);
        invalid.Required("eye").Required(0).Required(0)["red"] = 32;
        AssertThrows<InvalidDataException>(() => TourianStatueColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Tourian statue RGB5 channel rejected");
        Console.WriteLine("  Tourian statue colors: 56 native RGB5 words, entrance/eye/grey shown colours match native writes, ROM guard and channel rejection pass.");

        void CheckSource(int address, ushort expected)
        {
            forbidden.Add(address);
            forbidden.Add(address + 1);
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), expected,
                $"Tourian statue palette source ${address:X6}");
        }

        // Native palette writes from $AA:D7C8, $86:B88E and $87:837F, read from the cartridge independently of the installed
        // catalog: the colours shown at the written places the pictures hold are the written words, and at least one written
        // place is held.
        void AssertNativeWrites(RgbColor[] shown, HashSet<int> held, int source, int destination, int count, string context)
        {
            int compared = 0;
            for (int color = 0; color < count; color++)
            {
                if (!held.Contains(destination + color))
                    continue;
                compared++;
                ushort word = ReadReferenceWord(source + color * 2);
                AssertEqual(new FadeLevelColour(word & 31, word >> 5 & 31, word >> 10 & 31).ToColour(), shown[destination + color],
                    $"{context}: colour {destination + color}");
            }
            AssertTrue(compared > 0, $"{context}: the pictures hold a written colour");
        }

        ushort ReadReferenceWord(int address) =>
            LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, address), CpuBusRegionRead.Read(bus, address + 1));

        static RoomEnemySystem CreateEnemy(ImportEnemyArtwork artwork) =>
            new(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
            {
                BossArtwork = artwork.Bosses,
            };

        static void Initialize(RoomEnemySystem enemy, ushort parameter)
        {
            RoomEnemySlot slot = enemy.SlotTable.Slots[0];
            slot.SetSpawnParameter1(EnemyDefinitionId.TourianStatue, parameter);
            slot.Bind(EnemyDefinitionId.TourianStatue, slot.Definition);
            enemy.TourianEntranceStatueBehavior.InitializeTourianEntranceStatue(slot);
        }

        static SuperMetroidRuntime CreateGreyRoom(
            ISnesAddressSpace source, ImportEnemyArtwork artwork)
        {
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.BossArtwork.KraidEyes = artwork.Bosses.KraidColors?.Eyes();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            for (int area = 1; area <= 4; area++)
                runtime.System.RecordBossesDefeated(area, DefeatedBosses.AreaBoss);
            foreach (StoryEvent greyEvent in new[] { StoryEvent.PhantoonStatueGrey, StoryEvent.RidleyStatueGrey, StoryEvent.DraygonStatueGrey, StoryEvent.KraidStatueGrey })
                runtime.System.Progress.Events.Add(greyEvent);
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Statues);
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
}
