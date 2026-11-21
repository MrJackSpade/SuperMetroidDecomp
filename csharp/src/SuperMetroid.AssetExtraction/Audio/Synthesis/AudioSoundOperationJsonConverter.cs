using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Reads and writes an <see cref="AudioSoundOperation"/> in the installed audio manifest by its installed key (#2548).</summary>
internal sealed partial class AudioSoundOperationJsonConverter() : InstalledKeyJsonConverter<AudioSoundOperation>(AudioSoundOperationKeys.Installed);
