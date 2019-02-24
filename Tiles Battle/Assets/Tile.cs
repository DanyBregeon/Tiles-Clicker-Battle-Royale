using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour {

    public static KeyCode button1;
    public static KeyCode button2;
    public static KeyCode button3;

    private SpriteRenderer image;
    private Animator anim;
    private bool isRessource;
    private Board board;
    private AudioSource audioSource;
    private bool isDestroy;
    private int pv;

    private Building building;
    private int nbAttack;
    private TMP_Text textNbAttack;

    private Color currentColor;
    public static Color damageColor;
    public static Color destroyColor;
    private Color ressourceColor;
    private Color errorColor;
    private Color validColor;

    public bool IsRessource
    {
        get
        {
            return isRessource;
        }

        set
        {
            isRessource = value;
            if (isRessource)
            {
                StartCoroutine("BecomeRessource");
            }
            else
            {
                if (!isDestroy)
                {
                    int bonusScore = Board.baseIncome + board.Combo;
                    board.Score = board.Score + bonusScore;
                    board.Combo++;
                    int rngSound = Random.Range(0, board.soundValid.Length);
                    audioSource.clip = board.soundValid[rngSound];
                    audioSource.Play();
                    StartCoroutine("StopBeingRessource", bonusScore);
                }
            }

        }
    }

    public bool IsDestroy
    {
        get
        {
            return isDestroy;
        }

        set
        {
            isDestroy = value;
            if (isDestroy)
            {
                board.NbTileDestroy++;
                if (IsRessource)
                {
                    IsRessource = false;
                    board.AddRessource();
                }
                CurrentColor = destroyColor;
                if(building != null)
                {
                    Destroy(building.gameObject);
                }
            }
        }
    }

    public int NbAttack
    {
        get
        {
            return nbAttack;
        }

        set
        {
            nbAttack = value;
            if(nbAttack > 0)
            {
                textNbAttack.text = "" + nbAttack;
            }
            else
            {
                textNbAttack.text = "";
            }
        }
    }

    public int Pv
    {
        get
        {
            return pv;
        }

        set
        {
            pv = value;
            if(pv == 1)
            {
                CurrentColor = damageColor;
                board.allOpponentBoard[Board.lastHitBy].GetComponent<AI>().NbChevron += 2;
            }
            else if(pv == 0)
            {
                board.allOpponentBoard[Board.lastHitBy].GetComponent<AI>().NbChevron += 2;
                IsDestroy = true;
            }
        }
    }

    public Color CurrentColor
    {
        get
        {
            return currentColor;
        }

        set
        {
            if(!IsRessource || IsDestroy)
            {
                image.color = value;
            }
            else if (IsRessource && !IsDestroy)
            {
                image.color = new Color(ressourceColor.r * value.r, ressourceColor.g * value.g, ressourceColor.b * value.b, value.a);
            }
            currentColor = value;
            ressourceColor = new Color(ressourceColor.r * currentColor.r, ressourceColor.g * currentColor.g, ressourceColor.b * currentColor.b, currentColor.a);
            validColor = new Color(validColor.r * currentColor.r, validColor.g * currentColor.g, validColor.b * currentColor.b, currentColor.a);
            errorColor = new Color(errorColor.r * currentColor.r, errorColor.g * currentColor.g, errorColor.b * currentColor.b, currentColor.a);
        }
    }

    public Building Building
    {
        get
        {
            return building;
        }

        set
        {
            building = value;
        }
    }

    // Use this for initialization
    void Awake () {
        button1 = KeyCode.X;
        button2 = KeyCode.W;
        button3 = KeyCode.Mouse0;

        board = GameObject.Find("Board").GetComponent<Board>();
        image = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        textNbAttack = Instantiate(board.textProfitPrefab, transform.position, transform.rotation, Board.canvasFront).GetComponent<TMP_Text>();
        textNbAttack.color = Color.white;
        Pv = 2;
        CurrentColor = new Color(1, 1, 1, 0.8f);
        ressourceColor = new Color(0, 0.8f, 0.8f, 0.8f);
        errorColor = new Color(1, 0.8f, 0.8f, 0.8f);
        validColor = new Color(0.5f, 1, 0, 0.8f);
        damageColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        destroyColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
    }
	
	// Update is called once per frame
	void Update () {

    }

    private void OnMouseOver()
    {
        if (!board.isBuilding)
        {
            if ((Input.GetKeyDown(button1) || Input.GetKeyDown(button2) || Input.GetKeyDown(button3)))
            {
                if (NbAttack != 0)
                {
                    print("BUUUUUUUUUUUG :(");
                    Debug.Break();
                }
                
                if (IsRessource)
                {
                    board.AddRessource();
                    IsRessource = false;
                }
                else if (building != null && building.cdReady)
                {
                    if (building.numBuilding == 1)
                    {
                        building.cdReady = false;
                        building.LaunchMissileLauncher();
                        audioSource.clip = board.soundShoot;
                        audioSource.Play();
                    }
                    else if (building.numBuilding == 3)
                    {
                        building.cdReady = false;
                        building.LaunchLaser();
                        audioSource.clip = board.soundEnergyAttack;
                        audioSource.Play();
                    }
                    else if (building.numBuilding == 4)
                    {
                        building.cdReady = false;
                        building.LaunchNova();
                        audioSource.clip = board.soundNovaAttack;
                        audioSource.Play();
                    }
                    else if (building.numBuilding == 5)
                    {
                        building.cdReady = false;
                        building.LaunchUltime();
                        audioSource.clip = board.soundUltimeAttack;
                        audioSource.Play();
                    }
                }
                else
                {
                    board.Combo = 0;
                    audioSource.clip = board.soundUnvalid;
                    audioSource.Play();
                    StartCoroutine("BreakCombo");
                }
            }
        }
        else
        {
            if(building == null)
            {
                board.buildingImg.transform.position = transform.position;
                if (Input.GetKeyDown(button1) || Input.GetKeyDown(button2) || Input.GetKeyDown(button3))
                {
                    board.focusBoard.color = new Color(0, 0, 0, 0);
                    //create building
                    board.isBuilding = false;
                    Destroy(board.buildingImg);
                    board.Score -= Board.buildingPrice[board.building.GetComponent<Building>().numBuilding];
                    //building = Instantiate(board.building, transform.position, transform.rotation, transform);
                    GameObject instance = Instantiate(board.building, transform.position, transform.rotation, Board.canvas);
                    building = instance.GetComponent<Building>();
                }
            }

        }
    }

    IEnumerator BecomeRessource()
    {
        for (int i = 0; i < 10; i++)
        {
            //image.color = new Color(currentColor.r - (float)i / 9f, currentColor.g - (float)i / 45f, currentColor.b - (float)i / 45f, currentColor.a);
            image.color = Color.Lerp(CurrentColor, ressourceColor, (float)i / 9f);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator StopBeingRessource(int bonusScore)
    {
        GameObject textProfitInstance = (GameObject) Instantiate(board.textProfitPrefab, transform.position, transform.rotation, Board.canvas);
        TMP_Text textProfit = textProfitInstance.GetComponent<TMP_Text>();
        textProfit.text = "+" + bonusScore;
        for (int i = 0; i < 10; i++)
        {
            //image.color = new Color(0.5f + (float)i / 18f, 1, (float)i / 9f, 0.8f);
            image.color = Color.Lerp(validColor, CurrentColor, (float)i / 9f);
            textProfit.color = new Color(textProfit.color.r, textProfit.color.g, textProfit.color.b, 1f - (float)i / 9f);
            yield return new WaitForFixedUpdate();
        }
        Destroy(textProfitInstance);
    }

    IEnumerator BreakCombo()
    {
        for (int i = 0; i < 10; i++)
        {
            //image.color = new Color(1f, 0.8f + (float)i / 45f, 0.8f + (float)i / 45f, 0.8f);
            image.color = Color.Lerp(errorColor, CurrentColor, (float)i / 9f);
            yield return new WaitForFixedUpdate();
        }
    }
}
