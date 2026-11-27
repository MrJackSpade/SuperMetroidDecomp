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
/// <c>updates.json.gz</c> (the converter's list of updates) and <c>expected.bin.gz</c> (every record in the order
/// captured: two 32-bit words, the movie's frame and where the cartridge stood, then the layout's cells in ascending
/// order of address, each record's cells given as their difference from the record before). Only the playthroughs'
/// layout is read here.
/// </remarks>
public sealed class MovieTrace : IDisposable
{
    /// <summary>The layout the three playthroughs and the Mother Brain movie are kept in.</summary>
    public const string PlaythroughLayout = "movie-desync";

    /// <summary>The cells of the playthroughs' layout: where each run of them starts in the cartridge's memory, and how long it is.</summary>
    private static readonly (int Start, int Length)[] PlaythroughRanges = BuildPlaythroughRanges();

    private readonly Stream _records;
    private readonly byte[] _cells;
    private readonly byte[] _read;
    private readonly int[] _offsets;
    private int _recordsRead;

    private MovieTrace(string name, IReadOnlyList<MovieUpdate> updates, int recordCount, Stream records)
    {
        (Name, Updates, RecordCount, _records) = (name, updates, recordCount, records);
        int length = 0;
        _offsets = new int[PlaythroughRanges.Length];
        for (int index = 0; index < PlaythroughRanges.Length; index++)
        {
            _offsets[index] = length;
            length += PlaythroughRanges[index].Length;
        }

        _cells = new byte[length];
        _read = new byte[RecordHeaderBytes + length];
    }

    private const int RecordHeaderBytes = 8;

    /// <summary>The movie's name: its folder's.</summary>
    public string Name { get; }

    /// <summary>The movie's updates, in order.</summary>
    public IReadOnlyList<MovieUpdate> Updates { get; }

    /// <summary>How many records the trace holds.</summary>
    public int RecordCount { get; }

    /// <summary>The record last read (<see cref="ReadTo"/>): the movie's frame it was captured on. None before the first.</summary>
    public int RecordFrame { get; private set; }

    /// <summary>Opens the trace in <paramref name="folder"/>.</summary>
    /// <exception cref="InvalidDataException">The folder does not hold a compact trace of the playthroughs' layout, or its files disagree.</exception>
    public static MovieTrace Open(string folder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(folder, "compact-manifest.json")));
        JsonElement about = manifest.RootElement;
        string layout = about.GetProperty("layout").GetString() ?? "";
        if (layout != PlaythroughLayout)
            throw new InvalidDataException($"The trace in {folder} is kept in the layout \"{layout}\"; only \"{PlaythroughLayout}\" is read.");
        int payload = PlaythroughRanges.Sum(range => range.Length);
        if (about.GetProperty("payloadBytes").GetInt32() != payload)
            throw new InvalidDataException($"The trace in {folder} keeps {about.GetProperty("payloadBytes").GetInt32()} bytes a record; the layout comes to {payload}.");

        var updates = new List<MovieUpdate>();
        using (var packed = new GZipStream(File.OpenRead(Path.Combine(folder, "updates.json.gz")), CompressionMode.Decompress))
        using (JsonDocument list = JsonDocument.Parse(packed))
        {
            foreach (JsonElement update in list.RootElement.GetProperty("updates").EnumerateArray())
                updates.Add(MovieUpdate.Read(update));
        }

        var records = new GZipStream(File.OpenRead(Path.Combine(folder, "expected.bin.gz")), CompressionMode.Decompress);
        return new MovieTrace(Path.GetFileName(Path.TrimEndingDirectorySeparator(folder)), updates, about.GetProperty("recordCount").GetInt32(), records);
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

    /// <summary>The 8-bit cell at <paramref name="address"/> of the cartridge's memory, in the record last read.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The layout does not keep that cell.</exception>
    public int Byte(int address) => _cells[OffsetOf(address)];

    private int OffsetOf(int address)
    {
        for (int index = 0; index < PlaythroughRanges.Length; index++)
        {
            (int start, int length) = PlaythroughRanges[index];
            if (address >= start && address < start + length)
                return _offsets[index] + address - start;
        }

        throw new ArgumentOutOfRangeException(nameof(address), $"The layout \"{PlaythroughLayout}\" does not keep the cell ${address:X4}.");
    }

    /// <inheritdoc/>
    public void Dispose() => _records.Dispose();

    /// <summary>
    /// The playthroughs' layout, as the Classic game's catalog of compared cells gives it
    /// (<c>ReplayTraceMemoryCatalog.MovieDesync</c>), in ascending order of address.
    /// </summary>
    private static (int Start, int Length)[] BuildPlaythroughRanges()
    {
        var ranges = new List<(int Start, int Length)>
        {
            (Cartridge.NmiCounter, 2),
            (Cartridge.Random, 2),
            (0x0629, 0x0650 - 0x0629),
            (Cartridge.Room, 2),
            (Cartridge.ViewXPart, Cartridge.ViewY + 2 - Cartridge.ViewXPart),
            (0x093f, 2),
            (Cartridge.EscapeTime, 0x0948 - Cartridge.EscapeTime),
            (Cartridge.GameState, 2),
            (0x099c, 2),
            (Cartridge.EquipmentOn, Cartridge.BeamsOwned + 2 - Cartridge.EquipmentOn),
            (Cartridge.Energy, Cartridge.Reserve + 2 - Cartridge.Energy),
            (0x0a1c, 2),
            (0x0a94, 0x0a98 - 0x0a94),
            (Cartridge.SamusX, Cartridge.SamusYPart + 2 - Cartridge.SamusX),
            (0x0b2c, 0x0b30 - 0x0b2c),
            (0x0b42, 0x0b4c - 0x0b42),
            (0x0b64, 0x0c2c - 0x0b64),
            (0x0dba, 0x0dc0 - 0x0dba),
        };
        for (int place = 0; place < Cartridge.EnemyPlaces; place++)
            ranges.Add((Cartridge.Enemies + place * Cartridge.EnemyStride, Cartridge.EnemyCellsKept));
        ranges.AddRange(
        [
            (0x0fa8, 0x0fae - 0x0fa8),
            (0x106a, 0x106e - 0x106a),
            (Cartridge.LaunchedKinds, Cartridge.LaunchedPlaces * 2),
            (Cartridge.LaunchedX, Cartridge.LaunchedPlaces * 2),
            (Cartridge.LaunchedY, Cartridge.LaunchedPlaces * 2),
            (0x1b23, 0x1bb3 - 0x1b23),
            (0x1f51, 2),
            (0x2000, 2),
            (0x2020, 20 * 7),
            (0x8026, 2),
        ]);
        ranges.Sort();
        return [.. ranges];
    }
}
