using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.World;

/// <summary>
/// Escape-sequence room effects: their setup on room entry and their per-frame step.
/// </summary>
internal sealed class EscapeRoomEffects
{
    /// <summary>Suppresses explosion generation while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Sets the native two- or four-frame explosion cadence.</summary>
    private readonly FrameClock _clock;
    /// <summary>Provides the active room callback, camera and level.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies the cartridge random sequence and Zebes timebomb event.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds room, enemy, freeze, clock, and system owners used by escape callbacks.</summary>
    internal EscapeRoomEffects(GameplayTimeFreeze freeze, FrameClock clock, ActiveRoomState room, SharedGameState system)
    {
        _freeze = freeze;
        _clock = clock;
        _room = room;
        _system = system;
    }
    /// <summary>Initializes the selected room's quake and clears diagonal countdowns only for the two native switchers.</summary>
    internal void SetupEscapeRoomEffects(RoomSetupCallback setup, RoomEnemySystem enemies, EscapeDiagonalQuakeState diagonalQuake)
    {
        // $8F:C93C/$C964 clear the diagonal countdown for the two rooms whose mains switch quake types; the other
        // setups leave the countdown the previous room's main left.
        if (setup is RoomSetupCallback.SetLightHorizontalRoomShaking or
            RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking)
            diagonalQuake.Countdown = WrappingCountdown.Start(0);
        int? type = setup switch
        {
            RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking or
            RoomSetupCallback.SetLightHorizontalRoomShaking => ZebesEscapeDefinitions.LightHorizontal,
            RoomSetupCallback.SetMediumHorizontalRoomShaking or
            RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking => ZebesEscapeDefinitions.MediumHorizontal,
            RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen => ZebesEscapeDefinitions.MainstreetQuake,
            RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape => ZebesEscapeDefinitions.LandingQuake,
            RoomSetupCallback.None or RoomSetupCallback.AutoDestroyWallDuringEscape or
                RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape or
                RoomSetupCallback.ReturnAfterEscapeWallSetup or RoomSetupCallback.ReturnBeforeEscapeSkySetup or
                RoomSetupCallback.ScrollingSkyLand or RoomSetupCallback.ScrollingSkyOcean or
                RoomSetupCallback.Return or RoomSetupCallback.ReturnAfterOceanSkySetup or
                RoomSetupCallback.ReturnBeforeStatueSetupA or RoomSetupCallback.ReturnBeforeStatueSetupB or
                RoomSetupCallback.RunStatueUnlockingAnimations or RoomSetupCallback.SharedReturn or
                RoomSetupCallback.SharedReturnB or RoomSetupCallback.SharedReturnC or
                RoomSetupCallback.SharedReturnD or RoomSetupCallback.OrdinaryReturn or
                RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile or RoomSetupCallback.BossRoomReturn or
                RoomSetupCallback.BossRoomReturnB or RoomSetupCallback.BossRoomReturnC or
                RoomSetupCallback.SetupShaktoolRoomPlm or RoomSetupCallback.ReturnBeforeDraygonSetup or
                RoomSetupCallback.SetPausingCodeForDraygon or RoomSetupCallback.SetCollectedMap or
                RoomSetupCallback.ReturnBeforeZebesTimebombSetup or
                RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze or RoomSetupCallback.SpawnCeresHaze or
                RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze => null,
            _ => throw new InvalidOperationException($"Undefined RoomSetupCallback {setup}."),
        };
        if (type is { } quake)
        {
            enemies.Earthquake.EarthquakeType = quake;
            enemies.Earthquake.EarthquakeTimer = WrappingCountdown.Start(ushort.MaxValue);
        }
        if (setup == RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking)
            _system.SetEvent(SuperMetroid.Core.Progression.EventNumber.ZebesTimebombSet);
    }

    /// <summary>Dispatches the escape room-main callbacks at the native post-draw boundary.</summary>
    internal void StepEscapeRoomEffects(RoomScope room)
    {
        RoomEnemySystem enemies = room.Enemies;
        EscapeDiagonalQuakeState diagonalQuake = room.EscapeDiagonalQuake;
        if (_room.ActiveRoom is null || _room.Camera is null || _room.LevelData is null) return;
        RoomMainCallback main = _room.ActiveRoom.State.MainCallback;
        bool nonblank = main is RoomMainCallback.ScrollingSkyLandZebesTimebombSet or
            RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions;
        bool light = main == RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal;
        bool medium = main == RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal;
        if (!nonblank && !light && !medium && main != RoomMainCallback.GenerateRandomExplosionEveryFourthFrame)
            return;
        if (light || medium)
        {
            if (!diagonalQuake.Countdown.IsZero)
            {
                diagonalQuake.Countdown = diagonalQuake.Countdown.Decrement().Remaining;
                if (diagonalQuake.Countdown.IsZero)
                    enemies.Earthquake.EarthquakeType = light ? ZebesEscapeDefinitions.LightHorizontal : ZebesEscapeDefinitions.MediumHorizontal;
            }
            else if (_system.NextRandom() < (light ? ZebesEscapeDefinitions.LightChance : ZebesEscapeDefinitions.MediumChance))
            {
                diagonalQuake.Countdown = WrappingCountdown.Start(ZebesEscapeDefinitions.DiagonalFrames);
                enemies.Earthquake.EarthquakeType = light ? ZebesEscapeDefinitions.MediumDiagonal : ZebesEscapeDefinitions.StrongDiagonal;
            }
        }
        if (!_freeze.TimeIsFrozen && (_clock.Counter & (nonblank ? 1 : 3)) == 0)
        {
            int random = _system.NextRandom();
            Position16<RoomPixels> explosion = new(
                CoordinateAxis.Wrap(_room.Camera.PixelPosition.X + SharedRandomState.LowerByte(random)),
                CoordinateAxis.Wrap(_room.Camera.PixelPosition.Y + SharedRandomState.UpperByte(random)));
            int blockIndex = _room.LevelData.Size.RowMajorIndex(explosion.ToRoomBlock());
            if (!nonblank || _room.LevelData.GetPlmCollisionBlockByIndex(blockIndex).Terrain.Appearance.Block != ZebesEscapeDefinitions.BlankTile)
                enemies.PlmSpawns.SpawnEscapeExplosion(explosion, _system.NextRandom(), nonblank ? CoordinateAxis.Wrap(blockIndex * 2) : 0);
        }
        if (nonblank) enemies.Earthquake.KeepShaking();
    }
}
