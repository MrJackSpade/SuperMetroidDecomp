namespace SuperMetroid.Core.Audio;

/// <summary>
/// How a sound effect plays while one music data set is loaded, where the set redefines an instrument it plays (#2645,
/// #2648, #2650, #2652).
/// </summary>
/// <param name="DataSet">The music data set.</param>
/// <param name="Entry">The sound as that set plays it.</param>
public sealed partial record SoundEffectSetClipEntry(MusicDataSet DataSet, SoundEffectClipEntry Entry);
