using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    GameManager gameManager;
    Ray ray;
    RaycastHit hit;

    public bool missleHit = false;
    Color32[] colorHit = new Color32[2];

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        colorHit[0] = gameObject.GetComponent<MeshRenderer>().material.color;
        colorHit[1] = gameObject.GetComponent<MeshRenderer>().material.color;
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit))
        {
            if(Input.GetMouseButton(0) && hit.collider.gameObject.name == gameObject.name)
            {
                if (!missleHit)
                {
                    gameManager.TileClicked(hit.collider.gameObject);
                }
            }

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Missle"))
        {
            missleHit = true;
        }
        else if (collision.gameObject.CompareTag("EnemyMissle"))
        {
            colorHit[0] = new Color32(38, 57, 76, 255);
            GetComponent<Renderer>().material.color = colorHit[0];
        }
    }


    public void SetTileColor(int idx, Color32 color)
    {
        colorHit[idx] = color;
    }

    public void SwitchColors(int colorIdx)
    {
        GetComponent<Renderer>().material.color = colorHit[colorIdx];
    }

}
