using System.IO.Compression;
using System.Text.Json;

namespace Game.Tests.Movies;

/// <summary>
/// One of the recorded movies as the repository keeps it (<c>csharp/test-fixtures/replay-traces/&lt;name&gt;</c>): the
/// game's own updates the movie comes to once hardware lag is left out, each with the buttons the cartridge read on
/// it, and with them the cartridge's record of chosen cells of its memory as each update ended. This is the oracle's
/// side of a movie's check: it names the cartridge's addresses, and nothing of the game learns them from it.
/// </summary>
/// <remarks>
/// The files are the Classic game's compact trace, read as they are: <c>compact-manifest.json</c>,
/// <c>updates.json.gz</c> (the converter's list of updates), <c>initial.wram.gz</c> (the cartridge's whole memory as
/// the movie's first update began) and <c>expected.bin.gz</c> (every record in the order captured: two 32-bit words,
/// the movie's frame and where the cartridge stood, then the layout's cells in ascending order of address, each
/// record's cells given as their difference from the record before). The layouts are <see cref="TraceLayouts"/>'s.
/// </remarks>
public sealed class MovieTrace : IDisposable
{
    private const int RecordHeaderBytes = 8;

    /// <summary>How much memory the cartridge has: what <see cref="InitialMemory"/> holds.</summary>
    public const int MemoryBytes = 0x20000;

    private readonly string _folder;
    private readonly (int Start, int Length)[] _ranges;
    private readonly Stream _records;
    private readonly byte[] _cells;
    private readonly byte[] _read;
    private readonly int[] _offsets;
    private int _recordsRead;

    private MovieTrace(string folder, string layout, IReadOnlyList<MovieUpdate> updates, int recordCount, bool startsFromReset, string movieSha256, Stream records)
    {
        (_folder, Layout, Updates, RecordCount, StartsFromReset, MovieSha256, _records) =
            (folder, layout, updates, recordCount, startsFromReset, movieSha256, records);
        Name = Path.GetFileName(Path.TrimEndingDirectorySeparator(folder));
        _ranges = TraceLayouts.Of(layout);
        int length = 0;
        _offsets = new int[_ranges.Length];
        for (int index = 0; index < _ranges.Length; index++)
        {
            _offsets[index] = length;
            length += _ranges[index].Length;
        }

        _cells = new byte[length];
        _read = new byte[RecordHeaderBytes + length];
    }

    /// <summary>The movie's name: its folder's.</summary>
    public string Name { get; }

    /// <summary>The name of the layout the records are kept in.</summary>
    public string Layout { get; }

    /// <summary>The movie's updates, in order.</summary>
    public IReadOnlyList<MovieUpdate> Updates { get; }

    /// <summary>How many records the trace holds.</summary>
    public int RecordCount { get; }

    /// <summary>Whether the movie begins as the console is switched on; otherwise it begins from a snapshot of the cartridge mid-game.</summary>
    public bool StartsFromReset { get; }

    /// <summary>The SHA-256 of the movie's own file, in capital hexadecimal digits.</summary>
    public string MovieSha256 { get; }

    /// <summary>The record last read (<see cref="ReadTo"/>): the movie's frame it was captured on. None before the first.</summary>
    public int RecordFrame { get; private set; }

    /// <summary>Opens the trace in <paramref name="folder"/>.</summary>
    /// <exception cref="InvalidDataException">The folder does not hold a compact trace of a known layout, or its files disagree.</exception>
    public static MovieTrace Open(string folder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(folder, "compact-manifest.json")));
        JsonElement about = manifest.RootElement;
        string layout = about.GetProperty("layout").GetString() ?? "";
        int payload = TraceLayouts.Of(layout).Sum(range => range.Length);
        if (about.GetProperty("payloadBytes").GetInt32() != payload)
            throw new InvalidDataException($"The trace in {folder} keeps {about.GetProperty("payloadBytes").GetInt32()} bytes a record; the layout \"{layout}\" comes to {payload}.");

        var updates = new List<MovieUpdate>();
        bool startsFromReset;
        string movieSha256;
        using (var packed = new GZipStream(File.OpenRead(Path.Combine(folder, "updates.json.gz")), CompressionMode.Decompress))
        using (JsonDocument list = JsonDocument.Parse(packed))
        {
            startsFromReset = list.RootElement.GetProperty("startsFromReset").GetBoolean();
            movieSha256 = list.RootElement.GetProperty("movieSha256").GetString() ?? "";
            foreach (JsonElement update in list.RootElement.GetProperty("updates").EnumerateArray())
                updates.Add(MovieUpdate.Read(update));
        }

        var records = new GZipStream(File.OpenRead(Path.Combine(folder, "expected.bin.gz")), CompressionMode.Decompress);
        return new MovieTrace(folder, layout, updates, about.GetProperty("recordCount").GetInt32(), startsFromReset, movieSha256, records);
    }

    /// <summary>The cartridge's whole memory as the movie's first update began: <see cref="MemoryBytes"/> bytes.</summary>
    /// <exception cref="InvalidDataException">The trace's starting memory is not that long.</exception>
    public byte[] InitialMemory()
    {
        using var packed = new GZipStream(File.OpenRead(Path.Combine(_folder, "initial.wram.gz")), CompressionMode.Decompress);
        using var whole = new MemoryStream();
        packed.CopyTo(whole);
        byte[] bytes = whole.ToArray();
        if (bytes.Length != RecordHeaderBytes + MemoryBytes)
            throw new InvalidDataException($"The starting memory of {Name} is {bytes.Length} bytes; {RecordHeaderBytes + MemoryBytes} were expected.");
        return bytes[RecordHeaderBytes..];
    }

    /// <summary>
    /// Reads on to the record numbered <paramref name="record"/>, which must not lie before the one last read: the
    /// records are kept as differences, so they are read in order, once.
    /// </summary>
    /// <exception cref="InvalidOperationException">The record lies before the one last read.</exception>
    /// <exception cref="InvalidDataException">The trace ends before the record.</exception>
    public void ReadTo(int record)
    {
        if (record < _recordsRead - 1)
            throw new InvalidOperationException($"Record {record} of {Name} lies before record {_recordsRead - 1}, which has been read: a trace is read in order.");
        while (_recordsRead <= record)
        {
            _records.ReadExactly(_read);
            RecordFrame = BitConverter.ToInt32(_read, 0);
            for (int index = 0; index < _cells.Length; index++)
                _cells[index] ^= _read[RecordHeaderBytes + index];
            _recordsRead++;
        }
    }

    /// <summary>The 16-bit cell at <paramref name="address"/> of the cartridge's memory, in the record last read.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The layout does not keep that cell.</exception>
    public int Word(int address)
    {
        int at = OffsetOf(address);
        return _cells[at] | (_cells[at + 1] << 8);
    }

    private int OffsetOf(int address)
    {
        for (int index = 0; index < _ranges.Length; index++)
        {
            (int start, int length) = _ranges[index];
            if (address >= start && address + 1 < start + length)
                return _offsets[index] + address - start;
        }

        throw new ArgumentOutOfRangeException(nameof(address), $"The layout \"{Layout}\" does not keep the cell ${address:X4}.");
    }

    /// <inheritdoc/>
    public void Dispose() => _records.Dispose();
}
