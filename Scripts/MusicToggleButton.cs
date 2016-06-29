using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MusicToggleButton : MonoBehaviour {

    bool musicOn;
    [SerializeField] Color btnOn, btnOff;
	[SerializeField] float volOn, volOff;
    [SerializeField] Image img;
    [SerializeField] GameObject quiltMaganer;
	private goro_quilt quiltScript;
	[SerializeField] SoundManager soundMgr;

	// Use this for initialization
	void Start () {
		if (soundMgr == null) Debug.Log ("sound manager not connected to the music toggle button");
		setOn();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void toggle(){
        if (musicOn == true){
            setOff ();
        } else {
            setOn ();
        }
    }

    void setOn(){
		if (soundMgr == null) Debug.Log ("sound manager not connected to the music toggle button 2");
		soundMgr.SetMusic(true);
		img.color = btnOn;
        musicOn = true;
    }

    void setOff(){
		soundMgr.SetMusic(false);
		img.color = btnOff;
        musicOn = false;
    }
}
