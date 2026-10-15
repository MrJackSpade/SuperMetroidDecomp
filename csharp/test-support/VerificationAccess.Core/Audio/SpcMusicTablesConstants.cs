using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge values of <see cref="SpcMusicTables"/> that only verification reads.</summary>
internal static class SpcMusicTablesConstants
{
    /// <summary>$CF:8A6E uploaded driver kBaseNoteFreqs, thirteen little-endian pitch words.</summary>
    public const int BaseNoteReferenceAddress = 0xcf8a6e;
    /// <summary>$CF:87A8 uploaded driver kEffectByteLength, opcode E0 through FE.</summary>
    public const int EffectLengthReferenceAddress = 0xcf87a8;
    /// <summary>$CF:80EC uploaded driver kNoteGateOffPct, eight unsigned gate fractions.</summary>
    public const int NoteGateReferenceAddress = 0xcf80ec;
    /// <summary>$CF:80F4 uploaded driver kNoteVol, sixteen unsigned volume fractions.</summary>
    public const int NoteVolumeReferenceAddress = 0xcf80f4;

    /// <summary>Provides native SPC note, gate, volume, effect length, and base-note table references.</summary>
    extension(SpcMusicTables)
    {
        /// <inheritdoc cref="SpcMusicTablesConstants.BaseNoteReferenceAddress"/>
        internal static int BaseNoteReferenceAddress => SpcMusicTablesConstants.BaseNoteReferenceAddress;
        /// <inheritdoc cref="SpcMusicTablesConstants.EffectLengthReferenceAddress"/>
        internal static int EffectLengthReferenceAddress => SpcMusicTablesConstants.EffectLengthReferenceAddress;
        /// <inheritdoc cref="SpcMusicTablesConstants.NoteGateReferenceAddress"/>
        internal static int NoteGateReferenceAddress => SpcMusicTablesConstants.NoteGateReferenceAddress;
        /// <inheritdoc cref="SpcMusicTablesConstants.NoteVolumeReferenceAddress"/>
        internal static int NoteVolumeReferenceAddress => SpcMusicTablesConstants.NoteVolumeReferenceAddress;
    }
}
