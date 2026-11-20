namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The role each enemy of an Evir plays as the room lists it (#2556).</summary>
internal enum EvirMember
{
    /// <summary>The body, which bobs and faces Samus.</summary>
    Body,

    /// <summary>The arms, which follow the body's facing and position.</summary>
    Arms,

    /// <summary>The spit the body shoots.</summary>
    Spit,
}
