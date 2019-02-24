using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTile {

    public int tile;
    public float time;
    public int from;

    public AttackTile(int tile, float time, int from)
    {
        this.tile = tile;
        this.time = time;
        this.from = from;
    }
}
