using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The cue and sound of one verify kraid head command mapping case.</summary>
/// <param name="Cue">The cue.</param>
/// <param name="Sound">The sound.</param>
internal readonly record struct VerifyKraidHeadCommandMappingCase(KraidHeadCue Cue, SoundEffectId Sound);
