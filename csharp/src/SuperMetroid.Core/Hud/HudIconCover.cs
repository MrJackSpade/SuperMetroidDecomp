namespace SuperMetroid.Core.Hud;

/// <summary>What sits in an item icon's cells (#1466): the template, or the icon the HUD added over it.</summary>
public enum HudIconCover
{
    /// <summary>The template's cells, where the icon has not been added or the template does not leave room for it.</summary>
    Template,

    /// <summary>The icon, added over a template that leaves its cells blank.</summary>
    Icon,
}
