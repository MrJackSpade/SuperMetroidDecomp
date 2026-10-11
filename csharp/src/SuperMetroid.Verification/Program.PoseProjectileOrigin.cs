using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPoseProjectileOrigin(SuperMetroidAddressSpace rom)
    {
        short Word(int a) => unchecked((short)(rom.ReadByte(a) | rom.ReadByte(a + 1) << 8));
        var initialize = typeof(SamusProjectileSystem).GetMethod("InitializePosition", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusProjectileSlot>>();
        var refresh = typeof(SamusGrappleMovement).GetMethod("RefreshFiringDrawOrigins", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusGrappleState>>();
        var bus = new PoseOriginPresentationBus(rom);
        var samus = PrepareRetailSamusFixture(new SamusState());
        var slot = new SamusProjectileSlot(0);
        SamusBodyArtworkCatalog template = CreateSamusIdentityFixture();
        // The graphics-Y byte only enters as a signed offset, so one artwork fixture per signed
        // byte class covers it.
        var artwork = SignedByteClasses.Select(art => new SamusBodyArtworkCatalog(
            template.TopSetPointers.ToArray(), template.BottomSetPointers.ToArray(),
            template.PosePointers.ToArray(),
            Enumerable.Repeat(unchecked((sbyte)art), SamusBodyArtworkCatalog.PoseCount).ToArray(),
            template.Frames.ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.TopSetCount).Select(index => template.TopSet(index).ToArray()).ToArray(),
            Enumerable.Range(0, SamusBodyArtworkCatalog.BottomSetCount).Select(index => template.BottomSet(index).ToArray()).ToArray(),
            template.Spritemaps, template.Atmosphere, template.DeathPalettes,
            template.DeathTiles, template.ArmCannon, template.LandingYOffsets.ToArray(),
            template.PostureYOffsets.ToArray(), template.DrainedYOffsets.ToArray())).ToArray();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            byte expected = rom.ReadByte(0x91b629 + pose * 8 + 4);
            AssertEqual(expected, SamusPoseProjectileOriginDefinitions.ReadYOffset((SamusPoseId)pose),
                "Every byte-valued correction is compiled, including three adjacent-code indexes");
        }
        for (int pose = 0; pose <= 0xfc; pose++)
        for (byte direction = 0; direction < 10; direction++)
        for (int artIndex = 0; artIndex < artwork.Length; artIndex++)
        {
            bus.Pose = samus.Pose = (SamusPoseId)pose;
            int art = SignedByteClasses[artIndex];
            samus.TileTransfers.BindArtwork(artwork[artIndex]);
            samus.Position = samus.Position.WithWholeX(unchecked((ushort)(pose * 251 + art)));
            samus.Position = samus.Position.WithWholeY(unchecked((ushort)~samus.Position.X.Whole));
            byte physicalY = rom.ReadByte(0x91b629 + pose * 8 + 4);
            bool running = rom.ReadByte(0x91b629 + pose * 8 + 1) == 1;
            bool beamRunning = running || pose is 0x75 or 0x76;
            slot.Direction = direction;
            initialize(bus, samus, slot);
            AssertEqual(unchecked((ushort)(samus.Position.X.Whole + Word((beamRunning ? SamusProjectileRomData.Origins.RunningX : SamusProjectileRomData.Origins.DefaultX) + direction * 2))), slot.Position.X.Whole, "Pose art leaves projectile X unchanged");
            AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + Word((beamRunning ? SamusProjectileRomData.Origins.RunningY : SamusProjectileRomData.Origins.DefaultY) + direction * 2) - physicalY)), slot.Position.Y.Whole, "Pose art leaves projectile collision Y unchanged");
            AssertEqual(unchecked((sbyte)art), samus.ReadGraphicsYOffset(bus), "Visual body offset remains replaceable");

            var g = samus.Grapple;
            g.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus);
            bool held = pose is (int)SamusPoseId.DraygonGrabbedMovingLeftPose or (int)SamusPoseId.DraygonGrabbedMovingRightPose;
            int firingDirection = held ? (pose == (int)SamusPoseId.DraygonGrabbedMovingLeftPose ? 7 : 2) : rom.ReadByte(0x91b629 + pose * 8 + 3);
            if ((firingDirection & 0xf0) != 0)
            {
                AssertEqual(GrapplePhase.CancelPending, g.Phase, "Artwork cannot enable Grapple in a restricted authored pose");
                continue;
            }
            int physicalCorrection = held ? 6 : physicalY;
            int visualCorrection = held ? 6 : unchecked((sbyte)art);
            short originY = Word((running ? 0x9bc186 : 0x9bc136) + firingDirection * 2);
            short flareY = Word((running ? 0x9bc1ae : 0x9bc15e) + firingDirection * 2);
            AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + originY - physicalCorrection)), g.Anchor.Y, "Grapple physical endpoint ignores art correction");
            AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + flareY - visualCorrection)), g.BeamStart.Y, "Grapple flare retains visual correction");
            ushort endpoint = g.Anchor.Y;
            samus.Position += new Offset16<RoomPixels>(0, 5);
            refresh(bus, samus, g);
            AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + originY - physicalY)), g.RopeStart.Y, "Late Grapple physical start ignores art correction");
            AssertEqual(unchecked((ushort)(samus.Position.Y.Whole + flareY - unchecked((sbyte)art))), g.BeamStart.Y, "Late Grapple flare uses current artwork");
            AssertEqual(endpoint, g.Anchor.Y, "Late draw does not move collision endpoint");
        }
        Console.WriteLine("Pose projectile origins: every pose and direction with each signed graphics-Y class, and authored Grapple launch/cancellation checks, preserve physics while replacing the graphics-Y byte.");
    }

    private sealed class PoseOriginPresentationBus(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public SamusPoseId Pose;
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address == 0x91b629 + (int)Pose * 8 + 3) throw new InvalidOperationException("Compiled pose aim read artwork metadata.");
            if (address == 0x91b629 + (int)Pose * 8 + 4) throw new InvalidOperationException("Installed graphics offset read ROM.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
