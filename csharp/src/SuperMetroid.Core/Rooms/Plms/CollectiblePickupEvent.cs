using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One cartridge-defined permanent pickup completed by the current PLM pass.</summary>
/// <param name="Kind">The item whose inventory effect and collected-item persistence have been applied.</param>
/// <param name="MessageBoxIndex">The native gameplay-message identity requested for the synchronous pickup notification.</param>
public readonly record struct CollectiblePickupEvent(
    InWorldCollectibleKind Kind,
    GameplayMessageId MessageBoxIndex);
