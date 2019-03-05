using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentBoard : MonoBehaviour {

    private SpriteRenderer targetImg;
    private SpriteRenderer targetByImg;
    private Board board;
    private AI ai;

    public SpriteRenderer TargetImg
    {
        get
        {
            return targetImg;
        }

        set
        {
            targetImg = value;
        }
    }

    public SpriteRenderer TargetByImg
    {
        get
        {
            return targetByImg;
        }

        set
        {
            targetByImg = value;
        }
    }

    // Use this for initialization
    void Awake () {
        TargetImg = transform.Find("target").GetComponent<SpriteRenderer>();
        TargetByImg = transform.Find("targetBy").GetComponent<SpriteRenderer>();
        board = GameObject.Find("Board").GetComponent<Board>();
        ai = GetComponent<AI>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseOver()
    {
        if ((Input.GetKeyDown(Tile.button1) || Input.GetKeyDown(Tile.button2) || Input.GetKeyDown(Tile.button3)) && !board.IsGameFinish && ai.IsAlive)
        {
            board.Target = gameObject;
        }
    }
}
