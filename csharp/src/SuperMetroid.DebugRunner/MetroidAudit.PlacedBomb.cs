using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static partial class MetroidAudit
{
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
