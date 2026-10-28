using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The rescued animals' escape pod's compositions, as the ending's sheet composes them (#1463).</summary>
internal sealed class EndingAnimalEscapeParts : ICinematicSpriteFrames
{
    /// <summary>The pod's four frames.</summary>
    internal static readonly EndingAnimalEscapeParts Sprites = new();
    /// <summary>Centers the eight-pixel pod sprite four pixels left and above its actor origin.</summary>
    private const int SpriteOffset = 4;
    /// <summary>
    /// $8C:BC43: the pod's four single-small-OBJ frames in cycle order, centred on the actor, showing the priority-three,
    /// palette-seven characters $1E0-$1E3.
    /// </summary>
    private static readonly Dictionary<CinematicSpritePose, SpriteComposition> frames = new()
    {
        [new(155)] = Pod(CharacterSheetGrid.Cell(30, 0)),
        [new(156)] = Pod(CharacterSheetGrid.Cell(30, 1)),
        [new(157)] = Pod(CharacterSheetGrid.Cell(30, 2)),
        [new(158)] = Pod(CharacterSheetGrid.Cell(30, 3)),
    };
    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingAnimalEscape;
    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;
    /// <summary>A pod frame: its one small piece of <paramref name="character"/> in palette seven, priority three, centred on the actor.</summary>
    private static SpriteComposition Pod(int character) => new(
        [new CompiledSpritePart(SpritePart.Create(Offset16<ScreenPixels>.Of(-SpriteOffset, -SpriteOffset), large: false, character, palette: 7, priority: 3),
            InheritPalette: false)]);
}
