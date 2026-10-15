
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="LayerBlendingConfigurations"/> members production does not use.</summary>
internal static class LayerBlendingConfigurationsAccess
{
    /// <summary>Rejects undefined layer-blend selectors presented by host-side verification callers.</summary>
    extension(LayerBlendingConfigurations)
    {
        /// <summary>Rejects undefined configuration values supplied by host-side callers.</summary>
        internal static void Validate(
            LayerBlendingConfiguration configuration,
            string parameterName) =>
            LayerBlendingConfigurations.ValidateDefined(configuration, parameterName, cartridgeData: false);
    }
}
