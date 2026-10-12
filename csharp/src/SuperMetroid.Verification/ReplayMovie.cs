using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Hardware;

/// <summary>
/// A recording a converted replay was produced from: its exact bytes, the cartridge SRAM it
/// starts with, the controller word of every source frame and, for a snapshot-start movie,
/// the emulator snapshot it begins from.
/// </summary>
internal sealed class ReplayMovie
{
    private ReplayMovie(string name, byte[] bytes, byte[] initialSaveRam, ushort[] frameInputs, ReplaySnapshot? snapshot)
    {
        Name = name;
        Bytes = bytes;
        InitialSaveRam = initialSaveRam;
        FrameInputs = frameInputs;
        Snapshot = snapshot;
    }

    /// <summary>Short label used in replay messages.</summary>
    public string Name { get; }
    public byte[] Bytes { get; }

    /// <summary>The 8 KiB cartridge SRAM the recording starts with.</summary>
    public byte[] InitialSaveRam { get; }

    /// <summary>The emulator snapshot a snapshot-start movie begins from; null for a power-on movie.</summary>
    public ReplaySnapshot? Snapshot { get; }

    /// <summary>
    /// The controller word of every source frame plus the trailing word the game reads
    /// after the last one, matching the converter's indexing.
    /// </summary>
    public ushort[] FrameInputs { get; }

    /// <summary>Loads a movie after checking it is exactly the expected recording.</summary>
    public static ReplayMovie Load(string name, string path, string expectedSha256)
    {
        byte[] bytes = File.ReadAllBytes(path);
        string actual = Convert.ToHexString(SHA256.HashData(bytes));
        if (actual != expectedSha256)
            throw new InvalidDataException($"{name} movie identity: expected {expectedSha256}, got {actual}.");
        return bytes.AsSpan(0, 4).SequenceEqual("SMV\x1a"u8) ? FromSmv(name, bytes)
            : bytes.AsSpan(0, 4).SequenceEqual("PK\x03\x04"u8) ? FromLsmv(name, bytes)
            : throw new InvalidDataException($"{name} is neither an SMV nor an lsnes movie.");
    }

    private static ReplayMovie FromSmv(string name, byte[] movie)
    {
        const int FrameCountField = 0x10, ControllerCountField = 0x14, ControllerOffsetField = 0x1c;
        if (movie[ControllerCountField] != 1)
            throw new InvalidDataException("Only one-controller movies are supported.");
        int frames = BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(FrameCountField));
        int offset = BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(ControllerOffsetField));
        var inputs = new ushort[frames + 1];
        for (int frame = 0; frame <= frames; frame++)
            inputs[frame] = BinaryPrimitives.ReadUInt16LittleEndian(movie.AsSpan(offset + 2 * frame));
        const int MovieOptionsOffset = 0x15, StateOffsetField = 0x18;
        const byte StartFromReset = 0x01;
        uint version = BinaryPrimitives.ReadUInt32LittleEndian(movie.AsSpan(4));
        if (version is not (4 or 5))
            throw new InvalidDataException($"SMV version {version} start-state layout is not supported.");
        byte[] start = ReadSmvStartState(movie,
            BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(StateOffsetField)), offset);
        if ((movie[MovieOptionsOffset] & StartFromReset) != 0)
            return new ReplayMovie(name, movie, CartridgeSaveRam(start, "Embedded reset-start SRAM"), inputs, snapshot: null);
        var snapshot = ReplaySnapshot.Parse(start);
        return new ReplayMovie(name, movie, CartridgeSaveRam(snapshot.SaveRam, "Snapshot SRA block"), inputs, snapshot);
    }

    /// <summary>
    /// Reads the start state a Snes9x v4/v5 movie embeds before its controller data: the
    /// reset-start SRAM or the snapshot. Snes9x opens it with zlib's <c>gzdopen</c>, whose
    /// reads decompress a gzip member and pass any other bytes through unchanged, so an
    /// uncompressed start state is equally valid. Snes9x appends bytes after a gzip member.
    /// </summary>
    private static byte[] ReadSmvStartState(byte[] movie, int start, int end)
    {
        if (movie[start] != 0x1f || movie[start + 1] != 0x8b)
            return movie.AsSpan(start, end - start).ToArray();
        using var gzip = new GZipStream(new MemoryStream(movie, start, end - start), CompressionMode.Decompress);
        using var state = new MemoryStream();
        gzip.CopyTo(state);
        return state.ToArray();
    }

    private static byte[] CartridgeSaveRam(byte[] image, string source)
    {
        if (image.Length < SuperMetroidAddressSpace.SaveRamByteCount)
            throw new InvalidDataException($"{source} is shorter than the cartridge's 8 KiB.");
        return image.AsSpan(0, SuperMetroidAddressSpace.SaveRamByteCount).ToArray();
    }

    /// <summary>
    /// Reads an lsnes rr1 movie recorded from power-on on bsnes v085 with default settings,
    /// one gamepad and no resets or subframes, as tools/lsmv-native-capture/lsmv_movie.py
    /// does. With no movie SRAM, lsnes leaves the cartridge RAM bsnes allocated, all $FF.
    /// </summary>
    private static ReplayMovie FromLsmv(string name, byte[] movie)
    {
        using var archive = new ZipArchive(new MemoryStream(movie), ZipArchiveMode.Read);
        string Member(string member)
        {
            ZipArchiveEntry entry = archive.GetEntry(member)
                ?? throw new InvalidDataException($"lsnes movie has no {member} member.");
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            return reader.ReadToEnd();
        }
        if (Member("systemid").Trim() != "lsnes-rr1" || Member("gametype").Trim() != "snes_ntsc" ||
            !Member("coreversion").Trim().StartsWith("bsnes v085", StringComparison.Ordinal))
            throw new InvalidDataException("Only NTSC lsnes rr1 movies recorded on bsnes v085 are supported.");
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            foreach (string unsupported in new[] { "settings", "savestate", "moviesram", "anchorsave", "rtc.second" })
                if (entry.FullName == unsupported || entry.FullName.StartsWith(unsupported + ".", StringComparison.Ordinal))
                    throw new InvalidDataException($"lsnes movie member {entry.FullName} needs state the power-on replay cannot model.");
        }

        var inputs = new List<ushort>();
        string[] lines = Member("input").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        for (int number = 0; number < lines.Length; number++)
        {
            string line = lines[number].TrimEnd('\r');
            int bar = line.IndexOf('|');
            string pad = bar < 0 ? "" : line[(bar + 1)..];
            if (!line.StartsWith('F') || bar < 0 || pad.Length != 12 || pad.Contains('|') ||
                line[..bar].Split(' ', StringSplitOptions.RemoveEmptyEntries) is not ["F.", "0", "0"])
                throw new InvalidDataException($"lsnes input line {number} is not a plain one-gamepad frame.");
            // Gamepad field order B Y Select Start Up Down Left Right A X L R: id n is $4218 bit 15 - n.
            ushort word = 0;
            for (int index = 0; index < 12; index++)
                if (pad[index] != '.')
                    word |= (ushort)(1 << (15 - index));
            inputs.Add(word);
        }
        // The last line is latched in the movie's final frame and read one frame later, so
        // it is the trailing word, like an SMV's: lines are source frames plus one.
        if (inputs.Count < 2)
            throw new InvalidDataException("lsnes movie has no source frames.");
        byte[] saveRam = new byte[SuperMetroidAddressSpace.SaveRamByteCount];
        Array.Fill(saveRam, (byte)0xff);
        return new ReplayMovie(name, movie, saveRam, inputs.ToArray(), snapshot: null);
    }
}
