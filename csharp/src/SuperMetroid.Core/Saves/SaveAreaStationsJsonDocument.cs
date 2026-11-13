using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>The save stations and elevators used in one area of a saved game.</summary>
/// <param name="Area">The area.</param>
/// <param name="Stations">Its used stations, each by its number in the area's station list, ascending.</param>
public sealed record SaveAreaStationsJsonDocument(AreaId Area, int[] Stations);
