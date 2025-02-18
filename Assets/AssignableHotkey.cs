using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AssignableHotkey : MonoBehaviour
{
    [SerializeField]
    private string _id;
    public string ID { get { return _id; } }

    [SerializeField]
    private Button _button;
    [SerializeField]
    private TMPro.TMP_Text _text;
    private bool _waitingForHotkey = false;
    [SerializeField]
    private WindowsKeyHook.VirtualKeys _hotkey = WindowsKeyHook.VirtualKeys.Noname;
    public WindowsKeyHook.VirtualKeys Hotkey { get { return _hotkey; } }
    public UnityEvent onClick = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(ToggleWaiting);
        WindowsKeyHook.Instance.OnKey.AddListener(ListenForKey);
    }

    private void Update()
    {
        if(_waitingForHotkey){
            _text.text = "Press any key to assign hotkey...";
        }else if(_hotkey == WindowsKeyHook.VirtualKeys.Noname){
            _text.text = "Click to assign hotkey...";
        }
    }

    private void ToggleWaiting()
    {
        _waitingForHotkey = !_waitingForHotkey;
    }

    public void ListenForKey(WindowsKeyHook.VirtualKeys key){
        if(_waitingForHotkey){
            SetHotkey(key);
            _waitingForHotkey = false;
        }else if(key == _hotkey){
            onClick.Invoke();
        }
    }

    public void SetHotkey(WindowsKeyHook.VirtualKeys key)
    {
        _text.text = key.ToString();
        _hotkey = key;
    }

    [Serializable]
    public class SaveData
    {
        public string id;
        public WindowsKeyHook.VirtualKeys key;
    }
}
