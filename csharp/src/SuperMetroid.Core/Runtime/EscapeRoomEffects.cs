using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Escape-sequence room effects: their setup on room entry and their per-frame step.
/// </summary>
internal sealed class EscapeRoomEffects
{
    private readonly RoomEnemySystem _enemies;
    private readonly GameplayTimeFreeze _freeze;
    private readonly NmiClock _nmi;
    private readonly ActiveRoomState _room;
    private readonly Bank80SystemState _system;

    internal EscapeRoomEffects(RoomEnemySystem enemies, GameplayTimeFreeze freeze, NmiClock nmi, ActiveRoomState room, Bank80SystemState system)
    {
        _enemies = enemies;
        _freeze = freeze;
        _nmi = nmi;
        _room = room;
        _system = system;
    }
    internal void SetupEscapeRoomEffects(RoomSetupCallback setup)
    {
        // $8F:C93C/$C964 clear the diagonal countdown (RoomMainASMVar1) for the two rooms
        // whose mains switch quake types; the other setups leave the shared word alone.
        if (setup is RoomSetupCallback.SetLightHorizontalRoomShaking or
            RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking)
            _room.RoomMainScratch.Var1 = 0;
        ushort? type = setup switch
        {
            RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking or
            RoomSetupCallback.SetLightHorizontalRoomShaking => ZebesEscapeRomData.LightHorizontal,
            RoomSetupCallback.SetMediumHorizontalRoomShaking or
            RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking => ZebesEscapeRomData.MediumHorizontal,
            RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen => ZebesEscapeRomData.MainstreetQuake,
            RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape => ZebesEscapeRomData.LandingQuake,
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
                RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze => null,
            _ => throw new InvalidOperationException($"Undefined RoomSetupCallback {setup}."),
        };
        if (type is { } quake)
        {
            _enemies.Earthquake.EarthquakeType = quake;
            _enemies.Earthquake.EarthquakeTimer = ushort.MaxValue;
        }
        if (setup == RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking)
            _system.SetEvent(Game.EventNumber.ZebesTimebombSet);
    }

    /// <summary>Dispatches the escape room-main callbacks at the native post-draw boundary.</summary>
    internal void StepEscapeRoomEffects()
    {
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
            if (_room.RoomMainScratch.Var1 != 0)
            {
                if (--_room.RoomMainScratch.Var1 == 0)
                    _enemies.Earthquake.EarthquakeType = light ? ZebesEscapeRomData.LightHorizontal : ZebesEscapeRomData.MediumHorizontal;
            }
            else if (_system.NextRandom() < (light ? ZebesEscapeRomData.LightChance : ZebesEscapeRomData.MediumChance))
            {
                _room.RoomMainScratch.Var1 = ZebesEscapeRomData.DiagonalFrames;
                _enemies.Earthquake.EarthquakeType = light ? ZebesEscapeRomData.MediumDiagonal : ZebesEscapeRomData.StrongDiagonal;
            }
        }
        if (!_freeze.TimeIsFrozen && (_nmi.NmiFrameCounter & (nonblank ? 1 : 3)) == 0)
        {
            ushort random = _system.NextRandom();
            Position16<RoomPixels> explosion = new(
                unchecked((ushort)(_room.Camera.PixelPosition.X + (byte)random)),
                unchecked((ushort)(_room.Camera.PixelPosition.Y + (SnesLittleEndian.HighByte(random)))));
            int blockIndex = _room.LevelData.Size.RowMajorIndex(explosion.ToRoomBlock());
            if (!nonblank || new RoomLevelWord(_room.LevelData.GetPlmCollisionBlockByIndex(blockIndex).LevelWord).VisualBlockIndex != ZebesEscapeRomData.BlankTile)
                _enemies.EscapeExplosionsBehavior.SpawnEscapeExplosion(explosion, _system.NextRandom(), nonblank ? unchecked((ushort)(blockIndex * 2)) : (ushort)0);
        }
        if (nonblank) _enemies.Earthquake.EarthquakeTimer |= ZebesEscapeRomData.ContinuousTimerBit;
    }
}
