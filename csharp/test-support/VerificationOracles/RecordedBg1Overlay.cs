namespace SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Verification.Enemies;

/// <summary>
/// Enemy services that record the words an actor writes over BG1 (#1346) and BG2 (#1348), for the Ceres escape's
/// warning text and the escape typewriters.
/// </summary>
internal sealed class RecordedBg1Overlay
{
    /// <summary>Every word written over BG1 so far, at its BG1 tilemap word.</summary>
    public RoomTilemapOverlay Overlay { get; private set; } = RoomTilemapOverlay.Empty;

    /// <summary>Every word written over BG2 so far, at its BG2 tilemap word.</summary>
    public RoomTilemapOverlay Bg2Overlay { get; private set; } = RoomTilemapOverlay.Empty;

    /// <summary>Standalone services whose overlay writes land in <see cref="Overlay"/> and <see cref="Bg2Overlay"/>; they generate no random numbers.</summary>
    public StandaloneRoomEnemyServices Services() =>
        new(() => throw new InvalidOperationException("The overlay fixture generates no random numbers."))
        {
            DrawBg1OverlayWords = (firstWord, words) => Overlay = Overlay.With(firstWord, words),
            DrawBg2OverlayWords = (firstWord, words) => Bg2Overlay = Bg2Overlay.With(firstWord, words),
        };
}
