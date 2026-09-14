using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGrappleFlarePlacement(SuperMetroidAddressSpace rom)
    {
        byte[] json = GrappleFlarePlacementExtractor.Extract(rom);
        var stock = ChargeFlarePlacementCatalog.Load(new MemoryStream(json));
        var document = JsonNode.Parse(json)!;
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction < 16; direction++)
        {
            var offset = stock.Resolve(running, direction);
            int x = running ? SamusGrappleRomData.Firing.RunningFlareX : SamusGrappleRomData.Firing.DefaultFlareX;
            int y = running ? SamusGrappleRomData.Firing.RunningFlareY : SamusGrappleRomData.Firing.DefaultFlareY;
            AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(rom, x + direction * 2)), offset.X, "Extracted Grapple flare X including adjacent rows");
            AssertEqual(unchecked((short)RomDataReader.ReadWordFixedBank(rom, y + direction * 2)), offset.Y, "Extracted Grapple flare Y including adjacent rows");
            var editedOffset = document["offsets"]![ChargeFlarePlacementDefinitions.Key(running, direction)]!;
            editedOffset["x"] = unchecked((short)(offset.X + 7));
            editedOffset["y"] = unchecked((short)(offset.Y - 9));
        }
        var edited = ChargeFlarePlacementCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        var refresh = typeof(SamusGrappleMovement).GetMethod("RefreshFiringDrawOrigins", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusGrappleState>>();
        var connect = typeof(SamusGrappleMovement).GetMethod("ConnectAcceptedFiring", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Func<ISnesAddressSpace, SamusState, SamusGrappleState, ushort, ushort, bool, bool, GrappleMovementResult>>();
        var empty = CreateRoom(64, 64, new ushort[4096], new byte[4096]);
        int locked = 0;
        foreach (byte movement in new byte[] { 0, 1 })
        for (byte direction = 0; direction < 10; direction++)
        {
            byte pose = movement == 1 ? SamusPoseIds.MovingRightNormalPose : SamusPoseIds.FacingRightNormalPose;
            var nativeBus = new GrappleFiringReadGuard(rom) { SourcePose = pose, Direction = direction, SyntheticAim = true };
            var guarded = new GrappleFlareReadGuard(nativeBus);
            var native = Seed(null); var selected = Seed(stock); var changed = Seed(edited);
            SamusGrappleMovement.BeginFiring(nativeBus, native);
            SamusGrappleMovement.BeginFiring(guarded, selected);
            SamusGrappleMovement.BeginFiring(guarded, changed);
            Compare();
            refresh(nativeBus, native, native.Grapple); refresh(guarded, selected, selected.Grapple); refresh(guarded, changed, changed.Grapple);
            Compare();
            for (int tick = 0; tick < 10; tick++)
            {
                SamusGrappleMovement.StepFiring(nativeBus, empty, native, (ushort)SnesButton.X);
                SamusGrappleMovement.StepFiring(guarded, empty, selected, (ushort)SnesButton.X);
                SamusGrappleMovement.StepFiring(guarded, empty, changed, (ushort)SnesButton.X);
                Compare();
            }
            var expectedConnection = connect(nativeBus, native, native.Grapple, 512, 512, true, false);
            var selectedConnection = connect(guarded, selected, selected.Grapple, 512, 512, true, false);
            var editedConnection = connect(guarded, changed, changed.Grapple, 512, 512, true, false);
            AssertEqual(expectedConnection, selectedConnection, "Stock placement retains connection result");
            AssertEqual(expectedConnection, editedConnection, "Edited placement retains connection result");
            Compare(expectedConnection.LockedInPlace);
            if (expectedConnection.LockedInPlace) locked++;

            SamusState Seed(ChargeFlarePlacementCatalog? placement)
            {
                var samus = new SamusState { Pose = 0xfd, XPosition = 512, YPosition = 512 };
                samus.Grapple.FlarePlacement = placement;
                return samus;
            }
            void Compare(bool checkVisibleOffset = true)
            {
                AssertTrue(SaveGrappleFixture(native).SequenceEqual(SaveGrappleFixture(selected)), "Stock extracted flare preserves complete Samus graph");
                var a = native.Grapple; var b = changed.Grapple;
                if (checkVisibleOffset)
                {
                    AssertEqual(unchecked((ushort)(a.BeamStartX + 7)), b.BeamStartX, "Edited Grapple flare X is visible");
                    AssertEqual(unchecked((ushort)(a.BeamStartY - 9)), b.BeamStartY, "Edited Grapple flare Y is visible");
                }
                var saved = (b.FlareXOffset, b.FlareYOffset, b.BeamStartX, b.BeamStartY);
                b.FlareXOffset = a.FlareXOffset; b.FlareYOffset = a.FlareYOffset;
                b.BeamStartX = a.BeamStartX; b.BeamStartY = a.BeamStartY;
                AssertTrue(SaveGrappleFixture(native).SequenceEqual(SaveGrappleFixture(changed)), "Edited flare changes only four presentation words, not full-body/anchor/trajectory state");
                (b.FlareXOffset, b.FlareYOffset, b.BeamStartX, b.BeamStartY) = saved;
            }
        }
        AssertTrue(locked > 0, "Placement fixture exercises locked connections");
        var runtime = new SuperMetroidRuntime(rom);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.RunNmi(0, true);
        byte[] beforeBinding = SaveGrappleFixture(runtime.Samus!);
        var swingFrames = GrappleSwingFrameCatalog.Load(new MemoryStream(GrappleSwingFrameExtractor.Extract(rom)));
        runtime.GrappleArtwork = GrappleTileAtlas.Load(new MemoryStream(GrappleTileExtractor.Extract(rom)), flarePlacement: edited, swingFrames: swingFrames);
        AssertTrue(ReferenceEquals(edited, runtime.Samus!.Grapple.FlarePlacement), "Artwork rebinding reaches current Samus immediately");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Samus.Grapple.SwingFrames), "Artwork rebinding reaches current swing-frame selection");
        AssertTrue(beforeBinding.SequenceEqual(SaveGrappleFixture(runtime.Samus)), "Presentation binding is excluded from Samus saved graph");
        runtime.Samus.Grapple.FlarePlacement = null;
        runtime.Samus.Grapple.SwingFrames = null;
        runtime.StepFrame(0);
        AssertTrue(ReferenceEquals(edited, runtime.Samus.Grapple.FlarePlacement), "Real gameplay frame rebinds missing/restored visual origins before producers");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Samus.Grapple.SwingFrames), "Real gameplay frame rebinds swing art before pendulum update");
        runtime.Samus.Grapple.FlarePlacement = null;
        runtime.Samus.Grapple.SwingFrames = null;
        typeof(SuperMetroidRuntime).GetMethod("DrawGameplayActors", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(runtime, new object?[] { false, null, null, false });
        AssertTrue(ReferenceEquals(edited, runtime.Samus.Grapple.FlarePlacement), "Independent actor drawing also rebinds visual origins");
        AssertTrue(ReferenceEquals(swingFrames, runtime.Samus.Grapple.SwingFrames), "Independent actor drawing also rebinds swing-frame selection");
        Console.WriteLine($"Grapple flare placement: 32 extracted pairs, 20 launch/late paths, 200 trajectory frames and {locked} locked connections preserve physical state under visual edits and ROM guard.");
    }
    private sealed class GrappleFlareReadGuard(ISnesAddressSpace bus) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x9bc14a and < 0x9bc172 or >= 0x9bc19a and < 0x9bc1c2)
                throw new InvalidOperationException("Grapple still reads visual flare origins from ROM.");
            return bus.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => bus.WriteByte(address, value);
    }
}
