using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>The rescued animals' escape pod's compositions, as the ending's sheet composes them (#1463).</summary>
internal sealed class EndingAnimalEscapeParts : ICinematicSpriteFrames
{
    /// <summary>The pod's two pictures.</summary>
    internal static readonly EndingAnimalEscapeParts Sprites = new();
    /// <summary>Centers the eight-pixel pod sprite four pixels left and above its actor origin.</summary>
    private const int SpriteOffset = 4;
    /// <summary>
    /// $8C:BC43: the pod's single-small-OBJ pictures, centred on the actor, showing the in-front, palette-seven
    /// characters $1E0 (the short trail) and $1E1 (the long trail; $1E2 and $1E3, which the flicker also steps through, hold
    /// the same pixels).
    /// </summary>
    private static readonly Dictionary<CinematicSpritePose, SpriteComposition> frames = new()
    {
        [CinematicSpritePose.EscapePodShortTrail] = Pod(CharacterSheetGrid.Cell(30, 0)),
        [CinematicSpritePose.EscapePodLongTrail] = Pod(CharacterSheetGrid.Cell(30, 1)),
    };
    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingAnimalEscape;
    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;
    /// <summary>A pod frame: its one small piece of <paramref name="character"/> in palette seven, in front of the room, centred on the actor.</summary>
    private static SpriteComposition Pod(int character) => new(
        [new CompiledSpritePart(SpritePart.Create(Offset16<ScreenPixels>.Of(-SpriteOffset, -SpriteOffset), large: false, character, palette: 7, depth: ActorDepth.InFront),
            InheritPalette: false)]);
}
