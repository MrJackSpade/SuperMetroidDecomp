namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The three actions a Zebetite performs.</summary>
public enum ZebetiteAction
{
    /// <summary>A new barrier of two halves spawns its linked half; every barrier then waits out the door passage in the same update.</summary>
    SpawnLinkedHalf,
    /// <summary>Stays inert while Samus's passage through a door is under way, then becomes active in the same update.</summary>
    WaitForDoorTransition,
    /// <summary>Shows the pulse and its health's picture and regains one health an update up to <see cref="ZebetiteDefinitions.MaximumHealth"/>; at no health it is destroyed, and a destroyed primary half brings on the next barrier.</summary>
    Active,
}
