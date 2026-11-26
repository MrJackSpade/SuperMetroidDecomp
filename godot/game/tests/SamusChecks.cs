using Engine.Tests;
using Game.Data;
using Godot;

namespace Game.Tests;

/// <summary>Checks of Samus as her scene sets her up on the engine's player controller.</summary>
public sealed class SamusChecks
{
    [Check]
    public void TheMorphBallSwitchesOnTheLowFormAndTheSpringBallItsJump()
    {
        Samus samus = MakeSamus();
        Expect.False(samus.MayCrawl);
        Expect.False(samus.MayJumpWhileCrawling);

        samus.Inventory.Grant(Equipment.MorphBall);
        Expect.True(samus.MayCrawl);
        Expect.False(samus.MayJumpWhileCrawling);

        samus.Inventory.Grant(Equipment.SpringBall);
        Expect.True(samus.MayJumpWhileCrawling);

        samus.Inventory.Switch(Equipment.MorphBall, false);
        Expect.False(samus.MayCrawl);
        samus.Free();
    }

    [Check]
    public void TheEquipmentHerSceneNamesIsEquipmentTheGameHas()
    {
        Samus samus = MakeSamus();
        foreach (string needed in new[] { samus.CrawlNeeds, samus.JumpWhileCrawlingNeeds })
            Expect.True(needed.Length == 0 || Equipment.All.Contains(needed), needed);
        samus.Free();
    }

    /// <summary>Samus, as her scene makes her.</summary>
    private static Samus MakeSamus() => ResourceLoader.Load<PackedScene>("res://game/samus/samus.tscn").Instantiate<Samus>();
}
