using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Node : MonoBehaviour
{

    public Vector2 pozicija => transform.position;
    
    public Block occupiedBlock;
}
