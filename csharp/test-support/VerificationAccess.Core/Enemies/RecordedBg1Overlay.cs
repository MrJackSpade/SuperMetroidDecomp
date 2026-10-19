namespace SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Enemy services that record the words an actor writes over BG1 (#1346), for the Ceres escape's warning text.</summary>
internal sealed class RecordedBg1Overlay
{
    /// <summary>Every word written so far, at its BG1 tilemap word.</summary>
    public RoomTilemapOverlay Overlay { get; private set; } = RoomTilemapOverlay.Empty;

    /// <summary>Standalone services whose BG1 overlay writes land in <see cref="Overlay"/>; they generate no random numbers.</summary>
    public StandaloneRoomEnemyServices Services() =>
        new(() => throw new InvalidOperationException("The BG1 overlay fixture generates no random numbers."))
        {
            DrawBg1OverlayWords = (firstWord, words) => Overlay = Overlay.With(firstWord, words),
        };
}
