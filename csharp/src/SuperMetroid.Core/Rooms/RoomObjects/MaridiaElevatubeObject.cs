namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The Maridia elevatube's start (#1554, <see cref="ScriptedTerrainChangeDefinitions.MaridiaElevatube"/>): it draws its
/// block, waits, and plays the elevatube sound as it leaves the room.
/// </summary>
internal sealed class MaridiaElevatubeObject() : TerrainChangeObject(ScriptedTerrainChangeDefinitions.MaridiaElevatube)
{
    /// <inheritdoc/>
    private protected override void End(RoomObjectStepContext context) =>
        context.QueueSound(EnemySound.Elevatube, SoundRequestPriority.Normal);
}
