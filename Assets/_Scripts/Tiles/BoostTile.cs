using UnityEditor;
using UnityEngine;

public class BoostTile : MonoBehaviour
{
    public GameObject player;
    public float boostAmount;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == player) 
        {
            player.GetComponent<PlayerMovement>().Boost(boostAmount);
        }
    }
}
