using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The installed key of each <see cref="AudioMusicInstructionKind"/> (#2548), the operation name the installed audio
/// manifest spells a music channel command by, written out so renaming a member never changes it. An effect command is
/// spelled by its effect's key (<see cref="SpcMusicEffectKeys"/>), so the manifest never holds the effect kind's own key.
/// </summary>
internal static class AudioMusicInstructionKindKeys
{
    /// <summary>Each AudioMusicInstructionKind member's installed key.</summary>
    internal static InstalledKeys<AudioMusicInstructionKind> Installed { get; } = new(new Dictionary<AudioMusicInstructionKind, string>
    {
        [AudioMusicInstructionKind.End] = "end",
        [AudioMusicInstructionKind.Note] = "note",
        [AudioMusicInstructionKind.Tie] = "tie",
        [AudioMusicInstructionKind.Rest] = "rest",
        [AudioMusicInstructionKind.Percussion] = "percussion",
        [AudioMusicInstructionKind.Effect] = "effect",
    });
}
