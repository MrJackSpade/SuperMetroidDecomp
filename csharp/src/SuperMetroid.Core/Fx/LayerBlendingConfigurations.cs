namespace SuperMetroid.Core.Fx;

/// <summary>
/// Validation boundary for bank-$88's mutually exclusive layer-blending dispatcher index.
/// These values select routines from one jump table and are deliberately not flags.
/// </summary>
public static class LayerBlendingConfigurations
{
    /// <summary>Converts an FX-record byte into a proven bank-$88 configuration.</summary>
    public static LayerBlendingConfiguration FromRecordByte(byte value, string sourceContext)
    {
        if (!Enum.IsDefined((LayerBlendingConfiguration)value))
            throw new NotSupportedException(
                $"Layer-blending configuration ${value:X4} from {sourceContext} " +
                "is not translated.");
        return (LayerBlendingConfiguration)value;
    }
}
