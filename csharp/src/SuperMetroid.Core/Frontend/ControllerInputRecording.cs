using System.Buffers.Binary;

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
        Magic.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[8..], FormatVersion);
        BinaryPrimitives.WriteInt64LittleEndian(header[12..], StartedUtc.UtcTicks);

        // Byte twenty is a format-owned bitfield. Seven reserved zero bytes follow it so
        // future host switches can be added without shifting the ROM digest or payload.
        header[20] = GameOptions.SkipOpeningCinematic ? (byte)1 : (byte)0;
        RomSha256.CopyTo(header[28..(28 + RomDigestByteCount)]);
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
        if ((optionFlags & ~1) != 0 || !header[21..28].SequenceEqual(new byte[7]))
            throw new InvalidDataException("Controller recording contains unknown option/reserved bits.");

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
                SkipOpeningCinematic = (optionFlags & 1) != 0,
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
