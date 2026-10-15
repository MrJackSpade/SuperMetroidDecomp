namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>One named, fixed extended-frame identity; collision data is not editable art.</summary>
internal readonly record struct EnemyExtendedFrameDefinition(
    byte Bank, ushort Pointer, string Name);
