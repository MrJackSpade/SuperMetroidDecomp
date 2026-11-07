namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The semantic state of the PLM family a slot belongs to, bound where the family sets the slot up. Families are
/// mutually exclusive: a slot carries at most one, and every spawn clears the previous one.
/// </summary>
internal abstract class RoomObjectFamilyState
{
    /// <summary>The family this state belongs to.</summary>
    internal abstract RoomObjectFamily Family { get; }
}
