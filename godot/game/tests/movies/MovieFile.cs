using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Game.Tests.Movies;

/// <summary>
/// A movie's own file, as its recorder wrote it: the buttons held on every frame of the movie. A trace's updates carry
/// their own buttons; the file is read for the frames the cartridge spends inside one update, reading the controller
/// itself: a message box's.
/// </summary>
public sealed class MovieFile
{
    /// <summary>The movie file of each trace, by the trace's name, from the repository's root.</summary>
    private static readonly Dictionary<string, string> Files = new(StringComparer.Ordinal)
    {
        ["full-100-percent"] = "csharp/test-fixtures/full-100-percent/Super Metroid 100%.smv",
        ["low-13-percent"] = "csharp/test-fixtures/low-13-percent/13_speedbooster.lsmv",
        ["lowc"] = "csharp/test-fixtures/contributed-movies/lowc.smv",
        ["mother-brain-glitch"] = "csharp/test-fixtures/issue-1275-movies/mother brain glitch.smv",
        ["ridley"] = "csharp/test-fixtures/issue-1266-ridley/Ridley fight showcase.smv",
    };

    private readonly int[] _frames;

    private MovieFile(int[] frames, bool holdsNoSavedGame) => (_frames, StartsWithNoSavedGame) = (frames, holdsNoSavedGame);

    /// <summary>
    /// Whether the cartridge the movie begins on, switched on, holds no saved game. One that begins from a snapshot
    /// says nothing of it here: its start is the snapshot's.
    /// </summary>
    public bool StartsWithNoSavedGame { get; }

    /// <summary>The cartridge's buttons held on <paramref name="frame"/> of the movie, as its controller word has them.</summary>
    public int ButtonsOn(int frame) => _frames[frame];

    /// <summary>Reads the movie file of <paramref name="trace"/>, from the repository at <paramref name="repository"/>.</summary>
    /// <exception cref="InvalidDataException">The file is not the movie the trace was made from, or is of a kind this does not read.</exception>
    public static MovieFile Of(MovieTrace trace, string repository)
    {
        ArgumentNullException.ThrowIfNull(trace);
        if (!Files.TryGetValue(trace.Name, out string? relative))
            throw new InvalidDataException($"No movie file is listed for the trace \"{trace.Name}\".");
        byte[] bytes = File.ReadAllBytes(Path.Combine(repository, relative));
        string sha = Convert.ToHexString(SHA256.HashData(bytes));
        if (!string.Equals(sha, trace.MovieSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"{relative} is not the movie the trace \"{trace.Name}\" was made from: its SHA-256 is {sha}.");
        if (bytes.AsSpan(0, 4).SequenceEqual("SMV\x1a"u8))
            return FromSnes9x(bytes);
        if (bytes.AsSpan(0, 4).SequenceEqual("PK\x03\x04"u8))
            return FromLsnes(bytes);
        throw new InvalidDataException($"{relative} is neither a Snes9x nor an lsnes movie.");
    }

    /// <summary>A Snes9x movie of one controller: a 16-bit word a frame, and before them the cartridge's kept memory or a snapshot.</summary>
    private static MovieFile FromSnes9x(byte[] movie)
    {
        const int FrameCountField = 0x10, ControllerCountField = 0x14, OptionsField = 0x15, StateOffsetField = 0x18, ControllerOffsetField = 0x1c;
        const byte StartsFromReset = 0x01;
        const int SavedBytes = 0x2000;
        if (movie[ControllerCountField] != 1)
            throw new InvalidDataException("Only movies of one controller are read.");
        int count = BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(FrameCountField));
        int offset = BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(ControllerOffsetField));
        var frames = new int[count + 1];
        for (int frame = 0; frame <= count; frame++)
            frames[frame] = BinaryPrimitives.ReadUInt16LittleEndian(movie.AsSpan(offset + 2 * frame));
        if ((movie[OptionsField] & StartsFromReset) == 0)
            return new MovieFile(frames, holdsNoSavedGame: false);

        // The kept memory the cartridge is switched on with, which Snes9x may have packed. A cartridge whose kept
        // memory is one byte throughout was never saved to.
        int start = BinaryPrimitives.ReadInt32LittleEndian(movie.AsSpan(StateOffsetField));
        byte[] saved = movie.AsSpan(start, offset - start).ToArray();
        if (saved.Length >= 2 && saved[0] == 0x1f && saved[1] == 0x8b)
        {
            using var packed = new GZipStream(new MemoryStream(saved), CompressionMode.Decompress);
            using var whole = new MemoryStream();
            packed.CopyTo(whole);
            saved = whole.ToArray();
        }

        if (saved.Length < SavedBytes)
            throw new InvalidDataException("The movie's kept memory is shorter than the cartridge's.");
        return new MovieFile(frames, saved.AsSpan(0, SavedBytes).IndexOfAnyExcept(saved[0]) < 0);
    }

    /// <summary>An lsnes movie recorded from switching on with one controller: a line of text a frame.</summary>
    private static MovieFile FromLsnes(byte[] movie)
    {
        using var archive = new ZipArchive(new MemoryStream(movie), ZipArchiveMode.Read);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (entry.FullName.StartsWith("moviesram", StringComparison.Ordinal) || entry.FullName.StartsWith("savestate", StringComparison.Ordinal))
                throw new InvalidDataException($"The lsnes movie's member {entry.FullName} is a start this does not read.");
        }

        ZipArchiveEntry input = archive.GetEntry("input") ?? throw new InvalidDataException("The lsnes movie has no input.");
        using var reader = new StreamReader(input.Open(), Encoding.UTF8);
        var frames = new List<int>();
        foreach (string raw in reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string line = raw.TrimEnd('\r');
            int bar = line.IndexOf('|');
            string pad = bar < 0 ? "" : line[(bar + 1)..];
            if (!line.StartsWith('F') || pad.Length != 12)
                throw new InvalidDataException($"The lsnes movie's input line {frames.Count} is not one frame of one controller.");
            // The buttons in the order B Y Select Start Up Down Left Right A X L R: the cartridge's word from its top bit down.
            int word = 0;
            for (int index = 0; index < 12; index++)
            {
                if (pad[index] != '.')
                    word |= 1 << (15 - index);
            }

            frames.Add(word);
        }

        return new MovieFile([.. frames], holdsNoSavedGame: true);
    }
}
