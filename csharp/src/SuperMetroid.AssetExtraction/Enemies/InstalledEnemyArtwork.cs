using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy directory as the game reads it (#1636): every installed enemy frame's display and Kraid's eye
/// colours, read from the installed frame and eye colour documents, and the identity of the directory's installed documents.
/// The enemies', bosses' and areas' characters and colour documents are import data the installation never holds
/// (<see cref="ImportEnemyArtwork"/>, #1637); the game draws the sheets composed from them.
/// </summary>
public sealed class InstalledEnemyArtwork
{
    /// <summary>Binds the frames' displays and Kraid's eye colours, with the identity of the documents installed beside them.</summary>
    /// <param name="frameDisplays">Every installed enemy frame as gameplay draws it.</param>
    /// <param name="kraidEyes">Kraid's installed eye colours, which gameplay's eye glow reads back.</param>
    /// <param name="directoryIdentity">The identity of every document the enemy directory holds, as installed.</param>
    internal InstalledEnemyArtwork(EnemyFrameDisplays frameDisplays, KraidEyeColours kraidEyes, string directoryIdentity)
    {
        FrameDisplays = Ensure.NotNull(frameDisplays);
        KraidEyes = Ensure.NotNull(kraidEyes);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryIdentity);
        DirectoryIdentity = directoryIdentity;
    }

    /// <summary>Every installed enemy frame as gameplay draws it, built from both installed enemy frame documents.</summary>
    public EnemyFrameDisplays FrameDisplays { get; }

    /// <summary>Kraid's installed eye colours, which gameplay's eye glow reads back (#1512, #1637).</summary>
    public KraidEyeColours KraidEyes { get; }

    /// <summary>
    /// The selected presentation of the installed enemy directory: every document it holds, framed by name in name order, so
    /// an edit to any of them changes it. It holds no character page (#1636); the characters reach the game only through
    /// the enemy and boss body sheets, whose identities cover them.
    /// </summary>
    public string DirectoryIdentity { get; }
}
