using UnityEngine;
using UnityEditor;
using System.IO;

//Source from http://answers.unity3d.com/questions/179775/game-window-size-from-editor-window-in-editor-mode.html
//Modified by seieibob for use at the Virtual Environment and Multimodal Interaction Lab at the University of Maine.
//Use however you'd like!

//modified by kyle for Looking Glass HoloPlay SDK

namespace HoloPlaySDK_UI
{
    /// <summary>
    /// Displays a popup window that undocks, repositions and resizes the game window according to
    /// what is specified by the user in the popup. Offsets are applied to ensure screen borders are not shown.
    /// </summary>
    public class HoloPlayGameWindowMover : EditorWindow
    {
        public static HoloPlayGameWindowMover Instance { get; private set; }
        public static bool IsOpen
        {
            get { return Instance != null; }
        }

        public class GameWindowSettings
        {
            //Desired window resolution
            public Vector2 gameSize = new Vector2(1280, 800);
            //Desired window position
            public Vector2 gamePosition = new Vector2(1920, 0);
        }
        public GameWindowSettings gameWindowSettings;

        //Shows the popup
        [MenuItem("HoloPlay/Game Window Settings")]
        static void OpenPopup()
        {
            HoloPlayGameWindowMover window = EditorWindow.GetWindow<HoloPlayGameWindowMover>();
            //Set popup window properties
            Vector2 popupSize = new Vector2(300, 140);
            //When minSize and maxSize are the same, no OS border is applied to the window.
            window.minSize = popupSize;
            window.maxSize = popupSize;
            window.titleContent = new GUIContent("Game Window Mover");
            window.ShowPopup();
        }

        [MenuItem("HoloPlay/Toggle Game Window %e")]
        static void ToggleWindow()
        {
            EditorWindow gameWindow = GetMainGameView(true);
            if (gameWindow != null)
            {
                gameWindow.Close();
            }
            else
            {
                MoveGameWindow(LoadGameWindowSettings());
            }
        }

        //Returns the current game view as an EditorWindow object.
        public static EditorWindow GetMainGameView(bool dontCreate = false)
        {
            if (!dontCreate)
                EditorApplication.ExecuteMenuItem("Window/Game");

            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetMainGameView.Invoke(null, null);
            return (EditorWindow)Res;
        }

        void OnEnable()
        {
            gameWindowSettings = LoadGameWindowSettings();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            //Constrain fields to ints
            Vector2 newGameSize = EditorGUILayout.Vector2Field("Window Size (Pixels)", new Vector2((int)gameWindowSettings.gameSize.x, (int)gameWindowSettings.gameSize.y));
            if (Mathf.Abs(newGameSize.x - gameWindowSettings.gameSize.x) >= 1 || Mathf.Abs(newGameSize.y - gameWindowSettings.gameSize.y) >= 1)
            {
                gameWindowSettings.gameSize = new Vector2((int)newGameSize.x, (int)newGameSize.y);
            }

            //Constrain fields to ints
            Vector2 newGamePosition = EditorGUILayout.Vector2Field("Window Position", new Vector2((int)gameWindowSettings.gamePosition.x, (int)gameWindowSettings.gamePosition.y));
            if (Mathf.Abs(newGamePosition.x - gameWindowSettings.gamePosition.x) >= 1 || Mathf.Abs(newGamePosition.y - gameWindowSettings.gamePosition.y) >= 1)
            {
                gameWindowSettings.gamePosition = new Vector2((int)newGamePosition.x, (int)newGamePosition.y);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply"))
            {
                HoloPlaySDK.HoloPlay.Config.screenW.Value = newGameSize.x;
                HoloPlaySDK.HoloPlay.Config.screenH.Value = newGameSize.y;
                MoveGameWindow(gameWindowSettings);
            }
        }

        static void MoveGameWindow(GameWindowSettings gameWindowSettings)
        {

            //The size of the toolbar above the game view, excluding the OS border.
            int tabHeight = 22;

            //Get the size of the window borders. Changes depending on the OS.
#if UNITY_STANDALONE_WIN
            //Windows settings
            int osBorderWidth = 5;
#elif UNITY_STANDALONE_OSX
	    //Mac settings (untested)
    	int osBorderWidth = 0; //OSX windows are borderless.
#else
    	//Linux / other platform; sizes change depending on the variant you're running
    	int osBorderWidth = 5;
#endif

            EditorWindow gameView = GetMainGameView();
            gameView.titleContent = new GUIContent("Game (Stereo)");
            //When minSize and maxSize are the same, no OS border is applied to the window.
            gameView.minSize = new Vector2(gameWindowSettings.gameSize.x, gameWindowSettings.gameSize.y + tabHeight - osBorderWidth);
            gameView.maxSize = gameView.minSize;
            Rect newPos = new Rect(gameWindowSettings.gamePosition.x, gameWindowSettings.gamePosition.y - tabHeight, gameWindowSettings.gameSize.x, gameWindowSettings.gameSize.y + tabHeight - osBorderWidth);
            gameView.position = newPos;
            gameView.ShowPopup();
            string gameWindowSettingsStr = JsonUtility.ToJson(gameWindowSettings);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "gameWindowSettings.json"), gameWindowSettingsStr);
        }

        static GameWindowSettings LoadGameWindowSettings()
        {
            GameWindowSettings gameWindowSettings = new GameWindowSettings();
            string gameWindowSettingsPath = Path.Combine(Application.persistentDataPath, "gameWindowSettings.json");
            if (File.Exists(gameWindowSettingsPath))
            {
                string gameWindowSettingsStr = File.ReadAllText(gameWindowSettingsPath);
                gameWindowSettings = JsonUtility.FromJson<GameWindowSettings>(gameWindowSettingsStr);
            }
            else
            {
                gameWindowSettings = new GameWindowSettings();
            }
            gameWindowSettings.gameSize = new Vector2(PlayerSettings.defaultScreenWidth, PlayerSettings.defaultScreenHeight);
            return gameWindowSettings;
        }
    }
}