namespace SuperMetroid.Core.Audio;

/// <summary>
/// One bank-$80 music queue command: stop, load a music-data set, or select a track. A cartridge
/// word that has none of those shapes stays representable as an <see cref="MusicCommandKind.Unknown"/>
/// command holding its <see cref="UnresolvedWord"/>, which the dispatcher still interprets by its
/// upload-path bit and low bits exactly as the cartridge does. <see cref="FromCartridge"/> is the one
/// place a queue word is decoded and <see cref="RawValue"/> the one place a command is encoded back.
/// </summary>
public readonly record struct MusicCommand
{
    /// <summary>
    /// The music-data set a <see cref="MusicCommandKind.LoadData"/> command uploads, or the track a
    /// <see cref="MusicCommandKind.SelectTrack"/> command selects; zero for the other kinds.
    /// </summary>
    private readonly int index;

    /// <summary>Stores a decoded command; callers use the named factories or <see cref="FromCartridge"/>.</summary>
    private MusicCommand(MusicCommandKind kind, int index, int unresolvedWord)
    {
        Kind = kind;
        this.index = index;
        UnresolvedWord = unresolvedWord;
    }

    /// <summary>What the command does.</summary>
    public MusicCommandKind Kind { get; }

    /// <summary>
    /// For an <see cref="MusicCommandKind.Unknown"/> command, the cartridge queue word it was read as,
    /// 0 through 65,535, kept unresolved because no authored command has that shape; zero for every
    /// other kind.
    /// </summary>
    public int UnresolvedWord { get; }

    /// <summary>
    /// Whether the dispatcher takes its data-upload branch: every data load, and an unresolved word
    /// with the upload-path bit set, as the cartridge's sign test does even for noncanonical words.
    /// </summary>
    public bool UsesDataUploadPath => Kind switch
    {
        MusicCommandKind.Stop or MusicCommandKind.SelectTrack => false,
        MusicCommandKind.LoadData => true,
        MusicCommandKind.Unknown => (UnresolvedWord & AudioRomData.MusicWireFormat.UploadPathBit) != 0,
        _ => throw new InvalidOperationException($"Undefined {nameof(MusicCommandKind)} {(int)Kind}."),
    };

    /// <summary>
    /// The music-data set the upload branch resolves: a data load's set, or the low byte of an
    /// unresolved word. A stop reads zero and a track selection its track, as the shared low byte does.
    /// </summary>
    public int DataIndex => Kind switch
    {
        MusicCommandKind.Stop or MusicCommandKind.LoadData or MusicCommandKind.SelectTrack => index,
        MusicCommandKind.Unknown => SnesLittleEndian.LowByte((ushort)UnresolvedWord),
        _ => throw new InvalidOperationException($"Undefined {nameof(MusicCommandKind)} {(int)Kind}."),
    };

    /// <summary>
    /// The track the track branch writes to the music port, 0 through 127: a selection's track, zero for
    /// a stop, and the low seven bits the dispatcher keeps for any other command.
    /// </summary>
    public int TrackIndex => Kind switch
    {
        MusicCommandKind.Stop or MusicCommandKind.SelectTrack => index,
        MusicCommandKind.LoadData => index & AudioRomData.MusicWireFormat.TrackMask,
        MusicCommandKind.Unknown => UnresolvedWord & AudioRomData.MusicWireFormat.TrackMask,
        _ => throw new InvalidOperationException($"Undefined {nameof(MusicCommandKind)} {(int)Kind}."),
    };

    /// <summary>
    /// The cartridge queue word this command encodes to, 0 through 65,535, for word-level parity checks
    /// and diagnostics.
    /// </summary>
    public int RawValue => Kind switch
    {
        MusicCommandKind.Stop or MusicCommandKind.SelectTrack => index,
        MusicCommandKind.LoadData => AudioRomData.MusicWireFormat.DataCommandPrefix | index,
        MusicCommandKind.Unknown => UnresolvedWord,
        _ => throw new InvalidOperationException($"Undefined {nameof(MusicCommandKind)} {(int)Kind}."),
    };

    /// <summary>The canonical music-stop command.</summary>
    public static MusicCommand Stop { get; } = new(MusicCommandKind.Stop, 0, 0);

    /// <summary>Creates the canonical command that uploads music-data set <paramref name="dataIndex"/>.</summary>
    public static MusicCommand LoadData(byte dataIndex) =>
        new(MusicCommandKind.LoadData, dataIndex, 0);

    /// <summary>Creates a canonical nonzero track-selection command.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="trackIndex"/> is outside 1 through 127.</exception>
    public static MusicCommand SelectTrack(int trackIndex)
    {
        if (trackIndex is < 1 or > AudioRomData.MusicWireFormat.MaximumTrack)
        {
            throw new ArgumentOutOfRangeException(
                nameof(trackIndex),
                trackIndex,
                "Track selection must be $01..$7F; use MusicCommand.Stop for zero.");
        }

        return new MusicCommand(MusicCommandKind.SelectTrack, trackIndex, 0);
    }

    /// <summary>
    /// Converts a track whose zero value means stop, as used when restoring a previously sampled
    /// room track.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="trackIndex"/> is outside 0 through 127.</exception>
    public static MusicCommand SelectTrackOrStop(int trackIndex) =>
        trackIndex == 0 ? Stop : SelectTrack(trackIndex);

    /// <summary>
    /// Decodes a queue word read from cartridge code or data: zero stops, a canonical data-load word
    /// loads its set, a word up to the last track selects it, and any other word is kept unresolved.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rawValue"/> is outside 0 through 65,535.</exception>
    public static MusicCommand FromCartridge(int rawValue)
    {
        int word = Ensure.BetweenInclusive(0, ushort.MaxValue, rawValue);
        if (word == 0)
            return Stop;
        if ((word & AudioRomData.MusicWireFormat.DataCommandKindMask) == AudioRomData.MusicWireFormat.DataCommandPrefix)
            return LoadData(SnesLittleEndian.LowByte((ushort)word));
        if (word <= AudioRomData.MusicWireFormat.MaximumTrack)
            return new MusicCommand(MusicCommandKind.SelectTrack, word, 0);
        return new MusicCommand(MusicCommandKind.Unknown, 0, word);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}:${RawValue:X4}";
}
