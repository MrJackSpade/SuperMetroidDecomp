using System.Text.Json;

namespace Engine.Core;

/// <summary>
/// The save slots. It reads, writes, copies and clears them, each a plain text file in the saves folder. It holds no
/// live state of a game being played.
/// </summary>
public sealed class SaveFile
{
    private static readonly JsonSerializerOptions Format = new() { WriteIndented = true };

    private readonly string _folder;

    /// <summary>Save slots kept in <paramref name="folder"/>, an absolute path, of which there are <paramref name="slots"/>.</summary>
    public SaveFile(string folder, int slots)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(slots);
        _folder = Path.GetFullPath(folder);
        Slots = slots;
    }

    /// <summary>How many slots there are, numbered from 1.</summary>
    public int Slots { get; }

    /// <summary>Whether a slot holds a saved game.</summary>
    public bool Has(int slot) => File.Exists(FileOf(slot));

    /// <summary>Writes a saved game to a slot, replacing what it held.</summary>
    public void Write(int slot, SaveData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Directory.CreateDirectory(_folder);
        File.WriteAllText(FileOf(slot), JsonSerializer.Serialize(data, Format));
    }

    /// <summary>Reads the saved game a slot holds.</summary>
    /// <exception cref="InvalidOperationException">The slot is empty.</exception>
    /// <exception cref="InvalidDataException">The slot's file cannot be read as a saved game.</exception>
    public SaveData Read(int slot)
    {
        string file = FileOf(slot);
        if (!File.Exists(file))
            throw new InvalidOperationException($"Save slot {slot} is empty.");
        try
        {
            return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(file))
                ?? throw new InvalidDataException($"Save slot {slot} holds nothing that can be read.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Save slot {slot} cannot be read: {exception.Message}", exception);
        }
    }

    /// <summary>Copies one slot's saved game over another's.</summary>
    public void Copy(int from, int to) => Write(to, Read(from));

    /// <summary>Empties a slot. Emptying an empty slot changes nothing.</summary>
    public void Clear(int slot) => File.Delete(FileOf(slot));

    /// <summary>
    /// Whether the game these slots are of has been completed: a record kept beside the slots, which saving, copying
    /// and emptying a slot leave as it is.
    /// </summary>
    public bool GameCompleted => File.Exists(CompletedFile);

    /// <summary>Records that the game has been completed.</summary>
    public void MarkCompleted()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(CompletedFile, "");
    }

    /// <summary>Drops the record that the game has been completed. Dropping one that is not kept changes nothing.</summary>
    public void ForgetCompletion() => File.Delete(CompletedFile);

    /// <summary>Whether any slot holds a saved game.</summary>
    public bool HoldsAny => Enumerable.Range(1, Slots).Any(Has);

    /// <summary>The file that is the record of the game's completion.</summary>
    private string CompletedFile => Path.Combine(_folder, "completed");

    /// <summary>The file a slot is kept in.</summary>
    private string FileOf(int slot)
    {
        if (slot < 1 || slot > Slots)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, $"Slots are numbered 1 through {Slots}.");
        return Path.Combine(_folder, $"slot{slot}.json");
    }
}
