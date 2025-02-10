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
    private KeyCode _hotkey = KeyCode.None;
    public KeyCode Hotkey { get { return _hotkey; } }
    public UnityEvent onClick = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(ToggleWaiting);
    }

    // Update is called once per frame
    void Update()
    {
        if (_waitingForHotkey)
        {
            _text.text = "Press key to assign...";
            _hotkey = CheckKeys();
        }
        else
        {
            _text.text = _hotkey.ToString();
            if (Input.GetKeyDown(_hotkey))
            {
                onClick.Invoke();
            }
        }
    }

    private void ToggleWaiting()
    {
        _waitingForHotkey = !_waitingForHotkey;
    }

    private KeyCode CheckKeys()
    {
        foreach (KeyCode key in IEnumeratorUtils.GetEnumValues<KeyCode>())
        {
            if (Input.GetKey(key))
            {
                Debug.Log(key);
                _waitingForHotkey = false;
                return key;
            }
        }
        return KeyCode.None;
    }

    public void SetHotkey(KeyCode key)
    {
        _hotkey = key;
    }

    [System.Serializable]
    public class SaveData
    {
        public string id;
        public int key;
    }
}
