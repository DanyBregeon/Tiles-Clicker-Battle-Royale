using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {

    private Board board;

	// Use this for initialization
	void Start () {
        board = GameObject.Find("Board").GetComponent<Board>();
    }

    private void OnMouseOver()
    {
        if ((Input.GetKeyDown(Tile.button1) || Input.GetKeyDown(Tile.button2) || Input.GetKeyDown(Tile.button3)) && board.isBuilding)
        {
            board.focusBoard.color = new Color(0, 0, 0, 0);
            board.isBuilding = false;
            Destroy(board.buildingImg);
        }
    }
}
