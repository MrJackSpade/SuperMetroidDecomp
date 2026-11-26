using Engine.Actors;
using Engine.Collision;
using Engine.Content;

namespace Game.Enemies;

/// <summary>
/// A Space Pirate of any kind. What Samus and her shots meet of one is not one box about its middle but the boxes of
/// the picture it shows: a narrow one down its body and a small one where its claw is, or one box in a somersault.
/// </summary>
/// <remarks>
/// Its data's "poses" (<see cref="ColliderPoses"/>) place its hurtboxes "Part1" and "Part2" by picture, and each step
/// of its timelines begins with the cue "pose:" and the picture's name. A picture the cartridge gives no boxes is a
/// pose with none: nothing meets the pirate while it shows.
/// </remarks>
public abstract partial class SpacePirate : Enemy
{
    private ColliderPoses? _poses;

    /// <summary>The pose it is in, by name; empty before its first.</summary>
    protected string Pose => _poses?.Current ?? "";

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _poses = ColliderPoses.Read(data.Get("poses"), this);
    }

    /// <summary>Takes the pose a cue names, if the cue is one of a pose; whether it was.</summary>
    protected bool TakePose(string cue)
    {
        const string pose = "pose:";
        if (!cue.StartsWith(pose, StringComparison.Ordinal))
            return false;
        (_poses ?? throw new InvalidOperationException($"The data of the pirate \"{Name}\" has not been read.")).Take(cue[pose.Length..]);
        return true;
    }
}
