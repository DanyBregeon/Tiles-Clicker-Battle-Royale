using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AR : MonoBehaviour {

    private float time;
    private SpriteRenderer spriteRenderer;
    public Attack attack;

	// Use this for initialization
	void Start () {
        print("Attack from : " + attack.from);
        time = Board.baseARTime * (100f / (100f + attack.board.allOpponentBoard[attack.from].GetComponent<AI>().PourcentageAttackBonus)) * ((100f + attack.board.PourcentageDefenseBonus) / 100f);
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine("StartApprochRate");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator StartApprochRate()
    {
        float nbStep = time / Time.fixedDeltaTime;
        for (int i = 0; i < nbStep; i++)
        {
            spriteRenderer.size -= 2 * new Vector2(1f / nbStep, 1f / nbStep);
            //transform.localScale -= new Vector3(1f / nbStep, 1f / nbStep, 0);
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        attack.Boom();
    }
}
