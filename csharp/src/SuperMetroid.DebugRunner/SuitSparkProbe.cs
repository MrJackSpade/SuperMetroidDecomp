using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Post-message suit admission; windup itself is earned with real inputs.</summary>
internal static class SuitSparkProbe
{
    public static ushort BombInputAt(int frame, bool left) => frame <= 151
        ? CrystalSparkProbe.InputAt(frame, left, 0) : ReleaseInputAt(Math.Max(frame, 330), left, 0);

    public static void PrepareBomb(SuperMetroidRuntime runtime, SamusState samus, int frame, int mode)
    {
        var bomb = runtime.BombProjectiles.Slots[0];
        if (frame == 315) bomb.ClearFields();
        if (frame != 314) return;
        // Collision-only timer-eight boundary, as in HurtBombComparisonAudit.
        // No fuse/animation step; the actual runtime overlap publishes direction.
        bomb.Type = (ushort)SamusProjectileFamily.Bomb;
        bomb.Damage = 30; bomb.BombTimer = 8;
        int offset = (mode & 3) switch { 1 => 12, 2 => -12, 3 => 13, _ => 0 };
        bomb.XPosition = unchecked((ushort)(samus.XPosition + offset));
        bomb.YPosition = samus.YPosition;
        bomb.XRadius = bomb.YRadius = 8;
    }

    public static void VerifyBomb(SamusState samus, int frame, int mode)
    {
        bool hit = (mode & 3) != 3;
        if (frame == 314 && (samus.BombJumpStarting != hit ||
            (samus.Shinespark.Phase == ShinesparkPhase.Inactive) != hit))
            throw new InvalidDataException("Suit bomb overlap must replace windup only on a hit.");
        if (!hit) return;
        if (frame == 343 && (samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
            samus.BombJumpActive || samus.BombJumpStarting))
            throw new InvalidDataException("Bomb arc must return to normal movement, never resume old windup.");
        if (frame == 389 && samus.HorizontalSpeed.SpeedBoostCounter != 0x0400)
            throw new InvalidDataException("Suit bomb interruption lost its retained boost.");
        if (frame == 390 && samus.Shinespark.ShineTimer != 179)
            throw new InvalidDataException("Suit bomb boost cannot store another charge.");
        if (frame == 403 && (samus.Shinespark.Phase != ShinesparkPhase.Vertical ||
            samus.HorizontalSpeed.ContactDamageIndex != 2 || samus.Health != 98))
            throw new InvalidDataException("Suit bomb boost cannot launch a damaging, energy-consuming spark.");
    }

    public static ushort ReleaseInputAt(int frame, bool left, int mode)
    {
        if (frame < 330) return InputAt(frame, left, mode);
        if (frame == 390) return 0x410;
        if (frame > 390 && frame < 400) return 0x10;
        if (frame == 400) return 0x80;
        if (frame >= 401) return 0x880;
        return 0;
    }

    public static void VerifyRelease(SamusState samus, int frame)
    {
        if (frame == 332 && (samus.Xray.IsActive || samus.Xray.TimeIsFrozen ||
            samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
            samus.HorizontalSpeed.SpeedBoostCounter != 0x0400))
            throw new InvalidDataException("X-ray release must restore normal movement without resuming old windup or losing boost.");
        if (frame == 390 && samus.Shinespark.ShineTimer != 179)
            throw new InvalidDataException("Retained suit boost must store a new charge without a run-up.");
        if (frame == 403 && (samus.HorizontalSpeed.ContactDamageIndex != 2 || samus.Health != 98 ||
            samus.Kinematics.VerticalSpeedFixed != 0x00071c00))
            throw new InvalidDataException("Retained suit boost must launch a moving, damaging, energy-consuming spark.");
    }

    public static void PrepareRelease(ISnesAddressSpace bus, SamusState samus)
    {
        if (!samus.Xray.IsActive) throw new InvalidDataException("Expected controller-admitted X-ray.");
        // Isolate the three real teardown calls from VRAM/window setup, without
        // touching Samus's pose, speed, or boost. The following runtime calls own
        // both BG2 restore waits and the final return to normal movement.
        for (int step = 0; samus.Xray.BeamPhase != XrayBeamPhase.RestoreFirstHalf; step++)
        {
            if (step >= 16) throw new InvalidDataException("X-ray did not reach its release boundary.");
            samus.Xray.StepBeam(bus, samus, 0);
        }
    }
    public static ushort InputAt(int frame, bool left, int mode)
    {
        if (frame <= 151) return CrystalSparkProbe.InputAt(frame, left, 0);
        if ((mode & 3) == 0) return 0;
        return (ushort)(0x8000 | ((mode & 3) == 3 ? 0 : (mode & 3) == 1 ?
            (left ? 0x200 : 0x100) : (left ? 0x100 : 0x200)));
    }

    public static void Begin(ISnesAddressSpace bus, SuperMetroidRuntime runtime, SamusState samus, int mode)
    {
        if (samus.Shinespark.Phase != ShinesparkPhase.Windup)
            throw new InvalidDataException("Suit collection must interrupt controller-earned windup.");
        samus.EquippedItems |= (ushort)(mode >= 4 ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.VariaSuit);
        samus.CollectedItems = samus.EquippedItems;
        samus.SelectedHudItem = 5;
        runtime.SuitPickup.Begin(bus, samus, 0, 355,
            mode >= 4 ? SamusSuitPickupKind.Gravity : SamusSuitPickupKind.Varia);
    }

    public static void Verify(SuperMetroidRuntime runtime, SamusState samus, int frame, int mode)
    {
        if (frame == 152 && (!runtime.SuitPickup.IsActive || samus.Shinespark.StartStopTimer != 30))
            throw new InvalidDataException("Suit did not suspend windup.");
        if (frame == 322 && ((mode & 3) is 1 or 2) &&
            (!samus.Xray.IsActive || samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
             samus.HorizontalSpeed.SpeedBoostCounter != 0x0400))
            throw new InvalidDataException("Suit release did not replace windup with X-ray while retaining boost.");
    }

    public static string Snapshot(SuperMetroidRuntime runtime, SamusState samus)
    {
        var suit = runtime.SuitPickup;
        uint hash = 2166136261u;
        foreach (ushort word in suit.WindowTable)
        {
            hash = unchecked((hash ^ (byte)word) * 16777619u);
            hash = unchecked((hash ^ (byte)(word >> 8)) * 16777619u);
        }
        // X-ray subsequently reuses this shared table. The suit owns its numeric
        // window only until activation; its independent C# buffer is not that alias.
        return $"{(suit.IsActive ? 1 : 0):X4},{suit.Substate:X4},{samus.EquippedItems:X4}," +
            $"{samus.Shinespark.StartStopTimer:X4},{(samus.Xray.IsActive ? 1 : 0):X4}," +
            $"{hash:X8},{suit.LightBeamPosition:X4},{suit.LightBeamWideningSpeed:X4}," +
            $"{suit.FixedColorRed:X2},{suit.FixedColorGreen:X2},{suit.FixedColorBlue:X2}";
    }
}
