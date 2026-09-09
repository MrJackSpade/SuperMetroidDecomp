using System.Buffers.Binary;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A deterministic desktop-session seed followed by the exact controller word submitted
/// for every emulated frame.
/// </summary>
/// <remarks>
/// The recording deliberately stores SRAM, not a private C# object graph. Replaying starts
/// from the same cartridge-compatible save image that was present at reset, then drives the
/// public game dispatcher with the recorded words. That keeps recordings useful as the
/// translation changes and makes the starting point independently inspectable by SNES tools.
/// No ROM bytes or filesystem paths are embedded; the SHA-256 digest merely rejects a replay
/// against a different cartridge revision.
/// </remarks>
public sealed record ControllerInputRecording
{
    private static ReadOnlySpan<byte> Magic => "SMINPUT1"u8;

    private const uint FormatVersion = 1;
    private const int RomDigestByteCount = 32;
    private const int FixedHeaderByteCount = 8 + sizeof(uint) + sizeof(long) + 8 +
        RomDigestByteCount + sizeof(int) + sizeof(int);

    /// <summary>UTC time at which the reset/restart created this recording.</summary>
    public required DateTimeOffset StartedUtc { get; init; }

    /// <summary>SHA-256 of the complete private ROM image used by the session.</summary>
    public required byte[] RomSha256 { get; init; }

    /// <summary>The exact 8 KiB SRAM image loaded immediately before game reset.</summary>
    public required byte[] InitialSaveRam { get; init; }

    /// <summary>Host options which can change the otherwise identical startup route.</summary>
    public required SuperMetroidGameOptions GameOptions { get; init; }

    /// <summary>One raw SNES controller-one word for each call to <c>Step</c>.</summary>
    public required ushort[] ControllerInputs { get; init; }

    /// <summary>Writes the compact, versioned recording to a stream.</summary>
    public void Write(Stream destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        Validate();

        Span<byte> header = stackalloc byte[FixedHeaderByteCount];
        header.Clear();
        Magic.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[8..], FormatVersion);
        BinaryPrimitives.WriteInt64LittleEndian(header[12..], StartedUtc.UtcTicks);

        // Byte twenty is a format-owned bitfield. Seven reserved zero bytes follow it so
        // future host switches can be added without shifting the ROM digest or payload.
        if (!Enum.IsDefined(GameOptions.MapReveal))
            throw new InvalidDataException($"Cannot record undefined map reveal mode {GameOptions.MapReveal}.");
        header[20] = (byte)(
            (GameOptions.SkipOpeningCinematic
                ? ControllerInputRecordingFormat.SkipOpeningCinematic
                : 0) |
            (GameOptions.Invincibility ? ControllerInputRecordingFormat.Invincibility : 0) |
            (GameOptions.InfiniteAmmo ? ControllerInputRecordingFormat.InfiniteAmmo : 0) |
            (GameOptions.PreventEscapeTimeout ? ControllerInputRecordingFormat.PreventEscapeTimeout : 0) |
            ((byte)GameOptions.MapReveal << ControllerInputRecordingFormat.MapRevealShift));
        RomSha256.CopyTo(header[28..(28 + RomDigestByteCount)]);
        // Reserved bytes 21..22 encode ending minutes plus one; zero keeps old
        // recordings retail-authentic, and zero-minute overrides remain representable.
        if (GameOptions.EndingTimeOverrideMinutes is { } endingMinutes)
        {
            if (endingMinutes > 5999) throw new InvalidDataException("Ending override exceeds 99:59.");
            BinaryPrimitives.WriteUInt16LittleEndian(header[21..], (ushort)(endingMinutes + 1));
        }
        BinaryPrimitives.WriteInt32LittleEndian(header[60..], InitialSaveRam.Length);
        BinaryPrimitives.WriteInt32LittleEndian(header[64..], ControllerInputs.Length);
        destination.Write(header);
        destination.Write(InitialSaveRam);

