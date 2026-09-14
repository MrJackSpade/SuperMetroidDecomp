using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

/// <summary>Actual pause/equipment navigation around native movement checkpoints.</summary>
internal sealed class TemporaryBlueMenuController(SuperMetroidGame game, SamusState samus)
{
    private long _hostFrame;

    public static ushort At(int frame, bool left, int mode)
    {
        if (frame < 400) return TemporaryBlueCancellationInputs.At(frame, left, 0);
        if (frame == 400) return (ushort)(SnesButton.Start | SnesButton.R);
        if (frame <= 430) return (ushort)SnesButton.R;
        SnesButton forward = left ? SnesButton.Left : SnesButton.Right;
        return (ushort)(mode is 3 or 4 ? SnesButton.A | forward :
            mode == 5 ? SnesButton.B | SnesButton.R | forward : SnesButton.R);
    }

    public void Step(int frame, int mode, ushort input)
    {
        if (frame == 431) NavigateMenu(mode);
        StepInput((SnesButton)input);
        if (frame == 430 && game.GameState != SuperMetroidGameState.Pausing)
            throw new InvalidDataException("Native thirty-frame pause fade did not freeze gameplay.");
        if (frame == 431)
        {
            ushort counter = mode is 1 or 3 or 5 ? (ushort)0 : (ushort)0x0401;
            if (samus.HorizontalSpeed.SpeedBoostCounter != counter)
                throw new InvalidDataException("Unpause equipment reconciliation did not preserve/cancel temporary boost correctly.");
        }
    }

    private void NavigateMenu(int mode)
    {
        var before = FrozenState();
        Reach(SuperMetroidGameState.PausedB, SnesButton.R);
        StepInput(0);
        for (int tick = 0; tick < 40; tick++) StepInput(SnesButton.R);
        if (game.PauseScreenMode != 1) throw new InvalidDataException("Equipment page did not open.");
        StepInput(0);
        StepInput(SnesButton.Down);
        StepInput(0);
        if (mode is 1 or 2 or 3 or 5) StepInput(SnesButton.A);
        StepInput(0);
        if (mode == 2) StepInput(SnesButton.A);
        StepInput(0);
        ushort expectedItems = (ushort)(SamusEquipmentFlags.MorphBall |
            (mode is 1 or 3 or 5 ? 0 : SamusEquipmentFlags.SpeedBooster));
        if (samus.EquippedItems != expectedItems || samus.CollectedItems !=
            (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpeedBooster))
            throw new InvalidDataException("Menu input did not change only equipped Speed Booster.");
        if (FrozenState() != before) throw new InvalidDataException("Interactive menu changed frozen movement or boost state.");
        for (int tick = 0; tick < 8 && game.GameState == SuperMetroidGameState.PausedB; tick++) StepInput(SnesButton.Start);
        if (game.GameState == SuperMetroidGameState.PausedB) throw new InvalidDataException("Start did not leave equipment menu.");
        Reach(SuperMetroidGameState.Unpausing, SnesButton.R);
    }

    private string FrozenState() => $"{samus.Kinematics.XFixed},{samus.Kinematics.YFixed},{samus.Pose}," +
        $"{samus.AnimationFrame},{samus.AnimationFrameTimer},{samus.HorizontalSpeed.BaseFixed}," +
        $"{samus.HorizontalSpeed.SpeedBoostCounter},{samus.Shinespark.ShineTimer},{samus.Kinematics.VerticalSpeedFixed}";

    private void StepInput(SnesButton input) => game.StepCaptured((ushort)input, ++_hostFrame, 1);

    private void Reach(SuperMetroidGameState state, SnesButton input)
    {
        for (int tick = 0; tick < 120 && game.GameState != state; tick++) StepInput(input);
        if (game.GameState != state) throw new InvalidDataException($"Never reached {state}.");
    }
}
