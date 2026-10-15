namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Native physical component of a selected Crocomire tongue frame.</summary>
internal readonly record struct CrocomireTongueCollisionComponent(
CrocomireTongueHitboxList HitboxPointer);
