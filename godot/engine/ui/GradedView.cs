using Godot;

namespace Engine.Ui;

/// <summary>
/// A view of its own, shown through one grading of its colours, as a filter on a camera gives a scene one look.
/// Whatever is put on its <see cref="Stage"/> is drawn into the view's own picture, at whatever depths its things
/// lie and on whatever layers they make, and the picture is then shown with every colour as the colour of a ramp
/// nearest it in brightness; nothing on the stage knows of it. With no ramp given the picture is shown as it was
/// drawn. The view covers the screen, is see-through where nothing was drawn, and its own tint tints the whole
/// picture, so fading the view fades the scene after it is graded.
/// </summary>
/// <remarks>
/// It is a view and not a group of drawn things because Godot gathers into a group only the things drawn at the
/// group's own depth, and a room draws its tiles, what lies behind it and the player each at a depth of their own.
/// </remarks>
public partial class GradedView : SubViewportContainer
{
    /// <summary>The most colours a ramp may have.</summary>
    public const int MostColours = 16;

    private const string Code = """
        shader_type canvas_item;
        render_mode unshaded;

        uniform int ramp_count = 0;
        uniform vec3 ramp[16];
        varying vec4 tint;

        const vec3 WEIGHTS = vec3(0.299, 0.587, 0.114);

        void vertex() {
            tint = COLOR;
        }

        void fragment() {
            // The view's picture holds its colours already multiplied by how solid they are.
            vec4 shown = texture(TEXTURE, UV);
            if (shown.a > 0.0001) {
                shown.rgb /= shown.a;
            }

            float brightness = dot(shown.rgb, WEIGHTS);
            float nearest = 10.0;
            vec3 graded = shown.rgb;
            for (int i = 0; i < 16; i++) {
                if (i >= ramp_count) {
                    break;
                }
                float apart = abs(dot(ramp[i], WEIGHTS) - brightness);
                if (apart < nearest) {
                    nearest = apart;
                    graded = ramp[i];
                }
            }

            COLOR = vec4(graded, shown.a) * tint;
        }
        """;

    private readonly ShaderMaterial _grading = new() { Shader = new Shader { Code = Code } };

    /// <summary>A view with nothing on its stage and no grading.</summary>
    public GradedView()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        Stretch = true;
        MouseFilter = MouseFilterEnum.Ignore;
        var view = new SubViewport
        {
            Name = "View",
            TransparentBg = true,
            Disable3D = true,
            GuiDisableInput = true,
            CanvasItemDefaultTextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };
        AddChild(view);
        Stage = new Node2D { Name = "Stage" };
        view.AddChild(Stage);

        // The picture is always shown through the shader: with no ramp it only takes the colours back out of how
        // solid they are, so that what is half see-through shows as it was drawn.
        Grade([]);
        Material = _grading;
    }

    /// <summary>Where the view's things are put: whatever is under it is what the view shows.</summary>
    public Node2D Stage { get; }

    /// <summary>
    /// Shows the view's picture through <paramref name="ramp"/>: each colour as the ramp's colour nearest it in
    /// brightness. An empty ramp shows the picture as it was drawn.
    /// </summary>
    /// <exception cref="ArgumentException">The ramp has more colours than a grading can hold.</exception>
    public void Grade(IReadOnlyList<Color> ramp)
    {
        ArgumentNullException.ThrowIfNull(ramp);
        if (ramp.Count > MostColours)
            throw new ArgumentException($"A grading holds at most {MostColours} colours, and this ramp has {ramp.Count}.", nameof(ramp));
        var colours = new Vector3[MostColours];
        for (int index = 0; index < ramp.Count; index++)
            colours[index] = new Vector3(ramp[index].R, ramp[index].G, ramp[index].B);
        _grading.SetShaderParameter("ramp_count", ramp.Count);
        _grading.SetShaderParameter("ramp", colours);
    }
}
