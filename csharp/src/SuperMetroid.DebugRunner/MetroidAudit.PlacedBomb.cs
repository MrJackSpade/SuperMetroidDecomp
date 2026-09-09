using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class MetroidAudit
{
    private static void VerifyNativeBombExplosionRadii(
        SuperMetroidAddressSpace bus, CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        var samus = Load(bus, room, assets).Samus;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.Bombs;
        samus.XPosition = 128;
        samus.YPosition = 153;
        samus.RefreshCollisionRadii(bus);
        var bombs = new SamusBombProjectileSystem();
        // Literal native CPU observations, not calculated from the ROM by this oracle.
        ushort[] radii = [8, 8, 12, 12, 16, 16, 16, 16, 16, 16, 0];
        ushort[] lists = [0xA073, 0xA073, 0xA07B, 0xA07B, 0xA083, 0xA083,
            0xA08B, 0xA08B, 0xA093, 0xA093, 0];
        for (int frame = 0; frame <= 69; frame++)
        {
            ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
            bombs.StepFrame(bus, assets.LevelData, samus, input, input);
            if (frame < 59) continue;
            var bomb = bombs.Slots[0];
            int index = frame - 59;
            if (bomb.XRadius != radii[index] || bomb.YRadius != radii[index] ||
                bomb.InstructionPointer != lists[index] || bomb.BombTimer != 0 ||
                bomb.Type != (frame < 69 ? 0x0501 : 0))
                throw new InvalidDataException($"Native bomb explosion mismatch at update {frame}: radius={bomb.XRadius}/{bomb.YRadius}, list={bomb.InstructionPointer:X4}, type={bomb.Type:X4}.");
        }
        Console.WriteLine("Normal bomb explosion matches native CPU radius/list/type observations for updates 59–69.");
    }

    private static void VerifyStationaryPlacedBomb(
        SuperMetroidAddressSpace bus, CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        LoadedMetroids loaded = Load(bus, room, assets);
        var actor = loaded.Enemies.Slots[0];
        var state = RequireState(loaded.Enemies, actor);
        var samus = loaded.Samus;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.Bombs;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = actor.XPosition;
        samus.YPosition = unchecked((ushort)(actor.YPosition + 8));
        StepCentered(loaded.Enemies, assets, room, samus, actor);
        loaded.Enemies.ResolveOrdinarySamusContact(samus, 0);
        if (state.Function != MetroidAiFunction.AttachedToSamus)
            throw new InvalidDataException("Placed-bomb fixture did not establish attachment through contact.");

        var bombs = new SamusBombProjectileSystem();
        var projectiles = new SamusProjectileSystem();
        int firstHit = -1;
        int firstReattach = -1;
        for (int frame = 0; frame < 100; frame++)
        {
            // Keep Samus stationary deliberately; this isolates real placement/fuse
            // and enemy attachment from movement, not a complete gameplay replay.
            StepCentered(loaded.Enemies, assets, room, samus, actor);
            ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
            bombs.StepFrame(bus, assets.LevelData, samus, input, input);
            if (frame == 0 && bombs.BombCounter != 1)
                throw new InvalidDataException("Normal Shoot did not place one bomb.");
            int hits = loaded.Enemies.ResolveOrdinaryBombHits(bombs, projectiles, samus);
            if (hits != 0 && firstHit < 0)
            {
                firstHit = frame;
                if (state.Function != MetroidAiFunction.PowerBombEscape || samus.SpecialSuperPaletteFlags != 0)
                    throw new InvalidDataException("Placed bomb collided but did not detach the Metroid.");
            }
            if (firstHit >= 0 && state.Function == MetroidAiFunction.AttachedToSamus && firstReattach < 0)
                firstReattach = frame;
        }
        if (firstHit < 0)
            throw new InvalidDataException("Stationary normally placed bomb never hit the attached Metroid through its full fuse/explosion.");
        Console.WriteLine($"Stationary placed bomb: first detach frame={firstHit}; first reattachment frame={firstReattach} (100-frame isolated observation).");
    }
}