        Span<byte> word = stackalloc byte[sizeof(ushort)];
        foreach (ushort input in ControllerInputs)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(word, input);
            destination.Write(word);
        }
    }

    /// <summary>Reads and strictly validates one complete recording.</summary>
    public static ControllerInputRecording Read(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        Span<byte> header = stackalloc byte[FixedHeaderByteCount];
        source.ReadExactly(header);
        if (!header[..Magic.Length].SequenceEqual(Magic))
            throw new InvalidDataException("Controller recording has an invalid file signature.");

        uint version = BinaryPrimitives.ReadUInt32LittleEndian(header[8..]);
        if (version != FormatVersion)
        {
            throw new InvalidDataException(
                $"Controller recording version {version} is not supported (expected {FormatVersion}).");
        }

        byte optionFlags = header[20];
        if ((optionFlags & ~ControllerInputRecordingFormat.KnownOptionMask) != 0 ||
            !header[23..28].SequenceEqual(new byte[5]))
            throw new InvalidDataException("Controller recording contains unknown option/reserved bits.");
        ushort encodedEndingMinutes = BinaryPrimitives.ReadUInt16LittleEndian(header[21..]);
        if (encodedEndingMinutes > 6000)
            throw new InvalidDataException("Controller recording ending override exceeds 99:59.");
        var mapReveal = (MapRevealMode)(
            (optionFlags & ControllerInputRecordingFormat.MapRevealMask) >>
            ControllerInputRecordingFormat.MapRevealShift);
        if (!Enum.IsDefined(mapReveal))
            throw new InvalidDataException(
                $"Controller recording contains undefined map reveal mode {(byte)mapReveal}.");

        int saveRamLength = BinaryPrimitives.ReadInt32LittleEndian(header[60..]);
        if (saveRamLength != Hardware.SuperMetroidAddressSpace.SaveRamByteCount)
        {
            throw new InvalidDataException(
                $"Controller recording contains {saveRamLength} SRAM bytes; expected " +
                $"{Hardware.SuperMetroidAddressSpace.SaveRamByteCount}.");
        }

        int inputCount = BinaryPrimitives.ReadInt32LittleEndian(header[64..]);
        if (inputCount < 0)
            throw new InvalidDataException("Controller recording contains a negative frame count.");

        // Refuse impossible/truncated counts before allocating. Seekable files get an exact
        // length check; streams still receive ReadExactly plus the trailing-byte probe below.
        long payloadByteCount = (long)saveRamLength + (long)inputCount * sizeof(ushort);
        if (source.CanSeek && source.Length - source.Position != payloadByteCount)
        {
            throw new InvalidDataException(
                "Controller recording payload length does not agree with its frame count.");
        }

        var saveRam = new byte[saveRamLength];
        source.ReadExactly(saveRam);
        var inputs = new ushort[inputCount];
        Span<byte> word = stackalloc byte[sizeof(ushort)];
        for (int index = 0; index < inputs.Length; index++)
        {
            source.ReadExactly(word);
            inputs[index] = BinaryPrimitives.ReadUInt16LittleEndian(word);
        }
        if (!source.CanSeek && source.ReadByte() != -1)
            throw new InvalidDataException("Controller recording has trailing payload bytes.");

        return new ControllerInputRecording
        {
            StartedUtc = new DateTimeOffset(
                BinaryPrimitives.ReadInt64LittleEndian(header[12..]),
                TimeSpan.Zero),
            RomSha256 = header[28..(28 + RomDigestByteCount)].ToArray(),
            InitialSaveRam = saveRam,
            GameOptions = new SuperMetroidGameOptions
            {
                SkipOpeningCinematic =
                    (optionFlags & ControllerInputRecordingFormat.SkipOpeningCinematic) != 0,
                Invincibility =
                    (optionFlags & ControllerInputRecordingFormat.Invincibility) != 0,
                InfiniteAmmo =
                    (optionFlags & ControllerInputRecordingFormat.InfiniteAmmo) != 0,
                MapReveal = mapReveal,
                EndingTimeOverrideMinutes = encodedEndingMinutes == 0 ? null : (ushort)(encodedEndingMinutes - 1),
                PreventEscapeTimeout = (optionFlags & ControllerInputRecordingFormat.PreventEscapeTimeout) != 0,
            },
            ControllerInputs = inputs,
        };
    }

    /// <summary>Convenience loader which retains strict stream validation.</summary>
    public static ControllerInputRecording Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using FileStream source = File.OpenRead(path);
        return Read(source);
    }

    private void Validate()
    {
        ArgumentNullException.ThrowIfNull(RomSha256);
        ArgumentNullException.ThrowIfNull(InitialSaveRam);
        ArgumentNullException.ThrowIfNull(GameOptions);
        ArgumentNullException.ThrowIfNull(ControllerInputs);
        if (RomSha256.Length != RomDigestByteCount)
            throw new InvalidDataException("A controller recording requires a 32-byte ROM digest.");
        if (InitialSaveRam.Length != Hardware.SuperMetroidAddressSpace.SaveRamByteCount)
        {
            throw new InvalidDataException(
                $"A controller recording requires exactly " +
                $"{Hardware.SuperMetroidAddressSpace.SaveRamByteCount} SRAM bytes.");
        }
    }
}
