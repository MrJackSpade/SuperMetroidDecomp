namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>One member of a Norfair Pipe Bug formation: its place and its role (#1840).</summary>
/// <param name="Place">The member's place; a killed member's place is still the formation's.</param>
/// <param name="Role">The member's part in the formation.</param>
internal readonly record struct NorfairPipeBugMember(RoomEnemySlot Place, NorfairPipeBugRole Role);
