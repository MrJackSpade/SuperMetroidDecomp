using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    private void SetupEscapeRoomEffects(RoomSetupCallback setup)
    {
        // $8F:C93C/$C964 clear the diagonal countdown (RoomMainASMVar1) for the two rooms
        // whose mains switch quake types; the other setups leave the shared word alone.
        if (setup is RoomSetupCallback.SetLightHorizontalRoomShaking or
            RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking)
            RoomMainScratch.Var1 = 0;
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
            Enemies.EarthquakeType = quake;
            Enemies.EarthquakeTimer = ushort.MaxValue;
        }
        if (setup == RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking)
            System.SetEvent(Game.EventNumber.ZebesTimebombSet);
    }

    /// <summary>Dispatches the escape room-main callbacks at the native post-draw boundary.</summary>
    private void StepEscapeRoomEffects()
    {
        if (ActiveRoom is null || Camera is null || LevelData is null) return;
        RoomMainCallback main = ActiveRoom.State.MainCallback;
        bool nonblank = main is RoomMainCallback.ScrollingSkyLandZebesTimebombSet or
            RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions;
        bool light = main == RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal;
        bool medium = main == RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal;
        if (!nonblank && !light && !medium && main != RoomMainCallback.GenerateRandomExplosionEveryFourthFrame)
            return;
        if (light || medium)
        {
            if (RoomMainScratch.Var1 != 0)
            {
                if (--RoomMainScratch.Var1 == 0)
                    Enemies.EarthquakeType = light ? ZebesEscapeRomData.LightHorizontal : ZebesEscapeRomData.MediumHorizontal;
            }
            else if (System.NextRandom() < (light ? ZebesEscapeRomData.LightChance : ZebesEscapeRomData.MediumChance))
            {
                RoomMainScratch.Var1 = ZebesEscapeRomData.DiagonalFrames;
                Enemies.EarthquakeType = light ? ZebesEscapeRomData.MediumDiagonal : ZebesEscapeRomData.StrongDiagonal;
            }
        }
        if (!TimeIsFrozen && (NmiFrameCounter & (nonblank ? 1 : 3)) == 0)
        {
            ushort random = System.NextRandom();
            ushort x = unchecked((ushort)(Camera.XPosition + (byte)random));
            ushort y = unchecked((ushort)(Camera.YPosition + (random >> 8)));
            int blockIndex = (y >> 4) * LevelData.WidthInBlocks + (x >> 4);
            if (!nonblank || new RoomLevelWord(LevelData.GetPlmCollisionBlockByIndex(blockIndex).LevelWord).VisualBlockIndex != ZebesEscapeRomData.BlankTile)
                Enemies.SpawnEscapeExplosion(x, y, System.NextRandom(), nonblank ? unchecked((ushort)(blockIndex * 2)) : (ushort)0);
        }
        if (nonblank) Enemies.EarthquakeTimer |= ZebesEscapeRomData.ContinuousTimerBit;
    }
}
