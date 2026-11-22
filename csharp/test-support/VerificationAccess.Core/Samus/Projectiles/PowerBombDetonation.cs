using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Sets off a real power bomb through gameplay (#2602): Samus, curled into a morph ball with power bombs selected, lays
/// one with Shoot's press, and the game runs until its fuse runs out and its explosion starts.
/// </summary>
internal static class PowerBombDetonation
{
    /// <summary>
    /// Lays a power bomb in <paramref name="runtime"/>'s room and runs updates through <paramref name="step"/>, the first
    /// pressing Shoot, until the room's power bomb explosion is under way.
    /// </summary>
    /// <param name="runtime">The runtime whose Samus lays the bomb.</param>
    /// <param name="step">Runs one update with the controller buttons held.</param>
    /// <exception cref="InvalidOperationException">The bomb's fuse ran out without its explosion starting.</exception>
    internal static void ExplodeThroughGameplay(SuperMetroidRuntime runtime, Action<ControllerButton> step)
    {
        Ensure.NotNull(runtime);
        Ensure.NotNull(step);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.ChangePoseKeepingAnimation(SamusPoseId.MorphBallGroundRightPose);
        samus.EquippedItems |= SamusEquipmentFlags.MorphBall;
        samus.MaxPowerBombs = samus.PowerBombs = 5;
        samus.SelectedHudItem = HudItem.PowerBomb;
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPowerBombExplosionState explosion = runtime.Room.LoadedScope.BombProjectiles.PowerBombExplosion;
        step(ControllerButton.X);
        // The bomb's fuse lasts its timer's updates; the explosion starts as it runs out.
        for (int update = 0; !explosion.IsActive; update++)
        {
            if (update > MorphBallBombDefinitions.InitialBombTimer)
                throw new InvalidOperationException("The laid power bomb's fuse ran out without its explosion starting.");
            step(0);
        }
    }
}
