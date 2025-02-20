using UnityEngine;
using Localization;

public class TooltipButton : MonoBehaviour
{
    public Tooltip tooltip;

    public void DisplayTooltip()
    {
        DisplayTooltip(this.tooltip);
    }

    public static void DisplayTooltip(Tooltip tooltip)
    {
        switch (tooltip)
        {
            case Tooltip.CHOOSE_FILE:
                UIManager.Instance.ShowPopUp("choose_file_label", "choose_file_tooltip");
                break;
            case Tooltip.THRESHOLD:
                UIManager.Instance.ShowPopUp("threshold_label", "threshold_tooltip");
                break;
        }
    }
}

public enum Tooltip
{
    THRESHOLD = 1,
    CHOOSE_FILE = 2
}
