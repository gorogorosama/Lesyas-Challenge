using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {
	
	//connections
	[SerializeField] AudioClip clipWin, clipLose, clipMusic;
	[SerializeField] AudioSource sourceFanfare, sourceMusic;
	

	void Awake() {
	}

	void Start(){
	}


	public void PlaySound(string cname){
		switch (cname) {
		case "win":
			sourceFanfare.clip = clipWin; break;
		case "lose":
			sourceFanfare.clip = clipLose; break;
		}
		sourceFanfare.Play();
	} //close PlaySound()


	public void SetMusic (bool on){
		if (on){
			if (sourceMusic.isPlaying == false) sourceMusic.Play();
		} else {
			sourceMusic.Stop();
		}
	} //close SetMusic()
	
}

