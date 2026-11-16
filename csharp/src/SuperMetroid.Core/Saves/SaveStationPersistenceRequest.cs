using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Saves;

/// <summary>Confirmed save-point identity handed to the selected-slot save owner.</summary>
/// <param name="Station">The load station the save resumes at.</param>
public readonly record struct SaveStationPersistenceRequest(LoadStation Station);
