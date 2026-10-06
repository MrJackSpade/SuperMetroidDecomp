using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Enemy and draw owners of the door sound-drain coroutine, without Samus movement.</summary>
    public void RunDoorSoundWaitFrame(ushort controllerInput)
    {
        Plms.BindPowerBombAudio(BombProjectiles.PowerBombExplosion);
        RunNmi(controllerInput, mainLoopRequestedNmi: true);
        // Both state-$09 entry and each $E29E sound-drain dispatch return to
        // MainGameLoop. Its HDMA/RNG prologue still runs while Samus is locked.
        AdvanceNonGameplayMainLoopRandom(hdmaObjectsEnabled: true);
        DrawDoorTransitionActors(runEnemyProjectiles: false);
    }

    /// <summary>Native enemy/instruction and draw pass shared by source-door waits and fading.</summary>
    /// <param name="runEnemyProjectiles">
    /// True for the destination fade-in ($82:E743); the source waits ($E29E/$E2DB) run
    /// enemies and drawing without Enemy_Projectile_Handler.
    /// </param>
    internal void DrawDoorTransitionActors(bool runEnemyProjectiles)
    {
        Oam.BeginFrame();
        LastSamusBodyDrawn = false;
        LastShinesparkCrashDrawingHandlerActive = false;
        LastGrappleDrawingHandlerActive = false;
        LastGrappleBeamSpecificDrawingPath = false;
        LastGrappleFlareDrawn = false;
        // The draw handler can append spin-stop/charge sounds. Do not publish the
        // prior gameplay frame's liquid, movement, projectile or PLM requests again.
        Samus?.LiquidPhysics.BeginFrameSoundRequests(BombProjectiles.PowerBombExplosion);
        RunEnemyMainPhase();
        // The Samus-collision pass belongs to gameplay only.
        if (runEnemyProjectiles)
            RunEnemyProjectileHandler();
        DrawGameplayActors(deathOwnsSamus: false, advanceSamusPalette: false);
        // EnsureSamusDrawnEachFrame bypasses ordinary hurt flicker after the shared
        // draw. Elevator-owned drawing retains its separate alternating cadence.
        if (ElevatorStatus == 0 && Samus is not null && Camera is not null)
            LastSamusBodyDrawn = Samus.Draw(_addressSpace, Oam,
                Camera.XPosition, Camera.YPosition, mode7Transform: ActiveSamusMode7Transform);
        Oam.FinalizeFrame();
    }
    /// <summary>
    /// <c>Enemy_Projectile_Handler</c> ($86:8104): runs every projectile unless
    /// <c>EnemyProjectile_Enable</c> is clear (time freeze or X-ray suspension).
    /// </summary>
    private void RunEnemyProjectileHandler()
    {
        if (TimeIsFrozen || Samus?.Xray.AreEnemyProjectilesSuspended == true || LevelData is null || Camera is null)
            return;
        Enemies.StepEnemyProjectileInstructions(
            LevelData, Samus, Camera.XPosition, Camera.YPosition,
            NmiFrameCounter8, BombProjectiles);
    }

    /// <summary>Shared HDMA/RNG prologue of an outer dispatch without a gameplay frame.</summary>
    internal void AdvanceNonGameplayMainLoopRandom(bool hdmaObjectsEnabled)
    {
        if (hdmaObjectsEnabled)
        {
            RoomLayer3Fx.AdvanceHdmaSharedState(System, TimeIsFrozen);
            if (Samus is not null && RoomLayer3Fx.Type is RoomFxType.Lava or RoomFxType.Acid)
                RoomLayer3Fx.ApplyToSamusLiquidPhysics(Samus.LiquidPhysics);
        }
        System.NextRandom();
    }
}
