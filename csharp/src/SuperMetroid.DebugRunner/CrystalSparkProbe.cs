using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Constructs only the aged Power Bomb; Samus earns windup through controller input.</summary>
internal static class CrystalSparkProbe
{
    public static ushort HeightInputAt(int frame, bool left, int mode)
    {
        int posture = mode / 8;
        if (posture == 1 && frame >= 148 && frame <= 150)
            return frame == 150 ? (ushort)0x90 : (ushort)0x10;
        if (posture == 2 && frame >= 145 && frame < 150) return 0x800;
        return InputAt(frame, left, mode & 7);
    }

    public static void VerifyHeight(SamusState samus, int frame, int mode)
    {
        if (frame != 151) return;
        // Native controller sequences produce these three windup centers, not
        // injected poses/coordinates: plain crouch, aimed crouch, standing.
        ushort expectedY = (mode / 8) switch { 0 => 482, 1 => 492, 2 => 490,
            _ => throw new InvalidDataException("Unknown Crystal Spark posture.") };
        if (samus.YPosition != expectedY || samus.Kinematics.YSubposition != 0xffff ||
            samus.Shinespark.Phase != ShinesparkPhase.Windup)
            throw new InvalidDataException("Crystal Spark pre-cleanup height does not match native posture.");
    }

    public static ushort InputAt(int frame, bool left, int mode)
    {
        if (frame < 140) return (ushort)(0x8000 | (left ? 0x200 : 0x100));
        if (frame == 140) return 0x410;
        if (frame < 150) return 0;
        if (frame == 150) return 0x80;
        if (frame == 151) return 0x800;
        if (frame == 152) return mode == 7 ? (ushort)0x4f0 : (ushort)0x470;
        // A second spark must be earned from the persistent result, not reseeded.
        if (mode == 0 && frame == 460) return 0x410;
        if (mode == 0 && frame > 460 && frame < 470) return 0x10;
        if (mode == 0 && frame == 470) return 0x80;
        if (mode == 0 && frame >= 471) return 0x880;
        return 0;
    }

    public static void PrepareCleanup(ISnesAddressSpace bus, SamusState samus,
        SamusPowerBombExplosionState explosion, int mode)
    {
        if (samus.Shinespark.Phase != ShinesparkPhase.Windup)
            throw new InvalidDataException("Crystal Spark must start from controller-earned windup.");
        ushort x = unchecked((ushort)(samus.XPosition + (mode == 1 ? 1 : mode == 2 ? -1 : 0)));
        ushort y = unchecked((ushort)(samus.YPosition + (mode == 3 ? 1 : mode == 4 ? -1 : 0)));
        var clockProbe = new SamusPowerBombExplosionState();
        clockProbe.Spawn(x, y);
        int steps = 0;
        while (!clockProbe.StepFrame(bus))
            if (++steps >= 2000) throw new InvalidDataException("Power Bomb cleanup did not arrive.");
        explosion.Spawn(x, y);
        for (int step = 0; step < steps; step++)
            if (explosion.StepFrame(bus)) throw new InvalidDataException("Aged bomb crossed cleanup early.");
        // The next real runtime frame performs cleanup and the exact-center admission.
        // Advancing the visual owner here never mutates Samus or awards Flash/boost.
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        if (frame == 152 &&
            (samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive) != (mode == 0))
            throw new InvalidDataException("Exact-center/resource/chord Flash admission changed.");
        if (mode != 0) return;
        if (frame == 459 && (samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
            samus.HorizontalSpeed.SpeedBoostCounter != 0x0400 || samus.Health != 99 ||
            samus.Missiles != 0 || samus.SuperMissiles != 0 || samus.PowerBombs != 0))
            throw new InvalidDataException("Completed Flash did not leave native persistent Blue Suit.");
        if (frame == 460 && samus.Shinespark.ShineTimer != 179)
            throw new InvalidDataException("Crystal Spark result could not store another charge.");
        if (frame == 473 && (samus.Shinespark.Phase != ShinesparkPhase.Vertical ||
            samus.HorizontalSpeed.ContactDamageIndex != 2 || samus.Health != 98))
            throw new InvalidDataException("Crystal Spark result could not launch and drain normally.");
    }
}
