using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes a <see cref="SamusArmCannonCoverOrientation"/> in the extracted arm-cannon document by its key (#2556).</summary>
internal sealed partial class SamusArmCannonCoverOrientationJsonConverter()
    : InstalledKeyJsonConverter<SamusArmCannonCoverOrientation>(SamusArmCannonCoverOrientationKeys.Installed);
