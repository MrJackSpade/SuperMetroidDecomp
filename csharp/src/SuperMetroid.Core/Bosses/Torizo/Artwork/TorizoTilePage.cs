namespace SuperMetroid.Core.Bosses.Torizo.Artwork;

/// <summary>The pages of installed Torizo characters, in the installed manifest and content-hash order.</summary>
internal enum TorizoTilePage
{
    /// <summary>Characters shared by the death and recovery swaps.</summary>
    SharedDeath,
    /// <summary>Alternating statue crumble characters.</summary>
    StatueCrumble,
    /// <summary>Left-facing attack and death characters.</summary>
    LeftAttack,
    /// <summary>Right-facing attack and death characters.</summary>
    RightAttack,
    /// <summary>Golden Torizo's awakening characters.</summary>
    GoldenAwakening,
    /// <summary>Golden Torizo's alternate left attack characters.</summary>
    GoldenLeftAttack,
    /// <summary>Golden Torizo's alternate right attack characters.</summary>
    GoldenRightAttack,
    /// <summary>Bomb Torizo's PLM-shown Chozo debris characters.</summary>
    ChozoDebris,
}
