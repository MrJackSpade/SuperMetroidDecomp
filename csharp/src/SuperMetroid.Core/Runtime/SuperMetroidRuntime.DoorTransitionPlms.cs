using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>
    /// Executes the explicit <c>PLM_Handler</c> call made by the door coroutine at
    /// <c>$82:E53C</c>, after its final scrolling IRQ and before the following NMI.
    /// </summary>
    /// <remarks>
    /// The ordinary frame dispatcher intentionally suppresses PLMs while the shared
    /// door/elevator freeze flag is set. This call is the cartridge's one exception: it
    /// advances freshly loaded room setup lists once before the destination fades in.
    /// </remarks>
    internal void RunDoorTransitionPlmHandler()
    {
        // The explicit transition-time call still enters the common bank-$84 handler,
        // whose first instruction returns while native PLM word `$1C23` is disabled.
        // Indirect G-Mode deliberately carries that disabled word across the door.
        if (Samus?.Xray.ArePlmsSuspended == true)
            return;

        RunPlmHandlerCore(controllerNewInput: 0);

        // Setup-time PLMs do not have Samus contact/input available. If a future
        // translation publishes a player-facing event here, fail at the exact ownership
        // boundary instead of silently discarding it.
        if (Plms.CollectiblePickupEvents.Count != 0 || Plms.StationActivationEvents.Count != 0)
        {
            throw new InvalidDataException(
                "A door-transition PLM handler unexpectedly published a player interaction.");
        }
        foreach (MotherBrainGlassProjectileRequest request in Plms.MotherBrainGlassProjectileRequests)
            Enemies.SpawnMotherBrainGlassProjectile(request);
        foreach (BombTorizoStatueProjectileRequest request in Plms.BombTorizoStatueProjectileRequests)
            Enemies.SpawnBombTorizoStatueBreakingProjectile(request);
    }

    /// <summary>
    /// Runs the shared bank-$84 handler body used by ordinary gameplay and the explicit
    /// transition-time call. Player-facing events remain with each caller because the
    /// transition path cannot legitimately collect an item or activate a station.
    /// </summary>
    private void RunPlmHandlerCore(ushort controllerNewInput)
    {
        if (LevelData is null || BackgroundStreamer is null || Camera is null || Samus is null)
        {
            throw new InvalidOperationException(
                "The PLM handler requires an active room, camera, and Samus.");
        }

        ApplyPendingBotwoonWallPlm();
        ApplyPendingSporeSpawnCeilingPlm();
        ApplyPendingCrocomireArenaPlms();
        ApplyPendingKraidPlms();
        ApplyPendingMotherBrainPlms();
        ApplyPendingShitroidWallPlms();
        ApplyPendingChozoStatuePlms();

        IReadOnlyList<PlmTilemapUpdate> updates = Plms.Step(
            _addressSpace,
            LevelData,
            BackgroundStreamer,
            Camera.XPosition,
            Camera.YPosition,
            BackgroundScroll.Bg1XOffset,
            Camera.Scrolls,
            Enemies.EnemiesKilled,
            Enemies.DeathQuota,
            controllerNewInput,
            Samus.CollectedItems,
            powerBombExplosionStatus: BombProjectiles.PowerBombExplosion.Status);
        foreach (PlmTilemapUpdate update in updates)
            update.ExecuteTo(Vram);
        ApplyPendingDownwardGateProjectileRequests();

        foreach (PlmVramWriteRequest request in Plms.VramWriteRequests)
        {
            VramWrites.Enqueue(
                request.SizeInBytes,
                request.SourceAddress,
                request.EncodedVramDestination);
        }
    }
}
