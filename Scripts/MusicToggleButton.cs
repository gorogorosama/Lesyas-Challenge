using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MusicToggleButton : MonoBehaviour {

	//connections - these must be hooked up to -specific- things
    [SerializeField] Color btnOn, btnOff;
    [SerializeField] Image img;
	[SerializeField] SoundManager soundMgr;

	//bookkeeping
	bool musicOn;


	void Start () {
		setOn();
	}


    public void toggle(){
        if (musicOn == true){
            setOff ();
        } else {
            setOn ();
        }
    }


    void setOn(){
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
