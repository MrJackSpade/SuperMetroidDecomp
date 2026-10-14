namespace SuperMetroid.Core.Game;

/// <summary>
/// The specialized <c>Enemy_ItemDrop_MiniKraid</c> tail publishes this request after the
/// type-three death explosion. Pickup selection remains owned by the common drop subsystem;
/// retaining the exact origin and table pointer prevents the enemy-specific event from being
/// silently lost while that shared system is translated.
/// </summary>
public readonly record struct FakeKraidDropRequest();
