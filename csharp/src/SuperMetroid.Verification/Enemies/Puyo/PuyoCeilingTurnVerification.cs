namespace SuperMetroid.Verification.Enemies.Puyo;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks whether a Puyo striking a ceiling turns back, from the writers <see cref="PuyoCeilingTurnFromLastDraw"/>
/// models: the room's load, the HUD's redraw, an enemy's draw, a sprite object's draw inside and outside the draw's
/// window, and the charge flare's draw (#1592).
/// </summary>
internal static class PuyoCeilingTurnVerification
{
    /// <summary>
    /// Confirms each writer's effect on a synthetic sequence: a room just loaded and a HUD just redrawn turn the Puyo back;
    /// an enemy's or in-window sprite object's draw after the HUD lets it keep its aim; an out-of-window sprite object
    /// changes nothing; the charge flare turns it back until the HUD next redraws, whatever is drawn after it.
    /// </summary>
    internal static void VerifyPuyoCeilingTurn()
    {
        var loaded = new PuyoCeilingTurnFromLastDraw();
        AssertTrue(loaded.TurnsBack, "the room's load leaves the Puyo turning back");
        loaded.EnemyDrawn();
        AssertTrue(loaded.TurnsBack, "after the room's load an enemy's draw still turns the Puyo back, until the HUD redraws");

        var turn = new PuyoCeilingTurnFromLastDraw();
        turn.HudRedrawn();
        AssertTrue(turn.TurnsBack, "the HUD's redraw turns the Puyo back");
        turn.EnemyDrawn();
        AssertTrue(!turn.TurnsBack, "an enemy drawn after the HUD's redraw lets the Puyo keep its aim");

        turn.HudRedrawn();
        turn.SpriteObjectDrawn(-17, 100);
        turn.SpriteObjectDrawn(288 - 16, 100);
        turn.SpriteObjectDrawn(100, -1);
        turn.SpriteObjectDrawn(100, 272);
        turn.SpriteObjectDrawn(short.MaxValue, 100);
        AssertTrue(turn.TurnsBack, "a sprite object outside the draw's window changes nothing");
        turn.SpriteObjectDrawn(-16, 0);
        AssertTrue(!turn.TurnsBack, "a sprite object at the window's top left corner lets the Puyo keep its aim");
        turn.HudRedrawn();
        turn.SpriteObjectDrawn(288 - 17, 271);
        AssertTrue(!turn.TurnsBack, "a sprite object at the window's bottom right corner lets the Puyo keep its aim");

        turn.HudRedrawn();
        turn.ChargeFlareDrawn();
        turn.EnemyDrawn();
        turn.SpriteObjectDrawn(0, 0);
        AssertTrue(turn.TurnsBack, "the charge flare drawn since the HUD's redraw turns the Puyo back whatever is drawn after it");
        turn.HudRedrawn();
        turn.EnemyDrawn();
        AssertTrue(!turn.TurnsBack, "the HUD's next redraw clears what the charge flare set");
        Console.WriteLine("Puyo ceiling turn: the room's load, HUD redraws, enemy, sprite-object and charge-flare draws match.");
    }

    /// <summary>Runs the focused Puyo ceiling-turn check.</summary>
    [VerificationCommand("--puyo-ceiling-turn")]
    internal static int PuyoCeilingTurnCommand()
    {
        VerifyPuyoCeilingTurn();
        return 0;
    }
}
