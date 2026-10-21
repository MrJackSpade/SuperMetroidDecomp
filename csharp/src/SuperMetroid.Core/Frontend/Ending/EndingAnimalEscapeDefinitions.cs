using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;
using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Bank-$8B/$8C definitions for the rescued animals' escape pod ($EF21).</summary>
internal sealed class EndingAnimalEscapeDefinitions : IIntroCinematicSpritePresentation
{
    /// <summary>Shared presenter for the rescue pod's four frames.</summary>
    internal static readonly EndingAnimalEscapeDefinitions Presentation = new();
    /// <summary>$8B:DD98 tests event $0F, then $DDA7 forces native byte slot $04.</summary>
    internal const int NativeSlot = 2;
    /// <summary>$8B:EF99-EFA2 initializes both pod coordinates to $0080.</summary>
    internal const ushort Origin = 0x0080;
    /// <summary>$8B:EFA5 selects OBJ palette seven.</summary>
    internal const ushort Palette = 0x0e00;
    /// <summary>$8B:EFAB initializes the actor's general-purpose timer (not a spawn delay).</summary>
    internal const ushort GeneralTimer = 0x0104;
    /// <summary>$8B:EFB6 adds $0080 to the 16-bit Y fraction each actor call.</summary>
    internal const ushort YFractionVelocity = 0x0080;
    /// <summary>$8B:EFD2 advances one horizontal pixel per actor call.</summary>
    internal const ushort XVelocity = 1;
    /// <summary>$8B:EFD8 deletes the actor when its center reaches $0110.</summary>
    internal const ushort DeleteX = 0x0110;
    /// <summary>The poses of the pod's four single-small-OBJ frames, centered at (-4,-4), in cycle order.</summary>
    private static readonly CinematicSpritePose[] FramePoses = [new(155), new(156), new(157), new(158)];
    /// <summary>Centers the eight-pixel pod sprite four pixels left and above its actor origin.</summary>
    private const ushort SpriteOffset = 4;
    /// <summary>$8C:BC43 starts the priority-three, palette-seven pod tiles $1E0-$1E3.</summary>
    private const ushort FirstAttributes = 0x3fe0;
    /// <summary>Draws the selected one-part pod frame centered on the actor using its authored attributes.</summary>
    [SuppressMessage("Design", "SME6426", Justification =
        "The pod frame's attribute word is authored in full, so no palette bits are inherited, and its one small object is placed through ScreenSprites; every other cinematic sprite presentation reads both values.")]
    public void Draw(CinematicSpritePose pose, ScreenSprites objects, Position16<OamPixels> origin, ushort paletteBits, bool originIsOnScreen)
    {
        int frame = Array.IndexOf(FramePoses, pose);
        if (frame < 0)
            throw new InvalidDataException($"Animal escape pose {pose.Index} is not one of its four frames.");
        Position16<OamPixels> corner = origin - new Offset16<OamPixels>((short)SpriteOffset, (short)SpriteOffset);
        objects.AddSmallObject(corner,
            (ushort)(FirstAttributes + frame));
    }
}
