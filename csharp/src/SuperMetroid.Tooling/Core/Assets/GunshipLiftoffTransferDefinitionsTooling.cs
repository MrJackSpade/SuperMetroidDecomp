using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Development-tool members of <see cref="GunshipLiftoffTransferDefinitions"/>; never linked by player hosts.</summary>
internal static class GunshipLiftoffTransferDefinitionsTooling
{
    /// <summary>First $94:C800 character chunk, uploaded to VRAM word $7600.</summary>
    internal static GunshipLiftoffTransferDefinition First => GunshipLiftoffTransferDefinitions.Frame(0);
}
