using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BouncingBox : MonoBehaviour {

	//juice tuning - adjust to taste
	[Space(10)] [Header("Tuning")]
	public Material matBase;
	public Material matHighlight;
	[SerializeField] AudioClip[] selectSounds;

	//connections - these must be hooked up to -specific- things
	[Space(10)] [Header("Connections")]
	public int prizeID; //the order in which this box will start with a prize (as difficulty increases we add more prizes)
	public  int row, col;
	[SerializeField]  GameObject myBox;
	[SerializeField] GameObject prizePrefab;
	Animator myBoxAnimator; //must be component of myBox
	GameManager gameMgr; //found by tag

	//book keeping
    bool havePrize;
	bool revealed;
	int tileSize;
	int initialRow, initialCol;
    Vector3 initialPosition;


	void Start () {
		gameMgr = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
		tileSize = gameMgr.tileSize;
		myBoxAnimator = myBox.GetComponent<Animator>();
		myBox.GetComponent<Renderer>().enabled = false;
		initialPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		initialRow = row;
		initialCol = col;
		revealed = false;
	}


	public void MoveMe (int movX, int movZ, float dur) {
		Vector3 newPos = transform.position;
		newPos.x += movX * tileSize;
		newPos.z += movZ * tileSize;
		col += movX;
		row -= movZ;
		myBoxAnimator.speed = 1 / dur;
		myBoxAnimator.Play("bounce2");
		iTween.MoveTo(gameObject, iTween.Hash("position", newPos, "time", dur * 0.8, "delay", dur * 0.2, "easeType", iTween.EaseType.linear));
	}


	public void Select() {
		myBox.GetComponent<Renderer>().material = matHighlight;
		GetComponent<AudioSource>().clip = selectSounds[Random.Range(0, selectSounds.Length)];
		GetComponent<AudioSource>().Play();	
    }

	public void Deselect() {
		myBox.GetComponent<Renderer>().material = matBase;
	}
    

    public void Drop(float maxDelay) {
		transform.localRotation = Quaternion.Euler(0f, (float) Random.Range(0,5) * 90f, 0f);
		myBox.GetComponent<Renderer>().enabled = true;
		GetComponent<Collider>().enabled = true;
		myBoxAnimator.speed = 1;
		StartCoroutine(DelayedPlayAnim(Random.Range(0f, maxDelay), "fall"));
	}


	public void FirstReveal(int missingPrize, int maxPrize) {
		if (prizeID < maxPrize) {
			if (prizeID != missingPrize) Reveal(0f);
		}
	}


	public void Reveal(float maxDelay) {
		if (revealed == false) {  //some boxes are revealed during the "first reveal", and we don't want to externally keep track of which ones
			revealed = true;
			GetComponent<Collider>().enabled = false;
			myBoxAnimator.speed = 1;
			StartCoroutine(DelayedPlayAnim(Random.Range(0f, maxDelay), "reveal"));
		}
	}


	IEnumerator DelayedPlayAnim (float delay, string anim){
		yield return new WaitForSeconds(delay);
		myBoxAnimator.Play(anim);
	}
	

	public void SpawnPrizes(int howMany) {
		if (prizeID < howMany) {
			GameObject newPrize = (GameObject)Instantiate(prizePrefab, transform.position, Quaternion.identity);
			newPrize.transform.parent = transform;
			havePrize = true;
		} else {
			havePrize = false;
		}
	}


	public void Reset() {
		transform.position = initialPosition;
		row = initialRow;
		col = initialCol;
		myBox.GetComponent<Renderer>().enabled = false;
		revealed = false;
	}
}
