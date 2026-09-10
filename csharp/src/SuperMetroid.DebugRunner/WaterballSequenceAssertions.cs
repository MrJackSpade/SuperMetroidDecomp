using SuperMetroid.Core.Game;

/// <summary>Measured native witnesses for the run-96, morph-offset-32 liquid entry.</summary>
internal static class WaterballSequenceAssertions
{
    public static void Verify(SamusState samus, int medium, int frame)
    {
        if (frame == 146 && samus.LiquidPhysics.LiquidPhysicsType != 0)
            throw new InvalidDataException("Liquid FX became active at occupied-bottom equality.");
        int apex = medium == 0 ? 195 : 192;
        int end = medium == 0 ? 233 : 225;
        if (frame is 147 or 163 || frame == apex || frame == end)
        {
            ushort extra = medium == 1 ? (ushort)0 : (ushort)0xf000;
            ushort whole = medium == 1 ? (ushort)0 : (ushort)5;
            ushort boost = medium == 1 ? (ushort)0 : (ushort)0x0401;
            if (samus.HorizontalSpeed.ExtraRunSpeed != whole || samus.HorizontalSpeed.ExtraRunSubspeed != extra ||
                samus.HorizontalSpeed.SpeedBoostCounter != boost)
                throw new InvalidDataException("Liquid-entry boost/momentum retention differs from cartridge.");
        }
        if (frame == 147 && samus.HorizontalSpeed.BaseFixed != 0x00010000)
            throw new InvalidDataException("Liquid boundary friction changed.");
        if (frame == 163 && samus.MorphBallBounceState != 1)
            throw new InvalidDataException("Submerged landing did not start the native bounce.");
        uint apexY = medium == 0 ? 0x02697fffu : 0x026b45ffu;
        if (frame == apex && samus.Kinematics.YFixed != apexY)
            throw new InvalidDataException("Submerged bounce apex changed.");
        if (frame == end && (samus.Kinematics.YFixed != 0x0279ffff || samus.MorphBallBounceState != 0 ||
            samus.HorizontalSpeed.BaseFixed != 0))
            throw new InvalidDataException("Submerged bounce termination changed.");
    }
}
