using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCompositionsVerification;
using static SuperMetroid.Verification.Samus.Artwork.ProjectileTrailAtlasVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Retail OAM, compiled program, live override, and mutable-memory checks for projectile trails.</summary>
internal static class ProjectileTrailArtworkVerification
{
    /// <summary>Compares stock and edited trail appearance through production drawing while keeping timing, movement, and program control cartridge-owned.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled projectile trail program and frame attribute words")]
    internal static void VerifyProjectileTrailArtwork(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailAtlas), () => VerifyProjectileTrailAtlas(bus));
        byte[] json = ProjectileTrailExtractor.Extract(bus);
        // Runtime binding composes projectile pages from the catalog's characters, so the fixture carries the installed ones.
        ProjectileTrailAtlas installedTrailTiles = InstalledFixtures.Projectiles.Trails.Tiles
            ?? throw new InvalidOperationException("The installation has no trail characters.");
        var catalog = ProjectileTrailCatalog.Load(new MemoryStream(json), installedTrailTiles);
        var encountered = new HashSet<ushort>();
        foreach (ushort start in new[] { ProjectileTrailDefinitions.LeftIce, ProjectileTrailDefinitions.RightIce, ProjectileTrailDefinitions.Wave, ProjectileTrailDefinitions.Missile })
        {
            ushort cursor = start;
            while (true)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Banks.Movement | cursor);
                if (word == 0) break;
                if (word < 0x8000) { encountered.Add(cursor); cursor += 4; }
                else { cursor += 2; }
            }
            var nativeSystem = new SamusProjectileSystem(); var authoredSystem = new SamusProjectileSystem();
            foreach (var system in new[] { nativeSystem, authoredSystem })
            {
                var pair = system.TrailSlots[0];
                foreach (var side in new[] { pair.Left, pair.Right })
                { side.InstructionPointer = start; side.InstructionTimer = NativeCountdown.Start(1); side.Position = side.Position.WithX(100).WithY(100); }
            }
            for (int frame = 0; frame < 80; frame++)
            {
                var nativeOam = new OamBuffer();
                bool frozenFrame = frame % 5 == 0;
                DrawNativeTrailFrame(bus, nativeSystem.TrailSlots[0], nativeOam, frozenFrame);
                OamBuffer authoredOam = SpriteFrameBuffer.DrawnOam(sprites =>
                {
                    authoredSystem.AdvanceTrails(frozenFrame, catalog);
                    authoredSystem.DrawTrails(sprites, Position16<RoomPixels>.Zero, catalog, InstalledFixtures.ProjectileArtwork());
                });
                AssertTrue(nativeOam.LowTable.SequenceEqual(authoredOam.LowTable), "Trail catalog preserves live command/termination/freeze frame output");
                foreach (var sides in new[] { (nativeSystem.TrailSlots[0].Left, authoredSystem.TrailSlots[0].Left), (nativeSystem.TrailSlots[0].Right, authoredSystem.TrailSlots[0].Right) })
                {
                    AssertEqual(sides.Item1.InstructionPointer, sides.Item2.InstructionPointer, "Trail artwork cannot change instruction cursor");
                    AssertEqual(sides.Item1.InstructionTimer.RawValue, sides.Item2.InstructionTimer.RawValue, "Trail artwork cannot change live timing");
                    AssertEqual(sides.Item1.Position.Y, sides.Item2.Position.Y, "Trail artwork cannot change sibling-targeted movement");
                }
            }
        }
        AssertTrue(encountered.SetEquals(ProjectileTrailVisualDefinitions.Frames.ToArray()), "Independent native stream walk finds exactly the catalog's appearance records");
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
        foreach (ushort frame in ProjectileTrailVisualDefinitions.Frames)
        foreach (ushort coordinate in new ushort[] { 0, 1, 255, 256, 65535 })
        {
            ushort attributes = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Banks.Movement | (frame + 2));
            var system = new SamusProjectileSystem();
            var side = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1].Left;
            side.InstructionPointer = (ushort)(frame + 4); side.InstructionTimer = NativeCountdown.Start(3);
            side.Position = side.Position.WithX(coordinate).WithY(coordinate); side.TileNumberAttributes = attributes;
            // The OAM capacity stop is a dropped presentation quirk (#1308); culling and every byte remain.
            OamBuffer native = SpriteFrameBuffer.DrawnOam(sprites =>
            {
                system.AdvanceTrails(true, null);
                system.DrawTrails(sprites, Position16<RoomPixels>.Zero, null, InstalledFixtures.ProjectileArtwork());
            });
            OamBuffer authored = SpriteFrameBuffer.DrawnOam(sprites =>
            {
                system.AdvanceTrails(true, catalog);
                system.DrawTrails(sprites, Position16<RoomPixels>.Zero, catalog, InstalledFixtures.ProjectileArtwork());
            });
            AssertTrue(native.ImageEquals(authored), "Trail catalog preserves native culling and complete OAM bytes");
            AssertEqual(native.LastFinalizedSpriteCount, authored.LastFinalizedSpriteCount, "Trail catalog retains its sprite count");
            AssertEqual(3, side.InstructionTimer.RawValue, "Frozen draw cannot advance trail animation");
            AssertEqual(attributes, side.TileNumberAttributes, "Artwork selection does not mutate stored trail attributes");
            draws++;
        }
        var document = RequiredJson.Parse(json);
        ushort first = ProjectileTrailVisualDefinitions.Frames[0];
        var part = document.Required("frames").Required(ProjectileTrailVisualDefinitions.Name(first));
        part["flipX"] = !part.Required("flipX").GetValue<bool>();
        var edited = Load(document);
        Suite(nameof(VerifyRuntimeTrailBinding), () => VerifyRuntimeTrailBinding(bus, catalog, edited));
        AssertEqual((ushort)(catalog.Resolve(first) ^ 0x4000), edited.Resolve(first), "Trail flip edit changes only horizontal-flip bit");
        var animation = new SamusProjectileSystem();
        var trail = animation.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1].Left;
        trail.InstructionPointer = first; trail.InstructionTimer = NativeCountdown.Start(1); trail.Position = trail.Position.WithX(100).WithY(100);
        OamBuffer frozen = SpriteFrameBuffer.DrawnOam(sprites =>
        {
            animation.AdvanceTrails(true, edited);
            animation.DrawTrails(sprites, Position16<RoomPixels>.Zero, edited, InstalledFixtures.ProjectileArtwork());
        });
        AssertEqual(0, frozen.GetEntry(0).TileNumber, "New frozen trail retains native uninitialized tile rather than starting artwork early");
        animation.AdvanceTrails(false, catalog);
        OamBuffer original = SpriteFrameBuffer.DrawnOam(sprites =>
        {
            animation.AdvanceTrails(true, catalog);
            animation.DrawTrails(sprites, Position16<RoomPixels>.Zero, catalog, InstalledFixtures.ProjectileArtwork());
        });
        OamBuffer changed = SpriteFrameBuffer.DrawnOam(sprites =>
        {
            animation.AdvanceTrails(true, edited);
            animation.DrawTrails(sprites, Position16<RoomPixels>.Zero, edited, InstalledFixtures.ProjectileArtwork());
        });
        AssertTrue(original.GetEntry(0).FlipX != changed.GetEntry(0).FlipX, "Edited trail flip reaches production draw");
        AssertEqual(1, trail.InstructionTimer.RawValue, "Rebound art retains current timer");
        document.Required("frames").Required(ProjectileTrailVisualDefinitions.Name(first))["duration"] = 0;
        AssertThrows<InvalidDataException>(() => Load(document), "Trail art cannot change duration");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("frames").AsObject().Remove(ProjectileTrailVisualDefinitions.Name(first));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing trail art rejected");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("frames").Required(ProjectileTrailVisualDefinitions.Name(first))["palette"] = 8;
        AssertThrows<InvalidDataException>(() => Load(document), "Invalid trail palette rejected");
        Console.WriteLine($"Trail artwork: {draws} native OAM comparisons, frozen-start preservation, live edit isolation and strict metadata rejection pass.");
        // Round-trip an edited frame node through the same JSON catalog validation as installed assets.
        ProjectileTrailCatalog Load(JsonNode document) => ProjectileTrailCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())), installedTrailTiles);
    }

    /// <summary>Interprets the native trail words and sibling-targeted movement as an independent OAM and timer reference.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for trail frame")]
    private static void DrawNativeTrailFrame(ISnesAddressSpace rom,
        SamusProjectileTrailSlot pair, OamBuffer oam, bool frozen)
    {
        foreach (SamusProjectileTrailSide side in new[] { pair.Left, pair.Right })
        {
            if (side.InstructionTimer.IsZero) continue;
            if (!frozen && (side.InstructionTimer = side.InstructionTimer.Decrement().Remaining).IsZero)
            {
                ushort cursor = side.InstructionPointer;
                for (int operations = 0; ; operations++)
                {
                    AssertTrue(operations < 16, "native trail reaches a timed frame or terminator");
                    ushort command = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x900000 | cursor);
                    if (command < 0x8000)
                    {
                        side.InstructionTimer = NativeCountdown.FromRaw(command);
                        if (command != 0)
                        {
                            side.TileNumberAttributes = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x900000 | (unchecked((ushort)(cursor + 2))));
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
                oam.AddProjectileTrailSprite(new Position16<OamPixels>((byte)side.Position.X, (byte)side.Position.Y),
                    side.TileNumberAttributes);
        }
    }
    /// <summary>Checks runtime art rebinding changes OAM but leaves saved trail timing and serialized game state intact.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyRuntimeTrailBinding(ISnesAddressSpace bus, ProjectileTrailCatalog stock, ProjectileTrailCatalog edited)
    {
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Freeze.GameplayTimeFrozen = true;
        var side = runtime.Projectiles.TrailSlots[0].Left;
        side.InstructionPointer = (ushort)(ProjectileTrailVisualDefinitions.Frames[0] + 4);
        side.InstructionTimer = NativeCountdown.Start(3); side.TileNumberAttributes = stock.Resolve(ProjectileTrailVisualDefinitions.Frames[0]);
        side.Position = side.Position.WithX((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100)).WithY((ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100));
        var game = CreateRetailGameFixture(bus, new SuperMetroid.Core.Saves.SaveSlotStore());
        game.Session.runtime = runtime;
        // The fixture game has no host artwork of its own; adopting the runtime cleared its projectile sheets.
        InstalledFixtures.BindProjectileArtwork(runtime);
        // Run the gameplay actor draw pass into a fresh OAM frame and capture its low table.
        byte[] Draw(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(false, false);
            target.Sprites.FinalizeFrame();
            return target.Sprites.FinalizedAsOam().LowTable.ToArray();
        }
        byte[] native = Draw(runtime);
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = stock,
        });
        AssertTrue(native.SequenceEqual(Draw(runtime)), "Runtime trail stock binding preserves native OAM");
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = edited,
        });
        AssertTrue(!native.SequenceEqual(Draw(runtime)), "Runtime actor pass consumes edited trail appearance");
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = null,
        });
        using var without = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindArtwork(game.Artwork with
        {
            TrailArtwork = stock,
        });
        using var with = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.WrittenBytes().SequenceEqual(with.WrittenBytes()), "Trail catalogs are excluded from both frontend and runtime snapshots");
        with.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(with);
        var restoredRuntime = restored.Session.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.TrailArtwork is null, "Restored trail catalog requires host rebind");
        BindRetailRuntimeFixture(restoredRuntime);
        restored.BindArtwork(restored.Artwork with
        {
            TrailArtwork = edited,
        }); game.BindArtwork(game.Artwork with
{
    TrailArtwork = edited,
});
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Old state draws current selected trail appearance after rebind");
        AssertEqual(3, restoredRuntime.Projectiles.TrailSlots[0].Left.InstructionTimer.RawValue, "Trail rebind preserves saved timing");
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
