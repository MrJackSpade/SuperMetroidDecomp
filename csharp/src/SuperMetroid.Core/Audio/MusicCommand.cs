namespace SuperMetroid.Core.Audio;

/// <summary>
/// One bank-$80 music queue command: stop, load a music-data set, or select a track. A cartridge word that has none of
/// those shapes stays representable as an <see cref="Unresolved"/> command holding its word, which the dispatcher still
/// interprets by its upload-path bit and low bits exactly as the cartridge does. <see cref="FromCartridge"/> is the one
/// place a queue word is decoded and <see cref="RawValue"/> the one place a command is encoded back.
/// </summary>
public abstract record MusicCommand
{
    /// <summary>Closes the hierarchy to the commands declared here.</summary>
    private MusicCommand()
    {
    }

    /// <summary>
    /// Whether the dispatcher takes its data-upload branch: every data load, and an unresolved word
    /// with the upload-path bit set, as the cartridge's sign test does even for noncanonical words.
    /// </summary>
    public abstract bool UsesDataUploadPath { get; }

    /// <summary>
    /// The music-data set the upload branch resolves: a data load's set, or the low byte of an
    /// unresolved word. A stop reads zero and a track selection its track, as the shared low byte does.
    /// </summary>
    public abstract int DataIndex { get; }

    /// <summary>
    /// The track the track branch writes to the music port, 0 through 127: a selection's track, zero for
    /// a stop, and the low seven bits the dispatcher keeps for any other command.
    /// </summary>
    public abstract int TrackIndex { get; }

    /// <summary>
    /// The cartridge queue word this command encodes to, 0 through 65,535, for word-level parity checks
    /// and diagnostics.
    /// </summary>
    public abstract int RawValue { get; }

    /// <summary>The zero command writes zero to APU port zero and stops playback.</summary>
    public sealed record StopPlayback : MusicCommand
    {
        /// <inheritdoc />
        public override bool UsesDataUploadPath => false;

        /// <inheritdoc />
        public override int DataIndex => 0;

        /// <inheritdoc />
        public override int TrackIndex => 0;

        /// <inheritdoc />
        public override int RawValue => 0;

        /// <inheritdoc />
        public override string ToString() => $"Stop:${RawValue:X4}";
    }

    /// <summary>A canonical <c>$FFxx</c> command uploads music data set <c>xx</c>.</summary>
    /// <param name="DataSet">The music-data set uploaded, 0 through 255.</param>
    public sealed record DataLoad(int DataSet) : MusicCommand
    {
        /// <inheritdoc cref="DataLoad(int)" path="/param[@name='DataSet']"/>
        public int DataSet { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, DataSet);

        /// <inheritdoc />
        public override bool UsesDataUploadPath => true;

        /// <inheritdoc />
        public override int DataIndex => DataSet;

        /// <inheritdoc />
        public override int TrackIndex => DataSet & AudioRomData.MusicWireFormat.TrackMask;

        /// <inheritdoc />
        public override int RawValue => AudioRomData.MusicWireFormat.DataCommandPrefix | DataSet;

        /// <inheritdoc />
        public override string ToString() => $"LoadData:${RawValue:X4}";
    }

    /// <summary>A canonical <c>$00xx</c> command selects track <c>xx</c>, 1 through 127.</summary>
    /// <param name="Track">The track selected.</param>
    public sealed record TrackSelection(int Track) : MusicCommand
    {
        /// <inheritdoc />
        public override bool UsesDataUploadPath => false;

        /// <inheritdoc />
        public override int DataIndex => Track;

        /// <inheritdoc />
        public override int TrackIndex => Track;

        /// <inheritdoc />
        public override int RawValue => Track;

        /// <inheritdoc />
        public override string ToString() => $"SelectTrack:${RawValue:X4}";
    }

    /// <summary>A noncanonical word retained losslessly for native dispatcher behavior.</summary>
    /// <param name="Word">The cartridge queue word, 0 through 65,535, kept unresolved because no authored command has its shape.</param>
    public sealed record Unresolved(int Word) : MusicCommand
    {
        /// <inheritdoc />
        public override bool UsesDataUploadPath => (Word & AudioRomData.MusicWireFormat.UploadPathBit) != 0;

        /// <inheritdoc />
        public override int DataIndex => SnesLittleEndian.LowByte((ushort)Word);

        /// <inheritdoc />
        public override int TrackIndex => Word & AudioRomData.MusicWireFormat.TrackMask;

        /// <inheritdoc />
        public override int RawValue => Word;

        /// <inheritdoc />
        public override string ToString() => $"Unknown:${RawValue:X4}";
    }

    /// <summary>The canonical music-stop command.</summary>
    public static MusicCommand Stop { get; } = new StopPlayback();

    /// <summary>Creates the canonical command that uploads music-data set <paramref name="dataIndex"/>.</summary>
    public static MusicCommand LoadData(byte dataIndex) => new DataLoad(dataIndex);

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

        return new TrackSelection(trackIndex);
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
            return new TrackSelection(word);
        return new Unresolved(word);
    }
}
