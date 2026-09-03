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

/// <summary>
/// Lossless bank-$80 music queue command. Canonical stop, data-load, and track-selection
/// words have named factories; arbitrary cartridge words remain representable.
/// </summary>
public readonly record struct MusicCommand
{
    private MusicCommand(ushort rawValue) => RawValue = rawValue;

    /// <summary>The exact word stored in the cartridge's eight-entry music queue.</summary>
    public ushort RawValue { get; }

    /// <summary>The proven semantic shape of <see cref="RawValue"/>.</summary>
    public MusicCommandKind Kind => RawValue switch
    {
        0 => MusicCommandKind.Stop,
        _ when (RawValue & AudioRomData.MusicWireFormat.DataCommandKindMask) ==
            AudioRomData.MusicWireFormat.DataCommandPrefix => MusicCommandKind.LoadData,
        <= AudioRomData.MusicWireFormat.MaximumTrack => MusicCommandKind.SelectTrack,
        _ => MusicCommandKind.Unknown,
    };

    /// <summary>Native high-bit branch used even for noncanonical cartridge words.</summary>
    public bool UsesDataUploadPath =>
        (RawValue & AudioRomData.MusicWireFormat.UploadPathBit) != 0;

    /// <summary>Low byte used to index the 24-bit SPC-data pointer table.</summary>
    public byte DataIndex => unchecked((byte)RawValue);

    /// <summary>Low seven bits ultimately written to APU port zero.</summary>
    public byte TrackIndex => unchecked((byte)(
        RawValue & AudioRomData.MusicWireFormat.TrackMask));

    /// <summary>The canonical music-stop command.</summary>
    public static MusicCommand Stop { get; } = new(0);

    /// <summary>Creates canonical <c>$FFxx</c> music-data upload command.</summary>
    public static MusicCommand LoadData(byte dataIndex) =>
        new(unchecked((ushort)(AudioRomData.MusicWireFormat.DataCommandPrefix | dataIndex)));

    /// <summary>Creates a canonical nonzero track-selection command.</summary>
    public static MusicCommand SelectTrack(byte trackIndex)
    {
        if (trackIndex is 0 or > AudioRomData.MusicWireFormat.MaximumTrack)
        {
            throw new ArgumentOutOfRangeException(
                nameof(trackIndex),
                trackIndex,
                "Track selection must be $01..$7F; use MusicCommand.Stop for zero.");
        }

        return new MusicCommand(trackIndex);
    }

    /// <summary>
    /// Converts a native track byte whose zero value means stop, as used when restoring a
    /// previously sampled room track.
    /// </summary>
    public static MusicCommand SelectTrackOrStop(byte trackIndex) =>
        trackIndex == 0 ? Stop : SelectTrack(trackIndex);

    /// <summary>Preserves an arbitrary word read from cartridge code or data.</summary>
    public static MusicCommand FromCartridge(ushort rawValue) => new(rawValue);

    /// <inheritdoc />
    public override string ToString() => $"{Kind}:${RawValue:X4}";
}

/// <summary>
/// Effective countdown stored beside a music command after <c>QueueMusic_DelayedY</c>'s
/// minimum-eight-frame clamp.
/// </summary>
public readonly record struct MusicCommandDelay
{
    private MusicCommandDelay(ushort frames) => Frames = frames;

    /// <summary>Effective native countdown frames.</summary>
    public ushort Frames { get; }

    /// <summary>The fixed delay installed by <c>QueueMusic_Delayed8</c>.</summary>
    public static MusicCommandDelay EightFrames { get; } =
        new(AudioRomData.Queues.MinimumMusicDelayFrames);

    /// <summary>
    /// Converts a <c>QueueMusic_DelayedY</c> argument to the effective native countdown,
    /// clamping values below eight exactly as bank $80 does.
    /// </summary>
    public static MusicCommandDelay FromDelayedYArgument(ushort requestedFrames) =>
        new(Math.Max(requestedFrames, AudioRomData.Queues.MinimumMusicDelayFrames));

    /// <summary>
    /// Rehydrates an already-effective delay published by a translated subsystem.
    /// </summary>
    public static MusicCommandDelay FromEffectiveFrames(ushort frames)
    {
        if (frames < AudioRomData.Queues.MinimumMusicDelayFrames)
        {
            throw new InvalidDataException(
                $"Effective music delay {frames} is below bank $80's minimum of eight frames.");
        }

        return new MusicCommandDelay(frames);
    }

    /// <inheritdoc />
    public override string ToString() => $"{Frames} frames";
}
