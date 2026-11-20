using Engine.Core;
using Engine.Ui;

namespace Engine.Tests.Core;

/// <summary>A cinematic for checks: it lasts a set number of updates and is followed by a stand-in mode.</summary>
public partial class StandInCinematic : Cinematic
{
    /// <summary>How many updates it lasts.</summary>
    public int Length { get; set; } = 5;

    /// <summary>The updates it has played, as each was numbered.</summary>
    public List<int> Played { get; } = [];

    /// <inheritdoc/>
    protected override bool Play(int elapsed)
    {
        Played.Add(elapsed);
        return elapsed == Length - 1;
    }

    /// <inheritdoc/>
    protected override GameMode Next() => new StandInMode { Name = "AfterCinematic" };
}
