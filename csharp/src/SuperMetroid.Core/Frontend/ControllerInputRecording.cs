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
/// No ROM bytes or filesystem paths are embedded. The source-cartridge SHA-256 rejects a
/// different revision, while version-two recordings separately identify the selected
/// installed content so presentation or compiled-definition drift can be reported accurately.
/// </remarks>
public sealed record ControllerInputRecording
{
    /// <summary>UTC time at which the reset/restart created this recording.</summary>
    public required DateTimeOffset StartedUtc { get; init; }

    /// <summary>SHA-256 of the complete private ROM image used by the session.</summary>
    public required byte[] RomSha256 { get; init; }

    /// <summary>
    /// Installed content selected by the recording host, or <see langword="null"/> for a
    /// legacy version-one recording.
    /// </summary>
    public GameContentIdentitySnapshot? ContentIdentity { get; init; }

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

        uint formatVersion = ContentIdentity is null
            ? ControllerInputRecordingFormat.LegacyFormatVersion
            : ControllerInputRecordingFormat.CurrentFormatVersion;
        int headerByteCount = ContentIdentity is null
            ? ControllerInputRecordingFormat.LegacyHeaderByteCount
            : ControllerInputRecordingFormat.CurrentHeaderByteCount;
        Span<byte> header = stackalloc byte[headerByteCount];
        header.Clear();
        ControllerInputRecordingFormat.Magic.CopyTo(header);
        BinaryPrimitives.WriteUInt32LittleEndian(header[8..], formatVersion);
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
        RomSha256.CopyTo(header[
            ControllerInputRecordingFormat.RomDigestOffset..
            (ControllerInputRecordingFormat.RomDigestOffset + ControllerInputRecordingFormat.DigestByteCount)]);
        // Reserved bytes 21..22 encode ending minutes plus one; zero keeps old
        // recordings retail-authentic, and zero-minute overrides remain representable.
        if (GameOptions.EndingTimeOverrideMinutes is { } endingMinutes)
        {
            if (endingMinutes > 5999) throw new InvalidDataException("Ending override exceeds 99:59.");
            BinaryPrimitives.WriteUInt16LittleEndian(header[21..], (ushort)(endingMinutes + 1));
        }
        BinaryPrimitives.WriteInt32LittleEndian(header[60..], InitialSaveRam.Length);
        BinaryPrimitives.WriteInt32LittleEndian(header[64..], ControllerInputs.Length);
        if (ContentIdentity is { } contentIdentity)
        {
            BinaryPrimitives.WriteInt32LittleEndian(
                header[ControllerInputRecordingFormat.ContentIdentityOffset..],
                contentIdentity.FormatVersion);
            if (!contentIdentity.CompiledDefinitionsBuildId.TryWriteBytes(
                    header[ControllerInputRecordingFormat.ContentBuildIdOffset..
                        ControllerInputRecordingFormat.AudioDigestOffset]))
            {
                throw new InvalidDataException("Compiled-definition build ID did not fit the recording header.");
            }
            contentIdentity.AudioContentSha256.CopyTo(
                header[ControllerInputRecordingFormat.AudioDigestOffset..
                    ControllerInputRecordingFormat.MapDigestOffset]);
            contentIdentity.MapContentSha256.CopyTo(
                header[ControllerInputRecordingFormat.MapDigestOffset..
                    ControllerInputRecordingFormat.ProjectileDigestOffset]);
            contentIdentity.ProjectileContentSha256.CopyTo(
                header[ControllerInputRecordingFormat.ProjectileDigestOffset..
                    ControllerInputRecordingFormat.CompositeDigestOffset]);
            contentIdentity.CompositeSha256.CopyTo(
                header[ControllerInputRecordingFormat.CompositeDigestOffset..
                    ControllerInputRecordingFormat.CurrentHeaderByteCount]);
        }
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
        Span<byte> prefix = stackalloc byte[8 + sizeof(uint)];
        source.ReadExactly(prefix);
        if (!prefix[..ControllerInputRecordingFormat.Magic.Length].SequenceEqual(
                ControllerInputRecordingFormat.Magic))
            throw new InvalidDataException("Controller recording has an invalid file signature.");

