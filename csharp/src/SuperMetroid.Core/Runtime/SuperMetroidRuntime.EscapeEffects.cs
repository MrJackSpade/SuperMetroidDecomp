using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    private ushort _escapeDiagonalFrames;

    private void SetupEscapeRoomEffects(RoomSetupCallback setup)
    {
        _escapeDiagonalFrames = 0;
        ushort? type = setup switch
        {
            RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking or
            RoomSetupCallback.SetLightHorizontalRoomShaking => ZebesEscapeRomData.LightHorizontal,
            RoomSetupCallback.SetMediumHorizontalRoomShaking or
            RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking => ZebesEscapeRomData.MediumHorizontal,
            RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen => ZebesEscapeRomData.MainstreetQuake,
            RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape => ZebesEscapeRomData.LandingQuake,
            _ => null,
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
            if (_escapeDiagonalFrames != 0)
            {
                if (--_escapeDiagonalFrames == 0)
                    Enemies.EarthquakeType = light ? ZebesEscapeRomData.LightHorizontal : ZebesEscapeRomData.MediumHorizontal;
            }
            else if (System.NextRandom() < (light ? ZebesEscapeRomData.LightChance : ZebesEscapeRomData.MediumChance))
            {
                _escapeDiagonalFrames = ZebesEscapeRomData.DiagonalFrames;
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
