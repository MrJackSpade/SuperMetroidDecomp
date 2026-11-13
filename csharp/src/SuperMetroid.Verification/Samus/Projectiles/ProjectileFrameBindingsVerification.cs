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
    /// <summary>Checks every timed record's native sprite reference, both projectile handlers, the installed bindings and malformed binding documents.</summary>
    internal static void VerifyProjectileFrameBindings(ISnesAddressSpace rom)
    {
        byte[] stockJson = ProjectileFrameBindingExtractor.Extract(rom);
        var stock = ProjectileFrameBindingFile.Load(new MemoryStream(stockJson, writable: false));
        AssertEqual(ProjectileFrameBindingFormat.FrameCount,
            SamusProjectileRecordDefinitions.TimedRecordAddresses.Count,
            "physical timed records and visual frame bindings have identical coverage");
        foreach (ushort pointer in SamusProjectileRecordDefinitions.TimedRecordAddresses)
        {
            int address = SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)(pointer + 2));
            ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
            AssertEqual(native, stock.Resolve(pointer),
                $"timed projectile $93:{pointer:X4} keeps its native sprite reference");
        }
        foreach (ushort pointer in SamusProjectileRecordDefinitions.TimedRecordAddresses)
        {
            int record = SamusSourceDefinitions.ProjectileDataBank.Value | pointer;
            ushort Word(int offset) => LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, record + offset), CpuBusRegionRead.Read(rom, record + offset + 1));
            var nativeShot = new SamusProjectile(0)
            {
                Playback = SamusProjectileProgramAddresses.PlaybackAt(unchecked((ushort)(pointer + 8))),
                FrameHold = WrappingCountdown.FromRaw(Word(0)), SpritemapPointer = Word(2),
                Radius = new(CpuBusRegionRead.Read(rom, record + 4), CpuBusRegionRead.Read(rom, record + 5)),
                Damage = 30,
            };
            var nativeBomb = new SamusBombProjectileSlot(0)
            {
                Playback = nativeShot.Playback,
                FrameHold = nativeShot.FrameHold,
                SpritemapPointer = nativeShot.SpritemapPointer,
                Radius = new(nativeShot.Radius.X, nativeShot.Radius.Y),
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500), Damage = 30,
            };
            var installedShot = new SamusProjectile(0)
                { Playback = SamusProjectileProgramAddresses.PlaybackAt(pointer), FrameHold = WrappingCountdown.Start(1), Damage = 30 };
            var installedBomb = new SamusBombProjectileSlot(0)
                { Playback = SamusProjectileProgramAddresses.PlaybackAt(pointer), FrameHold = WrappingCountdown.Start(1), Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500), Damage = 30 };
            var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount)) { Animation = { FrameBindings = stock } };
            var bombs = new SamusBombProjectileSystem { FrameBindings = stock };
            AssertTrue(nativeShot.FrameHold.RawValue is > 0 and < 0x8000,
                "native inventory contains timed records");
            AssertEqual(false, shots.Animation.AdvanceProjectileAnimation(installedShot),
                "installed projectile timed record survives");
            AssertEqual(false, bombs.AdvanceProjectileAnimation(installedBomb),
                "installed bomb timed record survives");
            AssertEqual(nativeShot.SpritemapPointer, installedShot.SpritemapPointer,
                "installed projectile frame selects the native sprite");
            AssertEqual(nativeBomb.SpritemapPointer, installedBomb.SpritemapPointer,
                "installed bomb frame selects the native sprite");
            AssertEqual(nativeShot.FrameHold.RawValue, installedShot.FrameHold.RawValue,
                "projectile duration is independent of sprite binding");
            AssertEqual(nativeShot.InstructionPointer, installedShot.InstructionPointer,
                "projectile flow is independent of sprite binding");
            AssertEqual(nativeShot.Radius.X, installedShot.Radius.X,
                "projectile X radius is independent of sprite binding");
            AssertEqual(nativeShot.Radius.Y, installedShot.Radius.Y,
                "projectile Y radius is independent of sprite binding");
            AssertEqual(nativeBomb.FrameHold.RawValue, installedBomb.FrameHold.RawValue,
                "bomb duration is independent of sprite binding");
            AssertEqual(nativeBomb.Radius.X, installedBomb.Radius.X,
                "bomb X radius is independent of sprite binding");
            AssertEqual(nativeBomb.Radius.Y, installedBomb.Radius.Y,
                "bomb Y radius is independent of sprite binding");
        }

        var document = RequiredJson.Deserialize<ProjectileFrameBindingDocument>(stockJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ushort firstTimedRecord = SamusProjectileRecordDefinitions.TimedRecordAddresses[0];
        string frameName = ProjectileFrameBindingFormat.FrameNameAt(firstTimedRecord);
        AssertEqual(stock.Resolve(firstTimedRecord),
            RepositoryInstallation.Installation.LoadProjectiles().FrameBindings.Resolve(firstTimedRecord),
            "installed stock frame binding selects the native sprite");
        document.Frames.Remove(frameName);
        AssertThrows<InvalidDataException>(() => ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document))),
            "missing timed frame fails before gameplay");
        document.Frames[frameName] = "sprite_0000";
        AssertThrows<InvalidDataException>(() => ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document))),
            "uncatalogued sprite selection fails before gameplay");
        Console.WriteLine("Projectile frame bindings: 805 timed records, both handlers, installed bindings and malformed documents pass.");
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
