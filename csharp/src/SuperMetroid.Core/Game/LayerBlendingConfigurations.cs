namespace SuperMetroid.Core.Game;

/// <summary>
/// Validation boundary for bank-$88's mutually exclusive layer-blending dispatcher index.
/// These values select routines from one jump table and are deliberately not flags.
/// </summary>
public static class LayerBlendingConfigurations
{
    /// <summary>Converts an FX-record byte into a proven bank-$88 configuration.</summary>
    public static LayerBlendingConfiguration FromCartridge(byte value, string sourceContext)
    {
        if (!Enum.IsDefined((LayerBlendingConfiguration)value))
            ValidateDefined((LayerBlendingConfiguration)value, sourceContext, cartridgeData: true);
        return (LayerBlendingConfiguration)value;
    }

    /// <summary>Accepts exactly the even word offsets0..$34 in $88:803E..8072.</summary>
    /// <remarks>The native dispatcher indexes27 consecutive two-byte pointers; every
    /// slot has a named identity, including unused routines and repeated targets, so the
    /// enum's defined members are exactly the dispatcher's domain. Independently
    /// checked against supported NTSC J/U v1.0 and pinned bank_88.asm for #1165.</remarks>
    private static void ValidateDefined(
        LayerBlendingConfiguration configuration,
        string context,
        bool cartridgeData)
    {
        if (Enum.IsDefined(configuration))
            return;

        if (cartridgeData)
        {
            throw new NotSupportedException(
                $"Layer-blending configuration ${(ushort)configuration:X4} from {context} " +
                "is not translated.");
        }

        throw new ArgumentOutOfRangeException(
            context,
            configuration,
            "The value is not a translated bank-$88 layer-blending configuration.");
    }
}
