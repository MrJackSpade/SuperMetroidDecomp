using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Bank-$8B/$8C definitions for the rescued animals' escape pod ($EF21).</summary>
internal sealed class EndingAnimalEscapeDefinitions : ICinematicSpriteFrames
{
    /// <summary>The pod's four frames, as the ending's sheet composes them.</summary>
    internal static readonly EndingAnimalEscapeDefinitions Sprites = new();
    /// <summary>$8B:DD98 tests event $0F, then $DDA7 forces native byte slot $04.</summary>
    internal const int ForcedSlot = 2;
    /// <summary>$8B:EF99-EFA2 initializes both pod coordinates to $0080.</summary>
    internal const ushort Origin = 0x0080;
    /// <summary>$8B:EFA5 selects sprite palette seven.</summary>
    internal const int Palette = 7;
    /// <summary>$8B:EFAB initializes the actor's general-purpose timer (not a spawn delay).</summary>
    internal const ushort GeneralTimer = 0x0104;
    /// <summary>$8B:EFB6 adds $0080 to the 16-bit Y fraction each actor call.</summary>
    internal const ushort YFractionVelocity = 0x0080;
    /// <summary>$8B:EFD2 advances one horizontal pixel per actor call.</summary>
    internal const ushort XVelocity = 1;
    /// <summary>$8B:EFD8 deletes the actor when its center reaches $0110.</summary>
    internal const ushort DeleteX = 0x0110;
    /// <summary>Centers the eight-pixel pod sprite four pixels left and above its actor origin.</summary>
    private const int SpriteOffset = 4;
    /// <summary>
    /// $8C:BC43: the pod's four single-small-OBJ frames in cycle order, centred on the actor, showing the priority-three,
    /// palette-seven characters $1E0-$1E3.
    /// </summary>
    private static readonly Dictionary<CinematicSpritePose, SpriteComposition> frames = new()
    {
        [new(155)] = Pod(SnesCharacterGrid.Cell(30, 0)),
        [new(156)] = Pod(SnesCharacterGrid.Cell(30, 1)),
        [new(157)] = Pod(SnesCharacterGrid.Cell(30, 2)),
        [new(158)] = Pod(SnesCharacterGrid.Cell(30, 3)),
    };
    /// <inheritdoc/>
    CinematicSpriteFamily ICinematicSpriteFrames.Family => CinematicSpriteFamily.EndingAnimalEscape;
    /// <inheritdoc/>
    IReadOnlyDictionary<CinematicSpritePose, SpriteComposition> ICinematicSpriteFrames.Frames => frames;
    /// <summary>A pod frame: its one small piece of <paramref name="character"/> in palette seven, priority three, centred on the actor.</summary>
    private static SpriteComposition Pod(int character) => new(
        [new CompiledSpritePart(SpritePart.Create(-SpriteOffset, -SpriteOffset, large: false, character, palette: 7, priority: 3),
            InheritPalette: false)]);
}
