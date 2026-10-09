using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Calculated $8D:EB45..EC4D surface-lightning colors belonging to definition $F765.</summary>
internal static class CrateriaLightningColorDefinitions
{
    /// <summary>$8D:EB43..EC3D: thirteen surface-lightning records, each containing eight colors.</summary>
    internal static CrateriaLightningPaletteFxProgramDefinition SurfaceProgram =>
        CrateriaLightningPaletteFxProgramMechanicsDefinitions.All[0];

    /// <summary>Maps an even color-data address in the surface-lightning table to its record and color slot.</summary>
    /// <param name="pointer">Address to locate in the surface-lightning palette data.</param>
    /// <param name="frame">Receives the zero-based palette-record index when the address is owned by this table; otherwise receives zero.</param>
    /// <param name="color">Receives the zero-based color index within that record when the address is owned by this table; otherwise receives zero.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> identifies an aligned color word in the surface-lightning table.</returns>
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
    /// <summary>$8D:EC76..ED6A, unused dark-lightning definition $F769: fourteen seven-color records.</summary>
    internal static CrateriaLightningPaletteFxProgramDefinition DarkProgram =>
        CrateriaLightningPaletteFxProgramMechanicsDefinitions.All[1];

    /// <summary>Maps an even color-data address in the dark-lightning table to its record and color slot.</summary>
    /// <param name="pointer">Address to locate in the dark-lightning palette data.</param>
    /// <param name="frame">Receives the zero-based palette-record index when the address is owned by this table; otherwise receives zero.</param>
    /// <param name="color">Receives the zero-based color index within that record when the address is owned by this table; otherwise receives zero.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> identifies an aligned color word in the dark-lightning table.</returns>
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
    internal static bool TryCalculatedDarkColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> inputs, out ushort color)
    {
        color = 0;
        if (!TryDarkCoordinates(pointer, out int frame, out int index) || frame == 0 ||
            !inputs.TryGetValue(DarkProgram.ColorPointer(0, index), out ushort first))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : frame == 9 ? 0 : 14 - frame;
        int red = Math.Max(0, (first & 31) - 5 * phase);
        int green = Math.Max(0, ((first >> 5) & 31) - 5 * phase);
        int blue = Math.Max(0, ((first >> 10) & 31) - 5 * phase);
        color = (ushort)(red | green << 5 | blue << 10);
        return true;
    }
}
