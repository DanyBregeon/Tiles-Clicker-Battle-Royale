using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    public float speed; //nb de clic par seconde
    public float accuracy; //% de non missclic
    public int num;

    private int score = 0;
    private int combo = 0;
    private int comboShield;

    private bool isAlive = true;
    private int[] nbBuilding;
    private bool core;

    public GameObject[] prefabBuildingImg;
    public GameObject[] tiles;
    public GameObject attackPrefab;
    public GameObject KOPrefab;
    public bool[] isBuildingOnTiles;
    public GameObject[] BuildingOnTiles;
    public int[] tilesPv;
    private int nbTileDestroy;

    private Board board;

    public List<int> targetBy;
    private int target;

    public Queue<AttackTile> incomingAttack;

    private int nbShield;
    private int currentNbShield;
    private int pourcentageAttackBonus;
    private int pourcentageDefenseBonus;
    private int nbChevron;

    //bo : generator -> ML -> core -> laser -> nova -> ultime -> batiment à spam
    private int[] buildOrderDefensif;
    private int[] buildOrderClassic;
    private int[] buildOrderAggressif;
    private int[] buildOrder;

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
                Board.opponentAlive.Remove(num);
                Instantiate(KOPrefab, transform.position + transform.localScale.x * new Vector3(3, -3, 0), Quaternion.identity, Board.canvasFront);
                //change les targets
                string s = "targetBy before dead of " + num + "->" + targetBy.Count + " : ";
                foreach (int i in targetBy)
                {
                    s += i + "  ";
                }
                print(s);
                List<int> targetBy2 = new List<int>(targetBy);
                foreach (int i in targetBy2)
                {
                    if(i == board.num) //si c'est le joueur
                    {
                        print("Le joueur " + i + " change de cible suite à la mort de " + num);
                        board.Target = board.allOpponentBoard[Board.opponentAlive[Random.Range(0, Board.opponentAlive.Count)]];
                    }
                    else //si c'est une ia
                    {
                        print(i + " change de cible suite à la mort de " + num);
                        //(board.allOpponentBoard[targetBy[i]]).GetComponent<AI>().Target = Board.opponentAlive[Random.Range(0, Board.opponentAlive.Count-1)];
                        board.allOpponentBoard[i].GetComponent<AI>().ChangeTarget();
                    }
                }
                //enlève sa target
                if (target == board.num)
                {
                    board.targetBy.Remove(num);
                    GetComponent<OpponentBoard>().TargetByImg.enabled = false;
                    if (board.targetBy.Count >= 1)
                    {
                        board.PourcentageDefenseBonus -= Board.bonusDefenseFocus;
                        board.PourcentageAttackBonus -= Board.bonusAttackFocus;
                    }

                }
                else
                {
                    AI targetAI = board.allOpponentBoard[target].GetComponent<AI>();
                    targetAI.targetBy.Remove(num);
                    if (targetAI.targetBy.Count >= 1)
                    {
                        targetAI.PourcentageDefenseBonus -= Board.bonusDefenseFocus;
                        targetAI.PourcentageAttackBonus -= Board.bonusAttackFocus;
                    }
                    //print("REMOVE WHEN DEAD");
                }
            }
        }
    }

    public int Target
    {
        get
        {
            return target;
        }

        set
        {
            target = value;
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
            if(value - nbShield > 0) CurrentNbShield += value - nbShield;
            nbShield = value;
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
            nbChevron = value;
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
        }
    }

    private void DamageTaken(int indexTile, int hitBy)
    {
        int nbChevronGranted = 2;
        tilesPv[indexTile]--;
        if(tilesPv[indexTile] == 1)
        {
            tiles[indexTile].GetComponent<SpriteRenderer>().color = Tile.damageColor;
        }
        else if (tilesPv[indexTile] == 0)
        {
            tiles[indexTile].GetComponent<SpriteRenderer>().color = Tile.destroyColor;
            nbTileDestroy++;
            if(BuildingOnTiles[indexTile] != null)
            {
                if(BuildingOnTiles[indexTile].GetComponent<Building>().numBuilding == 1)
                {
                    PourcentageDefenseBonus -= Board.bonusDefenseMissileLauncher;
                }
                else if (BuildingOnTiles[indexTile].GetComponent<Building>().numBuilding == 2)
                {
                    NbShield -= Board.nbShieldCore;
                }
                Destroy(BuildingOnTiles[indexTile]);
            }
            if (nbTileDestroy >= Board.nbTileDestroyToLose)
            {
                IsAlive = false;
                nbChevronGranted += (NbChevron+1) / 2;
            }
        }

        if (hitBy == board.num)
        {
            board.NbChevron += nbChevronGranted;
        }
        else
        {
            board.allOpponentBoard[hitBy].GetComponent<AI>().NbChevron += nbChevronGranted;
        }
    }

    private void Awake()
    {
        board = GameObject.Find("Board").GetComponent<Board>();
        incomingAttack = new Queue<AttackTile>();
        nbBuilding = new int[6];
        target = -1;
        targetBy = new List<int>();
        isBuildingOnTiles = new bool[tiles.Length];
        BuildingOnTiles = new GameObject[tiles.Length];
        tilesPv = new int[tiles.Length];
        for (int i = 0; i < tilesPv.Length; i++)
        {
            tilesPv[i] = 2;
        }
        nbTileDestroy = 0;
    }

    // Use this for initialization
    void Start () {
        //choix du build order
        buildOrderDefensif = new int[7] { 4, 2, 4, 0, 1, 1, 3 };
        buildOrderClassic = new int[7] { 3, 5, 2, 2, 1, 1, 3 };
        buildOrderAggressif = new int[7] { 2, 7, 0, 3, 1, 1, 3 };
        buildOrder = new int[7];
        int rngBO = Random.Range(0, 3);
        if(rngBO == 0)
        {
            for (int i = 0; i < buildOrder.Length; i++)
            {
                buildOrder[i] = buildOrderDefensif[i];
            }
        }
        else if (rngBO == 0)
        {
            for (int i = 0; i < buildOrder.Length; i++)
            {
                buildOrder[i] = buildOrderClassic[i];
            }
        }
        else
        {
            for (int i = 0; i < buildOrder.Length; i++)
            {
                buildOrder[i] = buildOrderAggressif[i];
            }
        }

        ChangeTarget();
        StartCoroutine("AIRoutine");
        StartCoroutine("ChangeTargetOverTime");
	}
	
	// Update is called once per frame
	void Update () {
        /*string s = num + "->" + targetBy.Count + " : ";
        foreach (int i in targetBy)
        {
            s += i + "  ";
        }
        print(s);*/
        if (IsAlive && incomingAttack.Count > 0 && incomingAttack.Peek().time < Time.time)
        {
            AttackTile at = incomingAttack.Dequeue();
            DamageTaken(at.tile, at.from);
        }
    }

    IEnumerator AIRoutine()
    {
        while (IsAlive)
        {
            //Income
            if (Random.Range(1, 101) <= accuracy)
            {
                if(incomingAttack.Count > 0)
                {
                    incomingAttack.Dequeue();
                    comboShield++;
                    if(comboShield == Board.maxComboShield)
                    {
                        CurrentNbShield = nbShield;
                        comboShield = 0;
                    }
                }
                else
                {
                    score += Board.baseIncome + combo;
                    if (combo < Board.maxCombo)
                    {
                        combo++;
                    }
                }
            }
            else
            {
                combo = 0;
                comboShield = 0;
            }
            //Build order
            if (score >= Board.buildingPrice[0] && nbBuilding[0] < buildOrder[0])
            {
                //build generator
                List<int> tilesWithNoBuilding = new List<int>();
                for (int i = 0; i < isBuildingOnTiles.Length; i++)
                {
                    if(isBuildingOnTiles[i] == false)
                    {
                        tilesWithNoBuilding.Add(i);
                    }
                }
                if(tilesWithNoBuilding.Count > 0)
                {
                    score -= Board.buildingPrice[0];
                    nbBuilding[0]++;
                    int rng = Random.Range(0, tilesWithNoBuilding.Count);
                    GameObject generatorImg = Instantiate(prefabBuildingImg[0], tiles[tilesWithNoBuilding[rng]].transform.position, Quaternion.identity, Board.canvas);
                    generatorImg.transform.localScale = transform.localScale;
                    generatorImg.GetComponent<Building>().enabled = false;
                    BuildingOnTiles[tilesWithNoBuilding[rng]] = generatorImg;
                    isBuildingOnTiles[tilesWithNoBuilding[rng]] = true;
                    StartCoroutine("Generator", tilesWithNoBuilding[rng]);
                }

            }
            else if (score >= Board.buildingPrice[1] && nbBuilding[0] >= buildOrder[0] && nbBuilding[1] < buildOrder[1])
            {
                //build missileLauncher
                List<int> tilesWithNoBuilding = new List<int>();
                for (int i = 0; i < isBuildingOnTiles.Length; i++)
                {
                    if (isBuildingOnTiles[i] == false)
                    {
                        tilesWithNoBuilding.Add(i);
                    }
                }
                if (tilesWithNoBuilding.Count > 0)
                {
                    score -= Board.buildingPrice[1];
                    nbBuilding[1]++;
                    int rng = Random.Range(0, tilesWithNoBuilding.Count);
                    GameObject missileLauncherImg = Instantiate(prefabBuildingImg[1], tiles[tilesWithNoBuilding[rng]].transform.position, Quaternion.identity, Board.canvas);
                    missileLauncherImg.transform.localScale = transform.localScale;
                    missileLauncherImg.GetComponent<Building>().enabled = false;
                    BuildingOnTiles[tilesWithNoBuilding[rng]] = missileLauncherImg;
                    isBuildingOnTiles[tilesWithNoBuilding[rng]] = true;
                    PourcentageDefenseBonus += Board.bonusDefenseMissileLauncher;
                    StartCoroutine("MissileLauncher", tilesWithNoBuilding[rng]);
                }
            }
            else if (score >= Board.buildingPrice[2] && nbBuilding[0] >= buildOrder[0] && nbBuilding[1] >= buildOrder[1] && nbBuilding[2] < buildOrder[2])
            {
                //build core
                List<int> tilesWithNoBuilding = new List<int>();
                for (int i = 0; i < isBuildingOnTiles.Length; i++)
                {
                    if (isBuildingOnTiles[i] == false)
                    {
                        tilesWithNoBuilding.Add(i);
                    }
                }
                if (tilesWithNoBuilding.Count > 0)
                {
                    score -= Board.buildingPrice[2];
                    nbBuilding[2]++;
                    int rng = Random.Range(0, tilesWithNoBuilding.Count);
                    GameObject coreImg = Instantiate(prefabBuildingImg[2], tiles[tilesWithNoBuilding[rng]].transform.position, Quaternion.identity, Board.canvas);
                    coreImg.transform.localScale = transform.localScale;
                    coreImg.GetComponent<Building>().enabled = false;
                    BuildingOnTiles[tilesWithNoBuilding[rng]] = coreImg;
                    isBuildingOnTiles[tilesWithNoBuilding[rng]] = true;
                    NbShield += Board.nbShieldCore;
                }
            }
            else if (score >= Board.buildingPrice[3] && nbBuilding[0] >= buildOrder[0] && nbBuilding[1] >= buildOrder[1] && nbBuilding[2] >= buildOrder[2] && nbBuilding[3] < buildOrder[3])
            {
                //build laser
                List<int> tilesWithNoBuilding = new List<int>();
                for (int i = 0; i < isBuildingOnTiles.Length; i++)
                {
                    if (isBuildingOnTiles[i] == false)
                    {
                        tilesWithNoBuilding.Add(i);
                    }
                }
                if (tilesWithNoBuilding.Count > 0)
                {
                    score -= Board.buildingPrice[3];
                    nbBuilding[3]++;
                    int rng = Random.Range(0, tilesWithNoBuilding.Count);
                    GameObject laserImg = Instantiate(prefabBuildingImg[3], tiles[tilesWithNoBuilding[rng]].transform.position, Quaternion.identity, Board.canvas);
                    laserImg.transform.localScale = transform.localScale;
                    laserImg.GetComponent<Building>().enabled = false;
                    BuildingOnTiles[tilesWithNoBuilding[rng]] = laserImg;
                    isBuildingOnTiles[tilesWithNoBuilding[rng]] = true;
                    StartCoroutine("Laser", tilesWithNoBuilding[rng]);
                }
            }
            else if (score >= Board.buildingPrice[4] && nbBuilding[0] >= buildOrder[0] && nbBuilding[1] >= buildOrder[1] && nbBuilding[2] >= buildOrder[2] && nbBuilding[3] >= buildOrder[3] && nbBuilding[4] < buildOrder[4])
            {
                //build nova
                List<int> tilesWithNoBuilding = new List<int>();
                for (int i = 0; i < isBuildingOnTiles.Length; i++)
                {
                    if (isBuildingOnTiles[i] == false)
                    {
                        tilesWithNoBuilding.Add(i);
                    }
                }
                if (tilesWithNoBuilding.Count > 0)
                {
                    score -= Board.buildingPrice[4];
                    nbBuilding[4]++;
                    int rng = Random.Range(0, tilesWithNoBuilding.Count);
                    GameObject laserImg = Instantiate(prefabBuildingImg[4], tiles[tilesWithNoBuilding[rng]].transform.position, Quaternion.identity, Board.canvas);
                    laserImg.transform.localScale = transform.localScale;
                    laserImg.GetComponent<Building>().enabled = false;
                    BuildingOnTiles[tilesWithNoBuilding[rng]] = laserImg;
                    isBuildingOnTiles[tilesWithNoBuilding[rng]] = true;
                    StartCoroutine("Nova", tilesWithNoBuilding[rng]);
                }
            }
            else if (score >= Board.buildingPrice[5] && nbBuilding[0] >= buildOrder[0] && nbBuilding[1] >= buildOrder[1] && nbBuilding[2] >= buildOrder[2] && nbBuilding[3] >= buildOrder[3] && nbBuilding[4] >= buildOrder[4] && nbBuilding[5] < buildOrder[5])
            {
                //build ultime
                List<int> tilesWithNoBuilding = new List<int>();
                for (int i = 0; i < isBuildingOnTiles.Length; i++)
                {
                    if (isBuildingOnTiles[i] == false)
                    {
                        tilesWithNoBuilding.Add(i);
                    }
                }
                if (tilesWithNoBuilding.Count > 0)
                {
                    score -= Board.buildingPrice[5];
                    nbBuilding[5]++;
                    int rng = Random.Range(0, tilesWithNoBuilding.Count);
                    GameObject laserImg = Instantiate(prefabBuildingImg[5], tiles[tilesWithNoBuilding[rng]].transform.position, Quaternion.identity, Board.canvas);
                    laserImg.transform.localScale = transform.localScale;
                    laserImg.GetComponent<Building>().enabled = false;
                    BuildingOnTiles[tilesWithNoBuilding[rng]] = laserImg;
                    isBuildingOnTiles[tilesWithNoBuilding[rng]] = true;
                    StartCoroutine("Ultime", tilesWithNoBuilding[rng]);
                }
            }
            else if (score >= Board.buildingPrice[buildOrder[6]] && nbBuilding[0] >= buildOrder[0] && nbBuilding[1] >= buildOrder[1] && nbBuilding[2] >= buildOrder[2] && nbBuilding[3] >= buildOrder[3] && nbBuilding[4] >= buildOrder[4] && nbBuilding[5] >= buildOrder[5])
            {
                //build par defaut dans le bo
                List<int> tilesWithNoBuilding = new List<int>();
                for (int i = 0; i < isBuildingOnTiles.Length; i++)
                {
                    if (isBuildingOnTiles[i] == false)
                    {
                        tilesWithNoBuilding.Add(i);
                    }
                }
                if (tilesWithNoBuilding.Count > 0)
                {
                    score -= Board.buildingPrice[buildOrder[6]];
                    nbBuilding[buildOrder[6]]++;
                    int rng = Random.Range(0, tilesWithNoBuilding.Count);
                    GameObject laserImg = Instantiate(prefabBuildingImg[buildOrder[6]], tiles[tilesWithNoBuilding[rng]].transform.position, Quaternion.identity, Board.canvas);
                    laserImg.transform.localScale = transform.localScale;
                    laserImg.GetComponent<Building>().enabled = false;
                    BuildingOnTiles[tilesWithNoBuilding[rng]] = laserImg;
                    isBuildingOnTiles[tilesWithNoBuilding[rng]] = true;
                    StartCoroutine("Laser", tilesWithNoBuilding[rng]);
                }
            }

            yield return new WaitForSeconds(1f / speed);
        }
    }

    public void ChangeTarget()
    {
        if(target != -1)
        {
            if (target == board.num)
            {
                board.targetBy.Remove(num);
                GetComponent<OpponentBoard>().TargetByImg.enabled = false;
                if (board.targetBy.Count >= 1)
                {
                    board.PourcentageDefenseBonus -= Board.bonusDefenseFocus;
                    board.PourcentageAttackBonus -= Board.bonusAttackFocus;
                }
            }
            else
            {
                AI targetAI = board.allOpponentBoard[target].GetComponent<AI>();
                targetAI.targetBy.Remove(num);
                if (targetAI.targetBy.Count >= 1)
                {
                    targetAI.PourcentageDefenseBonus -= Board.bonusDefenseFocus;
                    targetAI.PourcentageAttackBonus -= Board.bonusAttackFocus;
                }
                //print("REMOVE WHEN CHANGE");
            }
        }

        int rngAlive = Random.Range(0, Board.opponentAlive.Count);
        target = Board.opponentAlive[rngAlive];
        if (target == num) //si la cible est lui même, la cible devient le joueur (numJoueur = 17)(si joueur vivant)
        {
            if (board.IsAlive)
            {
                target = board.num;
            }
            else
            {
                target = Board.opponentAlive[(rngAlive + 1) % (Board.opponentAlive.Count)];
            }

        }
        if (target == board.num)
        {
            board.targetBy.Add(num);
            GetComponent<OpponentBoard>().TargetByImg.enabled = true;
            if (board.targetBy.Count >= 2)
            {
                board.PourcentageDefenseBonus += Board.bonusDefenseFocus;
                board.PourcentageAttackBonus += Board.bonusAttackFocus;
            }
        }
        else
        {
            AI targetAI = board.allOpponentBoard[target].GetComponent<AI>();
            targetAI.targetBy.Add(num);
            if (targetAI.targetBy.Count >= 2)
            {
                targetAI.PourcentageDefenseBonus += Board.bonusDefenseFocus;
                targetAI.PourcentageAttackBonus += Board.bonusAttackFocus;
            }
            //print(target + "by->" + board.allOpponentBoard[target].GetComponent<AI>().targetBy.Count + "  " + num);
        }
    }

    IEnumerator ChangeTargetOverTime()
    {
        while (IsAlive)
        {
            yield return new WaitForSeconds(Random.Range(15, 60));
            if (IsAlive)
            {
                ChangeTarget();
            }
        }
    }

    IEnumerator Generator(int indexTileBuilding)
    {
        while (IsAlive && isBuildingOnTiles[indexTileBuilding])
        {
            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            score += Board.generatorIncome;

            for (int i = 0; i < 25; i++)
            {;
                yield return new WaitForFixedUpdate();
            }

            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator MissileLauncher(int indexTileBuilding)
    {
        while (IsAlive && isBuildingOnTiles[indexTileBuilding])
        {
            //cible un adeversaire aléatoire
            //int target = Board.opponentAlive[(Board.opponentAlive.IndexOf(num) + Random.Range(1, Board.opponentAlive.Count-1)) % Board.opponentAlive.Count];

            //choisi une case aléatoire
            /*List<Tile> tilesTarget = new List<Tile>();
            for (int y = 0; y < Board.sizeBoardY; y++)
            {
                for (int x = 0; x < Board.sizeBoardX; x++)
                {
                    if(!board.tiles[y, x].IsDestroy)
                    {
                        tilesTarget.Add(board.tiles[y, x]);
                    }
                }
            }
            Transform tileTarget = tilesTarget[Random.Range(0, tilesTarget.Count - 1)].transform;
            Instantiate(attackPrefab, tileTarget.position + new Vector3(0,0,-1), Quaternion.identity, tileTarget);
            tileTarget.GetComponent<Tile>().NbAttack++;*/
            yield return new WaitForSeconds(Board.cdMissileLauncher);
            if (IsAlive)
            {
                StartCoroutine("LaunchMissileLauncher");
            }
        }
    }

    IEnumerator LaunchMissileLauncher()
    {
        if(target == board.num) //contre joueur
        {
            //choisi une case aléatoire
            List<Tile> tilesTarget = new List<Tile>();
            for (int y = 0; y < Board.sizeBoardY; y++)
            {
                for (int x = 0; x < Board.sizeBoardX; x++)
                {
                    if (!board.tiles[y, x].IsDestroy)
                    {
                        tilesTarget.Add(board.tiles[y, x]);
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                Transform tileTarget = tilesTarget[Random.Range(0, tilesTarget.Count)].transform;
                if(board.CurrentNbShield == 0)
                {
                    GameObject atk = Instantiate(attackPrefab, tileTarget.position + new Vector3(0, 0, -1), Quaternion.identity, tileTarget);
                    tileTarget.GetComponent<Tile>().NbAttack++;
                    atk.GetComponent<Attack>().from = num;
                }
                else
                {
                    board.CurrentNbShield--;
                }
                yield return new WaitForSeconds(0.25f);
            }

        }
        else //contre ia
        {
            AI targetAI = board.allOpponentBoard[target].GetComponent<AI>();
            List<int> tilesTarget = new List<int>();
            for (int i = 0; i < targetAI.tilesPv.Length; i++)
            {
                if (targetAI.tilesPv[i] > 0)
                {
                    tilesTarget.Add(i);
                }
            }
            for (int i = 0; i < 3; i++)
            {
                if(targetAI.CurrentNbShield == 0)
                {
                    targetAI.incomingAttack.Enqueue(new AttackTile(tilesTarget[Random.Range(0, tilesTarget.Count)], Time.time + (Board.baseARTime * (100f / (100f + PourcentageAttackBonus)) * ((100f + targetAI.PourcentageDefenseBonus) / 100f)), num));
                }
                else
                {
                    targetAI.CurrentNbShield--;
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    IEnumerator Laser(int indexTileBuilding)
    {
        while (IsAlive && isBuildingOnTiles[indexTileBuilding])
        {
            yield return new WaitForSeconds(Board.cdLaser);
            if (IsAlive)
            {
                StartCoroutine("LaunchLaser");
            }
        }
    }

    IEnumerator LaunchLaser()
    {
        if (target == board.num) //contre joueur
        {
            //choisi une case aléatoire
            List<Tile> tilesTarget = new List<Tile>();
            for (int y = 0; y < Board.sizeBoardY; y++)
            {
                for (int x = 0; x < Board.sizeBoardX; x++)
                {
                    if (!board.tiles[y, x].IsDestroy)
                    {
                        tilesTarget.Add(board.tiles[y, x]);
                    }
                }
            }

            Tile baseTile = tilesTarget[Random.Range(0, tilesTarget.Count)];
            int baseX = 0;
            int baseY = 0;
            for (int y = 0; y < Board.sizeBoardY; y++)
            {
                for (int x = 0; x < Board.sizeBoardX; x++)
                {
                    if (board.tiles[y, x] == baseTile)
                    {
                        baseX = x;
                        baseY = y;
                    }
                }
            }
            List<Tile> aliveHorizontal = new List<Tile>();
            List<Tile> aliveVertical = new List<Tile>();

            for (int y = 0; y < Board.sizeBoardY; y++)
            {
                for (int x = 0; x < Board.sizeBoardX; x++)
                {
                    if (y == baseY)
                    {
                        if (!board.tiles[y, x].IsDestroy)
                        {
                            aliveHorizontal.Add(board.tiles[y, x]);
                        }
                    }
                    if(x == baseX)
                    {
                        if (!board.tiles[y, x].IsDestroy)
                        {
                            aliveVertical.Add(board.tiles[y, x]);
                        }
                    }
                }
            }
            List<Tile> tilesLaser = new List<Tile>();
            if (aliveHorizontal.Count > aliveVertical.Count)
            {
                foreach(Tile t in aliveHorizontal)
                {
                    tilesLaser.Add(t);
                }

            }
            else if (aliveHorizontal.Count < aliveVertical.Count)
            {
                foreach (Tile t in aliveHorizontal)
                {
                    tilesLaser.Add(t);
                }
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    foreach (Tile t in aliveHorizontal)
                    {
                        tilesLaser.Add(t);
                    }
                }
                else
                {
                    foreach (Tile t in aliveHorizontal)
                    {
                        tilesLaser.Add(t);
                    }
                }
            }

            for (int j = 0; j < tilesLaser.Count; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    Transform tileTarget = tilesLaser[j].transform;
                    if (board.CurrentNbShield == 0)
                    {
                        GameObject atk = Instantiate(attackPrefab, tileTarget.position + new Vector3(0, 0, -1), Quaternion.identity, tileTarget);
                        tileTarget.GetComponent<Tile>().NbAttack++;
                        atk.GetComponent<Attack>().from = num;
                    }
                    else
                    {
                        board.CurrentNbShield--;
                    }
                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        else //contre ia
        {
            AI targetAI = board.allOpponentBoard[target].GetComponent<AI>();
            List<int> tilesTarget = new List<int>();
            for (int i = 0; i < targetAI.tilesPv.Length; i++)
            {
                if (targetAI.tilesPv[i] > 0)
                {
                    tilesTarget.Add(i);
                }
            }
            int baseTile = tilesTarget[Random.Range(0, tilesTarget.Count)];
            List<int> aliveHorizontal = new List<int>();
            List<int> aliveVertical = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (targetAI.tilesPv[baseTile - (baseTile % 4) + i] > 0)
                {
                    aliveHorizontal.Add(baseTile - (baseTile % 4) + i);
                }
                if (targetAI.tilesPv[(baseTile % 4) + 4 * i] > 0)
                {
                    aliveVertical.Add((baseTile % 4) + 4 * i);
                }
            }
            List<int> tilesLaser;
            if (aliveHorizontal.Count > aliveVertical.Count)
            {
                tilesLaser = new List<int>(aliveHorizontal);
            }
            else if (aliveHorizontal.Count < aliveVertical.Count)
            {
                tilesLaser = new List<int>(aliveVertical);
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    tilesLaser = new List<int>(aliveHorizontal);
                }
                else
                {
                    tilesLaser = new List<int>(aliveVertical);
                }
            }

            for (int j = 0; j < tilesLaser.Count; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if(targetAI.CurrentNbShield == 0)
                    {
                        targetAI.incomingAttack.Enqueue(new AttackTile(tilesLaser[j], Time.time + (Board.baseARTime * (100f / (100f + PourcentageAttackBonus)) * ((100f + targetAI.PourcentageDefenseBonus) / 100f)), num));
                    }
                    else
                    {
                        targetAI.CurrentNbShield--;
                    }
                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator Nova(int indexTileBuilding)
    {
        while (IsAlive && isBuildingOnTiles[indexTileBuilding])
        {
            yield return new WaitForSeconds(Board.cdNova);
            while (IsAlive && isBuildingOnTiles[indexTileBuilding] && incomingAttack.Count < 10)
            {
                yield return new WaitForFixedUpdate();
            }
            if(IsAlive && isBuildingOnTiles[indexTileBuilding] && incomingAttack.Count >= 10)
            {
                incomingAttack.Clear();
            }
        }
    }

    IEnumerator Ultime(int indexTileBuilding)
    {
        while (IsAlive && isBuildingOnTiles[indexTileBuilding])
        {
            yield return new WaitForSeconds(Board.cdUltime);
            if (IsAlive)
            {
                StartCoroutine("LaunchUltime");
            }
        }
    }

    IEnumerator LaunchUltime()
    {
        if (target == board.num) //contre joueur
        {
            //choisi une case aléatoire
            List<Tile> tilesTarget = new List<Tile>();
            for (int y = 0; y < Board.sizeBoardY; y++)
            {
                for (int x = 0; x < Board.sizeBoardX; x++)
                {
                    if (!board.tiles[y, x].IsDestroy)
                    {
                        tilesTarget.Add(board.tiles[y, x]);
                    }
                }
            }

            Tile mostValuableTile = tilesTarget[0];
            int priceMostValuableTile = -1;
            foreach (Tile i in tilesTarget)
            {
                int price = 0;
                if (i.Building != null)
                {
                    price = Board.buildingPrice[i.Building.numBuilding];
                }
                if (price > priceMostValuableTile)
                {
                    mostValuableTile = i;
                    priceMostValuableTile = price;
                }
            }

            for (int i = 0; i < 30; i++)
            {
                Transform tileTarget = mostValuableTile.transform;
                if(board.CurrentNbShield == 0)
                {
                    GameObject atk = Instantiate(attackPrefab, tileTarget.position + new Vector3(0, 0, -1), Quaternion.identity, tileTarget);
                    tileTarget.GetComponent<Tile>().NbAttack++;
                    atk.GetComponent<Attack>().from = num;
                }
                else
                {
                    board.CurrentNbShield--;
                }
                yield return new WaitForSeconds(0.05f);
            }

        }
        else //contre ia
        {
            AI targetAI = board.allOpponentBoard[target].GetComponent<AI>();
            List<int> tilesTarget = new List<int>();
            for (int i = 0; i < targetAI.tilesPv.Length; i++)
            {
                if (targetAI.tilesPv[i] > 0)
                {
                    tilesTarget.Add(i);
                }
            }

            int mostValuableTile = 0;
            int priceMostValuableTile = -1;
            foreach (int i in tilesTarget)
            {
                int price = 0;
                if (targetAI.isBuildingOnTiles[i])
                {
                    price = Board.buildingPrice[targetAI.BuildingOnTiles[i].GetComponent<Building>().numBuilding];
                }
                if (price > priceMostValuableTile)
                {
                    mostValuableTile = i;
                    priceMostValuableTile = price;
                }
            }

            for (int i = 0; i < 30; i++)
            {
                if (targetAI.CurrentNbShield == 0)
                {
                    targetAI.incomingAttack.Enqueue(new AttackTile(mostValuableTile, Time.time + (Board.baseARTime * (100f / (100f + PourcentageAttackBonus)) * ((100f + targetAI.PourcentageDefenseBonus) / 100f)), num));
                }
                else
                {
                    targetAI.CurrentNbShield--;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
