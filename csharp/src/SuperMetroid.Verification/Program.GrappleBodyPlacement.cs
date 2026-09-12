using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyGrappleBodyPlacement(SuperMetroidAddressSpace rom)
    {
        var position = typeof(SamusGrappleMovement).GetMethod("PositionSamusFromPendulum", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusGrappleState>>();
        var bus = new GrappleBodyReadGuard(rom);
        var samus = new SamusState();
        foreach (bool replaceArt in new[] { false, true })
        foreach (bool left in new[] { false, true })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var g = samus.Grapple;
            samus.Pose = left ? SamusPoseIds.GrappleSwingLeftPose : SamusPoseIds.GrappleSwingRightPose;
            g.AnchorX = (ushort)raw; g.AnchorY = unchecked((ushort)~raw);
            g.RopeLength = (ushort)(raw & 127);
            g.ValidateAnchorBlock = (raw & 1) != 0;
            // The physical rope uses Angle; the body mapping uses the remembered mirror.
            g.Angle = SnesAngle.FromRaw(unchecked((ushort)(raw ^ 0x4080)));
            g.MirroredAngle = SnesAngle.FromRaw((ushort)raw);
            byte nativeFrame = rom.ReadByte(0x9bc1c2 + (raw >> 8));
            int table = left ? 0x9bc2c2 : 0x9bc302;
            sbyte x = unchecked((sbyte)rom.ReadByte(table + nativeFrame * 2));
            sbyte y = unchecked((sbyte)rom.ReadByte(table + nativeFrame * 2 + 1));
            bus.ReplaceArt = replaceArt;
            position(bus, samus, g);
            AssertEqual(unchecked((ushort)(g.RopeStartX + x)), samus.XPosition, "Art override cannot move Grapple collision-body X");
            AssertEqual(unchecked((ushort)(g.RopeStartY + y)), samus.YPosition, "Art override cannot move Grapple collision-body Y");
            AssertEqual(replaceArt ? (nativeFrame + 11) & 31 : nativeFrame, samus.AnimationFrame, "Visual frame override still applies");
            AssertEqual(15, samus.AnimationFrameTimer, "Native swing animation timer");
            AssertEqual(g.RopeStartX, g.BeamStartX, "Swing Flare follows physical Start X");
            AssertEqual(g.RopeStartY, g.BeamStartY, "Swing Flare follows physical Start Y");
        }
        Console.WriteLine("Grapple body placement: 262144 real updates preserve native physical offsets independently of replaced art frames, mirror angle, facing and anchor wrapping.");
    }

    private sealed class GrappleBodyReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public bool ReplaceArt;
        public byte ReadByte(int address)
        {
            if (address is >= 0x9bc2c2 and < 0x9bc342)
                throw new InvalidOperationException($"Compiled Grapple body offset read ROM ${address:X6}.");
            if (ReplaceArt && address is >= 0x9bc1c2 and < 0x9bc2c2)
                return (byte)((source.ReadByte(address) + 11) & 31);
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
