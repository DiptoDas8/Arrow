using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// this is the master game script, attached to th ebow

public class bowAndArrow : MonoBehaviour {
	
	// to determine the mouse position, we need a raycast
	private Ray mouseRay1;
	private RaycastHit rayHit;
	// position of the raycast on the screen
	private float posX;
	private float posY;

	// References to the gameobjects / prefabs
	public GameObject bowString;
	GameObject arrow;
	public GameObject arrowPrefab;
	public GameObject gameManager;	
	public GameObject risingText;
	public GameObject target;

	// Sound effects
	public AudioClip stringPull;
	public AudioClip stringRelease;
	public AudioClip arrowSwoosh;

	// has sound already be played
	bool stringPullSoundPlayed;
	bool stringReleaseSoundPlayed;
	bool arrowSwooshSoundPlayed;

	// the bowstring is a line renderer
	private List<Vector3> bowStringPosition;
	LineRenderer bowStringLinerenderer;

	// to determine the string pullout
	float arrowStartX;
	float length;

	// some status vars
	bool arrowShot;
	bool arrowPrepared;

	// position of the line renderers middle part 
	Vector3 stringPullout;
	Vector3 stringRestPosition = new Vector3 (-0.44f, -0.06f, 2f);

	// game states
	public enum GameStates {
		menu, 
		instructions,
		game,
		over,
		hiscore,
	};

	// store the actual game state
	public GameStates gameState = GameStates.menu;

	// references to main objects for the UI screens
	public Canvas menuCanvas;
	public Canvas instructionsCanvas;
	public Canvas highscoreCanvas;
	public Canvas gameCanvas;
	public Canvas gameOverCanvas;

	// referene to the text fields of game UI
	public Text arrowText;
	public Text scoreText;
	public Text endscoreText;
	public Text actualHighscoreText;
	public Text newHighscoreText;
	public Text newHighText;

	// amount of arrows for the game
	public int arrows = 20;
	// actual score
	public int score = 0;


	//
	// void resetGame()
	//
	// this method resets the game status
	//

	void resetGame() {
		arrows = 20;
		score = 0;
		// be sure that there is only one arrow in the game
		if (GameObject.Find("arrow") == null)
			createArrow (true);
	}


	// Use this for initialization
	void Start () {
		// set the UI screens
		menuCanvas.enabled = true;
		instructionsCanvas.enabled = false;
		highscoreCanvas.enabled = false;
		gameCanvas.enabled = false;
		gameOverCanvas.enabled = false;

		// create the PlayerPref
		initScore ();

		// create an arrow to shoot
		// use true to set the target
		createArrow (true);

		// setup the line renderer representing the bowstring
		bowStringLinerenderer = bowString.AddComponent<LineRenderer>();
		bowStringLinerenderer.SetVertexCount(3);
		bowStringLinerenderer.SetWidth(0.05F, 0.05F);
		bowStringLinerenderer.useWorldSpace = false;
		bowStringLinerenderer.material = Resources.Load ("Materials/bowStringMaterial") as Material;
		bowStringPosition = new List<Vector3> ();
		bowStringPosition.Add(new Vector3 (-0.44f, 1.43f, 2f));
		bowStringPosition.Add(new Vector3 (-0.44f, -0.06f, 2f));
		bowStringPosition.Add(new Vector3 (-0.43f, -1.32f, 2f));
		bowStringLinerenderer.SetPosition (0, bowStringPosition [0]);
		bowStringLinerenderer.SetPosition (1, bowStringPosition [1]);
		bowStringLinerenderer.SetPosition (2, bowStringPosition [2]);
		arrowStartX = 0.7f;

		stringPullout = stringRestPosition;
	}



