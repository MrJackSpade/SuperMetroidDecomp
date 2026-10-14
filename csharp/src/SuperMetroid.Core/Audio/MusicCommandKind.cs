namespace SuperMetroid.Core.Audio;

/// <summary>Semantic class of a bank-$80 music-queue word.</summary>
public enum MusicCommandKind : byte
{
    /// <summary>The zero command writes zero to APU port zero and stops playback.</summary>
    Stop,

    /// <summary>A canonical <c>$FFxx</c> command uploads music data set <c>xx</c>.</summary>
    LoadData,

    /// <summary>A canonical <c>$00xx</c> command selects track <c>xx</c>.</summary>
    SelectTrack,

    /// <summary>A noncanonical word retained losslessly for native dispatcher behavior.</summary>
    Unknown,
}
