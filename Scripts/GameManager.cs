using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	//juice tuning - adjust to taste
	[Space(10)] [Header("Tuning")]
	public int tileSize;
	[SerializeField] float timePause; //time to wait between each hop
	[SerializeField] float timeFall; //when the boxes fall, time between the first and last box
	[SerializeField] float timeReveal; //when the boxes reveal, time between the first and last box
	[SerializeField] float timeSetup; //how long before the boxes start spinning
	[SerializeField] float timeReset; //how long between matches
	[SerializeField] int heavyUpdateFrames;  //how many frames to skip between each "Heavy Update"
	[SerializeField] string[] correctTexts; //text to show the player when he finds the prize
	[SerializeField] string incorrectText;
	[SerializeField] string setupText, spinText, pickText; //Click to Begin, Follow the Things, Find the Missing Thing
	[SerializeField] lvlDifficulty[] difficultySettings;

	[System.Serializable]
	public class lvlDifficulty {
		public int prizes = 2;
		public int spins = 2;
		public float timeScale = 1f;
    }
    
	
	//connections - these must be hooked up to -specific- things
	[Space(10)] [Header("Connections")]
	[SerializeField] GUIText textMain;
	[SerializeField] GUIText textLevel;
	[SerializeField] goro_quilt quiltScript;
	[SerializeField] Camera cam;
	[SerializeField] SoundManager soundMgr;


	//bookkeeping
	int difficulty;
	float timeStep; //time step is taken from the difficulty settings
	BouncingBox[] boxScripts;
	BouncingBox selectedBox;
	Transform selected;
	int spins, maxSpins, prizes, missingPrize;
	int frameCount;
	enum gState {setup, spin, pick, reset};
	gState gameState;
	float readyTime;

	
	void Start () {
		readyTime = 0;
		difficulty = 0;
		if (quiltScript != null) difficulty = quiltScript.difficulty; //inherit difficulty setting from the quilt, if applicable

		//find and store all boxes
		GameObject[]boxes = GameObject.FindGameObjectsWithTag("Box");
		boxScripts = new BouncingBox[boxes.Length];
		for (int i = 0; i <  boxes.Length; i++){
			BouncingBox boxScript = boxes[i].GetComponent<BouncingBox>();
			boxScripts[i] = boxScript;
		}

		GameSetup();
	} //close Start()


	void GameSetup() {
		spins = 0;
		gameState = gState.setup;
		textLevel.text = "Level " + (difficulty + 1);

		if (difficulty < difficultySettings.Length){ //update difficulty settings
			prizes = difficultySettings[difficulty].prizes;
			maxSpins = difficultySettings[difficulty].spins;
			timeStep = difficultySettings[difficulty].timeScale;
		} else {
			maxSpins ++; //once we finish all the defined difficulty settings, just keep increasing the spins indefinitely
		}

		textMain.text = setupText;
		SpawnPrizes(prizes);
	} //close GameSetuo()


	void SpawnPrizes(int howMany) {
		var allPrizes = GameObject.FindGameObjectsWithTag("prize");
		if (allPrizes != null) {
			foreach (GameObject prize in allPrizes) { //destroy all existing prizes (from previous round)
				GameObject.Destroy(prize);
			}
		}
		foreach (BouncingBox box in boxScripts) {
			box.SpawnPrizes(howMany);
		}
	}
	
	
	void Update () {

		//if the game were more complicated we should use a state machine, but premature optimization is a wily enemy
		if (gameState == gState.setup) { //waiting to get started
			if ( Input.GetMouseButtonDown(0)) { //click to begin
				foreach (BouncingBox box in boxScripts) {
					box.Drop(timeFall);
				}
				readyTime = Time.time + timeSetup;
				gameState = gState.spin;
				textMain.text = spinText;
			}

		} else if (gameState == gState.spin) { //boxes are spinning
			if (Time.time > readyTime) {
				//keep spinning until we've spun enough, then transition to the "Find the thing" state
				if (spins < maxSpins) {
					if (spins == 0) {
						Spin(0); //first spin is always specific to pull the prizes away from each other
					} else { 
						RandomSpin();
					}
					readyTime = Time.time + timeStep + timePause;
					spins++;
				} else {
					PrepareToPick();
				}
			}

		} else if (gameState == gState.pick) {
			if ( Input.GetMouseButtonDown(0)) { //if clicking
				if (selected != null) {//already stored what we might be clicking on in the HeavyUpdate, no need to check again
					selectedBox.Reveal(0f);

					if (selectedBox.prizeID == missingPrize) { //if player chose correctly
						GameWin();
					} else {
						GameLose();
					}
				}
			} //close if clicking

		} else if  (gameState == gState.reset) {
			if (Time.time > readyTime) GameReset();
		}

		frameCount ++; //check if it's time to run Heavy Update
		if (frameCount >= heavyUpdateFrames){
			HeavyUpdate();
		}
	}//close Update()


	//Optimization. Some things you don't need to do every frame. Frequency defined by heavyUpdateFrames
	void HeavyUpdate() {  //currently only used to select boxes on mouse over
		frameCount = 0;

		RaycastHit hit = new RaycastHit();
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit, 100)){ // if something hit...
			Transform hitt = hit.transform;
			
			if (hitt != selected) {
				if (selected != null) {
					selectedBox.Deselect();
					selected = null;
					selectedBox = null;
				}
				if (hitt.tag == "Box") {
					selected = hitt;
					selectedBox = selected.GetComponent<BouncingBox>();
					selectedBox.Select();
				}
			}
		}//close Raycast
	} //close HeavyUpdate()


	void RandomSpin(){
		int k = Random.Range(0,8);
		Spin(k);
	}


	void Spin(int k) {
		switch (k) {
		case 0:  //four corners
				Clockwise(0,0,1,1);
				Clockwise(0,2,1,1);
				Clockwise(2,0,1,1);
				Clockwise(2,2,1,1);	
			    break;
		case 1:  //3x3 from 0,0
			Clockwise(0,0,2,2); break;
		case 2: //3x3 from 0,1
			Clockwise(0,1,2,2); break;
		case 3:  //3x3 from 1,0
			Clockwise(1,0,2,2); break;
		case 4:  //3x3 from 1,1
			Clockwise(1,1,2,2); break;
		case 5: //outer and inner
			Clockwise(0,0,3,3);
			Clockwise(1,1,1,1);
			break;
		case 6:  //vertical slices
	 			Clockwise(0,0,3,1);
				Clockwise(0,2,3,1);
			break;
		case 7:
				//horizontal slices
				Clockwise(0,0,1,3);
				Clockwise(2,0,1,3);
			break;
		} //close switch
    } //close Spin()


	void Clockwise (int tlcRow, int tlcCol, int szRows, int szCols) {
		//selects a square-ring of boxes, and moves each box one space clockwise around the ring
		//inputs: top-left-corner Row, top-left-corner Column, size of Rows, size of Columns
		foreach (BouncingBox box in boxScripts) {    
			if (box.row == tlcRow && box.col >= tlcCol && box.col < tlcCol + szCols){
				box.MoveMe(1,0,timeStep);
			} else if (box.col == tlcCol + szCols && box.row >= tlcRow && box.row < tlcRow + szRows){
				box.MoveMe(0,-1,timeStep);
			} else if (box.row == tlcRow + szRows && box.col > tlcCol && box.col <= tlcCol + szCols){
				box.MoveMe(-1,0,timeStep);
			} else if (box.col == tlcCol && box.row > tlcRow && box.row <= tlcRow + szRows){
				box.MoveMe(0,1,timeStep);
			}
		} //close for all boxes
	} //close Clockwise()
	

	void PrepareToPick() {
		gameState = gState.pick;
		textMain.text = pickText;
		textMain.enabled = true;
		missingPrize = Random.Range(0,prizes);
		foreach (BouncingBox box in boxScripts) {
			box.FirstReveal(missingPrize, prizes);
        }
    } //close PrepareToPick()
	

	void GameLose() {
		if (quiltScript != null) quiltScript.victory = false; 

		textMain.text = incorrectText;
		soundMgr.PlaySound("lose");
		foreach (BouncingBox box in boxScripts) {
			box.Reveal(timeReveal);
		}

		if (difficulty > 9) difficulty --;
		if (difficulty > 12) difficulty --;
		gameState = gState.reset;
		readyTime = Time.time + timeReset;
	} //close GameLose()


	void GameWin() {
		if (quiltScript != null) quiltScript.victory = true;

		textMain.text = correctTexts[Random.Range(0, Mathf.Min(difficulty, correctTexts.Length))];
		soundMgr.PlaySound("win");
		foreach (BouncingBox box in boxScripts) {
			box.Reveal(timeReveal);
		}

		difficulty ++;
		gameState = gState.reset;
		readyTime = Time.time + timeReset;
	} //close GameWin()

	
	void GameReset() {
		foreach (BouncingBox box in boxScripts) {
			box.Reset();
		}
		GameSetup();
	}
	
}
    