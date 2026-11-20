namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// One of the cartridge's dead-monster row upload lists (#1406, #1539), named by the corpse whose rows it uploads (#2556),
/// which the extraction decodes to find the characters the corpse's rows show in. Each corpse rots its own rows in the game;
/// the list is only the import's way to find where they lie in its pictures. Corpses that look alike have their rows staged
/// apart, so each has its own list; which species variant names a list is its codec's
/// (<see cref="DeadMonsterRowUploadDefinitions"/>).
/// </summary>
internal enum CorpseRowUploadList
{
    /// <summary>The Sidehopper the Baby Metroid drains.</summary>
    SidehopperBabyMetroidVictim,
    /// <summary>A Sidehopper already dead when the room loads.</summary>
    SidehopperAlreadyDead,
    /// <summary>The first Zoomer lying on its back.</summary>
    ZoomerOnItsBack,
    /// <summary>The Zoomer lying upright.</summary>
    ZoomerUpright,
    /// <summary>The second Zoomer lying on its back, its rows staged apart from the first's.</summary>
    SecondZoomerOnItsBack,
    /// <summary>The Ripper lying level.</summary>
    RipperLevel,
    /// <summary>The Ripper lying tilted.</summary>
    RipperTilted,
    /// <summary>The first dead Skree.</summary>
    Skree,
    /// <summary>The second dead Skree, its rows staged apart from the first's.</summary>
    SecondSkree,
    /// <summary>The third dead Skree, its rows staged apart from the others'.</summary>
    ThirdSkree,
}
