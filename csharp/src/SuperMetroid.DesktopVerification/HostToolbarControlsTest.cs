using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

/// <summary>Inspect the production host control tree without showing a window or touching player saves.</summary>
internal static class HostToolbarControlsTest
{
    public static void Run(string rom)
    {
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "toolbar-559", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        using var host = new PlayableGameControl(rom,
            new SuperMetroidGameOptions { AudioEnabled = false, Renderer = RendererSelection.Software },
            dataDirectory: directory);
        host.Size = new(900, 760);
        var layout = host.Controls.OfType<TableLayoutPanel>().Single();
        var toolbar = layout.Controls.OfType<ToolStrip>().Single();
        host.PerformLayout();
        layout.PerformLayout();
        toolbar.PerformLayout();
        string[] buttons = toolbar.Items.OfType<ToolStripButton>().Select(b => b.Text!).ToArray();
        if (!buttons.SequenceEqual(new[] { "Restart", "Pause", "Step", "Save State", "Load State" }))
            throw new InvalidDataException("Unexpected production toolbar buttons: " + string.Join(", ", buttons));
        var controls = toolbar.Items.Cast<ToolStripItem>().Where(item => item.Available).ToArray();
        for (int i = 1; i < controls.Length; i++)
            if (controls[i].Bounds.Left < controls[i - 1].Bounds.Right)
                throw new InvalidDataException("Toolbar items overlap after removing Press Start.");
        HostKeyboardInputSmokeTest.Run();
        var keyboard = new HostKeyboardInputState();
        if (keyboard.BuildControllerWord(SnesButton.Start) != (ushort)SnesButton.Start)
            throw new InvalidDataException("Polled gamepad Start was not preserved.");
        HostViewportLayoutSmokeTest.Run();
        Console.WriteLine("Production toolbar contains five expected buttons and no Press Start; layout, keyboard Enter and polled controller Start pass.");
    }
}