	// Update is called once per frame
	void Update () {
		// check the game states
		switch (gameState) {
		case GameStates.menu:
			// leave the game when back key is pressed (android)
			if (Input.GetKeyDown(KeyCode.Escape)) {
				Application.Quit();
			}
			break;

		case GameStates.game:
			// set UI related stuff
			showArrows();
			showScore();

			// return to main menu when back key is pressed (android)
			if (Input.GetKeyDown(KeyCode.Escape)) {
				showMenu();
			}

			// game is steered via mouse
			// (also works with touch on android)
			if (Input.GetMouseButton(0)) {
				// the player pulls the string
				if (!stringPullSoundPlayed) {
					// play sound
					GetComponent<AudioSource>().PlayOneShot(stringPull);
					stringPullSoundPlayed = true;
				}
				// detrmine the pullout and set up the arrow
				prepareArrow();
			}

			// ok, player released the mouse
			// (player released the touch on android)
			if (Input.GetMouseButtonUp (0) && arrowPrepared) {
				// play string sound
				if (!stringReleaseSoundPlayed) {
					GetComponent<AudioSource>().PlayOneShot(stringRelease);
					stringReleaseSoundPlayed = true;
				}
				// play arrow sound
				if (!arrowSwooshSoundPlayed) {
					GetComponent<AudioSource>().PlayOneShot(arrowSwoosh);
					arrowSwooshSoundPlayed = true;
				}
				// shot the arrow (rigid body physics)
				shootArrow();
			}
			// in any case: update the bowstring line renderer
			drawBowString();
			break;
		case GameStates.instructions:
			break;
		case GameStates.over:
			break;
		case GameStates.hiscore:
			break;
		}
	}


	//
	// public void initScore()
	//
	// The player score is stored via Playerprefs
	// to make sure they can be stored,
	// they have to be initialized at first
	//
	
	public void initScore() {
		if (!PlayerPrefs.HasKey ("Score"))
			PlayerPrefs.SetInt ("Score", 0);
	}


	public void showScore() {
		scoreText.text = "Score: " + score.ToString();
	}


	public void showArrows() {
		arrowText.text = "Arrows: " + arrows.ToString ();
	}


	//
	// public void createArrow()
	//
	// this method creates a new arrow based on the prefab
	//

	public void createArrow(bool hitTarget) {
		Camera.main.GetComponent<camMovement> ().resetCamera ();
		// when a new arrow is created means that:
		// sounds has been played
		stringPullSoundPlayed = false;
		stringReleaseSoundPlayed = false;
		arrowSwooshSoundPlayed = false;
		// does the player has an arrow left ?
		if (arrows > 0) {
			// may target's position be altered?
			if (hitTarget) {
				// if the player hit the target with the last arrow, 
				// it's set to a new random position
				float x = Random.Range(-1f,8f);
				float y = Random.Range(-3f,3f);
				Vector3 position = target.transform.position;
				position.x = x;
				position.y = y;
				target.transform.position = position;
			}
			// now instantiate a new arrow
			this.transform.localRotation = Quaternion.identity;
			arrow = Instantiate (arrowPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			arrow.name = "arrow";
			arrow.transform.localScale = this.transform.localScale;
			arrow.transform.localPosition = this.transform.position + new Vector3 (0.7f, 0, 0);
			arrow.transform.localRotation = this.transform.localRotation;
			arrow.transform.parent = this.transform;
			// transmit a reference to the arrow script
			arrow.GetComponent<rotateArrow> ().setBow (gameObject);
			arrowShot = false;
			arrowPrepared = false;
			// subtract one arrow
			arrows --;
		}
		else {
			// no arrow is left,
			// so the game is over
			gameState = GameStates.over;
			gameOverCanvas.enabled = true;
			endscoreText.text = "You shot all the arrows and scored " + score + " points.";
		}
	}


	//
	// public void shootArrow()
	//
	// Player released the arrow
	// get the bows rotationn and accelerate the arrow
	//

	public void shootArrow() {
		if (arrow.GetComponent<Rigidbody>() == null) {
			arrowShot = true;
			arrow.AddComponent<Rigidbody>();
			arrow.transform.parent = gameManager.transform;
			arrow.GetComponent<Rigidbody>().AddForce (Quaternion.Euler (new Vector3(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,transform.rotation.eulerAngles.z))*new Vector3(25f*length,0,0), ForceMode.VelocityChange);
		}
		arrowPrepared = false;
		stringPullout = stringRestPosition;

		// Cam
		Camera.main.GetComponent<camMovement> ().resetCamera ();
		Camera.main.GetComponent<camMovement> ().setArrow (arrow);

	}


	// 
	// public void prepareArrow()
	//
	// Player pulls out the string
	//

	public void prepareArrow() {
		// get the touch point on the screen
		mouseRay1 = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(mouseRay1, out rayHit, 1000f) && arrowShot == false)
		{
			// determine the position on the screen
			posX = this.rayHit.point.x;
			posY = this.rayHit.point.y;
			// set the bows angle to the arrow
			Vector2 mousePos = new Vector2(transform.position.x-posX,transform.position.y-posY);
			float angleZ = Mathf.Atan2(mousePos.y,mousePos.x)*Mathf.Rad2Deg;
			transform.eulerAngles = new Vector3(0,0,angleZ);
			// determine the arrow pullout
			length = mousePos.magnitude / 3f;
			length = Mathf.Clamp(length,0,1);
			// set the bowstrings line renderer
			stringPullout = new Vector3(-(0.44f+length), -0.06f, 2f);
			// set the arrows position
			Vector3 arrowPosition = arrow.transform.localPosition;
			arrowPosition.x = (arrowStartX - length);
			arrow.transform.localPosition = arrowPosition;
		}
		arrowPrepared = true;
	}



