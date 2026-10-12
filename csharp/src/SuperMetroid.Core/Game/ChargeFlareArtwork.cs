using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The installed charge-flare placement and sprite compositions (#633).</summary>
internal readonly record struct ChargeFlareArtwork
{
    /// <summary>Placement.</summary>
    public required Assets.ChargeFlarePlacementCatalog? Placement { get; init; }

    /// <summary>Compositions.</summary>
    public required Assets.ChargeFlareSpriteCatalog? Compositions { get; init; }
}
