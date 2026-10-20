
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="LayerBlendingConfigurations"/> members production does not use.</summary>
internal static class LayerBlendingConfigurationsAccess
{
    /// <summary>Rejects undefined layer-blend selectors presented by host-side verification callers.</summary>
    extension(LayerBlendingConfigurations)
    {
        /// <summary>Rejects undefined configuration values supplied by host-side callers.</summary>
        /// <remarks>The enum's defined members are exactly the even word offsets 0..$34 of the bank-$88 dispatcher at
        /// $88:803E..8072, every slot named, so definedness is the dispatcher's domain.</remarks>
        internal static void Validate(
            LayerBlendingConfiguration configuration,
            string parameterName)
        {
            if (!Enum.IsDefined(configuration))
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    configuration,
                    "The value is not a translated bank-$88 layer-blending configuration.");
        }
    }
}
