using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The key each enemy projectile frame's picture has in the installed spawned-object sheets (#2370): its document name
/// (<see cref="EnemyProjectileFrameNameDefinitions"/>) in Pascal case, as an enemy frame's is (<see cref="EnemyFrameSheetKeys"/>).
/// The key stays derived from the document name, so renaming a frame's identity never changes a key on disk.
/// </summary>
internal static class EnemyProjectileFrameSheetKeys
{
    /// <summary>Each frame's key.</summary>
    private static readonly IdentityTable<EnemyProjectileFrameId, string> Keys =
        new(frame => EnemyFrameSheetKeys.PascalCase(EnemyProjectileFrameNameDefinitions.Names[frame]));

    /// <summary>Each frame by its document name.</summary>
    private static readonly FrozenDictionary<string, EnemyProjectileFrameId> ByName = EnemyProjectileFrameNameDefinitions.Names
        .ToFrozenDictionary(entry => entry.Value, entry => entry.Key, StringComparer.Ordinal);

    /// <summary>The frame whose document name is <paramref name="documentName"/>.</summary>
    /// <exception cref="InvalidDataException">No projectile frame has that document name.</exception>
    internal static EnemyProjectileFrameId Named(string documentName) => ByName.TryGetValue(documentName, out EnemyProjectileFrameId frame)
        ? frame
        : throw new InvalidDataException($"No enemy projectile frame is named {documentName}.");

    /// <summary>The installed sheet key of <paramref name="frame"/>.</summary>
    internal static string Of(EnemyProjectileFrameId frame) => Keys[frame];
}
