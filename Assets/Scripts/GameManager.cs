using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Ships")]
    public GameObject[] ships;
    public EnemyScript enemyScript;
    private ShipScript shipScript;
    private List<int[]> enemyShips;
    private int shipIdx = 0;
    public List<TileScript> allTileScripts;
    

    [Header("HUD")]
    public Button nextBtn;
    public Button rotateBtn;
    public Button replayBtn;
    public TMP_Text topTXT;
    public TMP_Text playerShipTXT;
    public TMP_Text enemyShipTXT;


    [Header("Objects")]
    public GameObject misslePrefab;
    public GameObject enemyMisslePrefab;
    public GameObject firePrefab;
    public GameObject woodDeck;

    private bool setupComplete = false;
    private bool playerTurn = false;
    
    private List<GameObject> playerFires = new List<GameObject>();
    private List<GameObject> enemyFires = new List<GameObject>();
    
    private int enemyShipCount = 5;
    private int playerShipCount = 5;

    void Start()
    {
        shipScript = ships[shipIdx].GetComponent<ShipScript>();
        enemyShips = enemyScript.PlaceEnemyShips();
        nextBtn.onClick.AddListener(() => NextShipClicked());
        rotateBtn.onClick.AddListener(() => RotateShipClicked());
        replayBtn.onClick.AddListener(() => ReplayClicked());
        
    }

  

    private void NextShipClicked()
    {
        if(!shipScript.OnGameBoard())
        {
            shipScript.FlashColor(Color.red);
        }else
        {
            if(shipIdx <= ships.Length - 2)
            {
                shipIdx++;
                shipScript = ships[shipIdx].GetComponent<ShipScript>();
                shipScript.FlashColor(Color.yellow);
            }
            else
            {
                rotateBtn.gameObject.SetActive(false);
                nextBtn.gameObject.SetActive(false);
                woodDeck.gameObject.SetActive(false);
                topTXT.text = "Guess an enemy tile!";
                setupComplete = true;
                playerTurn = true;
                for (int i = 0; i < ships.Length; i++)
                {
                    ships[i].SetActive(false);
                }
            }
            
        }

    }

    public void TileClicked(GameObject tile)
    {
        if(setupComplete && playerTurn)
        {
            Vector3 tilePos = tile.transform.position;
            tilePos.y += 15;
            playerTurn = false;
            Instantiate(misslePrefab, tilePos, misslePrefab.transform.rotation);
        }
        else if (!setupComplete)
        {
            PlaceShip(tile);
            shipScript.SetClickedTile(tile);
        }
    }

    private void PlaceShip(GameObject tile)
    {
        shipScript = ships[shipIdx].GetComponent<ShipScript>();
        shipScript.ClearTileList();
        Vector3 newVec = shipScript.GetOffsetVec(tile.transform.position);
        ships[shipIdx].transform.localPosition = newVec;
    }

    private void RotateShipClicked()
    {
        shipScript.RotateShip();
    }

    public void CheckHit(GameObject tile)
    {
        int tileNumber = Int32.Parse(Regex.Match(tile.name, @"\d+").Value);
        int hitCount = 0;

        foreach (int[] tileNumArray in enemyShips)
        {
            if(tileNumArray.Contains(tileNumber))
            {
                for (int i = 0; i < tileNumArray.Length; i++)
                {
                    if (tileNumArray[i] == tileNumber)
                    {
                        tileNumArray[i] = -5;
                        hitCount++;
                    }
                    else if (tileNumArray[i] == -5)
                    {
                        hitCount++;
                    }
                }

                if (hitCount == tileNumArray.Length)
                {
                    enemyShipCount--;
                    enemyShipTXT.text = enemyShipCount.ToString();
                    topTXT.text = "SUNK!!!!";
                    enemyFires.Add(Instantiate(firePrefab, tile.transform.position, Quaternion.identity));
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(68, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                else
                {
                    topTXT.text = "HIT!";
                    tile.GetComponent<TileScript>().SetTileColor(1, new Color32(255, 0, 0, 255));
                    tile.GetComponent<TileScript>().SwitchColors(1);
                }
                break;
            }   
        }

        if(hitCount == 0)
        {
            tile.GetComponent<TileScript>().SetTileColor(1, new Color32(38, 57, 76, 255));
            tile.GetComponent<TileScript>().SwitchColors(1);
            topTXT.text = "Missed!";
        }
       Invoke("EndPlayerTurn", 1.0f);
    }

    public void EnemyHitPlayer(Vector3 tile, int tileNum, GameObject hitObject)
    {
        enemyScript.MissileHit(tileNum);
        tile.y += 0.2f;
        playerFires.Add(Instantiate(firePrefab, tile, Quaternion.identity));

        if (hitObject.GetComponent<ShipScript>().HitCheckSank())
        {
            playerShipCount--;
            playerShipTXT.text = playerShipCount.ToString();
            enemyScript.SunkPlayer();
        }
        Invoke("EndEnemyTurn", 2.0f);
    }

    private void EndPlayerTurn()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].SetActive(true);
        }
        foreach (GameObject fire in playerFires)
        {
            fire.SetActive(true);
        }
        foreach (GameObject fire in enemyFires)
        {
            fire.SetActive(false);
        }
        enemyShipTXT.text = enemyShipCount.ToString();
        topTXT.text = "Enemies turn";
        enemyScript.NPCTurn();
        ColorAllTiles(0);
        if (playerShipCount < 1) GameOver("YOU LOSE!");
    }

    public void EndEnemyTurn()
    {
        for (int i = 0; i < ships.Length; i++)
        {
            ships[i].SetActive(false);
        }
        foreach (GameObject fire in playerFires)
        {
            fire.SetActive(false);
        }
        foreach (GameObject fire in enemyFires)
        {
            fire.SetActive(true);
        }
        playerShipTXT.text = playerShipCount.ToString();
        topTXT.text = "Select a tile";
        playerTurn = true;
        ColorAllTiles(1);
        if (enemyShipCount < 1) GameOver("YOU WON!");
    }

    private void ColorAllTiles(int i)
    {
        foreach(TileScript tileScript in allTileScripts)
        {
            tileScript.SwitchColors(i);

            
        }
    }


    void GameOver(string winner)
    {

        topTXT.text = "Game over! " + winner;
        replayBtn.gameObject.SetActive(true);

    }

    void ReplayClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}

