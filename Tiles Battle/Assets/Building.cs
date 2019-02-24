using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour {

    private Board board;
    public int numBuilding;
    public bool cdReady;
    private Image cd;
    private GameObject textProfitInstance;

    // Use this for initialization
    void Start () {
        board = GameObject.Find("Board").GetComponent<Board>();

        if (numBuilding == 0)
        {
            StartCoroutine("AnimGenerator");
        }
        else if (numBuilding == 1)
        {
            cd = transform.Find("Cd").GetComponent<Image>();
            board.PourcentageDefenseBonus += Board.bonusDefenseMissileLauncher;
            StartMissileMauncher();
        }
        else if (numBuilding == 2)
        {
            board.core = true;
            board.NbShield += Board.nbShieldCore;
        }
        else if (numBuilding == 3)
        {
            cd = transform.Find("Cd").GetComponent<Image>();
            StartLaser();
        }
        else if (numBuilding == 4)
        {
            cd = transform.Find("Cd").GetComponent<Image>();
            StartNova();
        }
        else if (numBuilding == 5)
        {
            cd = transform.Find("Cd").GetComponent<Image>();
            StartUltime();
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void StartMissileMauncher()
    {
        StartCoroutine("AnimMissileLauncher");
    }

    public void LaunchMissileLauncher()
    {
        //si target est une ia
        StartCoroutine("LaunchMissileLauncherToAI");
        StartMissileMauncher();
    }

    public void StartLaser()
    {
        StartCoroutine("AnimLaser");
    }

    public void LaunchLaser()
    {
        //si target est une ia
        StartCoroutine("LaunchLaserToAI");
        StartLaser();
    }

    public void StartNova()
    {
        StartCoroutine("AnimNova");
    }

    public void LaunchNova()
    {
        foreach(GameObject attack in GameObject.FindGameObjectsWithTag("Attack"))
        {
            attack.GetComponent<Attack>().ParryAttack();
        }
        //effet nova cercle qui s'aggrandit
        StartNova();
    }

    public void StartUltime()
    {
        StartCoroutine("AnimUltime");
    }

    public void LaunchUltime()
    {
        //si target est une ia
        StartCoroutine("LaunchUltimeToAI");
        StartLaser();
    }

    IEnumerator AnimGenerator()
    {
        //TODO : séparer l'affichage des capacités
        while (true)
        {
            for (int i = 0; i < 25; i++)
            {
                transform.localScale = new Vector2(1f + (float)i / 240, 1f + (float)i / 240);
                transform.Rotate(new Vector3(0, 0, -1));
                yield return new WaitForFixedUpdate();
            }

            board.Score += Board.generatorIncome;
            textProfitInstance = (GameObject)Instantiate(board.textProfitPrefab, transform.position, Quaternion.identity, Board.canvas);
            TMP_Text textProfit = textProfitInstance.GetComponent<TMP_Text>();
            textProfit.fontSize /= 2;
            textProfit.text = "+" + Board.generatorIncome;

            for (int i = 0; i < 25; i++)
            {
                transform.localScale = new Vector2(1.1f - (float)i / 240, 1.1f - (float)i / 240);
                transform.Rotate(new Vector3(0, 0, -1));
                textProfitInstance.transform.position += new Vector3(0,0.02f,0);
                textProfit.color = new Color(textProfit.color.r, textProfit.color.g, textProfit.color.b, 1f - (float)i / 24);
                yield return new WaitForFixedUpdate();
            }
            Destroy(textProfitInstance);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator AnimMissileLauncher()
    {
        cd.fillAmount = 0;
        float nbStep = Board.cdMissileLauncher / Time.fixedDeltaTime;
        for (int i = 0; i < nbStep; i++)
        {
            cd.fillAmount = (float) i/ (nbStep - 1);
            yield return new WaitForFixedUpdate();
        }
        cdReady = true;
    }

    private void OnDestroy()
    {
        if (board != null) //<=> c'était le building d'un joueur
        {
            if (numBuilding == 1)
            {
                board.PourcentageDefenseBonus -= Board.bonusDefenseMissileLauncher;
            }
            else if (numBuilding == 2)
            {
                board.NbShield -= Board.nbShieldCore;
            }
        }

        if(textProfitInstance != null)
        {
            Destroy(textProfitInstance);
        }
    }

    IEnumerator LaunchMissileLauncherToAI()
    {
        AI target = board.Target.GetComponent<AI>();
        List<int> tilesTarget = new List<int>();
        for (int i = 0; i < target.tilesPv.Length; i++)
        {
            if (target.tilesPv[i] > 0)
            {
                tilesTarget.Add(i);
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if(target.CurrentNbShield == 0)
            {
                target.incomingAttack.Enqueue(new AttackTile(tilesTarget[Random.Range(0, tilesTarget.Count)], Time.time + (Board.baseARTime * (100f / (100f + board.PourcentageAttackBonus)) * ((100f + target.PourcentageDefenseBonus) / 100f)), board.num));
            }
            else
            {
                target.CurrentNbShield--;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator AnimLaser()
    {
        cd.fillAmount = 0;
        float nbStep = Board.cdLaser / Time.fixedDeltaTime;
        for (int i = 0; i < nbStep; i++)
        {
            cd.fillAmount = (float)i / (nbStep - 1);
            yield return new WaitForFixedUpdate();
        }
        cdReady = true;
    }

    IEnumerator LaunchLaserToAI()
    {
        AI target = board.Target.GetComponent<AI>();
        List<int> tilesTarget = new List<int>();
        for (int i = 0; i < target.tilesPv.Length; i++)
        {
            if (target.tilesPv[i] > 0)
            {
                tilesTarget.Add(i);
            }
        }
        int baseTile = tilesTarget[Random.Range(0, tilesTarget.Count)];
        List<int> aliveHorizontal = new List<int>();
        List<int> aliveVertical = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            if (target.tilesPv[baseTile - (baseTile % 4) + i] > 0)
            {
                aliveHorizontal.Add(baseTile - (baseTile % 4) + i);
            }
            if (target.tilesPv[(baseTile % 4) + 4*i] > 0)
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
                if (target.CurrentNbShield == 0)
                {
                    target.incomingAttack.Enqueue(new AttackTile(tilesLaser[j], Time.time + (Board.baseARTime * (100f / (100f + board.PourcentageAttackBonus)) * ((100f + target.PourcentageDefenseBonus) / 100f)), board.num));
                }
                else
                {
                    target.CurrentNbShield--;
                }
                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(0.1f);
        }

    }

    IEnumerator AnimNova()
    {
        cd.fillAmount = 0;
        float nbStep = Board.cdNova / Time.fixedDeltaTime;
        for (int i = 0; i < nbStep; i++)
        {
            cd.fillAmount = (float)i / (nbStep - 1);
            yield return new WaitForFixedUpdate();
        }
        cdReady = true;
    }

    IEnumerator AnimUltime()
    {
        cd.fillAmount = 0;
        float nbStep = Board.cdUltime / Time.fixedDeltaTime;
        for (int i = 0; i < nbStep; i++)
        {
            cd.fillAmount = (float)i / (nbStep - 1);
            yield return new WaitForFixedUpdate();
        }
        cdReady = true;
    }

    IEnumerator LaunchUltimeToAI()
    {
        AI target = board.Target.GetComponent<AI>();
        List<int> tilesTarget = new List<int>();
        for (int i = 0; i < target.tilesPv.Length; i++)
        {
            if (target.tilesPv[i] > 0)
            {
                tilesTarget.Add(i);
            }
        }

        int mostValuableTile = 0;
        int priceMostValuableTile = -1;
        foreach(int i in tilesTarget)
        {
            int price = 0;
            if (target.isBuildingOnTiles[i])
            {
                price = Board.buildingPrice[target.BuildingOnTiles[i].GetComponent<Building>().numBuilding];
            }
            if(price > priceMostValuableTile)
            {
                mostValuableTile = i;
                priceMostValuableTile = price;
            }
        }

        for (int i = 0; i < 30; i++)
        {
            if (target.CurrentNbShield == 0)
            {
                target.incomingAttack.Enqueue(new AttackTile(mostValuableTile, Time.time + (Board.baseARTime * (100f / (100f + board.PourcentageAttackBonus)) * ((100f + target.PourcentageDefenseBonus) / 100f)), board.num));
            }
            else
            {
                target.CurrentNbShield--;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

}
