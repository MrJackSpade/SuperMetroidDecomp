using SuperMetroid.Core.Game;

/// <summary>Controller-earned midair activation with adjacent launch delays and momentum controls.</summary>
internal static class DraygonMidairInputs
{
    public static ushort At(int frame, bool left, int mode)
    {
        ushort forward = left ? (ushort)0x200 : (ushort)0x100;
        ushort reverse = left ? (ushort)0x100 : (ushort)0x200;
        int dash = (mode & 2) != 0 ? 0x8000 : 0;
        if (frame < 140) return (ushort)(0x8000 | forward);
        if (frame == 140) return 0x410;
        if (frame < 150) return (ushort)(forward | dash);
        if (frame < 160) return (ushort)(0x80 | ((mode & 4) != 0 && frame >= 154 ? reverse : forward) | dash);
        if (frame < 161 + (mode & 1)) return 0x800;
        if (frame < 186) return 0x880;
        if (frame >= 340 && frame < 350) return forward;
        if (frame >= 360 && frame < 370) return (ushort)(0x8000 | forward);
        return 0;
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        if (frame == 160 && samus.Shinespark.Phase != ShinesparkPhase.Windup)
            throw new InvalidDataException("Midair aim did not activate spark windup on the native frame.");
        if (frame == 161 + (mode & 1) && samus.Shinespark.Phase != ShinesparkPhase.Vertical)
            throw new InvalidDataException("Midair jump did not select the native launch frame.");
        if (frame == 170 && (samus.HorizontalSpeed.FirstSpeedEchoXPosition != 0) != (mode != 3))
            throw new InvalidDataException("Retained-speed delayed-launch echo cue changed.");
        if (frame == 330 && samus.HorizontalSpeed.SpeedBoostCounter != (mode is 2 or 3 ? 0 : 0x400))
            throw new InvalidDataException("Midair momentum/turnaround Blue Suit outcome changed.");
        if (frame == 349 && samus.HorizontalSpeed.ContactDamageIndex != (mode is 2 or 3 ? 0 : 1))
            throw new InvalidDataException("Walking did not use the acquired Blue Suit contact damage.");
        if (frame == 361 && samus.HorizontalSpeed.SpeedBoostCounter != 1)
            throw new InvalidDataException("Dash did not cancel the acquired Blue Suit.");
    }
}
