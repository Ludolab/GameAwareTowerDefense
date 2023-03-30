using UnityEngine;
using UnityEngine.SceneManagement;
using GameAware;
using Newtonsoft.Json;

public class ToyManager : MonoBehaviour {

    public static ToyManager Instance { get; private set; } = null;
    public bool forceRunInBackground = true;

    private string[] WindowPanels = new string[] {
        "Toy Box",
        "Settings",
        "Tracked Objects",
        "Last Key Frame"
    };

    private bool menuOpen = false;
    private Rect windowRect = new Rect(20, 20, 400, 600);
    private int currentPanel = 0;
    private Vector2 scrollPos = new Vector2(0, 0);
    private ToyControls toyControl = null;
   
    public string[] scenes;

    // Start is called before the first frame update
    void Start() {
        if (forceRunInBackground) {
            Application.runInBackground = true;
        }
        if(Instance != null) {
            Debug.LogWarning("Multiple ToyManagers Destroying new one.");
            Destroy(this);
            return;
        }
        else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.activeSceneChanged += OnSceneChange;
            toyControl = FindObjectOfType<ToyControls>();
        }
        
    }

    private void OnSceneChange(Scene arg0, Scene arg1) {
        toyControl = FindObjectOfType<ToyControls>();
    }


    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menuOpen = !menuOpen;
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            currentPanel++;
            currentPanel %= 4;
        }
    }

    private void OnGUI() {
        if (menuOpen) {
            windowRect = GUI.Window(0, windowRect, WindowFunction, "Toy Box");
        }
    }


    
    void WindowFunction(int windowID) {
        GUILayout.BeginVertical();
        GUILayout.Label(string.Format("Current Time:{0}", MetaDataTracker.Instance.CurrentTimeMills));
        currentPanel = GUILayout.Toolbar(currentPanel, WindowPanels);
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        

        switch (currentPanel) {
            case 0:
            default:
                ToyBoxGUI();
                break;
            case 1:
                MiddlwareSettingsGUI();
                break;
            case 2:
                MetadataTrackerGUI();
                break;
            case 3:
                LastKeyFrameGUI();
                break;
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }


    public void ToyBoxGUI() {
        GUILayout.Label("Welcome to the Toy Box!");

        GUILayout.Label("This project is designed to show off and test different capabilities of the Game Aware streaming system. Select different scenes below to explore the capabilities.");

        GUILayout.Label("Change Scene:");
        foreach (string sceneName in scenes) {
            if (GUILayout.Button(sceneName)) {
                SceneManager.LoadScene(sceneName);
            }
        }
    }

    public void MiddlwareSettingsGUI() {
        bool guiEnabledBak = GUI.enabled;

        if(toyControl != null) {
            GUILayout.Label(toyControl.ToyName + " Settings");
            GUILayout.Label(toyControl.Description);
            toyControl.ToyControlGUI();
        }

        GUILayout.Label("Middleware Status");
        GUILayout.Label(string.Format("Connected: {0}", MetaDataTracker.Instance.Connected));
        GUILayout.Label(string.Format("Recording: {0}", MetaDataTracker.Instance.Recording));

        if (MetaDataTracker.Instance.Connected) {
            GUI.enabled = false;
        }
        if (GUILayout.Button("Start MetaData")) {
            MetaDataTracker.Instance.StartMetaData();
        }

        GUILayout.Space(15);
        GUILayout.Label("Middleware Settings");
        MetaDataTracker.Instance.middleWareURI = LabeledInputField("Middleware URI:", MetaDataTracker.Instance.middleWareURI);
        MetaDataTracker.Instance.middleWarePort = LabeledInputField("Middleware Port:", MetaDataTracker.Instance.middleWarePort);
        MetaDataTracker.Instance.middleWareRedisPassword = LabeledInputField("Middleware password:", MetaDataTracker.Instance.middleWareRedisPassword);   

        GUI.enabled = guiEnabledBak;
    }



    public void MetadataTrackerGUI() {

        MetaDataTracker.Instance.showMockOverlay = GUILayout.Toggle(MetaDataTracker.Instance.showMockOverlay, "Show Mock Overlay");

        MetaDataTracker.Instance.mockOverlayFilterString = LabeledInputField("Display Filter:", MetaDataTracker.Instance.mockOverlayFilterString);

        GUILayout.Label("CurrentTrackables:");
        foreach(IMetaDataTrackable mdt in MetaDataTracker.Instance.CurrentTrackables) {

            if (MetaDataTracker.Instance.mockOverlayFilterString == string.Empty || mdt.ObjectKey.Contains(MetaDataTracker.Instance.mockOverlayFilterString)) {
                GUILayout.Label(mdt.ObjectKey);
                DepthRect rect = mdt.ScreenRect();
                GUILayout.BeginHorizontal();
                GUILayout.Space(25);
                GUILayout.Label(string.Format("x:{0}, y:{1}, w:{2}, h:{3}, z:{4}", rect.rect.x, rect.rect.y, rect.rect.width, rect.rect.height, rect.z));
                GUILayout.EndHorizontal();
            }
        }
    }

    public void LastKeyFrameGUI() {
        if(GUILayout.Button("Copy to Clipboard")) {
            GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(MetaDataTracker.Instance.LatestFrame);
        }
        GUILayout.Label(JsonConvert.SerializeObject(MetaDataTracker.Instance.LatestFrame,Formatting.Indented));
    }

    private string LabeledInputField(string label, string value) {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(75));
        var ret = GUILayout.TextField(value);
        GUILayout.EndHorizontal();
        return ret;
    }

    private int LabeledInputField(string label, int value) {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(75));
        var ret = GUILayout.TextField(value.ToString());
        GUILayout.EndHorizontal();
        if (int.TryParse(ret, out value)) {
            return value;
        }
        return 0;
    }
}
