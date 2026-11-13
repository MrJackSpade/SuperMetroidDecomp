using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>The load station a save resumes at, as the save file names it.</summary>
/// <param name="Area">The area whose station list holds the station.</param>
/// <param name="SaveStation">The station's number in that list; the codec accepts only a number that names a load station.</param>
public sealed record SaveCheckpointJsonDocument(
    AreaId Area,
    int SaveStation);
