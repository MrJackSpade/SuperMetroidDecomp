namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's colours by owner (#1405, #1464): the background and sprite colour sets, and the owners, such as Samus's
/// colours, that hold some of the colours the sprites show. Writers write an owner's colours. A frame shows each owner's
/// sets as they are when it is published, the sprite sets with the colours an owner holds for them laid over their own.
/// </summary>
public sealed class GameplayColours
{
    /// <summary>The owners, besides the sets, that hold some of the colours the sprites show.</summary>
    internal readonly IGameplayColourOwner[] owners;

    /// <summary>Keeps the sprite colour sets and the owners that hold some of the colours the sprites show.</summary>
    /// <param name="spriteSets">The sprite colour sets, which owners such as Samus's colours may also show.</param>
    /// <param name="owners">The owners whose colours some sprite sets show over their own.</param>
    public GameplayColours(SpriteColourSets spriteSets, params IGameplayColourOwner[] owners)
    {
        SpriteSets = Ensure.NotNull(spriteSets);
        this.owners = [.. Ensure.NotNull(owners)];
    }

    /// <summary>A copy of every owner's colours as they are now.</summary>
    public ColourImage Capture() => new([.. owners.SelectMany(owner => owner.Stores), SpriteSets.All, BackgroundSets.All]);

    /// <summary>The sprite colour sets, which hold the sprite colours no other owner holds.</summary>
    public SpriteColourSets SpriteSets { get; }

    /// <summary>The background colour sets.</summary>
    public BackgroundColourSets BackgroundSets { get; } = new();

    /// <summary>The background sets as a frame shows them now.</summary>
    public ShownColourSets ShownBackgrounds() => ShownColourSets.Of(BackgroundSets);

    /// <summary>The sprite sets as a frame shows them now, each with the colours the owners hold for it over its own.</summary>
    public ShownColourSets ShownSprites() => ShownColourSets.Of(SpriteSets, new ShownOverSprites(owners));

    /// <summary>Gives every owner its starting colours: the cartridge's initial colours, decoded by owner.</summary>
    public void LoadStarting(GameplayStartingColours starting)
    {
        Ensure.NotNull(starting);
        starting.Backgrounds.CopyTo(BackgroundSets);
        starting.Sprites.CopyTo(SpriteSets);
        foreach (IGameplayColourOwner owner in owners)
            owner.LoadStarting(starting);
    }

    /// <summary>The owners' colours laid over the sprite sets in order, the last owner's over the earlier's.</summary>
    /// <param name="owners">The owners.</param>
    private sealed class ShownOverSprites(IGameplayColourOwner[] owners) : IInkRowColours
    {
        /// <inheritdoc />
        public void Overlay(int inkRow, Span<RgbColor> colours)
        {
            foreach (IGameplayColourOwner owner in owners)
                owner.Overlay(inkRow, colours);
        }
    }
}
