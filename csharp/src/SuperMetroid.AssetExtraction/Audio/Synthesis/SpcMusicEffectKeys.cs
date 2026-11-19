using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The installed key of each <see cref="SpcMusicEffect"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class SpcMusicEffectKeys
{
    /// <summary>Each SpcMusicEffect member's installed key.</summary>
    internal static InstalledKeys<SpcMusicEffect> Installed { get; } = new(new Dictionary<SpcMusicEffect, string>
    {
        [SpcMusicEffect.SetInstrument] = "setInstrument",
        [SpcMusicEffect.SetPan] = "setPan",
        [SpcMusicEffect.FadePan] = "fadePan",
        [SpcMusicEffect.EnableVibrato] = "enableVibrato",
        [SpcMusicEffect.DisableVibrato] = "disableVibrato",
        [SpcMusicEffect.SetMasterVolume] = "setMasterVolume",
        [SpcMusicEffect.FadeMasterVolume] = "fadeMasterVolume",
        [SpcMusicEffect.SetTempo] = "setTempo",
        [SpcMusicEffect.FadeTempo] = "fadeTempo",
        [SpcMusicEffect.SetGlobalTransposition] = "setGlobalTransposition",
        [SpcMusicEffect.SetChannelTransposition] = "setChannelTransposition",
        [SpcMusicEffect.EnableTremolo] = "enableTremolo",
        [SpcMusicEffect.DisableTremolo] = "disableTremolo",
        [SpcMusicEffect.SetChannelVolume] = "setChannelVolume",
        [SpcMusicEffect.FadeChannelVolume] = "fadeChannelVolume",
        [SpcMusicEffect.CallPattern] = "callPattern",
        [SpcMusicEffect.FadeVibrato] = "fadeVibrato",
        [SpcMusicEffect.PitchEnvelopeTo] = "pitchEnvelopeTo",
        [SpcMusicEffect.PitchEnvelopeFrom] = "pitchEnvelopeFrom",
        [SpcMusicEffect.DisablePitchEnvelope] = "disablePitchEnvelope",
        [SpcMusicEffect.SetFineTune] = "setFineTune",
        [SpcMusicEffect.EnableEcho] = "enableEcho",
        [SpcMusicEffect.DisableEcho] = "disableEcho",
        [SpcMusicEffect.ConfigureEcho] = "configureEcho",
        [SpcMusicEffect.FadeEchoVolume] = "fadeEchoVolume",
        [SpcMusicEffect.PitchSlide] = "pitchSlide",
        [SpcMusicEffect.SetPercussionBase] = "setPercussionBase",
        [SpcMusicEffect.SkipByte] = "skipByte",
        [SpcMusicEffect.CutKey] = "cutKey",
        [SpcMusicEffect.FastForwardForFrames] = "fastForwardForFrames",
        [SpcMusicEffect.SetFastForward] = "setFastForward",
    });
}
