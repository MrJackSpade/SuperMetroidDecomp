using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.World;

/// <summary>
/// Escape-sequence room effects: their setup on room entry and their per-frame step.
/// </summary>
internal sealed class EscapeRoomEffects
{
    /// <summary>Suppresses explosion generation while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Paces the explosions on the escape's two- or four-update cadence.</summary>
    private readonly FrameClock _clock;
    /// <summary>Provides the active room behaviour, camera and level.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies the game's random sequence and Zebes timebomb event.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds room, enemy, freeze, clock, and system owners used by escape behaviours.</summary>
    internal EscapeRoomEffects(GameplayTimeFreeze freeze, FrameClock clock, ActiveRoomState room, SharedGameState system)
    {
        _freeze = freeze;
        _clock = clock;
        _room = room;
        _system = system;
    }
    /// <summary>Initializes the selected room's quake and clears diagonal countdowns only for the two switchers.</summary>
    internal void SetupEscapeRoomEffects(RoomSetupBehavior setup, RoomEnemySystem enemies, EscapeDiagonalQuakeState diagonalQuake)
    {
        // The two rooms whose mains switch quake types clear the diagonal countdown in their setup; the other
        // setups leave the countdown the previous room's main left.
        if (setup is RoomSetupBehavior.SetLightHorizontalRoomShaking or
            RoomSetupBehavior.SetupEscapeRoom4ObjectAndMediumHorizontalShaking)
            diagonalQuake.Countdown = WrappingCountdown.Start(0);
        EarthquakeShake? shake = setup switch
        {
            RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking or
            RoomSetupBehavior.SetLightHorizontalRoomShaking => ZebesEscapeDefinitions.LightHorizontal,
            RoomSetupBehavior.SetMediumHorizontalRoomShaking or
            RoomSetupBehavior.SetupEscapeRoom4ObjectAndMediumHorizontalShaking => ZebesEscapeDefinitions.MediumHorizontal,
            RoomSetupBehavior.ClearBlocksAfterSavingAnimalsAndShakeScreen => ZebesEscapeDefinitions.MainstreetQuake,
            RoomSetupBehavior.ShakeScreenAndCallScrollingSkyLandDuringEscape => ZebesEscapeDefinitions.LandingQuake,
            RoomSetupBehavior.None or RoomSetupBehavior.AutoDestroyWallDuringEscape or
                RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape or
                RoomSetupBehavior.ReturnAfterEscapeWallSetup or RoomSetupBehavior.ReturnBeforeEscapeSkySetup or
                RoomSetupBehavior.ScrollingSkyLand or RoomSetupBehavior.ScrollingSkyOcean or
                RoomSetupBehavior.Return or RoomSetupBehavior.ReturnAfterOceanSkySetup or
                RoomSetupBehavior.ReturnBeforeStatueSetupA or RoomSetupBehavior.ReturnBeforeStatueSetupB or
                RoomSetupBehavior.RunStatueUnlockingAnimations or RoomSetupBehavior.SharedReturn or
                RoomSetupBehavior.SharedReturnB or RoomSetupBehavior.SharedReturnC or
                RoomSetupBehavior.SharedReturnD or RoomSetupBehavior.OrdinaryReturn or
                RoomSetupBehavior.SpawnPrePhantoonRoomEnemyProjectile or RoomSetupBehavior.BossRoomReturn or
                RoomSetupBehavior.BossRoomReturnB or RoomSetupBehavior.BossRoomReturnC or
                RoomSetupBehavior.SetupShaktoolRoomObject or RoomSetupBehavior.ReturnBeforeDraygonSetup or
                RoomSetupBehavior.SetPausingCodeForDraygon or RoomSetupBehavior.SetCollectedMap or
                RoomSetupBehavior.ReturnBeforeZebesTimebombSetup or
                RoomSetupBehavior.TurnCeresDoorToSolidBlocksAndSpawnHaze or RoomSetupBehavior.SpawnCeresHaze or
                RoomSetupBehavior.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze => null,
            _ => throw new InvalidOperationException($"Undefined RoomSetupBehavior {setup}."),
        };
        if (shake is { } quake)
            enemies.Earthquake.Start(quake, WrappingCountdown.LongestStart);
        if (setup == RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking)
            _system.SetEvent(SuperMetroid.Core.Progression.StoryEvent.ZebesTimebombSet);
    }

    /// <summary>Dispatches the escape room-main behaviours at the post-draw boundary.</summary>
    internal void StepEscapeRoomEffects(RoomScope room)
    {
        RoomEnemySystem enemies = room.Enemies;
        EscapeDiagonalQuakeState diagonalQuake = room.EscapeDiagonalQuake;
        if (_room.ActiveRoom is null || _room.Camera is null || _room.LevelData is null) return;
        RoomMainBehavior main = _room.ActiveRoom.State.MainBehavior;
        bool nonblank = main is RoomMainBehavior.ScrollingSkyLandZebesTimebombSet or
            RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions;
        bool light = main == RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal;
        bool medium = main == RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal;
        if (!ZebesEscapeDefinitions.ExplosionRoomMains.Contains(main))
            return;
        if (light || medium)
        {
            if (!diagonalQuake.Countdown.IsZero)
            {
                diagonalQuake.Countdown = diagonalQuake.Countdown.Decrement().Remaining;
                if (diagonalQuake.Countdown.IsZero)
                    enemies.Earthquake.ChangeShake(light ? ZebesEscapeDefinitions.LightHorizontal : ZebesEscapeDefinitions.MediumHorizontal);
            }
            else if ((light ? ZebesEscapeDefinitions.LightShakeTurnsDiagonal : ZebesEscapeDefinitions.MediumShakeTurnsDiagonal)
                .IsTakenBy(_system.NextRandom()))
            {
                diagonalQuake.Countdown = WrappingCountdown.Start(ZebesEscapeDefinitions.DiagonalFrames);
                enemies.Earthquake.ChangeShake(light ? ZebesEscapeDefinitions.MediumDiagonal : ZebesEscapeDefinitions.StrongDiagonal);
            }
        }
        UpdateCadence explosionCadence = nonblank
            ? ZebesEscapeDefinitions.OutdoorExplosionEverySecondUpdate
            : ZebesEscapeDefinitions.IndoorExplosionEveryFourthUpdate;
        if (!_freeze.TimeIsFrozen && explosionCadence.IsDue(_clock.Counter))
        {
            int random = _system.NextRandom();
            Position16<RoomPixels> explosion = new(
                CoordinateAxis.Wrap(_room.Camera.PixelPosition.X + ZebesEscapeDefinitions.ExplosionScreenX.Of(random)),
                CoordinateAxis.Wrap(_room.Camera.PixelPosition.Y + ZebesEscapeDefinitions.ExplosionScreenY.Of(random)));
            Position16<RoomBlocks> block = explosion.ToRoomBlock();
            // Outdoors an explosion goes off only over scenery, and one whose random choice leaves its kind open takes the kind
            // its block's column selects, the four columns of every four repeating it.
            if (!nonblank || !_room.LevelData.GetObjectCollisionBlock(block).Terrain.Appearance.ShowsNothing)
                enemies.RoomObjectSpawns.SpawnEscapeExplosion(explosion, _system.NextRandom(), nonblank ? 2 * (block.X % 4) : 0);
        }
        if (nonblank) enemies.Earthquake.KeepShaking();
    }
}
