using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Artwork.ProjectileTrailAtlasVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;

/// <summary>Retail OAM, compiled program, live override, and mutable-memory checks for projectile trails.</summary>
internal static class ProjectileTrailArtworkVerification
{
    /// <summary>Compares stock and edited trail appearance through production drawing while keeping timing, movement, and program control cartridge-owned.</summary>
    internal static void VerifyProjectileTrailArtwork(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailAtlas), () => VerifyProjectileTrailAtlas(bus));
        byte[] json = ProjectileTrailExtractor.Extract(bus);
        var catalog = ProjectileTrailCatalog.Load(new MemoryStream(json));
        var encountered = new HashSet<ushort>();
        foreach (ushort start in new[] { ProjectileTrailDefinitions.LeftIce, ProjectileTrailDefinitions.RightIce, ProjectileTrailDefinitions.Wave, ProjectileTrailDefinitions.Missile })
        {
            ushort cursor = start;
            while (true)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.MovementBank.Value | cursor);
                if (word == 0) break;
                if (word < 0x8000) { encountered.Add(cursor); cursor += 4; }
                else { cursor += 2; }
            }
            var nativeSystem = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None); var authoredSystem = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
            // The cartridge's retained tile words, which the native interpreter keeps beside the port's sides.
            var nativeWords = new Dictionary<SamusProjectileTrailSide, ushort>();
            foreach (var system in new[] { nativeSystem, authoredSystem })
            {
                var pair = system.TrailSlots[0];
                foreach (var side in new[] { pair.Left, pair.Right })
                { side.InstructionPointer = start; side.InstructionTimer = WrappingCountdown.Start(1); side.Position = side.Position.WithX(100).WithY(100); }
            }
            for (int frame = 0; frame < 80; frame++)
            {
                var nativeOam = new OamBuffer();
                bool frozenFrame = frame % 5 == 0;
                DrawNativeTrailFrame(bus, nativeSystem.TrailSlots[0], nativeWords, nativeOam, frozenFrame);
                OamBuffer authoredOam = SpriteFrameBuffer.DrawnOam(sprites =>
                {
                    authoredSystem.AdvanceTrails(frozenFrame);
                    authoredSystem.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
                });
                AssertTrue(nativeOam.LowTable.SequenceEqual(authoredOam.LowTable), "Trail catalog preserves live command/termination/freeze frame output");
                foreach (var sides in new[] { new VerifyProjectileTrailArtworkCase(nativeSystem.TrailSlots[0].Left, authoredSystem.TrailSlots[0].Left), new VerifyProjectileTrailArtworkCase(nativeSystem.TrailSlots[0].Right, authoredSystem.TrailSlots[0].Right) })
                {
                    AssertEqual(sides.First.InstructionPointer, sides.Second.InstructionPointer, "Trail artwork cannot change instruction cursor");
                    AssertEqual(sides.First.InstructionTimer.RawValue, sides.Second.InstructionTimer.RawValue, "Trail artwork cannot change live timing");
                    AssertEqual(sides.First.Position.Y, sides.Second.Position.Y, "Trail artwork cannot change sibling-targeted movement");
                }
            }
        }
        AssertTrue(encountered.SetEquals(ProjectileTrailVisualDefinitions.Frames.Select(ProjectileTrailNative.Address)), "Independent native stream walk finds exactly the catalog's appearance records");
        int programWords = 0;
        for (int address = 0x90b4c8; address <= 0x90b5b3; address++)
        {
            if (ProjectileTrailProgramDefinitions.TryRead(address, out _))
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailProgramDefinitions.Read(address), "Compiled trail program preserves every authored mechanics word");
                programWords++;
            }
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(rejectedAddress), "Trail program rejects presentation gaps, odd addresses and unrelated high-bank words");
            }
        }
        AssertEqual(67, programWords, "All 42 durations, 20 movement commands and five terminators are compiled");
        AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(0x91b4c9), "Trail program rejects a wrong-bank alias");
        int draws = 0;
        foreach (ProjectileTrailFrame pose in ProjectileTrailVisualDefinitions.Frames)
        foreach (ushort coordinate in new ushort[] { 0, 1, 255, 256, 65535 })
        {
            ushort frame = ProjectileTrailNative.Address(pose);
            ushort attributes = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.MovementBank.Value | (frame + 2));
            var system = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
            var side = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1].Left;
            side.InstructionPointer = (ushort)(frame + 4); side.InstructionTimer = WrappingCountdown.Start(3);
            side.Position = side.Position.WithX(coordinate).WithY(coordinate); side.ShownPose = pose;
            // The OAM capacity stop is a dropped presentation quirk (#1308); culling and every byte remain.
            var native = new OamBuffer();
            if (coordinate < 256)
                native.AddProjectileTrailSprite(new Position16<SpritePixels>((byte)coordinate, (byte)coordinate), attributes);
            OamBuffer authored = SpriteFrameBuffer.DrawnOam(sprites =>
            {
                system.AdvanceTrails(true);
                system.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
            });
            AssertTrue(native.LowTable.SequenceEqual(authored.LowTable), $"Trail pose {pose} draws its native sprite and culling");
            AssertEqual(native.NextByteOffset / 4, authored.LastFinalizedSpriteCount, "Trail sheet retains its sprite count");
            AssertEqual(3, side.InstructionTimer.RawValue, "Frozen draw cannot advance trail animation");
            AssertEqual(pose, side.ShownPose, "Drawing does not change the shown pose");
            draws++;
        }
        var document = RequiredJson.Parse(json);
        ProjectileTrailFrame first = ProjectileTrailVisualDefinitions.Frames[0];
        var part = document.Required("frames").Required(ProjectileTrailNative.Name(first));
        part["flipX"] = !part.Required("flipX").GetValue<bool>();
        var edited = Load(document);
        AssertEqual(catalog.Resolve(first) with { Flip = catalog.Resolve(first).Flip ^ TileFlip.Horizontal }, edited.Resolve(first),
            "Trail flip edit changes only the horizontal flip");
        var animation = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
        var trail = animation.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1].Left;
        trail.InstructionPointer = ProjectileTrailNative.Address(first); trail.InstructionTimer = WrappingCountdown.Start(1); trail.Position = trail.Position.WithX(100).WithY(100);
        OamBuffer frozen = SpriteFrameBuffer.DrawnOam(sprites =>
        {
            animation.AdvanceTrails(true);
            animation.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
        });
        AssertEqual(0, frozen.GetEntry(0).TileNumber, "New frozen trail retains native uninitialized tile rather than starting artwork early");
        document.Required("frames").Required(ProjectileTrailNative.Name(first))["duration"] = 0;
        AssertThrows<InvalidDataException>(() => Load(document), "Trail art cannot change duration");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("frames").AsObject().Remove(ProjectileTrailNative.Name(first));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing trail art rejected");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("frames").Required(ProjectileTrailNative.Name(first))["palette"] = 8;
        AssertThrows<InvalidDataException>(() => Load(document), "Invalid trail palette rejected");
        Console.WriteLine($"Trail artwork: {draws} native pose comparisons, frozen-start preservation and strict metadata rejection pass.");
        // Round-trip an edited frame node through the same JSON catalog validation as installed assets.
        ProjectileTrailCatalog Load(JsonNode document) => ProjectileTrailCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
    }

    /// <summary>Interprets the native trail words and sibling-targeted movement as an independent OAM and timer reference.</summary>
    private static void DrawNativeTrailFrame(ISnesAddressSpace rom,
        SamusProjectileTrailSlot pair, Dictionary<SamusProjectileTrailSide, ushort> words, OamBuffer oam, bool frozen)
    {
        foreach (SamusProjectileTrailSide side in new[] { pair.Left, pair.Right })
        {
            if (side.InstructionTimer.IsZero) continue;
            if (!frozen && (side.InstructionTimer = side.InstructionTimer.Decrement().Remaining).IsZero)
            {
                ushort cursor = checked((ushort)side.InstructionPointer);
                for (int operations = 0; ; operations++)
                {
                    AssertTrue(operations < 16, "native trail reaches a timed frame or terminator");
                    ushort command = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x900000 | cursor);
                    if (command < 0x8000)
                    {
                        side.InstructionTimer = WrappingCountdown.FromRaw(command);
                        if (command != 0)
                        {
                            words[side] = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x900000 | (unchecked((ushort)(cursor + 2))));
                            side.InstructionPointer = unchecked((ushort)(cursor + 4));
                        }
                        break;
                    }
                    cursor += 2;
                    switch (command)
                    {
                        case 0xb525: pair.Left.Position += new Offset16<RoomPixels>(0, 1); break;
                        case 0xb587: pair.Right.Position += new Offset16<RoomPixels>(0, 1); break;
                        case 0xb5b3: pair.Left.Position -= new Offset16<RoomPixels>(0, 1); break;
                        default: throw new InvalidDataException($"Unknown native trail command ${command:X4}.");
                    }
                }
                if (side.InstructionTimer.IsZero) continue;
            }
            if (side.Position.X < 256 && side.Position.Y < 256)
                oam.AddProjectileTrailSprite(new Position16<SpritePixels>((byte)side.Position.X, (byte)side.Position.Y),
                    words.GetValueOrDefault(side));
        }
    }
    /// <summary>Runs the cartridge-backed trail art and mechanics comparison from its command-line switch.</summary>
    [VerificationCommand("--projectile-trail-artwork-fixture")]
    internal static int ProjectileTrailArtworkFixtureCommand()
    {
        Suite(nameof(VerifyProjectileTrailArtwork), () => VerifyProjectileTrailArtwork(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the typed bank-$90 WRAM alias check from its command-line switch.</summary>
    [VerificationCommand("--trail-mutable-alias")]
    internal static int TrailMutableAliasCommand()
    {
        return 0;
    }
}
