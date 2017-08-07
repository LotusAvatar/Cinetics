using UnityEngine;
using System.Collections;

public class SplashLoading : MonoBehaviour {

    public UISprite loadingpProgress;
    float loadingProgress;
    float loadingDelay = 4f;
    bool loadGame;
    // Use this for initialization
	
	// Update is called once per frame
	void Update ()
    {
        LoadingProgress();
    }

    void LoadingProgress()
    {
        loadingProgress += Time.deltaTime / loadingDelay;
        if (loadingProgress < 1f)
            loadingpProgress.fillAmount = loadingProgress;
        else
            LoaingComplete();
    }

    void LoaingComplete()
    {
        if (!loadGame)
        {
            loadGame = true;
            Application.LoadLevelAsync(Application.loadedLevel +1);
        }
    }
}
