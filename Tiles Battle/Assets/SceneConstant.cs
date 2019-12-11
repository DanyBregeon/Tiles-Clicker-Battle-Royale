using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneConstant : MonoBehaviour
{
    public static SceneConstant sc;

    public MenuManager.Difficulty difficulty;

    public Dictionary<MenuManager.Difficulty, float> speedByDifficulty;
    public Dictionary<MenuManager.Difficulty, float> accuracyByDifficulty;

    // Start is called before the first frame update
    void Start()
    {
        if(sc != null)
        {
            Destroy(sc.gameObject);
        }

        sc = this;

        DontDestroyOnLoad(gameObject);

        speedByDifficulty = new Dictionary<MenuManager.Difficulty, float>();
        speedByDifficulty.Add(MenuManager.Difficulty.easy, 2);
        speedByDifficulty.Add(MenuManager.Difficulty.medium, 3);
        speedByDifficulty.Add(MenuManager.Difficulty.hard, 4);
        speedByDifficulty.Add(MenuManager.Difficulty.veryHard, 5);
        speedByDifficulty.Add(MenuManager.Difficulty.brutal, 6);

        accuracyByDifficulty = new Dictionary<MenuManager.Difficulty, float>();
        accuracyByDifficulty.Add(MenuManager.Difficulty.easy, 90);
        accuracyByDifficulty.Add(MenuManager.Difficulty.medium, 95);
        accuracyByDifficulty.Add(MenuManager.Difficulty.hard, 98);
        accuracyByDifficulty.Add(MenuManager.Difficulty.veryHard, 99);
        accuracyByDifficulty.Add(MenuManager.Difficulty.brutal, 100);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
