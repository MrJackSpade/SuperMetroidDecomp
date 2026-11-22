using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>A scroll extension's setup (#2481): what it writes to extend the trigger beside it in its direction.</summary>
/// <param name="Extension">The collision and the step back toward the trigger the extension writes.</param>
internal sealed partial record ScrollExtensionSetup(ScrollExtension Extension) : RoomObjectPlacementSetup
{
    /// <inheritdoc/>
    internal override RoomObjectKind Kind => RoomObjectKindDefinitions.ScrollExtensions[Extension];

    /// <inheritdoc/>
    internal override void SetUp(IRoomObjectFamilySetups setups) => setups.SetUpScrollExtension(Extension);
}