        uint version = BinaryPrimitives.ReadUInt32LittleEndian(prefix[8..]);
        int headerByteCount = version switch
        {
            ControllerInputRecordingFormat.LegacyFormatVersion =>
                ControllerInputRecordingFormat.LegacyHeaderByteCount,
            ControllerInputRecordingFormat.CurrentFormatVersion =>
                ControllerInputRecordingFormat.CurrentHeaderByteCount,
            _ => throw new InvalidDataException(
                $"Controller recording version {version} is not supported " +
                $"(expected {ControllerInputRecordingFormat.LegacyFormatVersion} or " +
                $"{ControllerInputRecordingFormat.CurrentFormatVersion})."),
        };
        Span<byte> header = stackalloc byte[headerByteCount];
        prefix.CopyTo(header);
        source.ReadExactly(header[prefix.Length..]);

        GameContentIdentitySnapshot? contentIdentity = null;
        if (version == ControllerInputRecordingFormat.CurrentFormatVersion)
        {
            int identityVersion = BinaryPrimitives.ReadInt32LittleEndian(
                header[ControllerInputRecordingFormat.ContentIdentityOffset..]);
            if (identityVersion <= 0)
                throw new InvalidDataException("Controller recording has an invalid content-identity version.");
            contentIdentity = new GameContentIdentitySnapshot
            {
                FormatVersion = identityVersion,
                CompiledDefinitionsBuildId = new Guid(
                    header[ControllerInputRecordingFormat.ContentBuildIdOffset..
                        ControllerInputRecordingFormat.AudioDigestOffset]),
                AudioContentSha256 = header[ControllerInputRecordingFormat.AudioDigestOffset..
                    ControllerInputRecordingFormat.MapDigestOffset].ToArray(),
                MapContentSha256 = header[ControllerInputRecordingFormat.MapDigestOffset..
                    ControllerInputRecordingFormat.ProjectileDigestOffset].ToArray(),
                ProjectileContentSha256 = header[ControllerInputRecordingFormat.ProjectileDigestOffset..
                    ControllerInputRecordingFormat.CompositeDigestOffset].ToArray(),
                CompositeSha256 = header[ControllerInputRecordingFormat.CompositeDigestOffset..
                    ControllerInputRecordingFormat.CurrentHeaderByteCount].ToArray(),
            };
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
            RomSha256 = header[
                ControllerInputRecordingFormat.RomDigestOffset..
                (ControllerInputRecordingFormat.RomDigestOffset +
                    ControllerInputRecordingFormat.DigestByteCount)].ToArray(),
            ContentIdentity = contentIdentity,
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
        if (RomSha256.Length != ControllerInputRecordingFormat.DigestByteCount)
            throw new InvalidDataException("A controller recording requires a 32-byte ROM digest.");
        if (ContentIdentity is { } contentIdentity)
        {
            if (contentIdentity.FormatVersion <= 0)
                throw new InvalidDataException("A controller recording requires a positive content-identity version.");
            ValidateDigest(contentIdentity.AudioContentSha256, "audio");
            ValidateDigest(contentIdentity.MapContentSha256, "map");
            ValidateDigest(contentIdentity.ProjectileContentSha256, "projectile");
            ValidateDigest(contentIdentity.CompositeSha256, "composite");
        }
        if (InitialSaveRam.Length != Hardware.SuperMetroidAddressSpace.SaveRamByteCount)
        {
            throw new InvalidDataException(
                $"A controller recording requires exactly " +
                $"{Hardware.SuperMetroidAddressSpace.SaveRamByteCount} SRAM bytes.");
        }
    }

    private static void ValidateDigest(byte[]? digest, string component)
    {
        if (digest is null || digest.Length != ControllerInputRecordingFormat.DigestByteCount)
        {
            throw new InvalidDataException(
                $"A controller recording requires a 32-byte {component} content digest.");
        }
    }
}
