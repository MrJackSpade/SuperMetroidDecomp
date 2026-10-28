namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One sound effect of a resident SPC library: its instruction stream and its voice-allocation policy.</summary>
/// <typeparam name="TPolicy">The library's policy type.</typeparam>
/// <param name="Stream">Where the effect's instruction stream starts in SPC RAM.</param>
/// <param name="Policy">The voice count and priority its command handler allocates.</param>
internal readonly record struct SpcSoundEffectDefinition<TPolicy>(int Stream, TPolicy Policy) where TPolicy : struct, Enum;
