using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Independent cartridge samples exercise the production radial and
    /// release helpers, including anchor bias, 16-bit wrap and phase-dependent speed.</summary>
    private static void VerifyCompiledGrappleMath(SuperMetroidAddressSpace rom)
    {
        var native = new short[320];
        for (int i = 0; i < native.Length; i++)
        {
            int address = EnemyMathReferenceData.SignedNegativeCosine + i * 2;
            native[i] = unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        }

        // No bus argument is accepted by either production delegate. This makes an
        // accidental ROM dependency a failing contract, not merely equal output.
        var point = typeof(SamusGrappleMovement)
            .GetMethod("CalculateCollisionPoint", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Func<SamusGrappleState, byte, int, GrappleCollisionPoint>>();
        var release = typeof(SamusGrappleMovement)
            .GetMethod("PropelSamusFromSwing", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<SamusState, SamusGrappleState>>();
        var state = new SamusState();
        (ushort X, ushort Y)[] anchors = [(0, 0), (7, 8), (128, 255), (4095, 4096), (65520, 65535), (65535, 0)];
        int pointCount = 0;
        foreach (var anchor in anchors)
        for (int angle = 0; angle < 256; angle++)
        for (int distance = 0; distance <= 119; distance++)
        {
            // 119 includes maximum connected length 63 plus the growth frontier 56.
            // Native multiplies magnitudes and truncates BEFORE restoring the sign.
            short sx = native[angle + 64], sy = native[angle];
            ushort ax = (ushort)((anchor.X & 0xfff0) | (sx < 0 ? 7 : 8));
            ushort ay = (ushort)((anchor.Y & 0xfff0) | (sy < 0 ? 7 : 8));
            ushort x = unchecked((ushort)(ax + Math.Sign(sx) * (Math.Abs(sx) * distance / 256)));
            ushort y = unchecked((ushort)(ay + Math.Sign(sy) * (Math.Abs(sy) * distance / 256)));
            state.Grapple.AnchorX = anchor.X;
            state.Grapple.AnchorY = anchor.Y;
            var actual = point(state.Grapple, (byte)angle, distance);
            if (actual != new GrappleCollisionPoint(x, y, (x / 16) % 256, (y / 16) % 256) ||
                state.Grapple.AnchorX != ax || state.Grapple.AnchorY != ay)
                throw new InvalidDataException($"Compiled grapple point differs: anchor={anchor}, angle={angle:X2}, length={distance}.");
            pointCount++;
        }

        // Every permitted base velocity and every angle byte: includes the shifts
        // at doubled-velocity multiples of 512 and both signs at zero cosine.
        int releaseCount = 0;
        for (int angle = 0; angle < 256; angle++)
        for (int velocity = -SamusGrappleRomData.Physics.MaximumAngularVelocity;
             velocity <= SamusGrappleRomData.Physics.MaximumAngularVelocity; velocity++)
        {
            int doubled = 2 * Math.Abs(velocity);
            short vertical = native[angle + 64];
            uint expectedY = (uint)(Math.Abs(vertical) * doubled);
            int horizontalIndex = ((angle - 64 + 3 * (doubled / 512)) & 255) + 64;
            uint expectedX = (uint)(Math.Abs(native[horizontalIndex]) * doubled);
            ushort direction = (ushort)(velocity < 0 ? (vertical < 0 ? 2 : 1) : (vertical >= 0 ? 2 : 1));
            state.Grapple.Angle = SnesAngle.FromRaw((ushort)((angle << 8) | 0xff));
            state.Grapple.AngularVelocity = (short)velocity;
            release(state, state.Grapple);
            if (state.Kinematics.YSpeed != expectedY >> 16 || state.Kinematics.YSubspeed != (expectedY & 65535) ||
                state.Kinematics.YDirection != direction || state.HorizontalSpeed.BaseSpeed != expectedX >> 16 ||
                state.HorizontalSpeed.BaseSubspeed != (expectedX & 65535) || state.HorizontalSpeed.AccelerationMode != 2)
                throw new InvalidDataException($"Compiled grapple release differs: angle={angle:X2}, velocity={velocity}.");
            releaseCount++;
        }
        Console.WriteLine($"Compiled grapple: {pointCount} native radial coordinates/biases and {releaseCount} release velocities match without a production bus.");
    }
}
