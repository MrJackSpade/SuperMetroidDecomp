namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The installed charge-flare placement and sprite compositions (#633).</summary>
internal readonly record struct ChargeFlareArtwork
{
    /// <summary>Placement.</summary>
    public required SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacementCatalog? Placement { get; init; }

    /// <summary>Compositions.</summary>
    public required SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteCatalog? Compositions { get; init; }
}
