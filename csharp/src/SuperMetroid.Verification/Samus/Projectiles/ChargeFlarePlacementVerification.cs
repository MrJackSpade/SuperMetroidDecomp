using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareCompositionsVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks editable charge-flare offsets against native OAM placement and the production rendering paths.</summary>
internal static class ChargeFlarePlacementVerification
{
    /// <summary>Compares stock flare placement across pose, direction, coordinate, and Mode 7 cases, then checks edited offsets and state isolation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for charge flare placement")]
    internal static void VerifyChargeFlarePlacement(SuperMetroidAddressSpace bus)
    {
        Suite(nameof(VerifyChargeFlareCompositions), () => VerifyChargeFlareCompositions(bus));
        byte[] json = ChargeFlarePlacementExtractor.Extract(bus);
        var stock = ChargeFlarePlacementCatalog.Load(new MemoryStream(json));
        var document = RequiredJson.Parse(json);
        foreach (var entry in document.Required("offsets").AsObject())
            (entry.Value ?? throw new InvalidOperationException("Verification requires entry.Value."))["x"] = (entry.Value ?? throw new InvalidOperationException("Verification requires entry.Value.")).Required("x").GetValue<int>() + 7;
        var edited = Load(document);
        Suite(nameof(VerifyRuntimeFlarePlacement), () => VerifyRuntimeFlarePlacement(bus, stock, edited));
        var system = new SamusProjectileSystem();
        var sprites = ChargeFlareSpriteCatalog.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(bus)));
        var drawMetadata = new FlareMetadataDraw(system.Animation.DrawFlareComponentWithMetadata);
        var draw = new Action<SpriteFrameBuffer, SamusState, Position16<RoomPixels>, int, SamusMode7Transform?, ChargeFlareArtwork>(system.Animation.DrawFlareComponent);
        ProjectileDrawArtwork objects = InstalledFixtures.ProjectileArtwork();
        int cases = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        for (byte direction = 0; direction < 16; direction++)
        foreach (ushort coordinate in new ushort[] { 0, 100, 255, ushort.MaxValue })
        foreach (SamusMode7Transform? transform in new SamusMode7Transform?[] { null, new(240, 16, 65520, new Position16<RoomPixels>(128, 112)) })
        for (int component = 0; component < 3; component++)
        {
            // Exercise all original low-nibble operands at the renderer's metadata
            // boundary; compiled pose rules cannot be overridden by fake ROM bytes.
            var samus = new SamusState { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(coordinate, coordinate)) };
            int record = SamusMovementRomData.Poses.Definitions + (int)pose * 8;
            bool running = bus.ReadCartridgeByte(record + 1) == (byte)SamusMovementType.Running;
            bool facingLeft = bus.ReadCartridgeByte(record) == 4;
            byte yOffset = bus.ReadCartridgeByte(record + 4);
            var native = new OamBuffer();
            int xTable = running ? SamusProjectileRomData.Origins.FlareRunningX : SamusProjectileRomData.Origins.FlareDefaultX;
            int yTable = running ? SamusProjectileRomData.Origins.FlareRunningY : SamusProjectileRomData.Origins.FlareDefaultY;
            short Word(int address) => unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWord(address));
            var world = new Position16<RoomPixels>(coordinate, coordinate);
            var point = transform is { } selectedTransform ? selectedTransform.Transform(world) : world;
            ushort x = unchecked((ushort)(point.X + Word(xTable + direction * 2)));
            ushort y = unchecked((ushort)(point.Y + Word(yTable + direction * 2) - yOffset));
            ushort selector = (ushort)(component == 0 ? 0 : facingLeft ? (component == 1 ? 42 : 48) : (component == 1 ? 30 : 36));
            if ((SnesLittleEndian.HighByteInPlace(y)) == 0) DrawImportedFlareSpritemap(bus, native, selector, new(x, y));
            OamBuffer actual = SpriteFrameBuffer.DrawnOam(frame => drawMetadata(frame, samus, Position16<RoomPixels>.Zero, component, transform,
                new ChargeFlareArtwork { Placement = stock, Compositions = sprites, Objects = objects },
                new ChargeFlarePose { Direction = direction, Running = running, PoseYOffset = yOffset, FacingLeft = facingLeft }));
            AssertTrue(native.ImageEquals(actual), "Flare placement stock preserves native OAM/culling for running, facing, overread directions and Mode7");
            AssertEqual(native.NextByteOffset / 4, actual.LastFinalizedSpriteCount, "Flare placement preserves OAM admission");
            cases++;
        }
        var subject = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
        PrepareRetailSamusFixture(subject);
        byte[] beforeSamus = GraphDigest(subject), beforeSystem = GraphDigest(system);
        OamBuffer original = SpriteFrameBuffer.DrawnOam(frame => draw(frame, subject, Position16<RoomPixels>.Zero, 0, null,
            new ChargeFlareArtwork { Placement = stock, Compositions = sprites, Objects = objects }));
        OamBuffer changed = SpriteFrameBuffer.DrawnOam(frame => draw(frame, subject, Position16<RoomPixels>.Zero, 0, null,
            new ChargeFlareArtwork { Placement = edited, Compositions = sprites, Objects = objects }));
        AssertTrue(original.LastFinalizedSpriteCount > 0, "Edited flare placement fixture draws visible sprites");
        AssertEqual(original.LastFinalizedSpriteCount, changed.LastFinalizedSpriteCount, "Visual shift retains sprite count");
        for (int i = 0; i < original.LastFinalizedSpriteCount; i++)
        {
            AssertEqual((original.GetEntry(i).X + 7) & 511, changed.GetEntry(i).X, "Editable placement shifts the actual flare OBJ seven pixels");
            AssertEqual(original.GetEntry(i).Y, changed.GetEntry(i).Y, "X-only flare edit preserves Y");
        }
        AssertTrue(beforeSamus.SequenceEqual(GraphDigest(subject)) && beforeSystem.SequenceEqual(GraphDigest(system)), "Flare art edit changes neither Samus physics nor any projectile/timer state");
        foreach (ushort hyper in new ushort[] { 0, 1 })
        {
            var originalSystem = new SamusProjectileSystem(); var editedSystem = new SamusProjectileSystem();
            foreach (var target in new[] { originalSystem, editedSystem })
            {
                target.Charge.FlareCounter = (ushort)30;
                var frames = target.Charge._flareFrames;
                Array.Fill(frames, (ushort)2);
            }
            subject.HyperBeam = hyper;
            var guarded = new FlarePlacementGuard(bus, subject.Pose, 2, true);
            original = SpriteFrameBuffer.DrawnOam(frame => originalSystem.DrawChargeFlare(frame, subject, new(0, 0),
                originalSystem.AdvanceChargeFlare(subject), null,
                new ChargeFlareArtwork { Placement = stock, Compositions = sprites, Objects = objects }));
            changed = SpriteFrameBuffer.DrawnOam(frame => editedSystem.DrawChargeFlare(frame, subject, new(0, 0),
                editedSystem.AdvanceChargeFlare(subject), null,
                new ChargeFlareArtwork { Placement = edited, Compositions = sprites, Objects = objects }));
            AssertTrue(original.LastFinalizedSpriteCount > 0, "Public normal/Hyper flare producer emits visible components");
            AssertEqual(original.LastFinalizedSpriteCount, changed.LastFinalizedSpriteCount, "Public flare placement preserves component admission");
            for (int i = 0; i < original.LastFinalizedSpriteCount; i++)
                AssertEqual((original.GetEntry(i).X + 7) & 511, changed.GetEntry(i).X, "Public normal/Hyper path forwards selected placement to all components");
            AssertTrue(GraphDigest(originalSystem).SequenceEqual(GraphDigest(editedSystem)), "Normal/Hyper flare visual edits retain identical complete post-tick simulation state");
        }
        document.Required("offsets").AsObject().Remove(ChargeFlarePlacementDefinitions.Key(false, 0));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing flare placement rejected");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("offsets").Required(ChargeFlarePlacementDefinitions.Key(false, 0))["x"] = 32768;
        AssertThrows<InvalidDataException>(() => Load(document), "Out-of-range flare position rejected");
        document = JsonNode.Parse(json); (document ?? throw new InvalidOperationException("Verification requires document."))["damage"] = 1;
        AssertThrows<InvalidDataException>(() => Load(document), "Flare placement cannot contain mechanics fields");
        AssertThrows<InvalidDataException>(() => ChargeFlarePlacementCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))), "Duplicate flare properties rejected");
        Console.WriteLine($"Flare placement: {cases} native OAM cases with position ROM forbidden, visible edited displacement, whole-state isolation and invalid-resource rejection pass.");
        static ChargeFlarePlacementCatalog Load(JsonNode node) => ChargeFlarePlacementCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(node.ToJsonString())));
    }
    /// <summary>Invokes the renderer's private flare component path with explicit compiled pose metadata.</summary>
    internal delegate void FlareMetadataDraw(SpriteFrameBuffer sprites, SamusState samus,
        Position16<RoomPixels> layer1, int component, SamusMode7Transform? transform,
        ChargeFlareArtwork artwork, ChargeFlarePose pose);

    /// <summary>Overlays a chosen pose and direction while rejecting reads from the migrated flare-origin table.</summary>
    [CpuBusBoundary("exact bus test: pose-table overlay that substitutes one synthetic pose row and direction byte while forbidding the flare-origin table")]
    internal sealed class FlarePlacementGuard(ISnesAddressSpace source, SamusPoseId pose, byte direction, bool forbid) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-byte requests through the same pose overlay as ordinary bus reads.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Substitutes pose metadata and optionally rejects the native flare-origin range.</summary>
        public byte ReadByte(int address)
        {
            int syntheticOffset = address - (SamusMovementRomData.Poses.Definitions + 0xfd * 8);
            if ((uint)syntheticOffset < 8)
                return syntheticOffset == 3 ? direction : CpuBusRegionRead.Read(source, SamusMovementRomData.Poses.Definitions + (int)pose * 8 + syntheticOffset);
            if (address == SamusMovementRomData.Poses.Definitions + (int)pose * 8 + 3) return direction;
            if (forbid && address is >= 0x90c1a8 and < 0x90c210)
                throw new InvalidDataException("Flare placement still reads origin ROM.");
            return CpuBusRegionRead.Read(source, address);
        }
        /// <summary>Forwards writes unchanged to the underlying address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Confirms stock and edited flare offsets flow through actor drawing, artwork rebinding, and debugger restoration.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyRuntimeFlarePlacement(SuperMetroidAddressSpace bus, ChargeFlarePlacementCatalog stock, ChargeFlarePlacementCatalog edited)
    {
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Player.IntroducedSamus.Pose = SamusPoseId.FacingRightNormalPose;
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        runtime.Player.IntroducedSamus.Presentation.InitializeAnimation(); runtime.Player.IntroducedSamus.Presentation.PrimeGraphics();
        runtime.Projectiles.Charge.FlareCounter = (ushort)30;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Array.Fill(runtime.Projectiles.Charge._flareTimers, NativeCountdown.Start(100));
        var game = CreateRetailGameFixture(bus);
        game.Session.runtime = runtime;
        // The fixture game has no host artwork of its own; adopting the runtime cleared its projectile sheets.
        InstalledFixtures.BindProjectileArtwork(runtime);
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
            ChargeFlarePlacement = stock,
        });
        AssertTrue(native.SequenceEqual(Draw(runtime)), "Runtime stock flare placement preserves complete actor OAM");
        game.BindArtwork(game.Artwork with
        {
            ChargeFlarePlacement = edited,
        });
        AssertTrue(!native.SequenceEqual(Draw(runtime)), "Runtime actor pass consumes edited flare placement");
        game.BindArtwork(game.Artwork with
        {
            ChargeFlarePlacement = null,
        });
        using var without = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindArtwork(game.Artwork with
        {
            ChargeFlarePlacement = stock,
        });
        using var with = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.WrittenBytes().SequenceEqual(with.WrittenBytes()), "Flare catalog does not enter frontend/runtime saved graphs");
        with.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(with);
        var restoredRuntime = restored.Session.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.ChargeFlarePlacement is null, "Restored flare catalog requires host rebind");
        BindRetailRuntimeFixture(restoredRuntime);
        game.BindArtwork(game.Artwork with
        {
            ChargeFlarePlacement = edited,
        }); restored.BindArtwork(restored.Artwork with
{
    ChargeFlarePlacement = edited,
});
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored runtime emits current flare content at the saved animation state");
    }

    /// <summary>Runs the charge-flare placement comparison against the local retail cartridge image.</summary>
    [VerificationCommand("--charge-flare-placement-fixture")]
    internal static int ChargeFlarePlacementFixtureCommand()
    {
        Suite(nameof(VerifyChargeFlarePlacement), () => VerifyChargeFlarePlacement(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
