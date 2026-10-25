namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's colours (#1405): the background and sprite colour sets and the owners, such as Samus's colours, that hold
/// some of the colours the sprites show. Writers write an owner's colours. The composition, every colour as it shows, is
/// what a frame is drawn in, and the cartridge's initial image is the one load that names colours by position.
/// </summary>
public sealed class GameplayColours
{
    /// <summary>The colours gameplay addresses by position.</summary>
    public const int ColorCount = 256;

    /// <summary>The owners, consulted in order: the explicit owners, then the sprite and background colour sets.</summary>
    private readonly IGameplayColourOwner[] owners;

    /// <summary>Every colour as the owners and the router hold it, rebuilt on each read of <see cref="Colors"/>.</summary>
    [NonSerialized]
    private RgbColor[]? composed;

    /// <summary>Routes the positions <paramref name="owners"/> hold to them, and the positions they do not to the sets.</summary>
    /// <param name="spriteSets">The sprite colour sets, which owners such as Samus's colours may also show.</param>
    /// <param name="owners">The owners consulted before the sets.</param>
    public GameplayColours(SpriteColourSets spriteSets, params IGameplayColourOwner[] owners)
    {
        SpriteSets = Ensure.NotNull(spriteSets);
        this.owners = [.. Ensure.NotNull(owners), SpriteSets, BackgroundSets];
    }

    /// <summary>A copy of every owner's colours as they are now.</summary>
    public ColourImage Capture() => new([.. owners.SelectMany(owner => owner.Stores)]);

    /// <summary>The sprite colour sets, which hold the sprite colours no other owner holds.</summary>
    public SpriteColourSets SpriteSets { get; }

    /// <summary>The background colour sets.</summary>
    public BackgroundColourSets BackgroundSets { get; } = new();

    /// <summary>Every colour as it shows, in the cartridge's colour order.</summary>
    public ReadOnlySpan<RgbColor> Colors
    {
        get
        {
            composed ??= new RgbColor[ColorCount];
            for (int position = 0; position < ColorCount; position++)
                composed[position] = Owner(position).At(position);
            return composed;
        }
    }

    /// <summary>
    /// Loads the cartridge's initial 256-colour image, the one load that names colours by position: each colour goes to
    /// the owner that holds its position.
    /// </summary>
    public void LoadCartridgeImage(ReadOnlySpan<RgbColor> image)
    {
        Ensure.LengthEqual(image, ColorCount);
        for (int position = 0; position < ColorCount; position++)
            Owner(position).Set(position, image[position]);
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
