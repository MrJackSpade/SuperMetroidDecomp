using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

public sealed partial class SuperMetroidRuntime
{
    private ushort _escapeDiagonalFrames;

    private void SetupEscapeRoomEffects(ushort setup)
    {
        _escapeDiagonalFrames = 0;
        ushort? type = setup switch
        {
            RoomSetupCodePointers.SetZebesTimebombEventAndLightHorizontalShaking or
            RoomSetupCodePointers.SetLightHorizontalRoomShaking => ZebesEscapeRomData.LightHorizontal,
            RoomSetupCodePointers.SetMediumHorizontalRoomShaking or
            RoomSetupCodePointers.SetupEscapeRoom4PlmAndMediumHorizontalShaking => ZebesEscapeRomData.MediumHorizontal,
            RoomSetupCodePointers.ClearBlocksAfterSavingAnimalsAndShakeScreen => ZebesEscapeRomData.MainstreetQuake,
            RoomSetupCodePointers.ShakeScreenAndCallScrollingSkyLandDuringEscape => ZebesEscapeRomData.LandingQuake,
            _ => null,
        };
        if (type is { } quake)
        {
            Enemies.EarthquakeType = quake;
            Enemies.EarthquakeTimer = ushort.MaxValue;
        }
        if (setup == RoomSetupCodePointers.SetZebesTimebombEventAndLightHorizontalShaking)
            System.SetEvent(Game.EventNumber.ZebesTimebombSet);
    }

    /// <summary>Dispatches the escape room-main callbacks at the native post-draw boundary.</summary>
    private void StepEscapeRoomEffects()
    {
        if (ActiveRoom is null || Camera is null || LevelData is null) return;
        ushort main = ActiveRoom.State.MainCodePointer;
        bool nonblank = main is RoomMainCodePointers.ScrollingSkyLandZebesTimebombSet or
            RoomMainCodePointers.SetScreenShakingAndGenerateRandomExplosions;
        bool light = main == RoomMainCodePointers.ShakeScreenLightHorizontalAndMediumDiagonal;
        bool medium = main == RoomMainCodePointers.ShakeScreenMediumHorizontalAndStrongDiagonal;
        if (!nonblank && !light && !medium && main != RoomMainCodePointers.GenerateRandomExplosionEveryFourthFrame)
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
