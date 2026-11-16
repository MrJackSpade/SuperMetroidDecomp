using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The colour effects the room's bosses show on gameplay's pictures (#2194): each boss attaches its own as it sets up, a
/// boss set up again replacing what it attached, and the room keeps them for its lifetime. The display reads them here,
/// never through a boss's species.
/// </summary>
[Rooms.RoomLifetime]
internal sealed class BossShadings
{
    /// <summary>The layers in the order their colour effects are laid over each other: a later one wins where they overlap.</summary>
    private static readonly BossShadingLayer[] Order =
    [
        BossShadingLayer.SporeSpawn, BossShadingLayer.Phantoon, BossShadingLayer.Draygon, BossShadingLayer.Crocomire,
        BossShadingLayer.KraidBackdrop, BossShadingLayer.Kraid,
    ];

    /// <summary>The colour effects attached so far, by layer.</summary>
    private readonly Dictionary<BossShadingLayer, IGameplayShadingSource> attached = [];

    /// <summary>Shows <paramref name="source"/> as <paramref name="layer"/>'s colour effects from now on.</summary>
    internal void Show(BossShadingLayer layer, IGameplayShadingSource source) => attached[layer] = Ensure.NotNull(source);

    /// <summary>The attached colour effects, in the order they are laid over each other.</summary>
    internal IEnumerable<IGameplayShadingSource> InOrder
    {
        get
        {
            foreach (BossShadingLayer layer in Order)
            {
                if (attached.TryGetValue(layer, out IGameplayShadingSource? source))
                    yield return source;
            }
        }
    }
}
