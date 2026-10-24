using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable RGB5 visor colors shared by X-ray and room palette cycling.</summary>
public sealed class SamusVisorColorCatalog
{
    /// <summary>Only installed color edits differing from the reviewed $9B:A3C0 stock words.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> colors;

    /// <summary>Retains copied visor color edits independently of the mutable JSON document.</summary>
    internal SamusVisorColorCatalog(Dictionary<ColourCell, Bgr555> colors) => this.colors = colors;

    /// <summary>
    /// Resolves only the six installed even offsets. Corrupted or adjacent offsets
    /// return false so the caller can enforce its bounded palette contract.
    /// </summary>
    public bool TryResolveByteOffset(int byteOffset, out Bgr555 color)
    {
        if ((byteOffset & 1) == 0 && (uint)(byteOffset >> 1) < SamusVisorColorFormat.ColorCount)
        {
            color = Resolve(byteOffset >> 1);
            return true;
        }
        color = Bgr555.Black;
        return false;
    }

    /// <summary>Returns a selected visor color for the native owners that write Samus OBJ palette-four color four, CGRAM entry 196.</summary>
    /// <param name="index">Color ordinal 0..5, not a byte offset: 0..2 are X-ray widening colors and 3..5 are the steady X-ray/room-backdrop cycle.</param>
    /// <returns>Packed SNES BGR555 word from the independent edit or calculated stock definition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color ordinal is outside 0..5.</exception>
    public Bgr555 Resolve(int index)
    {
        if ((uint)index >= SamusVisorColorFormat.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors.TryGetValue(new ColourCell(0, index), out Bgr555 color) ? color : SamusVisorColorDefinitions.Color(index);
    }
}
