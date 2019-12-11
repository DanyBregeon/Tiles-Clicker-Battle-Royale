using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public enum Difficulty
    {
        easy,
        medium,
        hard,
        veryHard,
        brutal
    }

    Difficulty currentDifficulty;
    public GameObject[] uiDifficulties;
    public SceneConstant sceneConstant;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextDifficulty()
    {
        currentDifficulty = (Difficulty)(((int)(currentDifficulty + 1)) % Enum.GetNames(typeof(Difficulty)).Length);
        sceneConstant.difficulty = currentDifficulty;
        ChangeDifficulty();
    }

    public void PreviousDifficulty()
    {
        int nbDifficulties = Enum.GetNames(typeof(Difficulty)).Length;
        currentDifficulty = (Difficulty)((nbDifficulties + (int)currentDifficulty - 1) % nbDifficulties);
        sceneConstant.difficulty = currentDifficulty;
        ChangeDifficulty();
    }

    private void ChangeDifficulty()
    {
        for (int i = 0; i < uiDifficulties.Length; i++)
        {
            if(i == (int)currentDifficulty)
            {
                uiDifficulties[i].SetActive(true);
            }
            else
            {
                uiDifficulties[i].SetActive(false);
            }
        }
    }
}
