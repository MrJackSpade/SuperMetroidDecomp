using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>Checks scripted demo input edges against the preserved physical controller latch.</summary>
internal static class AttractDemoControllerVerification
{
    /// <summary>
    /// Checks that consecutive scripted X presses retain their explicit new-press edges
    /// while the Start latch history and repeat timer survive each temporary override.
    /// A later real Start latch must see no edge leaked from the demo.
    /// </summary>
    internal static void VerifyAttractDemoControllerOverride()
    {
        var controller = new ControllerInputState(4, 2);
        controller.Latch(ControllerButton.Start);
        ushort repeat = controller.RepeatTimer.RawValue;
        for (int frame = 0; frame < 2; frame++)
        {
            using (controller.UseDemoInput(ControllerButton.X, ControllerButton.X))
            {
                if (controller.Current != ControllerButton.X || controller.NewlyPressed != ControllerButton.X)
                    throw new InvalidDataException("Demo controller did not preserve the explicit scripted edge.");
                if (controller.Previous != ControllerButton.Start || controller.RepeatTimer.RawValue != repeat)
                    throw new InvalidDataException("Demo override modified the hardware latch history.");
            }
            if (controller.Current != ControllerButton.Start || controller.NewlyPressed != ControllerButton.Start)
                throw new InvalidDataException("Demo beta failed to restore the player cancellation input.");
        }
        controller.Latch(ControllerButton.Start);
        if (controller.NewlyPressed != 0)
            throw new InvalidDataException("Demo input leaked into the next real controller edge calculation.");
        Console.WriteLine("  Attract controller: explicit script edges and restored hardware history agree.");
    }
}
