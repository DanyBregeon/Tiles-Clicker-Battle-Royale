using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour {

    //private AR ar;
    public Board board;
    private AudioSource audioSource;
    private bool isDestroy;
    private Tile onTile;
    public int from;

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
                transform.localScale = Vector3.zero;
            }
        }
    }

    // Use this for initialization
    void Start () {
        //ar = GetComponentInChildren<AR>();
        board = GameObject.Find("Board").GetComponent<Board>();
        audioSource = GetComponent<AudioSource>();
        onTile = GetComponentInParent<Tile>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(Tile.button1) || Input.GetKeyDown(Tile.button2) || Input.GetKeyDown(Tile.button3))
        {
            ParryAttack();
        }
    }

    public void ParryAttack()
    {
        if (!isDestroy)
        {
            board.ComboShield++;
            if (board.ComboShield == Board.maxComboShield)
            {
                board.CurrentNbShield = board.NbShield;
                board.ComboShield = 0;
            }
            IsDestroy = true;
            int rngSound = Random.Range(0, board.soundParry.Length);
            audioSource.clip = board.soundParry[rngSound];
            audioSource.Play();
            onTile.NbAttack--;
            Destroy(gameObject, 2);
        }
    }

    public void Boom()
    {
        if (!isDestroy)
        {
            //deal dgt
            audioSource.clip = board.soundHit;
            audioSource.Play();
            transform.localScale = Vector3.zero;
            Board.lastHitBy = from;
            onTile.NbAttack--;
            onTile.Pv--;
            Destroy(gameObject, 2);
        }

    }
}
