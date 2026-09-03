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
        LayerBlendingConfiguration configuration = (LayerBlendingConfiguration)value;
        ValidateDefined(configuration, sourceContext, cartridgeData: true);
        return configuration;
    }

    /// <summary>Rejects undefined configuration values supplied by host-side callers.</summary>
    public static void Validate(
        LayerBlendingConfiguration configuration,
        string parameterName) =>
        ValidateDefined(configuration, parameterName, cartridgeData: false);

    private static void ValidateDefined(
        LayerBlendingConfiguration configuration,
        string context,
        bool cartridgeData)
    {
        if (Enum.IsDefined(typeof(LayerBlendingConfiguration), configuration))
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
