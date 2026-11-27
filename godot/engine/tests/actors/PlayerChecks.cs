using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Engine.Tests.World;

namespace Engine.Tests.Actors;

/// <summary>Checks of the player controller: its standard states and mechanics, as a player would see them.</summary>
public sealed class PlayerChecks
{
    private static readonly string[] OpenRoom =
    [
        "................", "................", "................", "................", "................", "################",
    ];

    [Check]
    public void ThePlayerArrivesStandingOnTheGround()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        Expect.True(world.Player.IsOnGround);
        Expect.True(world.Player.State is StandingState);
        Expect.Equal(99, world.Player.Inventory.Amount(Inventory.Energy));
    }

    [Check]
    public void HoldingRightRunsRightUpToTheTopSpeedAndLettingGoStops()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        world.Step(30, GameAction.Right);
        Expect.True(world.Player.State is RunningState);
        Expect.Equal(world.Player.Moves(PlayerState.RunningMoves).Top, world.Player.Velocity.X);
        Expect.True(world.Player.Position.X > Pixels.FromWhole(60));
        Expect.Equal(Facing.Right, world.Player.Facing);

        world.Step(30);
        Expect.True(world.Player.State is StandingState);
        Expect.True(world.Player.Velocity.X.IsZero);
    }

    [Check]
    public void AJumpRisesAndComesBackDownToTheSameGround()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        world.Press(GameAction.Jump);
        world.Step(1, GameAction.Jump);
        Expect.True(world.Player.State is JumpingState);
        Expect.True(world.Player.BodyRect.Bottom < 80);

        world.StepUntilLanded(GameAction.Jump);
        Expect.Equal(80, world.Player.BodyRect.Bottom);
        Expect.True(world.Player.State is StandingState or RunningState);
    }

    [Check]
    public void LettingGoOfTheJumpEarlyMakesALowerJump()
    {
        Pixels Highest(bool held)
        {
            using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
            world.Press(GameAction.Jump);
            Pixels highest = world.Player.Position.Y;
            for (int update = 0; update < 200; update++)
            {
                world.Step(1, held ? [GameAction.Jump] : []);
                if (world.Player.Position.Y < highest)
                    highest = world.Player.Position.Y;
                if (update > 2 && world.Player.IsOnGround)
                    break;
            }

            return highest;
        }

        Expect.True(Highest(held: true) < Highest(held: false));
    }

    [Check]
    public void AWallStopsTheRun()
    {
        using var world = new StandInWorld(
            "................", "................", "................", "......#.........", "......#.........", "################")
            .StartAt(40, 80).Setup();
        world.Step(120, GameAction.Right);
        // The wall's left face is at pixel 96 and the player reaches 5 to the right of their middle.
        Expect.Equal(Pixels.FromWhole(91), world.Player.Position.X);
    }

    [Check]
    public void CrouchingMakesTheBodyShorterAndStandingNeedsRoom()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        world.Press(GameAction.Down);
        Expect.True(world.Player.State is CrouchingState);

        // A form taken as a state is entered is her body's from her next update.
        world.Step(1);
        Expect.Equal(world.Player.HalfHeightOf(Player.Crouching) * 2, world.Player.BodyRect.Height);

        world.Press(GameAction.Up);
        Expect.True(world.Player.State is StandingState);
        world.Step(1);
        Expect.Equal(world.Player.HalfHeightOf(Player.Standing) * 2, world.Player.BodyRect.Height);
    }

    [Check]
    public void CrawlingIsOffUntilTheGameSwitchesItOn()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        world.Press(GameAction.Down);
        world.Press(GameAction.Down);
        Expect.True(world.Player.State is CrouchingState);

        world.Player.CanCrawl = true;
        world.Press(GameAction.Down);
        Expect.True(world.Player.State is CrawlingState);
        world.Step(1);
        Expect.Equal(world.Player.HalfHeightOf(Player.Crawling) * 2, world.Player.BodyRect.Height);
    }

    [Check]
    public void AMechanicThatNeedsEquipmentWaitsForItToBeSwitchedOn()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        world.Player.CanCrawl = true;
        world.Player.CrawlNeeds = "low form";
        world.Press(GameAction.Down);
        world.Press(GameAction.Down);
        Expect.True(world.Player.State is CrouchingState);

        world.Player.Inventory.Grant("low form");
        world.Press(GameAction.Down);
        Expect.True(world.Player.State is CrawlingState);

        world.Player.Inventory.Switch("low form", false);
        Expect.False(world.Player.MayCrawl);
    }

    [Check]
    public void TheLowFormFitsUnderALowCeilingAndCannotStandUpThere()
    {
        // A passage one tile high: only the low form fits.
        using var world = new StandInWorld(
            "................", "................", "................", "....########....", "................", "################")
            .StartAt(40, 80).Setup();
        world.Player.CanCrawl = true;
        world.Press(GameAction.Down);
        world.Press(GameAction.Down);
        world.Step(80, GameAction.Right);
        Expect.True(world.Player.Position.X > Pixels.FromWhole(100));

        world.Press(GameAction.Up);
        Expect.True(world.Player.State is CrawlingState);
        Expect.Equal(world.Player.HalfHeightOf(Player.Crawling) * 2, world.Player.BodyRect.Height);
    }

    [Check]
    public void TheLowFormJumpsOnlyWhenTheGameAllowsIt()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(40, 80).Setup();
        world.Player.CanCrawl = true;
        world.Press(GameAction.Down);
        world.Press(GameAction.Down);
        world.Press(GameAction.Jump);
        Expect.Equal(80, world.Player.BodyRect.Bottom);

        world.Player.CanJumpWhileCrawling = true;
        world.Press(GameAction.Jump);
        world.Step(2);
        Expect.True(world.Player.BodyRect.Bottom < 80);
        Expect.True(world.Player.State is CrawlingState);
    }

    [Check]
    public void AHitCostsEnergyKnocksThePlayerBackAndLeavesThemBrieflyUnhurtable()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(100, 80).Setup();
        var hit = new Hit(30, HitSide.Enemies, HitKind.Contact) { Direction = PixelOffset.FromWhole(1, 0) };

        Expect.Equal(Engine.World.HitResult.Hurt, world.Player.TakeHit(hit, world.Player.Hurtbox));
        Expect.Equal(69, world.Player.Inventory.Amount(Inventory.Energy));
        Expect.True(world.Player.IsInvincible);
        Expect.Equal(Engine.World.HitResult.Ignored, world.Player.TakeHit(hit, world.Player.Hurtbox));

        // The knockback is taken up on her next update, after she has moved.
        world.Step(1);
        Expect.True(world.Player.State is KnockedBackState);
        world.Step(4);
        Expect.True(world.Player.Position.X > Pixels.FromWhole(100));
        world.Step(world.Player.InvincibleUpdates);
        Expect.False(world.Player.IsInvincible);
        Expect.True(world.Player.State.InControl);
    }

    [Check]
    public void LosingAllEnergyStartsThePlayerDying()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(100, 80).Setup();
        world.Player.TakeHit(new Hit(500, HitSide.Enemies, HitKind.Contact), world.Player.Hurtbox);

        // Energy that has run out is settled as the update ends.
        world.Step(1);
        Expect.True(world.Player.State is DyingState);
    }

    [Check]
    public void BeingHeldStopsThePlayerUntilTheyAreReleased()
    {
        using var world = new StandInWorld(OpenRoom).StartAt(100, 80).Setup();
        world.Player.Hold();
        world.Step(20, GameAction.Right);
        Expect.Equal(Pixels.FromWhole(100), world.Player.Position.X);
        world.Player.Release();
        world.Step(20, GameAction.Right);
        Expect.True(world.Player.Position.X > Pixels.FromWhole(100));
    }

    [Check]
    public void TheInventoryKeepsAmountsWithinTheirCapacity()
    {
        var inventory = new Inventory();
        inventory.SetCapacity("missiles", 5);
        Expect.Equal(5, inventory.Add("missiles", 8));
        Expect.True(inventory.IsFull("missiles"));
        Expect.True(inventory.Spend("missiles", 2));
        Expect.False(inventory.Spend("missiles", 9));
        Expect.Equal(3, inventory.Amount("missiles"));
        Expect.True(inventory.IsFull("never held"));

        inventory.Grant("boots");
        Expect.True(inventory.IsOn("boots"));
        inventory.Switch("boots", false);
        Expect.True(inventory.Owns("boots"));
        Expect.False(inventory.IsOn("boots"));
        Expect.Throws<InvalidOperationException>(() => inventory.Switch("not owned", true));
    }
}
