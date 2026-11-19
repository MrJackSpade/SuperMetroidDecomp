namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Reads and writes a <see cref="TitleSequencePhase"/> in an installed document by its installed key (#2520).</summary>
internal sealed class TitleSequencePhaseJsonConverter() : InstalledKeyJsonConverter<TitleSequencePhase>(TitleSequencePhaseKeys.Installed);
