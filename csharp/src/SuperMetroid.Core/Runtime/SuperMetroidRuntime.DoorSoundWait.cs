using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    /// <summary>Enemy and draw owners of the door sound-drain coroutine, without Samus movement.</summary>
    public void RunDoorSoundWaitFrame(ushort controllerInput)
    {
        Plms.BindPowerBombAudio(BombProjectiles.PowerBombExplosion);
        RunNmi(controllerInput, mainLoopRequestedNmi: true);
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
        DrawGameplayActors(deathOwnsSamus: false, advanceSamusPalette: false);
        // EnsureSamusDrawnEachFrame bypasses ordinary hurt flicker after the shared
        // draw. Elevator-owned drawing retains its separate alternating cadence.
        if (ElevatorStatus == 0 && Samus is not null && Camera is not null)
            LastSamusBodyDrawn = Samus.Draw(_addressSpace, Oam,
                Camera.XPosition, Camera.YPosition, mode7Transform: ActiveSamusMode7Transform);
        Oam.FinalizeFrame();
    }
}
