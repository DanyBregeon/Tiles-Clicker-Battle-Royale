using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour {

    public Tile[,] tiles;
    public static int sizeBoardX = 4;
    public static int sizeBoardY = 4;
    public static int nbStartRessources = 3;
    public static int maxCombo = 20;
    public static int maxComboShield = 20;
    public static int baseIncome = 10;
    public static int[] buildingPrice;
    public static int generatorIncome = 10;
    public static int bonusDefenseMissileLauncher = 5;
    public static int nbShieldCore = 5;
    public static float cdMissileLauncher = 15;
    public static float cdLaser = 30;
    public static float cdNova = 45;
    public static float cdUltime = 45;
    public static float baseARTime = 1.5f;
    public static int nbPlayer = 17;
    public static int nbTileDestroyToLose = 5;
    public static int bonusDefenseFocus = 50;
    public static int bonusAttackFocus = 25;
    private float maxFontSize;
    [SerializeField]
    private TMP_Text scoreText;

    public SpriteRenderer focusBoard;
    public GameObject textProfitPrefab;
    public static Transform canvas;
    public static Transform canvasFront;
    private AudioSource audioSource;
    public AudioClip[] soundValid;
    public AudioClip soundUnvalid;
    public AudioClip[] soundParry;
    public AudioClip soundShoot;
    public AudioClip soundHit;
    public AudioClip soundChangeTarget;
    public AudioClip soundEnergyAttack;
    public AudioClip soundNovaAttack;
    public AudioClip soundUltimeAttack;
    public AudioClip soundBadgeWin;
    public AudioClip soundKillOpponent;
    public AudioClip soundShieldBlock;
    public AudioClip soundGameOver;
    public AudioClip soundWin;

    public GameObject endGame;
    private bool isGameFinish;

    public bool isBuilding;
    public GameObject buildingImg;
    public GameObject building;

    public static List<int> opponentAlive;
    public GameObject[] allOpponentBoard;

    private GameObject target;
    public int num = 16;
    public List<int> targetBy;
    private bool isAlive;
    public bool core;
    private int score;
    private int combo;
    private int comboShield;

    private int nbShield;
    private int currentNbShield;
    public GameObject shieldImg;
    private AudioSource shieldAudioSource;
    private TMP_Text shieldText;
    private int pourcentageAttackBonus;
    public GameObject attackImg;
    private TMP_Text attackText;
    private int pourcentageDefenseBonus;
    public GameObject defenseImg;
    private TMP_Text defenseText;
    private int nbChevron;
    public GameObject chevronImg;
    private AudioSource chevronAudioSource;
    private TMP_Text chevronText;
    public Image chevronGauche;
    public Image chevronDroit;

    public bool buildingNovaIsBuild;
    public bool buildingUltimeIsBuild;

    private float baseCdMissileLauncher;
    private float baseCdLaser;
    private float baseCdNova;
    private float baseCdUltime;

    public static int lastHitBy;

    private int nbTileDestroy;

    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            int difference = value - score;
            score = value;
            scoreText.text = "" + value;
            StartCoroutine("IncrementScore", difference);
        }
    }

    public int Combo
    {
        get
        {
            return combo;
        }

        set
        {
            if(value <= maxCombo)
            combo = value;
        }
    }

    public int NbTileDestroy
    {
        get
        {
            return nbTileDestroy;
        }

        set
        {
            nbTileDestroy = value;
            if(nbTileDestroy >= nbTileDestroyToLose)
            {
                IsAlive = false;
                //Debug.Break();
            }
        }
    }

    public GameObject Target
    {
        get
        {
            return target;
        }

        set
        {
            if(target != null)
            {
                if (target.GetComponent<AI>() != null)
                {
                    target.GetComponent<AI>().targetBy.Remove(num);
                    if (target.GetComponent<AI>().targetBy.Count >= 1)
                    {
                        target.GetComponent<AI>().PourcentageDefenseBonus -= bonusDefenseFocus;
                        target.GetComponent<AI>().PourcentageAttackBonus -= bonusAttackFocus;
                    }
                    //print("REMOVE BY CHANGE PLAYER");
                }
            }
            target = value;
            target.GetComponent<OpponentBoard>().TargetImg.enabled = true;
            for (int i = 0; i < allOpponentBoard.Length; i++)
            {
                if(allOpponentBoard[i] != target)
                {
                    allOpponentBoard[i].GetComponent<OpponentBoard>().TargetImg.enabled = false;
                }
            }
            audioSource.clip = soundChangeTarget;
            audioSource.Play();
            //si ia
            if(target.GetComponent<AI>() != null)
            {
                target.GetComponent<AI>().targetBy.Add(num);
                if (target.GetComponent<AI>().targetBy.Count >= 2)
                {
                    target.GetComponent<AI>().PourcentageDefenseBonus += bonusDefenseFocus;
                    target.GetComponent<AI>().PourcentageAttackBonus += bonusAttackFocus;
                }
            }
            
        }
    }

    public bool IsAlive
    {
        get
        {
            return isAlive;
        }

        set
        {
            isAlive = value;
            if (!isAlive)
            {
                allOpponentBoard[lastHitBy].GetComponent<AI>().NbChevron += 2;
                List<int> targetBy2 = new List<int>(targetBy);
                foreach (int i in targetBy2)
                {
                    allOpponentBoard[i].GetComponent<AI>().ChangeTarget();
                }
                EndGame();
            }
        }
    }

    public int NbShield
    {
        get
        {
            return nbShield;
        }

        set
        {
            /*if (value - nbShield > 0)*/ currentNbShield += value - nbShield;
            nbShield = value;
            if (currentNbShield == nbShield)
            {
                shieldText.text = "" + nbShield;
            }
            else
            {
                shieldText.text = currentNbShield + "/" + nbShield;
            }
        }
    }

    public int PourcentageAttackBonus
    {
        get
        {
            return pourcentageAttackBonus;
        }

        set
        {
            pourcentageAttackBonus = value;
            attackText.text = pourcentageAttackBonus + "%";
        }
    }

    public int PourcentageDefenseBonus
    {
        get
        {
            return pourcentageDefenseBonus;
        }

        set
        {
            pourcentageDefenseBonus = value;
            defenseText.text = pourcentageDefenseBonus + "%";
        }
    }

    public int NbChevron
    {
        get
        {
            return nbChevron;
        }

        set
        {
            if(nbChevron < 25 && value >= 25)
            {
                PourcentageAttackBonus += 25 * (value / 25);
                PourcentageDefenseBonus += 25 * (value / 25);
            }
            else if (nbChevron < 50 && value >= 50)
            {
                PourcentageAttackBonus += 25 * ((value / 25)-1);
                PourcentageDefenseBonus += 25 * ((value / 25) - 1);
            }
            else if (nbChevron < 75 && value >= 75)
            {
                PourcentageAttackBonus += 25 * ((value / 25) - 2);
                PourcentageDefenseBonus += 25 * ((value / 25) - 2);
            }
            else if(nbChevron < 100 && value >= 100)
            {
                PourcentageAttackBonus += 25;
                PourcentageDefenseBonus += 25;
            }
            nbChevron = value;
            if (nbChevron > 100) nbChevron = 100;
            chevronText.text = "" + nbChevron;
            //fill amount max = 0.75, fill amount min = 0.2
            chevronGauche.fillAmount = 0.2f + 0.55f * ((float)nbChevron / 100f);
            chevronDroit.fillAmount = 0.2f + 0.55f * ((float)nbChevron / 100f);
            //shieldAudioSource.clip = soundBadgeWin;
            if(nbChevron > 0 && nbChevron < 100)
            {
                shieldAudioSource.PlayOneShot(soundBadgeWin);
            }
        }
    }

    public int CurrentNbShield
    {
        get
        {
            return currentNbShield;
        }

        set
        {
            currentNbShield = value;
            if (currentNbShield < 0) currentNbShield = 0;
            if (currentNbShield == NbShield)
            {
                shieldText.text = "" + currentNbShield;
            }
            else
            {
                shieldText.text = currentNbShield + "/" + NbShield;
                //shieldAudioSource.clip = soundShieldBlock;
                shieldAudioSource.PlayOneShot(soundShieldBlock);
            }
        }
    }

    public int ComboShield
    {
        get
        {
            return comboShield;
        }

        set
        {
            comboShield = value;
        }
    }

    public AudioSource ChevronAudioSource
    {
        get
        {
            return chevronAudioSource;
        }

        set
        {
            chevronAudioSource = value;
        }
    }

    public bool IsGameFinish
    {
        get
        {
            return isGameFinish;
        }

        set
        {
            isGameFinish = value;
        }
    }

    private void Awake()
    {
        isAlive = true;
        canvas = GameObject.Find("Canvas").transform;
        canvasFront = GameObject.Find("CanvasFront").transform;
        focusBoard = GameObject.Find("focusBoard").GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        shieldText = shieldImg.GetComponentInChildren<TMP_Text>();
        attackText = attackImg.GetComponentInChildren<TMP_Text>();
        defenseText = defenseImg.GetComponentInChildren<TMP_Text>();
        chevronText = chevronImg.GetComponentInChildren<TMP_Text>();
        chevronGauche = chevronImg.transform.Find("ChevronGauche").GetComponent<Image>();
        chevronDroit = chevronImg.transform.Find("ChevronDroite").GetComponent<Image>();
        shieldAudioSource = shieldImg.GetComponent<AudioSource>();
        ChevronAudioSource = chevronImg.GetComponent<AudioSource>();
        baseCdMissileLauncher = cdMissileLauncher;
        baseCdLaser = cdLaser;
        baseCdNova = cdNova;
        baseCdUltime = cdUltime;
        NbChevron = 0;
        opponentAlive = new List<int>();
        targetBy = new List<int>();
        for (int i = 0; i < nbPlayer -1; i++)
        {
            opponentAlive.Add(i);
        }
        buildingPrice = new int[6] { 500,1000,1500,2500,3500,5000};
    }

    // Use this for initialization
    void Start () {
        tiles = new Tile[sizeBoardY, sizeBoardX];
        for (int y = 0; y < sizeBoardY; y++)
        {
            char letter = Convert.ToChar(y + 65);
            for (int x = 0; x < sizeBoardX; x++)
            {
                tiles[y, x] = transform.Find("t" + letter + (x+1)).GetComponent<Tile>();
            }
        }
        Target = allOpponentBoard[UnityEngine.Random.Range(0, allOpponentBoard.Length)];

        initializeRessources();
    }
	
	// Update is called once per frame
	void Update () {
        string s = "";
        for (int i = 0; i < allOpponentBoard.Length; i++)
        {
            if (allOpponentBoard[i].GetComponent<AI>().IsAlive)
            {
                s += i + "->" + allOpponentBoard[i].GetComponent<AI>().Target + "  ";
            }
        }
        //print(s);

        string s2 = "targetBy : ";
        foreach (int ai in opponentAlive)
        {
            s2 += ai + "-> ";
            foreach (int i in allOpponentBoard[ai].GetComponent<AI>().targetBy)
            {
                s2 += i + "  ";
            }
            s2 += "; ";
        }
        //print(s2);


        /*if(Target != null && !Target.GetComponent<AI>().IsAlive)
        {
            print("Target de la target : " + Target.GetComponent<AI>().Target);
            print("Target focus par : ");
            foreach(int i in Target.GetComponent<AI>().targetBy)
            {
                print(i);
            }
            print("Target : " + Target);
            print("focus par : ");
            foreach (int i in targetBy)
            {
                print(i);
            }
            Debug.Break();
        }*/
    }

    private void initializeRessources()
    {
        int[] rngTab = new int[(sizeBoardX * sizeBoardY)];
        for (int i = 0; i < rngTab.Length; i++)
        {
            rngTab[i] = i;
        }
        //pour le nombre de tirage qui doit être fait, on tire au hasard un élément puis on le retire,
        //et on décrémente de 1 la range max
        for (int i = 0; i < nbStartRessources; i++)
        {
            int rng = UnityEngine.Random.Range(0, rngTab.Length - i);
            tiles[rngTab[rng] / sizeBoardX, rngTab[rng] % sizeBoardX].IsRessource = true;
            rngTab[rng] = rngTab[rngTab.Length - 1 - i];
        }

    }

    public void AddRessource()
    {
        List<Tile> listRessourceOff = new List<Tile>();
        for (int y = 0; y < sizeBoardY; y++)
        {
            for (int x = 0; x < sizeBoardX; x++)
            {
                if (!tiles[y, x].IsRessource && !tiles[y, x].IsDestroy)
                {
                    listRessourceOff.Add(tiles[y, x]);
                }
            }
        }
        int rng = UnityEngine.Random.Range(0, listRessourceOff.Count);
        listRessourceOff[rng].IsRessource = true;
    }

    IEnumerator IncrementScore(int difference)
    {
        //float incrementFontSize = 0;
        //float decrementFontSize = 0;
        for (int i = 0; i < 5; i++)
        {
            scoreText.color = new Color(0f + (float)i / 8f, 0.8f + (float)i / 20f, 0.8f - (4f*i) / 20f);
            //if(scoreText.fontSize < maxFontSize)
            //{
            if(difference > 0)
                scoreText.fontSize += 0.15f * difference;
                //incrementFontSize += 0.2f * difference;
            //}
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < 5; i++)
        {
            scoreText.color = new Color(0.5f - (float)i / 8f, 1f - (float)i / 20f, 0f + (4f * i) / 20f);
            //if(decrementFontSize < incrementFontSize)
            //{
            if (difference > 0)
                scoreText.fontSize -= 0.15f * difference;
                //decrementFontSize -= 0.2f * difference;
            //}
            yield return new WaitForFixedUpdate();
        }
    }

    public void SpeedUpCd(float newSpeed)
    {
        cdMissileLauncher = baseCdMissileLauncher * newSpeed;
        cdLaser = baseCdLaser * newSpeed;
        cdNova = baseCdNova * newSpeed;
        cdUltime = baseCdUltime * newSpeed;
    }

    public void EndGame()
    {
        int rank = (opponentAlive.Count + 1);
        endGame.SetActive(true);
        endGame.transform.Find("ImageRank").Find("TextRank").GetComponent<TMP_Text>().text = "" + rank;
        if (rank == 1)
        {
            audioSource.PlayOneShot(soundWin);
            endGame.transform.Find("ImageRank").gameObject.SetActive(false);
            endGame.transform.Find("ImageRank1").gameObject.SetActive(true);
        }
        else
        {
            audioSource.PlayOneShot(soundGameOver);
        }

        IsGameFinish = true;
    }
}
