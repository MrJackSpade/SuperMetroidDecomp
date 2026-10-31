namespace SuperMetroid.Core.Enemies;

/// <summary>Which reaction an extended-hitbox probe selects: Samus's touch or a projectile's shot.</summary>
internal enum HitboxContactKind
{
    /// <summary>The touch reaction.</summary>
    Touch,

    /// <summary>The shot reaction.</summary>
    Shot,
}
