using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One visual component of an extended enemy frame as the import pipeline composes it, without hitbox metadata: where it
/// is placed and the OAM parts its sheet picture is composed from.
/// </summary>
/// <param name="Offset">Signed displacement of the component's spritemap from the enemy's on-screen origin.</param>
/// <param name="Parts">The component's OAM parts.</param>
internal readonly record struct EnemyExtendedDrawComponent(
    Offset16<SpritePixels> Offset, EnemySpritemapParts Parts);
