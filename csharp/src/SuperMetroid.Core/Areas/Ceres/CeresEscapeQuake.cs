using SuperMetroid.Core.Earthquakes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The quake that shakes the Ceres station while it self-destructs (#1953). Every door and the elevator room's wall keeps
/// it going: each update, the first of them to find the room still once the last shake has ended starts the next, so the
/// room's population order decides which one chooses it.
/// </summary>
/// <param name="room">Supplies the shared random sample the quake's strength is read from.</param>
/// <param name="earthquake">Receives the shake and its length.</param>
/// <param name="escape">Supplies how far the station's escape has gone.</param>
internal sealed partial class CeresEscapeQuake(EnemyRoomBinding room, RoomEarthquake earthquake, ICeresEscapeStatus escape)
{
    /// <summary>
    /// Starts the next diagonal shake, reaching <paramref name="reach"/>, once the station is self-destructing and the room
    /// is still. It reads the shared random sample without advancing it: one in 32 samples start the strong shake.
    /// </summary>
    internal void Keep(EarthquakeReach reach)
    {
        // A separate engine stage runs the earthquake down; a door only chooses the next shake and its length.
        if (escape.CeresStatus is CeresEscapeStatus.RidleyNotEscaped or CeresEscapeStatus.RidleyEscaped || earthquake.IsShaking)
            return;

        if (CeresDoorQuakeDefinitions.StrongQuake.IsTakenBy(room.Random.Read()))
        {
            earthquake.Start(new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Diagonal, reach), CeresDoorQuakeDefinitions.StrongQuakeFrames);
            return;
        }

        earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Diagonal, reach), CeresDoorQuakeDefinitions.LightQuakeFrames);
    }
}
