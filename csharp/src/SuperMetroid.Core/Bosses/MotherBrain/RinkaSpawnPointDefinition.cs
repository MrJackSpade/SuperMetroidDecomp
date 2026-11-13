namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of Mother Brain's Rinka spawn points and where it lies.</summary>
/// <param name="Point">The spawn point.</param>
/// <param name="Centre">
/// Authored spawn centre in whole room pixels (not a sprite or camera-relative coordinate): where a Rinka placed at the
/// point appears, and what the reservation tests against the current camera.
/// </param>
internal readonly record struct RinkaSpawnPointDefinition(
    RinkaSpawnPoint Point,
    Position16<RoomPixels> Centre);
