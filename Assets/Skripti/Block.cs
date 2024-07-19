using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int vrednost;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private TextMeshPro text;
    public Vector2 pozicija => transform.position;
    

    public void init(blocktype type)
    {
        vrednost=type.vrednost;
        renderer.color = type.color;
        text.text = type.vrednost.ToString();
    }
    public void setblock(Node node)
    {
        if (Node != null) Node.occupiedBlock = null;
        Node = node;
        Node.occupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith)
    {
        MergingBlock = blockToMergeWith; // Blokot so koj treba da se spoi

        Node.occupiedBlock = null; //Stavi go na sloboden za da mozat drugi blokovi da go koristat
        blockToMergeWith.Merging = true; //Stavi go vo process na spojuvanje za da ne moze nekoj 3ti blok da se spoi so prviot

    }

    public bool mozeDaSeSpoi(int value) => value == vrednost && !Merging && MergingBlock == null; //Moze i so samo !Merging ili samo MergingBlock uslovot
}
