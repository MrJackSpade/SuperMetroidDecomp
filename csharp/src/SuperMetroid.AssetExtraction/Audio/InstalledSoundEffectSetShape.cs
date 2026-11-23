using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// How a sound effect sounds while one music data set is loaded, where that differs from its own clip (#2645, #2648, #2650,
/// #2652): the data set redefines an instrument the sound plays, so the game plays it with the set's instrument there.
/// </summary>
/// <param name="DataSet">The music data set.</param>
/// <param name="Shape">The sound's shape, and the clips it plays, with that set loaded.</param>
public sealed partial record InstalledSoundEffectSetShape(MusicDataSet DataSet, InstalledSoundEffectShape Shape);
