using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated $8D:EB45..EC4D surface-lightning colors belonging to definition $F765.</summary>
internal static class CrateriaLightningColorDefinitions
{
    /// <summary>$8D:EB43..EC3D: thirteen surface-lightning records, each containing eight colors.</summary>
    internal static CrateriaLightningPaletteFxProgramDefinition SurfaceProgram =>
        CrateriaLightningPaletteFxProgramMechanicsDefinitions.All[0];

    internal static bool TryCoordinates(ushort pointer, out int frame, out int color)
    {
        var program = SurfaceProgram;
        for (frame = 0; frame < program.Frames.Count; frame++)
        {
            int offset = pointer - program.ColorPointer(frame, 0);
            if ((uint)offset < program.ColorsPerFrame * 2 && (offset & 1) == 0)
            {
                color = offset / 2;
                return true;
            }
        }
        frame = color = 0;
        return false;
    }

    /// <summary>
    /// $8D:EB45..EC4D: four equally spaced RGB5 brightness phases and a white crest.
    /// Record phases ascend zero through four, descend to zero, then descend four to one.
    /// Each nonwhite row darkens by one RGB5 unit per color position.
    /// </summary>
    internal static bool TryCalculatedColor(ushort pointer, out ushort color)
    {
        color = 0;
        if (!TryCoordinates(pointer, out int frame, out int index))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : 13 - frame;
        color = phase == 4 ? (ushort)0x7fff : (ushort)(0x2d6c + 0x18c6 * phase - 0x0421 * index);
        return true;
    }
}
