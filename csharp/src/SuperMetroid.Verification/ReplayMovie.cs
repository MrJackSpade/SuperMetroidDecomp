using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using SuperMetroid.Core.Hardware;

/// <summary>
/// A power-on recording a converted replay was produced from: its exact bytes, the
/// cartridge SRAM it starts with, and the controller word of every source frame.
/// </summary>
internal sealed class ReplayMovie
{
    private ReplayMovie(string name, byte[] bytes, byte[] powerOnSaveRam, ushort[] frameInputs)
    {
        Name = name;
        Bytes = bytes;
        PowerOnSaveRam = powerOnSaveRam;
        FrameInputs = frameInputs;
    }

    /// <summary>Short label used in replay messages.</summary>
    public string Name { get; }
    public byte[] Bytes { get; }
    public byte[] PowerOnSaveRam { get; }

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
        return new ReplayMovie(name, movie, ReadSmvResetSaveRam(movie), inputs);
    }

    /// <summary>
    /// Extracts the 8 KiB cartridge SRAM that a reset-start Snes9x v4/v5 movie embeds in
    /// place of a snapshot. Snapshot-start movies are rejected: they need state import.
    /// </summary>
    private static byte[] ReadSmvResetSaveRam(byte[] movie)
    {
        const int MovieOptionsOffset = 0x15, StateOffsetField = 0x18, ControllerOffsetField = 0x1c;
        const byte StartFromReset = 0x01;
        uint version = BinaryPrimitives.ReadUInt32LittleEndian(movie.AsSpan(4));
        if (version is not (4 or 5))
            throw new InvalidDataException($"SMV version {version} SRAM layout is not supported.");
        if ((movie[MovieOptionsOffset] & StartFromReset) == 0)
            throw new InvalidDataException("Movie starts from a snapshot, not from power-on reset.");
        int start = BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(StateOffsetField));
        int end = BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(ControllerOffsetField));
        using var gzip = new GZipStream(new MemoryStream(movie, start, end - start), CompressionMode.Decompress);
        using var sram = new MemoryStream();
        gzip.CopyTo(sram);
        if (sram.Length < SuperMetroidAddressSpace.SaveRamByteCount)
            throw new InvalidDataException("Embedded movie SRAM is shorter than the cartridge's 8 KiB.");
        return sram.GetBuffer().AsSpan(0, SuperMetroidAddressSpace.SaveRamByteCount).ToArray();
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
        return new ReplayMovie(name, movie, saveRam, inputs.ToArray());
    }
}
