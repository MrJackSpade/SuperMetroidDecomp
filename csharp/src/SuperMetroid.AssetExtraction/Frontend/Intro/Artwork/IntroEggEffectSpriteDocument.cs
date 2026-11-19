using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Mutable JSON authoring data for SR388's six shell fragments, moving slime, and four slime-impact frames; actor physics and instruction timing are not stored here.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the egg effect's packed sprite parts, which only the import reads to compose its pictures; IntroCinematicArtworkFiles.Extract writes it to the opening cinematic's import data (its CompositionFiles), which the installer deletes before it publishes.")]
public sealed record IntroEggEffectSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="IntroEggEffectSpriteFormat.Version"/> when loaded or written.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the caller-owned frame arrays keyed by the shell pieces (shell-upper-left, shell-upper-right, shell-left, shell-centre, shell-right, shell-lower), slime-moving, and slime-impact-0..slime-impact-3; array order determines OAM draw order.</summary>
    /// <remarks>Parts use pixel offsets and 8- or 16-pixel OBJ sizes within a 16-column by 32-row tile atlas. A null palette inherits the drawing actor's selector. Loading compiles and copies these values rather than retaining the mutable arrays.</remarks>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
