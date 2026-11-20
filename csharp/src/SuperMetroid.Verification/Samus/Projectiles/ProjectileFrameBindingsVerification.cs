using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed projectile sprite bindings independently of timed mechanics.</summary>
internal static class ProjectileFrameBindingsVerification
{
    /// <summary>Checks every projectile frame's native sprite reference, both projectile handlers, the installed bindings and malformed binding documents.</summary>
    internal static void VerifyProjectileFrameBindings(ISnesAddressSpace rom)
    {
        byte[] stockJson = ProjectileFrameBindingExtractor.Extract(rom);
        var stock = ProjectileFrameBindingFile.Load(new MemoryStream(stockJson, writable: false));
        AssertEqual(ProjectileFrameBindingFormat.FrameCount,
            SamusProjectileFrameDefinitions.TimedRecordAddresses.Select(record => ProjectileSpriteNative.Frame(record)).Distinct().Count(),
            "the frames the timed records show and their visual bindings have identical coverage");
        foreach (ushort pointer in SamusProjectileFrameDefinitions.TimedRecordAddresses)
        {
            int address = SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)(pointer + 2));
            ushort native = RomDataReader.ReadWordFixedBank(rom, address);
            AssertEqual(ProjectileSpriteNative.FirstStored(native), stock.Resolve(pointer),
                $"timed projectile $93:{pointer:X4} keeps the composition its native sprite reference stores");
        }
        foreach (ushort pointer in SamusProjectileFrameDefinitions.TimedRecordAddresses)
        {
            int record = SamusSourceDefinitions.ProjectileDataBank.Value | pointer;
            ushort Word(int offset) => LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, record + offset), CpuBusRegionRead.Read(rom, record + offset + 1));
            var nativeShot = new SamusProjectile(0)
            {
                Animation = SamusProjectileProgramAddresses.PlayerAt(unchecked((ushort)(pointer + 8)), updates: Word(0)),
                SpritemapPointer = Word(2),
                Radius = new(CpuBusRegionRead.Read(rom, record + 4), CpuBusRegionRead.Read(rom, record + 5)),
                Damage = 30,
            };
            var nativeBomb = new SamusBomb(0)
            {
                Animation = SamusProjectileProgramAddresses.PlayerAt(unchecked((ushort)(pointer + 8)), updates: Word(0)),
                SpritemapPointer = nativeShot.SpritemapPointer,
                Radius = new(nativeShot.Radius.X, nativeShot.Radius.Y),
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500), Damage = 30,
            };
            var installedShot = new SamusProjectile(0)
                { Animation = SamusProjectileProgramAddresses.PlayerAt(pointer), Damage = 30 };
            var installedBomb = new SamusBomb(0)
                { Animation = SamusProjectileProgramAddresses.PlayerAt(pointer), Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500), Damage = 30 };
            var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: stock), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: stock));
            // A list repeating one still picture is held, and no bomb shows one (#2347).
            bool still = SamusProjectileProgramAddresses.ShowsStillPicture(pointer);
            AssertTrue(Word(0) is > 0 and < 0x8000,
                "native inventory contains timed records");
            AssertEqual(false, shots.Animation.AdvanceProjectileAnimation(installedShot),
                "installed projectile frame survives");
            AssertEqual(nativeShot.SpritemapPointer, installedShot.SpritemapPointer,
                "installed projectile frame selects the native sprite");
            AssertEqual<int?>(still ? null : nativeShot.NativeFrameHold, installedShot.NativeFrameHold,
                "projectile duration is independent of sprite binding, a still picture held");
            AssertEqual(nativeShot.InstructionPointer, installedShot.InstructionPointer,
                "projectile flow is independent of sprite binding");
            AssertEqual(nativeShot.Radius.X, installedShot.Radius.X,
                "projectile X radius is independent of sprite binding");
            AssertEqual(nativeShot.Radius.Y, installedShot.Radius.Y,
                "projectile Y radius is independent of sprite binding");
            if (still)
                continue;
            AssertEqual(false, bombs.AdvanceProjectileAnimation(installedBomb),
                "installed bomb frame survives");
            AssertEqual(nativeBomb.SpritemapPointer, installedBomb.SpritemapPointer,
                "installed bomb frame selects the native sprite");
            AssertEqual(nativeBomb.NativeFrameHold, installedBomb.NativeFrameHold,
                "bomb duration is independent of sprite binding");
            AssertEqual(nativeBomb.Radius.X, installedBomb.Radius.X,
                "bomb X radius is independent of sprite binding");
            AssertEqual(nativeBomb.Radius.Y, installedBomb.Radius.Y,
                "bomb Y radius is independent of sprite binding");
        }

        var document = RequiredJson.Deserialize<ProjectileFrameBindingDocument>(stockJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ushort firstTimedRecord = SamusProjectileFrameDefinitions.TimedRecordAddresses[0];
        string frameName = ProjectileFrameBindingFormat.FrameNameAt(firstTimedRecord);
        AssertEqual(stock.Resolve(firstTimedRecord),
            RepositoryInstallation.Installation.LoadProjectiles().FrameBindings.Resolve(firstTimedRecord),
            "installed stock frame binding selects the native sprite");
        // #1762, #2295: every projectile composition, and every flare and hit-burst composition, is named for the pose and
        // stage it shows, never by its place in its family or animation.
        string[] numbered = [.. ProjectileSpriteDefinitions.All.Select(sprite => sprite.Name)
            .Concat(ChargeFlareSpriteDefinitions.All.Select(sprite => sprite.Name))
            .Concat(HitBurstSpriteDefinitions.All.Select(sprite => sprite.Name))
            .Where(name => name.Any(char.IsAsciiDigit))];
        AssertTrue(numbered.Length == 0, $"compositions are named for their poses and stages, not numbered: {string.Join(", ", numbered)}");
        AssertEqual("Power.UpRight", document.Frames[nameof(SamusProjectileFrameId.PowerUpRight)], "the up-right Power beam binds the up-right Power pose");
        AssertEqual("Wave.Right.UpNear", document.Frames[nameof(SamusProjectileFrameId.WaveHorizontalUpNear)], "the right-travelling Wave beam's first swing binds the composition of that stage");
        // #2295: the down-left Spazer group stores no splitting lanes; its frames bind the compositions of the stages they show.
        AssertEqual("Spazer.DownLeft.Parting", document.Frames[nameof(SamusProjectileFrameId.SpazerDownLeftParting)], "the down-left Spazer's parting lanes bind its parting composition");
        AssertEqual("Spazer.DownLeft.Spreading", document.Frames[nameof(SamusProjectileFrameId.SpazerWaveDownLeftSpreading)], "the down-left Spazer's spreading lanes bind its spreading composition");
        AssertEqual("Spazer.DownLeft.NearlySpread", document.Frames[nameof(SamusProjectileFrameId.SpazerWaveDownLeftNearlySpread)], "the down-left Spazer's nearly spread lanes bind its nearly spread composition");
        AssertEqual("Spazer.DownLeft.Spread", document.Frames[nameof(SamusProjectileFrameId.SpazerDownLeftSpread)], "the down-left Spazer's spread lanes bind its spread composition");
        // #2295: a record storing a composition again, byte for byte, binds that one composition: charged Ice's cross and star
        // are charged Wave's alternate core and core, and the Wave beam's centre is the leftward Power beam's character.
        AssertEqual("ChargedWave.CoreStarburst", document.Frames[nameof(SamusProjectileFrameId.ChargedIceCrossed)], "charged Ice's cross binds the composition it stores");
        AssertEqual("ChargedWave.Core", document.Frames[nameof(SamusProjectileFrameId.ChargedIceStar)], "charged Ice's star binds the composition it stores");
        AssertEqual("Power.Left", document.Frames[nameof(SamusProjectileFrameId.WaveHorizontalCentre)], "the Wave beam's centre binds the composition it stores");
        string[] unbound = [.. ProjectileSpriteDefinitions.All.Select(sprite => sprite.Name).Except(document.Frames.Values)];
        AssertTrue(unbound.Length == 0, $"every installed composition is one a frame binds: {string.Join(", ", unbound)}");
        document.Frames.Remove(frameName);
        AssertThrows<InvalidDataException>(() => ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document))),
            "missing timed frame fails before gameplay");
        document.Frames[frameName] = "Power.3";
        AssertThrows<InvalidDataException>(() => ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document))),
            "a composition named by its place in its family fails before gameplay");
        document.Frames[frameName] = "sprite_0000";
        AssertThrows<InvalidDataException>(() => ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document))),
            "uncatalogued sprite selection fails before gameplay");
        Console.WriteLine($"Projectile frame bindings: {ProjectileFrameBindingFormat.FrameCount} frames over {SamusProjectileFrameDefinitions.TimedRecordAddresses.Count} records, both handlers, installed bindings and malformed documents pass.");
    }

    /// <summary>Runs the projectile frame binding checks.</summary>
    [VerificationCommand("--projectile-frame-bindings")]
    internal static int ProjectileFrameBindingsCommand()
    {
        Suite(nameof(VerifyProjectileFrameBindings), () => VerifyProjectileFrameBindings(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
