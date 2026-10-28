namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>One named English narration page exposed to presentation content.</summary>
public enum IntroNarrationPageId
{
    /// <summary>First story page, recalling the Zebes battle and Mother Brain's plans; native <c>CinematicBGObjectInstLists_IntroTextPage1</c> at <c>$8C:C383</c>.</summary>
    Page1 = 1,
    /// <summary>Second story page, recalling SR388 and the surviving larva; native <c>CinematicBGObjectInstLists_IntroTextPage2</c> at <c>$8C:C797</c>.</summary>
    Page2,
    /// <summary>Third story page, describing the larva's delivery to Ceres for research; native <c>CinematicBGObjectInstLists_IntroTextPage3</c> at <c>$8C:CB45</c>.</summary>
    Page3,
    /// <summary>Fourth story page, describing the scientists' findings about Metroid energy; native <c>CinematicBGObjectInstLists_IntroTextPage4</c> at <c>$8C:CE33</c>.</summary>
    Page4,
    /// <summary>Fifth story page, describing Samus's departure and receipt of a distress signal; native <c>CinematicBGObjectInstLists_IntroTextPage5</c> at <c>$8C:D15D</c>.</summary>
    Page5,
    /// <summary>Final story page, announcing the attack on Ceres and using a timed final hold instead of the earlier pages' input wait; native <c>CinematicBGObjectInstLists_IntroTextPage6</c> at <c>$8C:D511</c>.</summary>
    Page6,
}
