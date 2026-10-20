using SuperMetroid.Core.Escape;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>The control words of an escape typewriter program between its lines' character bytes.</summary>
internal enum EscapeTypewriterCommand : ushort
{
    /// <summary>The program ends.</summary>
    End = EscapeTypewriterRomData.End,

    /// <summary>A new line starts at the destination word that follows.</summary>
    Destination = EscapeTypewriterRomData.Destination,
}
