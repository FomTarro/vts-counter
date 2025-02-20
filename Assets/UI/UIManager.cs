using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private PopUp _popUp;


    public override void Initialize()
    {
        this._popUp.gameObject.SetActive(false);
    }

    public void Start()
    {
        CheckVersion();
    }

    public void ShowPopUp(string titleKey, string bodyKey, params PopUp.PopUpOption[] options)
    {
        this._popUp.Show(titleKey, bodyKey, options);
    }

    public void HidePopUp()
    {
        this._popUp.Hide();
    }

    private const string VERSION_URL = @"https://www.skeletom.net/vts-counter/version";

    private void CheckVersion()
    {
        StartCoroutine(HttpUtils.GetRequest(
            VERSION_URL,
            (e) =>
            {
                Debug.LogError(e.message);
                Dictionary<string, string> strings = new Dictionary<string, string>();
                strings.Add("error_cannot_resolve_tooltip_populated",
                    string.Format(Localization.LocalizationManager.Instance.GetString("error_cannot_resolve_tooltip"), e.message));
                Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
                UIManager.Instance.ShowPopUp(
                    "error_generic_title",
                    "error_cannot_resolve_tooltip_populated",
                    new PopUp.PopUpOption(
                            "feedback_button_email",
                            ColorUtils.ColorPreset.GREEN,
                            () => { Application.OpenURL("mailto:tom@skeletom.net"); })
                );
            },
            (s) =>
            {
                VersionUtils.VersionInfo info = JsonUtility.FromJson<VersionUtils.VersionInfo>(s);
                Debug.Log(VersionUtils.CompareVersion(info) ? "Newer version needed: " + info.url : "Up to date.");
                if (VersionUtils.CompareVersion(info))
                {
                    Dictionary<string, string> strings = new Dictionary<string, string>();
                    strings.Add("version_tooltip_populated",
                        string.Format(Localization.LocalizationManager.Instance.GetString("version_tooltip"),
                        info.version,
                        info.date,
                        info.url));
                    Localization.LocalizationManager.Instance.AddStrings(strings, Localization.LocalizationManager.Instance.CurrentLanguage);
                    UIManager.Instance.ShowPopUp(
                        "version_label",
                        "version_tooltip_populated",
                        new PopUp.PopUpOption(
                            "version_button",
                            ColorUtils.ColorPreset.GREEN,
                            () => { Application.OpenURL(info.url); })
                        );
                }
            },
            null
        ));
    }

}
