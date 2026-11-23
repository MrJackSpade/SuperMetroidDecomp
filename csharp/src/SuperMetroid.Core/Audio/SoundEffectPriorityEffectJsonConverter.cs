namespace SuperMetroid.Core.Audio;

/// <summary>Reads and writes a <see cref="SoundEffectPriorityEffect"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class SoundEffectPriorityEffectJsonConverter() : InstalledKeyJsonConverter<SoundEffectPriorityEffect>(SoundEffectPriorityEffectKeys.Installed);
