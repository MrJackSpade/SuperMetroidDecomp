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
        var samus = new SamusState();
        var slot = new SamusProjectileSlot(0);
        var noReads = new SlopeHeightNoReadBus();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            byte expected = rom.ReadByte(0x91b629 + pose * 8 + 4);
            AssertEqual(expected, SamusPoseProjectileOriginDefinitions.ReadYOffset(pose <= 0xfc ? noReads : rom, (byte)pose),
                "Every authored correction is compiled; three trailing poses retain adjacent-data reads");
        }
        for (int pose = 0; pose <= 0xfc; pose++)
        for (byte direction = 0; direction < 10; direction++)
        for (int art = 0; art <= byte.MaxValue; art++)
        {
            bus.Pose = samus.Pose = (byte)pose; bus.Direction = direction; bus.ArtY = (byte)art;
            samus.XPosition = unchecked((ushort)(pose * 251 + art));
            samus.YPosition = unchecked((ushort)~samus.XPosition);
            byte physicalY = rom.ReadByte(0x91b629 + pose * 8 + 4);
            bool running = rom.ReadByte(0x91b629 + pose * 8 + 1) == 1;
            bool beamRunning = running || pose is 0x75 or 0x76;
            slot.Direction = direction;
            initialize(bus, samus, slot);
            AssertEqual(unchecked((ushort)(samus.XPosition + Word((beamRunning ? SamusProjectileRomData.Origins.RunningX : SamusProjectileRomData.Origins.DefaultX) + direction * 2))), slot.XPosition, "Pose art leaves projectile X unchanged");
            AssertEqual(unchecked((ushort)(samus.YPosition + Word((beamRunning ? SamusProjectileRomData.Origins.RunningY : SamusProjectileRomData.Origins.DefaultY) + direction * 2) - physicalY)), slot.YPosition, "Pose art leaves projectile collision Y unchanged");
            AssertEqual(unchecked((sbyte)art), samus.ReadGraphicsYOffset(bus), "Visual body offset remains replaceable");

            var g = samus.Grapple;
            g.Phase = GrapplePhase.Inactive;
            samus.LiquidPhysics.BeginFrameSoundRequests();
            SamusGrappleMovement.BeginFiring(bus, samus);
            bool held = pose is SamusPoseIds.DraygonGrabbedMovingLeftPose or SamusPoseIds.DraygonGrabbedMovingRightPose;
            int firingDirection = held ? (pose == SamusPoseIds.DraygonGrabbedMovingLeftPose ? 7 : 2) : direction;
            int physicalCorrection = held ? 6 : physicalY;
            int visualCorrection = held ? 6 : unchecked((sbyte)art);
            short originY = Word((running ? 0x9bc186 : 0x9bc136) + firingDirection * 2);
            short flareY = Word((running ? 0x9bc1ae : 0x9bc15e) + firingDirection * 2);
            AssertEqual(unchecked((ushort)(samus.YPosition + originY - physicalCorrection)), g.AnchorY, "Grapple physical endpoint ignores art correction");
            AssertEqual(unchecked((ushort)(samus.YPosition + flareY - visualCorrection)), g.BeamStartY, "Grapple flare retains visual correction");
            ushort endpoint = g.AnchorY;
            samus.YPosition = unchecked((ushort)(samus.YPosition + 5));
            refresh(bus, samus, g);
            AssertEqual(unchecked((ushort)(samus.YPosition + originY - physicalY)), g.RopeStartY, "Late Grapple physical start ignores art correction");
            AssertEqual(unchecked((ushort)(samus.YPosition + flareY - unchecked((sbyte)art))), g.BeamStartY, "Late Grapple flare uses current artwork");
            AssertEqual(endpoint, g.AnchorY, "Late draw does not move collision endpoint");
        }
        Console.WriteLine("Pose projectile origins: 647680 actual beam and Grapple launch/late-draw cases preserve physics while replacing every graphics-Y byte.");
    }

    private sealed class PoseOriginPresentationBus(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte Pose, Direction, ArtY;
        public byte ReadByte(int address)
        {
            if (address == 0x91b629 + Pose * 8 + 3) return Direction;
            if (address == 0x91b629 + Pose * 8 + 4) return ArtY;
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
