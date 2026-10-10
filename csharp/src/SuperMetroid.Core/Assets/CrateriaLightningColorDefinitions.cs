using SuperMetroid.Core.Hardware;
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
    internal static bool TryCalculatedColor(ushort pointer, out Bgr555 color)
    {
        color = Bgr555.Black;
        if (!TryCoordinates(pointer, out int frame, out int index))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : 13 - frame;
        // Native words $2D6C + $18C6 * phase - $0421 * index: channels (12, 11, 11) rise
        // six per phase and fall one per position, never borrowing between channels.
        int level = 6 * phase - index;
        color = phase == 4 ? Bgr555.White : new Bgr555(12 + level, 11 + level, 11 + level);
        return true;
    }
    /// <summary>$8D:EC76..ED6A, unused dark-lightning definition $F769: fourteen seven-color records.</summary>
    internal static CrateriaLightningPaletteFxProgramDefinition DarkProgram =>
        CrateriaLightningPaletteFxProgramMechanicsDefinitions.All[1];

    internal static bool TryDarkCoordinates(ushort pointer, out int frame, out int color)
    {
        var program = DarkProgram;
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
    /// $8D:EC78..ED78: each dark-lightning channel decreases five RGB5 units per phase,
    /// clamping at zero. Seven first-row colors remain supplied inputs; other independent
    /// samples are retained whenever they differ from the supplied-base calculation.
    /// </summary>
    internal static bool TryCalculatedDarkColor(ushort pointer, IReadOnlyDictionary<ushort, Bgr555> inputs, out Bgr555 color)
    {
        color = Bgr555.Black;
        if (!TryDarkCoordinates(pointer, out int frame, out int index) || frame == 0 ||
            !inputs.TryGetValue(DarkProgram.ColorPointer(0, index), out Bgr555 first))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : frame == 9 ? 0 : 14 - frame;
        int red = Math.Max(0, (first.Red) - 5 * phase);
        int green = Math.Max(0, (first.Green) - 5 * phase);
        int blue = Math.Max(0, (first.Blue) - 5 * phase);
        color = new Bgr555(red, green, blue);
        return true;
    }
}
