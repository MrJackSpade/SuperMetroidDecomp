using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Varia PLM/message integration with actual Power Bomb cleanup and bomb fuse.</summary>
internal static class FlashSuitCollectibleAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        foreach (int targetFuse in new[] { 8, 9, 10, 11 }) RunCase(bus, targetFuse);
        Console.WriteLine("Varia collectible: real PLM/message/suit flow preserves a spark only inside the native bomb-fuse window.");
        return 0;
    }

    private static void RunCase(SuperMetroidAddressSpace bus, int targetFuse)
    {
        var runtime = FlatFloorMovementFixture.Create(bus, water: false);
        runtime.LoadCartridgeRoomForDebug(FlashSuitCollectibleAuditData.VariaSuitRoomHeader, 0, 0);

        // Break the cartridge-authored Chozo orb through the same public
        // projectile-contact seam used by the production collision systems. The
        // fixture publishes only that already-resolved contact; every phase change,
        // draw instruction and reveal timer remains owned by the real PLM handler.
        var varia = runtime.Plms.Collectibles.Single(item => item.Kind == InWorldCollectibleKind.VariaSuit);
        if (!runtime.Plms.TryNotifyCollectibleProjectileHit(
                varia.BlockIndex,
                (ushort)SamusProjectileFamily.Beam))
        {
            throw new InvalidDataException("Varia Chozo orb rejected beam contact.");
        }
        int revealFrames = 0;
        while (runtime.Plms.Collectibles.Single(item => item.Kind == InWorldCollectibleKind.VariaSuit).Phase !=
               CollectiblePhase.Visible)
        {
            if (++revealFrames > 120)
                throw new InvalidDataException("Varia Chozo orb did not reveal its item.");
            runtime.Plms.Step(
                bus,
                runtime.LevelData!,
                runtime.BackgroundStreamer!,
                layer1XPosition: 0,
                layer1YPosition: 0,
                bg1XOffset: 0);
        }

        runtime.InitializeDebugGroundedSamus(
            FlashSuitCollectibleAuditData.SamusX,
            FlashSuitCollectibleAuditData.SamusY,
            FlashSuitCollectibleAuditData.CameraY);
        var samus = runtime.Samus ?? throw new InvalidDataException("Missing Samus.");
        samus.Health = 49; samus.MaxHealth = 99; samus.ReserveEnergy = 0;
        samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.PowerBombs = samus.MaxPowerBombs = 11;
        samus.EquippedItems = samus.CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.SelectedHudItem = 3;
        var bombs = runtime.BombProjectiles;
        bombs.StepFrame(
            bus,
            runtime.LevelData!,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            runtime.Plms,
            deferSamusOverlap: true);
        if (!bombs.PowerBombExplosion.IsArmed || samus.PowerBombs != 10)
            throw new InvalidDataException("Power Bomb placement failed.");

        // Leave exactly enough shared projectile frames for a subsequently placed
        // ordinary bomb to reach the requested fuse when Power Bomb cleanup occurs.
        // The Power Bomb cleanup boundary is fixed. Advancing its afterglow one
        // extra frame before placement adds one ordinary-bomb fuse step at that
        // boundary, so this selects the two native matrix edge cases directly.
        int framesBeforeOrdinaryBombPlacement =
            FlashSuitCollectibleAuditData.PowerBombPlacementFrameOffset + targetFuse;
        for (int frame = 0; frame < framesBeforeOrdinaryBombPlacement; frame++)
            bombs.StepFrame(bus, runtime.LevelData!, samus, 0, 0, runtime.Plms, deferSamusOverlap: true);
        samus.SelectedHudItem = 0;
        bombs.StepFrame(
            bus,
            runtime.LevelData!,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            runtime.Plms,
            deferSamusOverlap: true);
        var ordinary = bombs.Slots.Single(slot => slot.Damage != 0 && slot.Type == (ushort)SamusProjectileFamily.Bomb);
        if (ordinary.BombTimer == 0) throw new InvalidDataException("Ordinary bomb placement failed.");

        var timerField = typeof(SamusPowerBombExplosionState).GetField("_afterglowTimer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var stepsField = typeof(SamusPowerBombExplosionState).GetField("_afterglowStepsRemaining", BindingFlags.Instance | BindingFlags.NonPublic)!;
        while (!(bombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.Afterglow &&
                 (Convert.ToInt32(timerField.GetValue(bombs.PowerBombExplosion)) == 0 || Convert.ToInt32(timerField.GetValue(bombs.PowerBombExplosion)) >= 0x8000) &&
                 Convert.ToInt32(stepsField.GetValue(bombs.PowerBombExplosion)) == 1))
            bombs.StepFrame(bus, runtime.LevelData!, samus, 0, 0, runtime.Plms, deferSamusOverlap: true);
        if (ordinary.BombTimer != targetFuse)
            throw new InvalidDataException($"Prepared fuse {ordinary.BombTimer}, expected {targetFuse}; adjust only the placement frame.");

        samus.ApplyForwardFacingPoseSetup(bus);
        samus.SelectedHudItem = 0;
        varia = runtime.Plms.Collectibles.Single(item => item.Kind == InWorldCollectibleKind.VariaSuit);
        if (!runtime.Plms.TryNotifyCollectibleTouch(varia.BlockIndex))
            throw new InvalidDataException($"Varia PLM is not visible at phase {varia.Phase}.");
        runtime.StepFrame((ushort)(SnesButton.X | SnesButton.L | SnesButton.R | SnesButton.Down));
        if (!runtime.MessageBox.IsActive || samus.CrystalFlash.Phase != CrystalFlashPhase.Raising)
            throw new InvalidDataException("Pickup frame did not run cleanup admission before the PLM message.");
        int messageFrames = 0;
        while (runtime.MessageBox.IsActive)
        {
            if (++messageFrames > 1000) throw new InvalidDataException("Varia message did not close.");
            runtime.StepFrame(runtime.MessageBox.Phase == GameplayMessageBoxPhase.AwaitingInput ? (ushort)SnesButton.X : (ushort)0);
        }
        if (!runtime.SuitPickup.IsActive)
            throw new InvalidDataException(
                $"Suit/message handoff failed: suit={runtime.SuitPickup.IsActive}, " +
                $"bomb={ordinary.BombTimer}, expected={targetFuse}, message={runtime.MessageBox.Phase}, " +
                $"pickup={runtime.Plms.LastCollectiblePickup?.Kind}.");
        int suitFrames = 0;
        while (runtime.SuitPickup.IsActive)
        {
            if (++suitFrames > 1000) throw new InvalidDataException("Varia transformation did not finish.");
            runtime.StepFrame(0);
        }
        for (int frame = 0; frame < 230; frame++) runtime.StepFrame(0);
        bool retained = samus.SharedShineTimer != 0 && samus.CrystalFlash.SpecialPaletteKind == SamusSpecialPaletteType.CrystalFlash;
        if (retained != (targetFuse >= FlashSuitCollectibleAuditData.FirstSuccessfulPrePickupFuse) ||
            samus.CrystalFlash.Phase != CrystalFlashPhase.Inactive ||
            (samus.EquippedItems & (ushort)SamusEquipmentFlags.VariaSuit) == 0)
            throw new InvalidDataException($"Varia integration fuse {targetFuse}: retained={retained}, phase={samus.CrystalFlash.Phase}, items={samus.EquippedItems:X4}.");

        // Use ordinary controller input after every asynchronous owner has returned.
        // The retained cases must launch the same vertical spark as the cartridge
        // matrix; adjacent failures must restore control without inventing one.
        runtime.StepFrame((ushort)SnesButton.A);
        for (int frame = 0; frame < 3; frame++)
            runtime.StepFrame((ushort)(SnesButton.A | SnesButton.Up));
        bool launched = samus.Shinespark.Phase == ShinesparkPhase.Vertical;
        if (launched != retained || (launched && samus.HorizontalSpeed.ContactDamageIndex != 2))
            throw new InvalidDataException(
                $"Varia integration fuse {targetFuse}: retained={retained}, " +
                $"spark={samus.Shinespark.Phase}, contact={samus.HorizontalSpeed.ContactDamageIndex}.");
        Console.WriteLine(
            $"Varia fuse {targetFuse}: message={messageFrames} frames, suit={suitFrames} frames, " +
            $"retained={retained}, launched={launched}.");
    }
}
