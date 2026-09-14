using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    private static void VerifyGrappleBodyPlacement(SuperMetroidAddressSpace rom)
    {
        var position = typeof(SamusGrappleMovement).GetMethod("PositionSamusFromPendulum", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusGrappleState>>();
        var bus = new GrappleBodyReadGuard(rom);
        var samus = new SamusState();
        byte[] json = GrappleSwingFrameExtractor.Extract(rom);
        var stock = GrappleSwingFrameCatalog.Load(new MemoryStream(json));
        var edited = GrappleSwingFrameCatalog.Load(new MemoryStream(GrappleSwingFrameCatalog.Write(new()
        {
            Version = 1, Frames = Enumerable.Range(0, 256).Select(angle => (stock.Resolve((byte)angle) + 11) & 31).ToArray(),
        })));
        foreach (bool installed in new[] { false, true })
        foreach (bool replaceArt in new[] { false, true })
        foreach (bool left in new[] { false, true })
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var g = samus.Grapple;
            g.SwingFrames = installed ? replaceArt ? edited : stock : null;
            bus.ForbidArt = installed;
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
        foreach (string invalid in new[] { "null", "{}", "{", "{\"version\":1,\"version\":1,\"frames\":[]}" })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid))), "Invalid swing-frame JSON rejected");
        foreach (int invalid in new[] { -1, 32 })
            AssertThrows<InvalidDataException>(() => GrappleSwingFrameCatalog.Write(new() { Version = 1, Frames = Enumerable.Repeat(invalid, 256).ToArray() }), "Invalid swing-frame index rejected");
        AssertThrows<InvalidDataException>(() => GrappleSwingFrameCatalog.Write(new() { Version = 1, Frames = new int[255] }), "Incomplete swing-frame table rejected");
        var empty = CreateRoom(64, 64, new ushort[4096], new byte[4096]);
        foreach (bool faceRight in new[] { false, true })
        for (int angle = 0; angle < 256; angle += 32)
        {
            var native = new SamusState(); var changed = new SamusState();
            native.Grapple.SwingFrames = stock; changed.Grapple.SwingFrames = edited;
            bus.ReplaceArt = false; bus.ForbidArt = true;
            SamusGrappleMovement.ConnectUnobstructedSwing(bus, native, 512, 512, 32, SnesAngle.FromRaw((ushort)(angle << 8)), 128, faceRight);
            SamusGrappleMovement.ConnectUnobstructedSwing(bus, changed, 512, 512, 32, SnesAngle.FromRaw((ushort)(angle << 8)), 128, faceRight);
            for (int tick = 0; tick < 16; tick++)
            {
                ushort input = (ushort)(SuperMetroid.Core.Input.SnesButton.X | (tick < 8 ? SuperMetroid.Core.Input.SnesButton.Left : SuperMetroid.Core.Input.SnesButton.Right));
                AssertEqual(SamusGrappleMovement.Step(bus, empty, native, input, 0), SamusGrappleMovement.Step(bus, empty, changed, input, 0), "Edited swing frames preserve actual pendulum results");
                AssertEqual((native.AnimationFrame + 11) & 31, changed.AnimationFrame, "Edited frame reaches actual pendulum updater");
                AssertEqual(native.AnimationFrameTimer, changed.AnimationFrameTimer, "Visual mapping cannot change swing timing");
                ushort displayed = changed.AnimationFrame;
                changed.SetGrappleSwingAnimationFrame(native.AnimationFrame);
                AssertTrue(SaveGrappleFixture(native).SequenceEqual(SaveGrappleFixture(changed)), "Entire swing simulation state matches except displayed frame");
                changed.SetGrappleSwingAnimationFrame(displayed);
            }
        }
        Console.WriteLine("Grapple body placement: 524288 legacy/installed updates preserve physical offsets independently of edited JSON frames, mirror angle, facing and anchor wrapping; installed selector ROM reads forbidden.");
    }

    private sealed class GrappleBodyReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public bool ReplaceArt;
        public bool ForbidArt;
        public byte ReadByte(int address)
        {
            if (ForbidArt && address is >= 0x9bc1c2 and < 0x9bc2c2)
                throw new InvalidOperationException("Installed swing art still reads the ROM selector.");
            if (address is >= 0x9bc2c2 and < 0x9bc342)
                throw new InvalidOperationException($"Compiled Grapple body offset read ROM ${address:X6}.");
            if (ReplaceArt && address is >= 0x9bc1c2 and < 0x9bc2c2)
                return (byte)((source.ReadByte(address) + 11) & 31);
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
