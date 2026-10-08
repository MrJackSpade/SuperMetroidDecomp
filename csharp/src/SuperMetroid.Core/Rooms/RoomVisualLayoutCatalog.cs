using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Replaceable initial BG1/BG2 visual references for one decompressed room source.
/// Each word contains only the native ten-bit block index and two parent flip bits;
/// collision type and BTS remain in the application's unmodified level allocation.
/// </summary>
public sealed class RoomVisualLayout
{
    private readonly ushort[] foreground;
    private readonly ushort[] background;

    /// <summary>Copies and validates both initial visual planes for one room-level source, rejecting mismatched dimensions and any collision/type bits in the artwork words.</summary>
    /// <param name="sourceAddress">Room state's full 24-bit level-data source identity, used for artwork lookup rather than runtime cartridge access.</param>
    /// <param name="widthInBlocks">Positive row stride in native 16-pixel blocks.</param>
    /// <param name="heightInBlocks">Positive number of block rows; each plane must contain width times height words.</param>
    /// <param name="foregroundVisualWords">Row-major BG1 words: block selector bits 0..9 and parent horizontal/vertical flip bits 10..11 only.</param>
    /// <param name="backgroundVisualWords">Row-major BG2 words with the same permitted bit fields; collision/type and BTS data are not supplied here.</param>
    /// <exception cref="ArgumentOutOfRangeException">The source is outside 24 bits or a dimension is nonpositive.</exception>
    /// <exception cref="InvalidDataException">A plane has the wrong word count or includes bits 12..15.</exception>
    public RoomVisualLayout(int sourceAddress, int widthInBlocks, int heightInBlocks,
        ReadOnlySpan<ushort> foregroundVisualWords,
        ReadOnlySpan<ushort> backgroundVisualWords)
    {
        if ((uint)sourceAddress > 0xffffff)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(widthInBlocks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(heightInBlocks);
        int expected = checked(widthInBlocks * heightInBlocks);
        if (foregroundVisualWords.Length != expected || backgroundVisualWords.Length != expected)
            throw new InvalidDataException(
                $"Room layout ${sourceAddress:X6} needs {expected} visual words in each plane.");
        if (HasCollisionBits(foregroundVisualWords) || HasCollisionBits(backgroundVisualWords))
            throw new InvalidDataException(
                $"Room layout ${sourceAddress:X6} contains a collision/type bit in visual artwork.");

        SourceAddress = sourceAddress;
        WidthInBlocks = widthInBlocks;
        HeightInBlocks = heightInBlocks;
        foreground = foregroundVisualWords.ToArray();
        background = backgroundVisualWords.ToArray();
    }

    /// <summary>Full 24-bit room-level source identity shared by room states using the same initial artwork allocation.</summary>
    public int SourceAddress { get; }
    /// <summary>Row stride of each visual plane, measured in native 16-pixel blocks rather than 8-pixel SNES characters.</summary>
    public int WidthInBlocks { get; }
    /// <summary>Number of 16-pixel block rows represented by each visual plane; does not resize the compiled collision allocation.</summary>
    public int HeightInBlocks { get; }
    /// <summary>Copied initial BG1 block selectors and parent flips in row-major order, with collision/type bits excluded; runtime streaming combines them with the unmodified native type bits.</summary>
    public ReadOnlyMemory<ushort> ForegroundVisualWords => foreground;
    /// <summary>Copied initial BG2 block selectors and parent flips in row-major order, independent of native type bits, BTS, and subsequent scripted room changes.</summary>
    public ReadOnlyMemory<ushort> BackgroundVisualWords => background;

    private static bool HasCollisionBits(ReadOnlySpan<ushort> words)
    {
        foreach (ushort word in words)
            if ((word & 0xf000) != 0) return true;
        return false;
    }
}

/// <summary>Complete installed room-layout artwork selected by the room state's source.</summary>
public sealed class RoomVisualLayoutCatalog
{
    private readonly Dictionary<int, RoomVisualLayout> layouts;

    /// <summary>Copies a source-keyed installation and requires every supported room-level source to have a matching non-null layout.</summary>
    /// <param name="layouts">Initial visual layouts keyed by their own full 24-bit <see cref="RoomVisualLayout.SourceAddress"/>.</param>
    /// <exception cref="InvalidDataException">A value is null, its source differs from its key, or a required source is absent.</exception>
    public RoomVisualLayoutCatalog(IReadOnlyDictionary<int, RoomVisualLayout> layouts)
        : this(layouts, requireCompleteInstallation: true)
    {
    }

    private RoomVisualLayoutCatalog(IReadOnlyDictionary<int, RoomVisualLayout> layouts,
        bool requireCompleteInstallation)
    {
        ArgumentNullException.ThrowIfNull(layouts);
        this.layouts = new Dictionary<int, RoomVisualLayout>(layouts);
        foreach ((int source, RoomVisualLayout layout) in this.layouts)
            if (layout is null || layout.SourceAddress != source)
                throw new InvalidDataException(
                    $"Room visual layout key ${source:X6} must contain its matching nonnull layout.");
        if (requireCompleteInstallation)
            foreach (int source in RoomVisualLayoutSourceDefinitions.All)
                if (!this.layouts.ContainsKey(source))
                    throw new InvalidDataException(
                        $"Installed room visual layouts lack required source ${source:X6}.");
    }

    /// <summary>SHA-256 of selected room geometry and both ordered visual planes.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomVisualLayoutCatalog), content =>
    {
        foreach ((int source, RoomVisualLayout layout) in this.layouts.OrderBy(pair => pair.Key))
        {
            content.Append("source", source);
            content.Append("width", layout.WidthInBlocks);
            content.Append("height", layout.HeightInBlocks);
            content.AppendWords("foreground", layout.ForegroundVisualWords.Span);
            content.AppendWords("background", layout.BackgroundVisualWords.Span);
        }
    });

    /// <summary>Resolves installed initial artwork by the room state's level-data source, without selecting room state or reading cartridge bytes.</summary>
    /// <param name="sourceAddress">Full 24-bit source identity used as the installation key.</param>
    /// <returns>The matching validated visual layout.</returns>
    /// <exception cref="InvalidDataException">The requested source is not installed.</exception>
    public RoomVisualLayout Get(int sourceAddress) =>
        layouts.TryGetValue(sourceAddress, out RoomVisualLayout? layout)
            ? layout
            : throw new InvalidDataException(
                $"Installed room visual layouts lack source ${sourceAddress:X6}.");
}
