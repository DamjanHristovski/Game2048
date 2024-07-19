using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class Game : MonoBehaviour
{
    [SerializeField] private int dolzina = 4;
    [SerializeField] private int sirina = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPreFab;
    [SerializeField] private SpriteRenderer boardPreFab;
    [SerializeField] private List<blocktype>Types;
    [SerializeField] private float vremeNaDvizenje=0.2f;
    [SerializeField] private float winCondition = 2048;
    [SerializeField] private GameObject winScreen, loseScreen;
   

    private List<Node> nodes;
    private List<Block>blocks;
    private GameState state;
    private int round;
    private blocktype getBlockTypeByValue(int value) => Types.First(t => t.vrednost == value); //Getter


    void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState (GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerirajTabla();
                break;
            case GameState.SpawnBlocks:
                postaviBlokovi(round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingForIn:
                break;
            case GameState.Moving:
                break;
            case GameState.Lose:
                loseScreen.SetActive(true);
                break;
            case GameState.Win:
                winScreen.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState),newState,null);
        }

    }
    private void Update()
    {
        if (state != GameState.WaitingForIn) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left); //Levo
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right); //Desno
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up); //Gore
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down); //Dole

    }

    void GenerirajTabla()
    {
        round = 0;
        nodes = new List<Node>();
        blocks=new List<Block>();
        for (int i = 0; i < sirina; i++)
        {
            for (int j = 0; j < dolzina; j++)
            {
                var node = Instantiate(nodePrefab, new Vector2(i, j), Quaternion.identity);
                nodes.Add(node);
            }
        }
        var center = new Vector2((float)sirina / 2 - 0.5f, (float)dolzina / 2 - 0.5f);
        var board = Instantiate(boardPreFab,center, Quaternion.identity);
        board.size = new Vector2(sirina, dolzina);
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        ChangeState(GameState.SpawnBlocks);
    
    }
    void postaviBlokovi(int broj)
    {
        var availableNodes = nodes.Where(node => node.occupiedBlock == null).OrderBy(block => UnityEngine.Random.value).ToList();
       
        foreach (var node in availableNodes.Take(broj))
        {
            postaviBlock(node, UnityEngine.Random.value > 0.8f ? 4 : 2);
        } 

    
        if (availableNodes.Count == 1)
        {
            ChangeState(GameState.Lose);
            return; //izgubil
        }

        ChangeState(blocks.Any(block=>block.vrednost == winCondition) ? GameState.Win : GameState.WaitingForIn);
    }

    void postaviBlock (Node node, int vrednost)
    {
        var block = Instantiate(blockPreFab, node.pozicija, Quaternion.identity);
        block.init(getBlockTypeByValue(vrednost));
        block.setblock(node);
        blocks.Add(block);
    }
    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);
        var orderedBlocks = blocks.OrderBy(block => block.pozicija.x).ThenBy(block => block.pozicija.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.setblock(next);
                var possibleNode = getNodeAtPos(next.pozicija + dir);
                if (possibleNode != null)
                {
                    // Node e available
                    // Proveri dali treba da se spoi
                    if (possibleNode.occupiedBlock != null && possibleNode.occupiedBlock.mozeDaSeSpoi(block.vrednost))
                    {
                        block.MergeBlock(possibleNode.occupiedBlock);

                    }// Proveri dali moze da se pomesti 
                    else if (possibleNode.occupiedBlock == null)
                    {
                        next = possibleNode;
                    }
                }
            
            } while (next != block.Node); // se dodeka se razlicni

        }

        var sequence = DOTween.Sequence();

        foreach (var block in orderedBlocks)
        {
            var movePoint = block.MergingBlock!=null ? block.MergingBlock.Node.pozicija : block.Node.pozicija;


            sequence.Insert(0, block.transform.DOMove(movePoint, vremeNaDvizenje));
            
        }
        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b=>b.MergingBlock!=null))
            {
                mergeBlocks(block.MergingBlock,block);
            }
            ChangeState(GameState.SpawnBlocks);
        });

    }

    void mergeBlocks(Block baseBlock,Block merginBlock)
    {
        postaviBlock(baseBlock.Node, baseBlock.vrednost * 2);
        RemoveBlock(baseBlock);
        RemoveBlock(merginBlock);
    }

    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }
    Node getNodeAtPos(Vector2 pos)
    {
        return nodes.FirstOrDefault(node=>node.pozicija == pos);
    }

}
[Serializable]
public struct blocktype
{
    public int vrednost;
    public Color color;
}

public enum GameState
{
    GenerateLevel,
    SpawnBlocks,
    WaitingForIn,
    Moving,
    Lose,
    Win
}



