namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Calculated repeated-row identities in the three suit-loading palettes.</summary>
/// <remarks>Each suit's loading program contains nine
/// sixteen-color records. Every even record repeats the normal palette; record3
/// repeats record1. Records5 and7 contain distinct shades. The mapping is checked
/// against complete original rows, including transparent entries, for #1165.
/// The remaining25 whole words are22 distinct normal sprite inks plus three
/// transparent payloads ($3800/$0000/$2003). Opaque indices select categorical
/// painted pixels, not a numerical brightness/time input; native $91:DD5B
/// copies the same normal art from $9B:9400/9520/9800. An index formula would
/// only recite those chosen inks, the #1165 nonsense exception. OBJ rendering
/// skips index zero before palette lookup, so its differing source payloads
/// have no functional color-generation basis either. All432 original loading
/// words independently match their normal/speed-boost source art. Repeated
/// rows, suit sharing, shade transforms and derived endpoint channels are
/// calculated separately; LoadingPaletteInputView documents the nine retained
/// tint components. No complete generated palette row is cached. Other
/// full-body palette families still require their own review.</remarks>
public static class LoadingPaletteColorDefinitions
{
    /// <summary>The final record, which repeats the normal palette.</summary>
    internal const int FinalRow = 8;
}
