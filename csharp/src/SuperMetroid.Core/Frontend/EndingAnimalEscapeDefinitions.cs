using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B/$8C definitions for the rescued animals' escape pod ($EF21).</summary>
internal sealed class EndingAnimalEscapeDefinitions : IIntroCinematicSpritePresentation
{
    /// <summary>Shared presentation instance used to draw and interpret the rescued-animal escape pod.</summary>
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
    /// <summary>$8B:ECD9 starts the four one-frame spritemaps and unconditional loop.</summary>
    internal const ushort InstructionStart = 0xecd9;
    /// <summary>$8C:BC41-BC56 defines four single-small-OBJ frames, centered at (-4,-4).</summary>
    private const ushort FrameStart = 0xbc41;
    /// <summary>Encoded byte distance between consecutive seven-byte animal spritemap entries.</summary>
    private const ushort FrameStride = 7;
    /// <summary>Number of one-sprite frames in the escape-pod animation.</summary>
    private const int FrameCount = 4;
    /// <summary>Subtracts four pixels from the actor position to place each sprite around its center.</summary>
    private const ushort SpriteOffset = 4;
    /// <summary>$8C:BC43 starts the priority-three, palette-seven pod tiles $1E0-$1E3.</summary>
    private const ushort FirstAttributes = 0x3fe0;

    /// <summary>Encoded instruction words for the four one-frame spritemaps and their unconditional loop.</summary>
    private static ReadOnlySpan<ushort> Program =>
        [1, 0xbc41, 1, 0xbc48, 1, 0xbc4f, 1, 0xbc56, 0x94bc, InstructionStart];

    /// <summary>Reads one aligned instruction word from the native animal escape instruction list.</summary>
    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - InstructionStart;
        if (offset < 0 || offset >= Program.Length * 2 || (offset & 1) != 0)
            throw new InvalidDataException($"Animal escape instruction $8B:{pointer:X4} is outside its list.");
        return Program[offset / 2];
    }

    /// <summary>Draws the requested native escape-pod spritemap as a small OBJ at the actor's centered position.</summary>
    public void Draw(ushort pointer, OamBuffer oam, ushort x, ushort y, ushort paletteBits, bool originIsOnScreen)
    {
        int offset = pointer - FrameStart;
        if (offset < 0 || offset % FrameStride != 0 || offset / FrameStride >= FrameCount)
            throw new InvalidDataException($"Animal escape sprite $8C:{pointer:X4} is outside its four frames.");
        oam.AddRawSmallSprite(unchecked((ushort)(x - SpriteOffset)), unchecked((ushort)(y - SpriteOffset)),
            (ushort)(FirstAttributes + offset / FrameStride));
    }
}
