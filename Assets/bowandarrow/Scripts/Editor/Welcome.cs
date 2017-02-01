#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;

// this is a popup window which will be shown after the project is loaded
// once you dismiss it, a playerpref is set and you have to call it manually

[InitializeOnLoad]
public class Welcome : EditorWindow {

	// The window size
	static float sizeWidth = 630;
	static float sizeHeight = 650;

	// a reference to the window
	private GUIStyle welcomeStyle = null;

	// Add menu item named "My Window" to the Window menu
	[MenuItem("Tools/BOW&ARROW/FIRST START INFORMATIONS")]


	// 
	// public static void Initialize()
	//
	// Initialize the window and its appearance
	//

	public static void Initialize() {
		// Get existing open window or if none, make a new one:
		Welcome window = (Welcome)EditorWindow.GetWindow (typeof (Welcome), true, "PLEASE TAKE A LOOK AT THE PROJECT DOCUMENTATION");
			
		GUIStyle style = new GUIStyle();
		
		window.position = new Rect(196, 196, sizeWidth, sizeHeight);
		window.minSize = new Vector2(sizeWidth, sizeHeight);
		window.maxSize = new Vector2(sizeWidth, sizeHeight);
		window.welcomeStyle = style;
		window.Show();
	}


	//
	// void OnGUI()
	//
	// render the window
	// and implement event handling
	//

	void OnGUI()
	{
		// if there's no window, 
		// return
		if (welcomeStyle == null)
			return;

		// put a documentation button on the window
		// if user clicks, a PDF-Reader (has to be installed on the client system) opens
		// and show the documentation
	
		if (GUI.Button (new Rect (10, 10, 300, 150), "OPEN BOW AND ARROW DOCUMENTATION")) {
			Application.OpenURL (Application.dataPath + "//Documentation//" + "readme.txt");
		}


		// a link for rating the project
		// (please do not forget)

		if (GUI.Button(new Rect(20 + 300, 10, 300, 150), "RATE THIS ASSET"))
		{
			Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/32783");
		}


		// other assets
		// made by gemmine

		if (GUI.Button(new Rect(10 , 150 + 20, 300, 150), "MORE ASSETS: GEMMINE'S GEMMINE"))
		{
			Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=relevance/query=gemmine");
		}


		// open up the website

		if (GUI.Button(new Rect(20 + 300 , 150 + 20, 300, 150), "VISIT US"))
		{
			Application.OpenURL("http://www.gemmine.de");
		}


		// dob't bother me again

		if (GUI.Button(new Rect(10, 450 +40, 610, 150), "DON'T PROMPT ME AGAIN"))
		{
			// close the window
			DoClose();
		}
	}


	//
	// void DoClose()
	//
	// close the window and set playerprefs
	// to prevent window from popping up again
	//

	void DoClose()
	{
		SetOpened();
		Close();
	}

	void SetOpened()
	{
		PlayerPrefs.SetInt("Documentation_Opened", 1);
		PlayerPrefs.Save();
	}
}

// this class starts the window up

[InitializeOnLoad]
class StartupHelper
{
	// constructor
	// called after initialization
	static StartupHelper()
	{
		// tell editor to start "Startup" during update
		EditorApplication.update += Startup;
	}


	static void Startup()
	{
		// wait...
		// ...for one second

		if(Time.realtimeSinceStartup < 1)
		return;

		// remove handler
		EditorApplication.update -= Startup;

		// check if Playerprefs are set.
		// if not, open the welcome screen
		if (!PlayerPrefs.HasKey("Documentation_Opened"))
		{
			Welcome.Initialize();
		}
	}
} 

#endif