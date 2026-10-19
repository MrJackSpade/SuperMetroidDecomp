namespace SuperMetroid.Core.Hardware;

/// <summary>
/// What one OBJ character upload showed: a named owned artwork identity, or the bytes an owned staging surface held when
/// it was uploaded. Installed artwork is named, never copied, so saved state carries only its identity; domains add
/// their own named variants, which resolve through the installed gameplay artwork.
/// </summary>
public abstract record ObjCharacterSource
{
    /// <summary>The source's bytes.</summary>
    /// <param name="resolver">Resolves installed artwork identities.</param>
    public abstract ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver);
}
