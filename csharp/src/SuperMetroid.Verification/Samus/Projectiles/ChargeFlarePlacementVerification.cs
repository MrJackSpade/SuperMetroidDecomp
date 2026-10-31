using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;
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
using SuperMetroid.Core.Frontend;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Gameplay;

/// <summary>Checks editable charge-flare offsets against native OAM placement and the production rendering paths.</summary>
internal static class ChargeFlarePlacementVerification
{
    /// <summary>Compares stock flare placement across pose, direction, coordinate, and Mode 7 cases, then checks edited offsets and state isolation.</summary>
    internal static void VerifyChargeFlarePlacement(SuperMetroidAddressSpace bus)
    {
        Suite(nameof(VerifyChargeFlareCompositions), () => VerifyChargeFlareCompositions(bus));
        byte[] json = ChargeFlarePlacementExtractor.Extract(bus);
        var stock = ChargeFlarePlacements.Load(new MemoryStream(json));
        var document = RequiredJson.Parse(json);
        foreach (var entry in document.Required("offsets").AsObject())
            (entry.Value ?? throw new InvalidOperationException("Verification requires entry.Value."))["x"] = entry.Value.Required("x").GetValue<int>() + 7;
        var edited = Load(document);
        Suite(nameof(VerifyRuntimeFlarePlacement), () => VerifyRuntimeFlarePlacement(stock, edited));
        var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
        var sprites = ChargeFlareSprites.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(bus)));
        var drawMetadata = new FlareMetadataDraw(system.Animation.DrawFlareComponentWithMetadata);
        var draw = new Action<SpriteFrameBuffer, SamusState, Position16<RoomPixels>, int, TurningFloorTransform?, ChargeFlareArtwork>(system.Animation.DrawFlareComponent);
        ProjectileDrawArtwork objects = InstalledFixtures.ProjectileArtwork();
        int cases = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        for (byte direction = 0; direction < 16; direction++)
        foreach (ushort coordinate in new ushort[] { 0, 100, 255, ushort.MaxValue })
        foreach (TurningFloorTransform? transform in new TurningFloorTransform?[] { null, new(240, 16, -16, new Position16<RoomPixels>(128, 112)) })
        for (int component = 0; component < 3; component++)
        {
            // Exercise all original low-nibble operands at the renderer's metadata
            // boundary; compiled pose rules cannot be overridden by fake ROM bytes.
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(coordinate, coordinate)) };
            int record = SamusSourceDefinitions.PoseDefinitions.Value + (int)pose * 8;
            bool running = bus.ReadCartridgeByte(record + 1) == (byte)SamusMovementType.Running;
            bool facingLeft = bus.ReadCartridgeByte(record) == 4;
            byte yOffset = bus.ReadCartridgeByte(record + 4);
            var native = new OamBuffer();
            int xTable = running ? SamusSourceDefinitions.ChargeFlareRunningXOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultXOffsets.Value;
            int yTable = running ? SamusSourceDefinitions.ChargeFlareRunningYOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultYOffsets.Value;
            short Word(int address) => unchecked((short)CartridgeImportSource.Require(bus).ReadCartridgeWord(address));
            var world = new Position16<RoomPixels>(coordinate, coordinate);
            var point = transform is { } selectedTransform ? selectedTransform.Transform(world) : world;
            ushort x = unchecked((ushort)(point.X + Word(xTable + direction * 2)));
            ushort y = unchecked((ushort)(point.Y + Word(yTable + direction * 2) - yOffset));
            ushort selector = (ushort)(component == 0 ? 0 : facingLeft ? (component == 1 ? 42 : 48) : (component == 1 ? 30 : 36));
            if ((LittleEndianBytes.HighByteInPlace(y)) == 0) DrawImportedFlareSpritemap(bus, native, selector, new(x, y));
            OamBuffer actual = SpriteFrameBuffer.DrawnOam(frame => drawMetadata(frame, samus, Position16<RoomPixels>.Zero, component, transform,
                new ChargeFlareArtwork { Placement = stock, Objects = objects },
                new ChargeFlarePose { Direction = direction, Running = running, PoseYOffset = yOffset, FacingLeft = facingLeft }));
            AssertTrue(native.ImageEquals(actual), "Flare placement stock preserves native OAM/culling for running, facing, overread directions and Mode7");
            AssertEqual(native.NextByteOffset / 4, actual.LastFinalizedSpriteCount, "Flare placement preserves OAM admission");
            cases++;
        }
        var subject = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
        PrepareRetailSamusFixture(subject);
        byte[] beforeSamus = GraphDigest(subject), beforeSystem = GraphDigest(system);
        OamBuffer original = SpriteFrameBuffer.DrawnOam(frame => draw(frame, subject, Position16<RoomPixels>.Zero, 0, null,
            new ChargeFlareArtwork { Placement = stock, Objects = objects }));
        OamBuffer changed = SpriteFrameBuffer.DrawnOam(frame => draw(frame, subject, Position16<RoomPixels>.Zero, 0, null,
            new ChargeFlareArtwork { Placement = edited, Objects = objects }));
        AssertTrue(original.LastFinalizedSpriteCount > 0, "Edited flare placement fixture draws visible sprites");
        AssertEqual(original.LastFinalizedSpriteCount, changed.LastFinalizedSpriteCount, "Visual shift retains sprite count");
        for (int i = 0; i < original.LastFinalizedSpriteCount; i++)
        {
            AssertEqual((original.GetEntry(i).Position.X + 7) & 511, changed.GetEntry(i).Position.X, "Editable placement shifts the actual flare OBJ seven pixels");
            AssertEqual(original.GetEntry(i).Position.Y, changed.GetEntry(i).Position.Y, "X-only flare edit preserves Y");
        }
        AssertTrue(beforeSamus.SequenceEqual(GraphDigest(subject)) && beforeSystem.SequenceEqual(GraphDigest(system)), "Flare art edit changes neither Samus physics nor any projectile/timer state");
        foreach (bool hyper in new[] { false, true })
        {
            var originalSystem = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads); var editedSystem = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads);
            foreach (var target in new[] { originalSystem, editedSystem })
            {
                target.Charge.FlareCounter = 30;
                var frames = target.Charge.FlareFrames;
                Array.Fill(frames, 2);
            }
            subject.HyperBeamEnabled = hyper;
            var guarded = new FlarePlacementGuard(bus, subject.Pose, 2, true);
            original = SpriteFrameBuffer.DrawnOam(frame => originalSystem.DrawChargeFlare(frame, subject, new(0, 0),
                originalSystem.AdvanceChargeFlare(subject), null,
                new ChargeFlareArtwork { Placement = stock, Objects = objects }));
            changed = SpriteFrameBuffer.DrawnOam(frame => editedSystem.DrawChargeFlare(frame, subject, new(0, 0),
                editedSystem.AdvanceChargeFlare(subject), null,
                new ChargeFlareArtwork { Placement = edited, Objects = objects }));
            AssertTrue(original.LastFinalizedSpriteCount > 0, "Public normal/Hyper flare producer emits visible components");
            AssertEqual(original.LastFinalizedSpriteCount, changed.LastFinalizedSpriteCount, "Public flare placement preserves component admission");
            for (int i = 0; i < original.LastFinalizedSpriteCount; i++)
                AssertEqual((original.GetEntry(i).Position.X + 7) & 511, changed.GetEntry(i).Position.X, "Public normal/Hyper path forwards selected placement to all components");
            AssertTrue(GraphDigest(originalSystem).SequenceEqual(GraphDigest(editedSystem)), "Normal/Hyper flare visual edits retain identical complete post-tick simulation state");
        }
        document.Required("offsets").AsObject().Remove(ChargeFlarePlacementOffsets.Key(false, 0));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing flare placement rejected");
        document = JsonNode.Parse(json);
        (document ?? throw new InvalidOperationException("Verification requires document.")).Required("offsets").Required(ChargeFlarePlacementOffsets.Key(false, 0))["x"] = 32768;
        AssertThrows<InvalidDataException>(() => Load(document), "Out-of-range flare position rejected");
        document = JsonNode.Parse(json); (document ?? throw new InvalidOperationException("Verification requires document."))["damage"] = 1;
        AssertThrows<InvalidDataException>(() => Load(document), "Flare placement cannot contain mechanics fields");
        AssertThrows<InvalidDataException>(() => ChargeFlarePlacements.Load(new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))), "Duplicate flare properties rejected");
        Console.WriteLine($"Flare placement: {cases} native OAM cases with position ROM forbidden, visible edited displacement, whole-state isolation and invalid-resource rejection pass.");
        static ChargeFlarePlacements Load(JsonNode node) => ChargeFlarePlacements.Load(new MemoryStream(Encoding.UTF8.GetBytes(node.ToJsonString())));
    }
    /// <summary>Invokes the renderer's private flare component path with explicit compiled pose metadata.</summary>
    internal delegate void FlareMetadataDraw(SpriteFrameBuffer sprites, SamusState samus,
        Position16<RoomPixels> layer1, int component, TurningFloorTransform? transform,
        ChargeFlareArtwork artwork, ChargeFlarePose pose);

    /// <summary>Overlays a chosen pose and direction while rejecting reads from the migrated flare-origin table.</summary>
    internal sealed class FlarePlacementGuard(ISnesAddressSpace source, SamusPoseId pose, byte direction, bool forbid) : ISnesWritableAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-byte requests through the same pose overlay as ordinary bus reads.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Substitutes pose metadata and optionally rejects the native flare-origin range.</summary>
        public byte ReadByte(int address)
        {
            int syntheticOffset = address - (SamusSourceDefinitions.PoseDefinitions.Value + 0xfd * 8);
            if ((uint)syntheticOffset < 8)
                return syntheticOffset == 3 ? direction : CpuBusRegionRead.Read(source, SamusSourceDefinitions.PoseDefinitions.Value + (int)pose * 8 + syntheticOffset);
            if (address == SamusSourceDefinitions.PoseDefinitions.Value + (int)pose * 8 + 3) return direction;
            if (forbid && address is >= 0x90c1a8 and < 0x90c210)
                throw new InvalidDataException("Flare placement still reads origin ROM.");
            return CpuBusRegionRead.Read(source, address);
        }
        /// <summary>Forwards writes unchanged to the underlying address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Confirms stock and edited flare offsets flow through actor drawing, artwork rebinding, and debugger restoration.</summary>
    private static void VerifyRuntimeFlarePlacement(ChargeFlarePlacements stock, ChargeFlarePlacements edited)
    {
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Player.IntroducedSamus.Pose = SamusPoseId.FacingRightNormalPose;
        runtime.Player.IntroducedSamus.Position = runtime.Player.IntroducedSamus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        SamusPresentation.InitializeAnimation(runtime.Player.IntroducedSamus); runtime.Player.IntroducedSamus.Presentation.PrimeGraphics(runtime.Player.IntroducedSamus);
        runtime.Projectiles.Charge.FlareCounter = 30;
        Array.Fill(runtime.Projectiles.Charge.FlareTimers, WrappingCountdown.Start(100));
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), random: random);
        game.AdoptRuntime(runtime);
        // The fixture game has no host artwork of its own; adopting the runtime cleared its projectile sheets.
        InstalledFixtures.BindProjectileArtwork(runtime);
        byte[] Draw(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
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
