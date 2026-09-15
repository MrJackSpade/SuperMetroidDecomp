using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Advances a placed Power Bomb to the frame before actual cleanup.</summary>
internal static class FlashElevatorBombSetup
{
    public static void Prepare(ISnesAddressSpace bus, SuperMetroidRuntime runtime, SamusState samus)
    {
        var level = runtime.LevelData ?? throw new InvalidDataException("Missing elevator room.");
        samus.Health = 49; samus.MaxHealth = 99; samus.ReserveEnergy = 0;
        samus.Missiles = samus.SuperMissiles = 10;
        samus.PowerBombs = samus.MaxPowerBombs = 11;
        samus.EquippedItems = samus.CollectedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.SelectedHudItem = 3;
        var bombs = runtime.BombProjectiles;
        bombs.StepFrame(bus, level, samus, 0x40, 0x40, runtime.Plms, deferSamusOverlap: true);
        if (!bombs.PowerBombExplosion.IsArmed || samus.PowerBombs != 10)
            throw new InvalidDataException("Actual Power Bomb placement failed.");
        // Observe, never overwrite, the last native afterglow wait. The next real
        // runtime frame must execute cleanup and enemy AI in their production order.
        var timer = typeof(SamusPowerBombExplosionState).GetField("_afterglowTimer", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidDataException("Missing afterglow observation field.");
        var steps = typeof(SamusPowerBombExplosionState).GetField("_afterglowStepsRemaining", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidDataException("Missing afterglow observation field.");
        int frames = 0;
        while (!(bombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.Afterglow &&
                 (Convert.ToInt32(timer.GetValue(bombs.PowerBombExplosion)) == 0 || Convert.ToInt32(timer.GetValue(bombs.PowerBombExplosion)) >= 0x8000) &&
                 Convert.ToInt32(steps.GetValue(bombs.PowerBombExplosion)) == 1))
        {
            if (++frames > 1000) throw new InvalidDataException("Placed bomb never reached its final afterglow wait.");
            bombs.StepFrame(bus, level, samus, 0, 0, runtime.Plms, deferSamusOverlap: true);
        }
        // This is a fixed-position subsystem fixture, not a controller unmorph route.
        // Preserve the actual bomb origin while preparing the elevator's facing pose.
        samus.ApplyForwardFacingPoseSetup(bus);
        samus.SelectedHudItem = 0;
        if (samus.XPosition != bombs.PowerBombExplosion.XPosition || samus.YPosition != bombs.PowerBombExplosion.YPosition ||
            samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive)
            throw new InvalidDataException("Cleanup setup moved off the actual bomb origin or admitted Flash early.");
        Console.WriteLine($"Elevator bomb: placement spent one PB; {frames} projectile/explosion frames reached pre-cleanup.");
    }
}
