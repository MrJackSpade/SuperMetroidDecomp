namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's colours as the writers that still address them by position reach them (#1405): each position reads and
/// writes the colours of the owner that holds it. Every position has an owner: the explicit owners first, then the sprite
/// colour sets and the background colour sets. The router goes when no writer addresses colours by position.
/// </summary>
public sealed class GameplayColours : IPaletteColors
{
    /// <summary>The colours gameplay addresses by position.</summary>
    public const int ColorCount = 256;

    /// <summary>The owners, consulted in order: the explicit owners, then the sprite and background colour sets.</summary>
    private readonly IGameplayColourOwner[] owners;

    /// <summary>Every colour as the owners and the router hold it, rebuilt on each read of <see cref="Colors"/>.</summary>
    [NonSerialized]
    private Bgr555[]? composed;

    /// <summary>Routes the positions <paramref name="owners"/> hold to them, and the positions they do not to the sets.</summary>
    public GameplayColours(params IGameplayColourOwner[] owners) => this.owners = [.. Ensure.NotNull(owners), SpriteSets, BackgroundSets];

    /// <summary>The sprite colour sets, which hold the sprite colours no other owner holds.</summary>
    public SpriteColourSets SpriteSets { get; } = new();

    /// <summary>The background colour sets.</summary>
    public BackgroundColourSets BackgroundSets { get; } = new();

    /// <inheritdoc />
    public ReadOnlySpan<Bgr555> Colors
    {
        get
        {
            composed ??= new Bgr555[ColorCount];
            for (int position = 0; position < ColorCount; position++)
                composed[position] = Owner(position).At(position);
            return composed;
        }
    }

    /// <inheritdoc />
    public void SetColor(int index, Bgr555 color)
    {
        Ensure.BetweenInclusive(0, ColorCount - 1, index);
        Owner(index).Set(index, color);
    }

    /// <inheritdoc />
    public Rgba32 GetRgba(int index)
    {
        Ensure.BetweenInclusive(0, ColorCount - 1, index);
        return Owner(index).At(index).ToRgba32();
    }

    /// <inheritdoc />
    public void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0)
    {
        if ((bytes.Length & 1) != 0)
            throw new ArgumentException("Palette data must contain complete two-byte colours.", nameof(bytes));
        int count = bytes.Length / 2;
        if (destinationIndex < 0 || destinationIndex + count > ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex), "A palette load must remain within 256 colours.");
        for (int color = 0; color < count; color++)
            SetColor(destinationIndex + color, Bgr555.FromCgramPortWord(SnesLittleEndian.Word(bytes[color * 2], bytes[color * 2 + 1])));
    }

    /// <summary>The owner holding <paramref name="position"/>.</summary>
    private IGameplayColourOwner Owner(int position)
    {
        foreach (IGameplayColourOwner owner in owners)
        {
            if (owner.Holds(position))
                return owner;
        }
        throw new ArgumentOutOfRangeException(nameof(position), position, "No owner holds this gameplay colour.");
    }
}
