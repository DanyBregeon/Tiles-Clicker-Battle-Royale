using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonBuying : MonoBehaviour {

    private Board board;
    private TMP_Text priceText;
    private KeyCode shortcut;
    public int price;
    public GameObject prefabImg;
    public GameObject prefabBuild;

    // Use this for initialization
    void Start () {
        board = GameObject.Find("Board").GetComponent<Board>();
        priceText = GetComponentInChildren<TMP_Text>();
        shortcut = (KeyCode)System.Enum.Parse(typeof(KeyCode), transform.Find("ShortCut").GetComponent<TMP_Text>().text);
    }
	
	// Update is called once per frame
	void Update () {
		if(price > board.Score)
        {
            priceText.color = new Color(0.8f, 0, 0);
        }
        else
        {
            priceText.color = new Color(0.5f, 1, 0);
        }
        if (Input.GetKeyDown(shortcut))
        {
            OnClic();
        }
	}

    public void OnClic()
    {
        if(board.isBuilding && board.building.GetComponent<Building>().numBuilding == prefabBuild.GetComponent<Building>().numBuilding)
        {
            board.focusBoard.color = new Color(0, 0, 0, 0);
            board.isBuilding = false;
            Destroy(board.buildingImg);
        }
        else if (board.Score >= price)
        {
            if (board.isBuilding)
            {
                board.focusBoard.color = new Color(0, 0, 0, 0);
                board.isBuilding = false;
                Destroy(board.buildingImg);
            }
            //instantiate copy pos cursor puis selection d'une case
            GameObject instance = (GameObject)Instantiate(prefabImg, transform.position + Vector3.one * 10000, Quaternion.identity, Board.canvas);
            board.isBuilding = true;
            board.buildingImg = instance;
            board.building = prefabBuild;
            board.focusBoard.color = new Color(0, 0, 0, 0.25f);
        }
    }
}
