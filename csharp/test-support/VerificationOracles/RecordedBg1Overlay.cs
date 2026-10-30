namespace SuperMetroid.TestSupport.VerificationOracles;

using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Verification.Enemies;

/// <summary>
/// Enemy services that record the pictures an actor writes over BG1 (#1346) and BG2 (#1348), for the Ceres escape's
/// warning text and the escape typewriters (#1466).
/// </summary>
internal sealed class RecordedBg1Overlay
{
    /// <summary>Every picture written over BG1 so far, at its BG1 tilemap cell.</summary>
    public RoomWrittenOverlay Overlay { get; private set; } = RoomWrittenOverlay.Empty;

    /// <summary>Every picture written over BG2 so far, at its BG2 tilemap cell.</summary>
    public RoomWrittenOverlay Bg2Overlay { get; private set; } = RoomWrittenOverlay.Empty;

    /// <summary>Standalone services whose overlay writes land in <see cref="Overlay"/> and <see cref="Bg2Overlay"/>; they generate no random numbers.</summary>
    public StandaloneRoomEnemyServices Services() =>
        new(() => throw new InvalidOperationException("The overlay fixture generates no random numbers."))
        {
            WriteBg1Pictures = picture => Overlay = Overlay.With(picture),
            WriteBg2Pictures = picture => Bg2Overlay = Bg2Overlay.With(picture),
        };
}
