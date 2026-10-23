namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's colours as the writers that still address them by position reach them (#1405): the positions an owner
/// holds read and write that owner's colours, and the rest are held here. Each step of #1405 moves positions into an
/// owner; the router goes when it holds none.
/// </summary>
public sealed class GameplayColours : IPaletteColors
{
    /// <summary>The colours gameplay addresses by position.</summary>
    public const int ColorCount = 256;

    /// <summary>The colours no owner holds yet.</summary>
    private readonly Bgr555[] unowned = new Bgr555[ColorCount];

    /// <summary>The owners, consulted in order.</summary>
    private readonly IGameplayColourOwner[] owners;

    /// <summary>Every colour as the owners and the router hold it, rebuilt on each read of <see cref="Colors"/>.</summary>
    [NonSerialized]
    private Bgr555[]? composed;

    /// <summary>Routes the positions <paramref name="owners"/> hold to them.</summary>
    public GameplayColours(params IGameplayColourOwner[] owners) => this.owners = Ensure.NotNull(owners);

    /// <inheritdoc />
    public ReadOnlySpan<Bgr555> Colors
    {
        get
        {
            composed ??= new Bgr555[ColorCount];
            for (int position = 0; position < ColorCount; position++)
                composed[position] = Owner(position) is { } owner ? owner.At(position) : unowned[position];
            return composed;
        }
    }

    /// <inheritdoc />
    public void SetColor(int index, Bgr555 color)
    {
        Ensure.BetweenInclusive(0, ColorCount - 1, index);
        if (Owner(index) is { } owner)
            owner.Set(index, color);
        else
            unowned[index] = color;
    }

    /// <inheritdoc />
    public Rgba32 GetRgba(int index)
    {
        Ensure.BetweenInclusive(0, ColorCount - 1, index);
        return (Owner(index) is { } owner ? owner.At(index) : unowned[index]).ToRgba32();
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

    /// <summary>The owner holding <paramref name="position"/>, or null where the router holds it.</summary>
    private IGameplayColourOwner? Owner(int position)
    {
        foreach (IGameplayColourOwner owner in owners)
        {
            if (owner.Holds(position))
                return owner;
        }
        return null;
    }
}
