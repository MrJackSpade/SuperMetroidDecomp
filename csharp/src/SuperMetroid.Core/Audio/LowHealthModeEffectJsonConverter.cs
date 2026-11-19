namespace SuperMetroid.Core.Audio;

/// <summary>Reads and writes a <see cref="LowHealthModeEffect"/> in an installed document by its installed key (#2520).</summary>
internal sealed class LowHealthModeEffectJsonConverter() : InstalledKeyJsonConverter<LowHealthModeEffect>(LowHealthModeEffectKeys.Installed);
