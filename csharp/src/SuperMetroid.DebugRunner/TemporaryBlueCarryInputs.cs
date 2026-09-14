using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

/// <summary>Independent controller timeline and native success boundaries for temporary boost carry.</summary>
internal static class TemporaryBlueCarryInputs
{
    public static ushort At(int frame, bool left, int mode)
    {
        SnesButton forward = left ? SnesButton.Left : SnesButton.Right;
        if (frame < 140) return (ushort)(SnesButton.B | forward);
        if (frame == 140) return (ushort)(SnesButton.Down | SnesButton.R);
        if (frame < 400) return (ushort)SnesButton.R;
        if (mode < 4)
        {
            if (frame < 415 || mode == 0) return (ushort)(SnesButton.A | forward);
            if (mode == 1) return 0;
            if (mode == 2) return (ushort)(SnesButton.A | (left ? SnesButton.Right : SnesButton.Left));
            return (ushort)SnesButton.A;
        }
        int unmorphFrame = 466 + mode;
        if (frame <= 410) return (ushort)(SnesButton.A | forward);
        if (frame < 420 || frame == 421) return (ushort)SnesButton.A;
        if (frame is 420 or 422) return (ushort)(SnesButton.A | SnesButton.Down);
        if (frame < unmorphFrame) return (ushort)(SnesButton.A | forward);
        if (frame == unmorphFrame) return (ushort)(SnesButton.A | SnesButton.Up | SnesButton.R | forward);
        if (frame < 510) return (ushort)(SnesButton.A | SnesButton.R);
        if (frame < 520) return (ushort)SnesButton.R;
        return (ushort)(SnesButton.A | forward);
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        if (frame == 399 && (samus.HorizontalSpeed.SpeedBoostCounter != 0x0401 || samus.Shinespark.ShineTimer != 0))
            throw new InvalidDataException("Carry must start from controller-earned expired charge and retained boost.");
        if (mode == 2 && frame is 416 or 417 &&
            (samus.HorizontalSpeed.SpeedBoostCounter != 0 || samus.HorizontalSpeed.ContactDamageIndex != (frame == 416 ? 1 : 0)))
            throw new InvalidDataException("Aerial-turn cancellation must preserve contact for its final native movement frame only.");
        if (mode == 1 && frame is 415 or 416 &&
            (samus.HorizontalSpeed.SpeedBoostCounter != 0 || samus.HorizontalSpeed.ContactDamageIndex != (frame == 415 ? 1 : 0)))
            throw new InvalidDataException("Neutral input cancellation has a different native boundary from turning.");
        if (frame == 540)
        {
            bool retained = mode >= 19 && mode != 27;
            if (samus.HorizontalSpeed.SpeedBoostCounter != (retained ? 0x0401 : 0) ||
                samus.HorizontalSpeed.ContactDamageIndex != (retained ? 1 : 0))
                throw new InvalidDataException("Soft-unmorph carry or adjacent failure changed boost/contact state on the second jump.");
            if (retained && samus.Kinematics.YFixed >= 0x01f00000)
                throw new InvalidDataException("Successful carry must actually jump again, not remain crouched.");
        }
    }
}
