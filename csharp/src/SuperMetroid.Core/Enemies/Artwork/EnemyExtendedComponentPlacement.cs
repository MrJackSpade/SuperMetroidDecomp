namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Where one component of an extended enemy frame is drawn (#1463). Its picture is the installed enemy sheet's frame for
/// the component, selected by the frame and the component's position in the frame; no hitbox metadata comes with it.
/// </summary>
/// <param name="Offset">Signed displacement of the component's picture from the enemy's on-screen origin.</param>
internal readonly record struct EnemyExtendedComponentPlacement(Offset16<SpritePixels> Offset);
