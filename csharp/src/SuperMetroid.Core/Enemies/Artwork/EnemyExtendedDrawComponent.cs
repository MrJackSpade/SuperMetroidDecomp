namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One visual component of an extended enemy frame, without hitbox metadata.</summary>
/// <param name="Offset">Signed displacement of the component's spritemap from the enemy's on-screen origin.</param>
/// <param name="Parts">The component's OAM parts.</param>
internal readonly record struct EnemyExtendedDrawComponent(
    Offset16<SpritePixels> Offset, EnemySpritemapParts Parts);
