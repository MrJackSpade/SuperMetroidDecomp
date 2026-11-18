
namespace SuperMetroid.Core.Rooms;

/// <summary>Samus as room objects see her (#633): every room that steps its objects has her (#2379).</summary>
public interface IRoomSamusAccess
{
    /// <summary>The current Samus.</summary>
    IRoomSamus Current { get; }
}
