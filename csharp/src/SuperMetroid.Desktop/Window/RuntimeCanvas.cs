
namespace SuperMetroid.Desktop.Window;

/// <summary>The window the Direct3D 11 swapchain presents the game into, and the keyboard's focus target.</summary>
public sealed class RuntimeCanvas : Control
{
    /// <summary>Whether the GPU swapchain currently owns the window's painting surface.</summary>
    internal bool gpuOwned;

    /// <summary>Excludes GDI paint/erase from the swapchain-owned HWND once the swapchain presents into it.</summary>
    public void SetGpuOwned(bool value)
    {
        gpuOwned = value;
        if (!value) Invalidate();
    }

    /// <summary>Allows normal background erasure only when GDI, rather than the GPU swapchain, owns presentation.</summary>
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if (!gpuOwned) base.OnPaintBackground(e);
    }

    /// <summary>Creates a black canvas that GDI only erases until the swapchain owns it.</summary>
    public RuntimeCanvas()
    {
        BackColor = Color.Black;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
    }

    /// <summary>
    /// Claims the keys that WinForms otherwise reserves for dialog navigation.
    /// </summary>
    /// <remarks>
    /// <see cref="PlayableGameControl"/> listens to ordinary <c>KeyDown</c>/<c>KeyUp</c>
    /// events so the translated controller owns held/newly-pressed semantics. Without this
    /// override, the framework consumes the four arrows while moving focus and may consume
    /// Enter as a default-button key; those inputs would never reach the SNES controller word.
    /// </remarks>
    protected override bool IsInputKey(Keys keyData)
    {
        Keys keyCode = keyData & Keys.KeyCode;
        return keyCode is Keys.Left or Keys.Right or Keys.Up or Keys.Down or Keys.Enter ||
            base.IsInputKey(keyData);
    }

    /// <summary>
    /// Clicking the picture is an explicit request to return keyboard control to gameplay.
    /// </summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        base.OnMouseDown(e);
    }
}
