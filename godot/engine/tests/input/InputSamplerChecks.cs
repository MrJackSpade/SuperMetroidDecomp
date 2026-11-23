using Engine.Input;

namespace Engine.Tests.Input;

/// <summary>Checks that each update's input says what is held and what is newly pressed.</summary>
public sealed class InputSamplerChecks
{
    [Check]
    public void AnActionIsNewlyPressedOnlyOnTheUpdateItGoesDown()
    {
        var sampler = new InputSampler(new ScriptedInputSource([GameAction.Jump], [GameAction.Jump], [], [GameAction.Jump]));

        GameInput first = sampler.Sample();
        Expect.True(first.IsHeld(GameAction.Jump));
        Expect.True(first.WasPressed(GameAction.Jump));

        GameInput second = sampler.Sample();
        Expect.True(second.IsHeld(GameAction.Jump));
        Expect.False(second.WasPressed(GameAction.Jump));

        GameInput third = sampler.Sample();
        Expect.False(third.IsHeld(GameAction.Jump));
        Expect.False(third.WasPressed(GameAction.Jump));

        GameInput fourth = sampler.Sample();
        Expect.True(fourth.WasPressed(GameAction.Jump));
    }

    [Check]
    public void ActionsAreReadIndependently()
    {
        var sampler = new InputSampler(new ScriptedInputSource([GameAction.Left], [GameAction.Left, GameAction.Attack]));
        sampler.Sample();
        GameInput second = sampler.Sample();
        Expect.True(second.IsHeld(GameAction.Left));
        Expect.False(second.WasPressed(GameAction.Left));
        Expect.True(second.WasPressed(GameAction.Attack));
        Expect.False(second.IsHeld(GameAction.Right));
    }
}
