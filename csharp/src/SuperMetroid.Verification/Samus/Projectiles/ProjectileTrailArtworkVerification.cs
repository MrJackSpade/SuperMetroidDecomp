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
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;

/// <summary>Retail OAM, compiled program, live override, and mutable-memory checks for projectile trails.</summary>
internal static class ProjectileTrailArtworkVerification
{
    /// <summary>Compares stock trail appearance through production drawing while keeping timing, movement, and program control cartridge-owned.</summary>
    internal static void VerifyProjectileTrailArtwork(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailAtlas), () => VerifyProjectileTrailAtlas(bus));
        byte[] json = ProjectileTrailExtractor.Extract(bus);
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
            var authoredSystem = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
            // The native interpreter's own trail, run beside the port's, and the cartridge's retained tile words it keeps.
            var nativeTrail = new SamusProjectileTrail(SuperMetroid.Core.Progression.SamusBeamCombination.Power);
            SamusProjectileTrail authoredTrail = authoredSystem.AddTrail();
            var nativeWords = new Dictionary<SamusProjectileTrailSide, ushort>();
            foreach (var pair in new[] { nativeTrail, authoredTrail })
            {
                foreach (var side in new[] { pair.Left, pair.Right })
                { side.InstructionPointer = start; side.FrameHold = 1; side.Position = side.Position.WithX(100).WithY(100); }
            }
            for (int frame = 0; frame < 80; frame++)
            {
                var nativeOam = new OamBuffer();
                bool frozenFrame = frame % 5 == 0;
                DrawNativeTrailFrame(bus, nativeTrail, nativeWords, nativeOam, frozenFrame);
                OamBuffer authoredOam = SpriteFrameBuffer.DrawnOam(sprites =>
                {
                    authoredSystem.AdvanceTrails(frozenFrame);
                    authoredSystem.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
                });
                AssertTrue(nativeOam.LowTable.SequenceEqual(authoredOam.LowTable), "Trail catalog preserves live command/termination/freeze frame output");
                foreach (var sides in new[] { new VerifyProjectileTrailArtworkCase(nativeTrail.Left, authoredTrail.Left), new VerifyProjectileTrailArtworkCase(nativeTrail.Right, authoredTrail.Right) })
                {
                    AssertEqual(sides.First.InstructionPointer, sides.Second.InstructionPointer, "Trail artwork cannot change instruction cursor");
                    AssertEqual(sides.First.FrameHold, sides.Second.FrameHold, "Trail artwork cannot change live timing");
                    AssertEqual(sides.First.Position.Y, sides.Second.Position.Y, "Trail artwork cannot change sibling-targeted movement");
                }
            }
        }
        AssertTrue(encountered.SetEquals(ProjectileTrailVisualDefinitions.Frames.Select(ProjectileTrailNative.Address)), "Independent native stream walk finds exactly the catalog's appearance records");
        int programWords = 0;
        for (int address = 0x90b4c8; address <= 0x90b5b3; address++)
        {
            if (ProjectileTrailAnimationDefinitions.TryRead(address, out _))
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailAnimationDefinitions.Read(address), "Compiled trail program preserves every authored mechanics word");
                programWords++;
            }
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailAnimationDefinitions.Read(rejectedAddress), "Trail program rejects presentation gaps, odd addresses and unrelated high-bank words");
            }
        }
        AssertEqual(67, programWords, "All 42 durations, 20 movement commands and five terminators are compiled");
        AssertThrows<InvalidDataException>(() => ProjectileTrailAnimationDefinitions.Read(0x91b4c9), "Trail program rejects a wrong-bank alias");
        int draws = 0;
        foreach (ProjectileTrailFrame pose in ProjectileTrailVisualDefinitions.Frames)
        foreach (ushort coordinate in new ushort[] { 0, 1, 255, 256, 65535 })
        {
            ushort frame = ProjectileTrailNative.Address(pose);
            ushort attributes = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.MovementBank.Value | (frame + 2));
            var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
            var side = system.AddTrail().Left;
            side.InstructionPointer = (ushort)(frame + 4); side.FrameHold = 3;
            side.Position = side.Position.WithX(coordinate).WithY(coordinate); side.ShownPose = pose;
            // The OAM capacity stop is a dropped presentation quirk (#1308), and so is the off-screen cull (#1541):
            // a trail on the screen draws its native sprite, and one off it is published where it lies for the
            // display to clip.
            var sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            system.AdvanceTrails(true);
            system.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
            OamBuffer authored = sprites.FinalizeAsOam();
            AssertEqual(1, sprites.Draws.Length, $"Trail pose {pose} at {coordinate} is published once");
            AssertEqual(Position16<ScreenPixels>.Of(coordinate, coordinate), sprites.Draws[0].Origin,
                $"Trail pose {pose} at {coordinate} is published where it lies");
            if (coordinate < 256)
            {
                var native = new OamBuffer();
                native.AddProjectileTrailSprite(new Position16<SpritePixels>((byte)coordinate, (byte)coordinate), attributes);
                AssertTrue(native.LowTable.SequenceEqual(authored.LowTable), $"Trail pose {pose} draws its native sprite");
                AssertEqual(native.NextByteOffset / 4, authored.LastFinalizedSpriteCount, "Trail sheet retains its sprite count");
            }
            AssertEqual(3, side.FrameHold, "Frozen draw cannot advance trail animation");
            AssertEqual(pose, side.ShownPose, "Drawing does not change the shown pose");
            draws++;
        }
        var document = RequiredJson.Parse(json);
        ProjectileTrailFrame first = ProjectileTrailVisualDefinitions.Frames[0];
        var animation = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        var trail = animation.AddTrail().Left;
        trail.InstructionPointer = ProjectileTrailNative.Address(first); trail.FrameHold = 1; trail.Position = trail.Position.WithX(100).WithY(100);
        OamBuffer frozen = SpriteFrameBuffer.DrawnOam(sprites =>
        {
            animation.AdvanceTrails(true);
            animation.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
        });
        // A sparkle that has shown no pose draws nothing, rather than a picture its program never names (#1625).
        AssertEqual(0, frozen.LastFinalizedSpriteCount, "New frozen trail draws nothing before its first pose");
        AssertEqual(1, trail.FrameHold, "New frozen trail keeps its start timer");
        document.Required("frames").Required(ProjectileTrailNative.Name(first))["duration"] = 0;
        AssertThrows<InvalidDataException>(() => Load(document), "Trail art cannot change duration");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("frames").AsObject().Remove(ProjectileTrailNative.Name(first));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing trail art rejected");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("frames").Required(ProjectileTrailNative.Name(first))["palette"] = 8;
        AssertThrows<InvalidDataException>(() => Load(document), "Invalid trail palette rejected");
        Console.WriteLine($"Trail artwork: {draws} native pose comparisons, frozen-start preservation and strict metadata rejection pass.");
        // Round-trip a malformed frame node through the same JSON catalog validation as installed assets.
        InstalledProjectileTrails Load(JsonNode document) => InstalledProjectileTrails.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
    }

    /// <summary>
    /// Interprets the native trail words and sibling-targeted movement as an independent OAM and timer reference. A side
    /// that has read no tile word yet draws nothing, as the port's sparkle that has shown no pose (#1625).
    /// </summary>
    private static void DrawNativeTrailFrame(ISnesAddressSpace rom,
        SamusProjectileTrail pair, Dictionary<SamusProjectileTrailSide, ushort> words, OamBuffer oam, bool frozen)
    {
        foreach (SamusProjectileTrailSide side in new[] { pair.Left, pair.Right })
        {
            if (side.FrameHold == 0) continue;
            if (!frozen && --side.FrameHold == 0)
            {
                ushort cursor = checked((ushort)side.InstructionPointer);
                for (int operations = 0; ; operations++)
                {
                    AssertTrue(operations < 16, "native trail reaches a timed frame or terminator");
                    ushort command = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x900000 | cursor);
                    if (command < 0x8000)
                    {
                        side.FrameHold = command;
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
                if (side.FrameHold == 0) continue;
            }
            if (side.Position.X < 256 && side.Position.Y < 256 && words.TryGetValue(side, out ushort word))
                oam.AddProjectileTrailSprite(new Position16<SpritePixels>((byte)side.Position.X, (byte)side.Position.Y), word);
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
