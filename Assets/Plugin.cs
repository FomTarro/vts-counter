using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using VTS.Core;
using VTS.Unity;
using System.IO;
using System;

public class Plugin : UnityVTSPlugin, ISaveable<Plugin.SaveData>
{
    [SerializeField]
    private Image _connectionIndicator;
    [SerializeField]
    private TMPro.TMP_Dropdown _ports;
    [SerializeField]
    private Button _connectButton;

    [SerializeField]
    private Button _incrementButton;
    [SerializeField]
    private Button _decrementButton;
    [SerializeField]
    private TMPro.TMP_Text _counterDisplay;
    [SerializeField]
    private int _counterValue = 0;

    [SerializeField]
    private TMPro.TMP_InputField _threshold;
    [SerializeField]
    private TMPro.TMP_Text _thresholdDisplay;

    private FileSystemWatcher _watcher = new FileSystemWatcher();
    [SerializeField]
    private TMPro.TMP_Text _filePathDisplay;

    private const string PARAM_COUNTER_NAME = "vts_counter_value";
    private VTSParameterInjectionValue PARAM_COUNTER_VALUE = new VTSParameterInjectionValue();

    private const string PARAM_ONES_NAME = "vts_counter_ones";
    private VTSParameterInjectionValue PARAM_ONES_VALUE = new VTSParameterInjectionValue();
    private const string PARAM_TENS_NAME = "vts_counter_tens";
    private VTSParameterInjectionValue PARAM_TENS_VALUE = new VTSParameterInjectionValue();
    private const string PARAM_HUNDREDS_NAME = "vts_counter_hundreds";
    private VTSParameterInjectionValue PARAM_HUNDREDS_VALUE = new VTSParameterInjectionValue();
    private const string PARAM_GRADIENT_NAME = "vts_counter_gradient";
    private VTSParameterInjectionValue PARAM_GRADIENT_VALUE = new VTSParameterInjectionValue();
    private List<VTSParameterInjectionValue> _params = new List<VTSParameterInjectionValue>();

    public string FileFolder => "settings";

    public string FileName => "settings.json";

    private void Start()
    {
        FromSaveData(SaveDataManager.Instance.ReadSaveData<SaveData>(this));
        _ports.ClearOptions();
        _ports.onValueChanged.AddListener((val) =>
        {
            this.SetPort(int.Parse(_ports.options[val].text));
            Connect();
        });
        this.Socket.OnPortDiscovered = (port) => { OnPortDiscovered(port); };
        _connectButton.onClick.AddListener(Connect);
        _incrementButton.onClick.AddListener(Increment);
        _decrementButton.onClick.AddListener(Decrement);
        Connect();
    }

    private void OnApplicationQuit()
    {
        SaveDataManager.Instance.WriteSaveData<SaveData>(this);
    }

    private void OnPortDiscovered(int port)
    {
        List<TMPro.TMP_Dropdown.OptionData> opts = new List<TMPro.TMP_Dropdown.OptionData>();
        opts.Add(new TMPro.TMP_Dropdown.OptionData(port + ""));
        _ports.AddOptions(opts);
    }

    public void Connect()
    {
        _connectionIndicator.color = Color.yellow;
        Initialize(
            new WebSocketImpl(this.Logger),
            new NewtonsoftJsonUtilityImpl(),
            new TokenStorageImpl(Application.persistentDataPath), () =>
            {
                this.Logger.Log("Connected!");
                _connectionIndicator.color = Color.green;
                CreateNewParameter(
                    PARAM_COUNTER_NAME,
                    "The raw numeric counter value",
                    1000000,
                    PARAM_COUNTER_VALUE);
                CreateNewParameter(
                    PARAM_ONES_NAME,
                    "The ones digit of the counter value",
                    9,
                    PARAM_ONES_VALUE);
                CreateNewParameter(
                    PARAM_TENS_NAME,
                    "The tens digit of the counter value",
                    9,
                    PARAM_TENS_VALUE);
                CreateNewParameter(
                    PARAM_HUNDREDS_NAME,
                    "The hundreds digit of the counter value",
                    9,
                    PARAM_HUNDREDS_VALUE);
                CreateNewParameter(
                    PARAM_GRADIENT_NAME,
                    "A value from 0 to 1 based on how close your current counter value is to the threshold",
                    1,
                    PARAM_GRADIENT_VALUE);
            },
            () =>
            {
                this.Logger.Log("Disconnected");
                _connectionIndicator.color = Color.red;
            },
            (err) =>
            {
                this.Logger.LogError(err.data.message);
                _connectionIndicator.color = Color.red;
            });
    }

