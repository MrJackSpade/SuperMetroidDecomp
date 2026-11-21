using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Which Torizo a Torizo is, Crateria's Bomb Torizo or Norfair's Golden Torizo (#2301, #2410): its entity supplies it as it
/// sets up, and it holds everything that tells the two apart in what they share: how each starts, where its death's drops
/// land, its artwork, the colours its statue starts in, how it walks with its face and walks on from a landing with it,
/// and the secret code its appearance answers.
/// </summary>
internal abstract partial class TorizoVariant
{
    /// <summary>A Torizo of the values given.</summary>
    /// <param name="start">Its starting place, first animation, processing and radii.</param>
    /// <param name="walker">How it walks with its face (#2472).</param>
    /// <param name="dropArena">The stretch of its room its death's drops scatter over.</param>
    /// <param name="artwork">The encounter whose artwork it shows.</param>
    /// <param name="statueColouring">The colours its statue starts in.</param>
    /// <param name="secretCode">The loadout its appearance grants while the player holds its chord, or null for none.</param>
    /// <param name="firstAttackMissiles">Samus's missiles below which it takes the first of its two attacks.</param>
    private protected TorizoVariant(TorizoInitializationDefinition start, TorizoWalker walker, TorizoDropArena dropArena, EncounterArtwork artwork,
        TorizoLookKind statueColouring, TorizoSecretCode? secretCode, int firstAttackMissiles)
    {
        FirstAttackMissiles = firstAttackMissiles;
        Start = start;
        Walker = walker;
        DropArena = dropArena;
        Artwork = artwork;
        StatueColouring = new TorizoLook(statueColouring);
        SecretCode = secretCode;
    }

    /// <summary>Crateria's Bomb Torizo.</summary>
    internal static TorizoVariant Bomb { get; } = new BombTorizoVariant();

    /// <summary>Norfair's Golden Torizo.</summary>
    internal static TorizoVariant Golden { get; } = new GoldenTorizoVariant();

    /// <summary>Its starting place, first animation, processing and radii (#1931).</summary>
    internal TorizoInitializationDefinition Start { get; }

    /// <summary>How it walks with its face: the turns its walk and its wait behind Samus end in (#2472).</summary>
    internal TorizoWalker Walker { get; }

    /// <summary>It takes the first of its two attacks while Samus carries fewer missiles than this.</summary>
    internal int FirstAttackMissiles { get; }

    /// <summary>The stretch of its room its death's drops scatter over (#2032).</summary>
    internal TorizoDropArena DropArena { get; }

    /// <summary>The encounter whose artwork it shows (#1538).</summary>
    internal EncounterArtwork Artwork { get; }

    /// <summary>The colours its statue starts in (#1512).</summary>
    internal TorizoLook StatueColouring { get; }

    /// <summary>The loadout its appearance grants Samus while the player holds its chord: Golden Torizo's, and none for Bomb Torizo.</summary>
    internal TorizoSecretCode? SecretCode { get; }

    /// <summary>
    /// Where a landing Torizo that still has its face walks on, given the choice's walk <paramref name="alternate"/>: its
    /// own walk, or null where it plays on past both of the choice's targets.
    /// </summary>
    internal abstract AnimationContinuation<TorizoBodySegment>? WalkOnFromLanding(TorizoAnimation alternate);
}