	//
	// public void drawBowString()
	//
	// set the bowstrings line renderer position
	//

	public void drawBowString() {
		bowStringLinerenderer = bowString.GetComponent<LineRenderer>();
		bowStringLinerenderer.SetPosition (0, bowStringPosition [0]);
		bowStringLinerenderer.SetPosition (1, stringPullout);
		bowStringLinerenderer.SetPosition (2, bowStringPosition [2]);
	}
	

	//
	// public void setPoints()
	//
	// This method is called from the arrow script
	// and sets the points
	// and: if the player hit the bull's eye, 
	// he receives a bonus arrow
	//

	public void setPoints(int points){
		score += points;
		if (points == 50) {
			arrows++;
			GameObject rt1 = (GameObject)Instantiate(risingText, new Vector3(0,0,0),Quaternion.identity);
			rt1.transform.position = this.transform.position + new Vector3(0,0,0);
			rt1.transform.name = "rt1";
			// each target's "ring" is 0.07f wide
			// so it's relatively simple to calculate the ring hit (thus the score)
			rt1.GetComponent<TextMesh>().text= "Bonus arrow";
		}
	}


	//
	// Event functions triggered by UI buttons
	//


	// 
	// public void showInstructions()
	//
	// this method shows the instructions screen
	// can be triggered by main menu 
	//

	public void showInstructions() {
		menuCanvas.enabled = false;
		instructionsCanvas.enabled = true;
	}


	// 
	// public void hideInstructions()
	//
	// this method hides the instructions screen
	// and returns the player to main menu 
	//

	public void hideInstructions() {
		menuCanvas.enabled = true;
		instructionsCanvas.enabled = false;
	}


	// 
	// public void showHighscore()
	//
	// this method shows the highscore screen
	// can be triggered by main menu 
	//

	public void showHighscore() {
		menuCanvas.enabled = false;
		highscoreCanvas.enabled = true;
		actualHighscoreText.text = "Actual Hiscore: " + PlayerPrefs.GetInt ("Score") + " points";
		newHighscoreText.text = "Your Score: " + score + " points";
		if (score > PlayerPrefs.GetInt("Score"))
			newHighText.enabled = true;
		else
			newHighText.enabled = false;
	}


	// 
	// public void hideHighscore()
	//
	// this method hides the highscore screen
	// set the highscore, if neccessary
	// and returns the player to main menu 
	//

	public void hideHighScore() {
		menuCanvas.enabled = true;
		highscoreCanvas.enabled = false;
		if (score > PlayerPrefs.GetInt ("Score")) {
			PlayerPrefs.SetInt("Score",score);
		}
		resetGame();
	}


	//
	// public void checkHighScore()
	//
	// this method is called after the game over screen
	// it checks for a new high score and displays the 
	// highscore screen, if neccessary - else the menu screen

	public void checkHighScore() {
		gameOverCanvas.enabled = false;
		if (score > PlayerPrefs.GetInt ("Score")) {
			showHighscore();
		}
		else {
			menuCanvas.enabled = true;
			resetGame();
		}
	}

	// 
	// public void startGame()
	//
	// this method starts the game
	// can be triggered by main menu 
	//

	public void startGame() {
		menuCanvas.enabled = false;
		highscoreCanvas.enabled = false;
		instructionsCanvas.enabled = false;
		gameCanvas.enabled = true;
		gameState = GameStates.game;
	}

	public void showMenu() {
		menuCanvas.enabled = true;
		gameState = GameStates.menu;
		resetGame ();
	}
}