    private void Update()
    {
        this._counterDisplay.text = _counterValue + "";
        this.PARAM_ONES_VALUE.value = this._counterValue < 1 ? -1 : this._counterValue % 10;
        this.PARAM_TENS_VALUE.value = this._counterValue < 10 ? -1 : (this._counterValue % 100) / 10;
        this.PARAM_HUNDREDS_VALUE.value = this._counterValue < 100 ? -1 : this._counterValue / 100;
        this.PARAM_COUNTER_VALUE.value = this._counterValue;
        this.PARAM_GRADIENT_VALUE.value = Mathf.Min(1, (float)this._counterValue / Mathf.Max(1, int.Parse(this._threshold.text)));
        this._thresholdDisplay.text = "(" + _counterValue + "/" + this._threshold.text + " = " + this.PARAM_GRADIENT_VALUE.value + ")";
        if (this.IsAuthenticated)
        {
            this.InjectParameterValues(this._params.ToArray());
        }
    }

    public void OpenLogs(){
        Application.OpenURL(Application.persistentDataPath);
    }

    public void Increment()
    {
        this._counterValue += 1;
        if(File.Exists(GetFullFilePath())){
            File.WriteAllText(GetFullFilePath(), this._counterValue+"");
        }
    }


    public void Decrement()
    {
        this._counterValue -= 1;
        if(File.Exists(GetFullFilePath())){
            File.WriteAllText(GetFullFilePath(), this._counterValue+"");
        }
    }

    private void CreateNewParameter(string paramName, string paramDescription, int paramMax, VTSParameterInjectionValue value)
    {
        value.id = paramName;
        value.value = 0;
        value.weight = 1;
        this._params.Add(value);
        if (this.IsAuthenticated)
        {
            VTSCustomParameter newParam = new VTSCustomParameter();
            newParam.defaultValue = 0;
            newParam.min = 0;
            newParam.max = paramMax;
            newParam.parameterName = paramName;
            newParam.explanation = paramDescription;
            this.Logger.Log(string.Format("Creating tracking parameter: {0}", paramName));
            this.AddCustomParameter(
                newParam,
                (s) =>
                {
                    this.Logger.Log(string.Format("Successfully created parameter in VTube Studio: {0}", paramName));
                },
                (e) =>
                {
                    this.Logger.LogError(string.Format("Error while injecting Parameter Data {0} into VTube Studio: {1} - {2}",
                        paramName, e.data.errorID, e.data.message));
                });
        }
    }

    private string OpenFileBrowser(){
        string[] files = SFB.StandaloneFileBrowser.OpenFilePanel("Choose a file to read...", Application.persistentDataPath, "txt", false);
        if(files.Length > 0){
            return files[0];
        }
        return "";
    }

    public void ChooseFile(){
        string file = OpenFileBrowser();
        SetFileWatchPath(file);
    }

    private string GetFullFilePath(){
        return Path.Join(_watcher.Path, _watcher.Filter);
    }

    private void SetFileWatchPath(string path){
        Debug.Log(@path);
        _watcher.Dispose();
        if(File.Exists(path)){
            Debug.Log("Attaching watcher to " + path);
           _watcher = new FileSystemWatcher();
           _watcher.Filter = Path.GetFileName(@path);
           _watcher.Path = Path.GetDirectoryName(@path);
           _watcher.Changed += OnFileChanged;
           _watcher.EnableRaisingEvents = true;
           _filePathDisplay.text = GetFullFilePath();
           ParseFileContent(GetFullFilePath());
        }else{
            Debug.LogWarning(path + " does not exist.");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e){
        ParseFileContent(e.FullPath);
    }

    private void ParseFileContent(string path){
        if(File.Exists(path)){
            try{
                string text = File.ReadAllText(path);
                _counterValue = int.Parse(text);
            }catch(Exception err){
                Debug.LogError(err);
            }
        }
    }

    public string TransformAfterRead(string content)
    {
        return content;
    }

    public string TransformBeforeWrite(string content)
    {
        return content;
    }

    public void FromSaveData(SaveData data)
    {
        foreach (AssignableHotkey hotkey in FindObjectsOfType<AssignableHotkey>())
        {
            foreach (AssignableHotkey.SaveData keyData in data.hotkeys)
            {
                if (hotkey.ID.Equals(keyData.id))
                {
                    hotkey.SetHotkey(keyData.key);
                }
            }
        }
        this._threshold.text = data.threshold + "";
        SetFileWatchPath(data.filePath);
    }

    public SaveData ToSaveData()
    {
        List<AssignableHotkey.SaveData> hotkeys = new List<AssignableHotkey.SaveData>();
        foreach (AssignableHotkey hotkey in FindObjectsOfType<AssignableHotkey>())
        {
            AssignableHotkey.SaveData keyData = new AssignableHotkey.SaveData();
            keyData.id = hotkey.ID;
            keyData.key = hotkey.Hotkey;
            hotkeys.Add(keyData);
        }
        SaveData data = new SaveData();
        data.hotkeys = hotkeys;
        data.threshold = Mathf.Max(1, int.Parse(this._threshold.text));
        data.filePath = GetFullFilePath();
        return data;
    }

    public class SaveData : BaseSaveData
    {
        public List<AssignableHotkey.SaveData> hotkeys = new List<AssignableHotkey.SaveData>();
        public int threshold = 0;
        public string filePath = "";
    }
}

