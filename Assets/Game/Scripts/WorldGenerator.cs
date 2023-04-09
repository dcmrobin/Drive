using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WorldGenerator : MonoBehaviour
{
    public GameObject groundPrefab; // the prefab for the ground tiles
    public GameObject roadPrefab;
    public GameObject rockPrefab;
    public GameObject wheelPrefab;
    public GameObject simpleStructurePrefab;
    public int worldSize = 10; // the size of the world (in tiles) around the player
    public int tileSize = 1; // the size of each tile in world units
    private int probability = 0;
    GameObject tile;
    GameObject roadTile;

    private Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>(); // stores the generated ground tiles
    private Dictionary<Vector2, GameObject> roadTiles = new Dictionary<Vector2, GameObject>();
    private Vector2 playerPos; // the position of the player

    void Start()
    {
        // generate the initial world around the player
        playerPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));
        GenerateWorld();
    }

    void Update()
    {
        // check if the player has moved to a new tile, and generate new tiles if necessary
        Vector2 newPlayerPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.z));
        if (newPlayerPos != playerPos)
        {
            playerPos = newPlayerPos;
            GenerateWorld();
        }
    }

    void GenerateWorld()
    {
        // Calculate the tile position of the player
        Vector2Int playerTilePos = new Vector2Int(Mathf.RoundToInt(transform.position.x / tileSize), Mathf.RoundToInt(transform.position.z / tileSize));

        // Destroy tiles that are too far away from the player
        List<Vector2> tilesToRemove = new List<Vector2>();
        List<Vector2> roadTilesToRemove = new List<Vector2>();
        foreach (KeyValuePair<Vector2, GameObject> pair in tiles)
        {
            Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(pair.Key.x), Mathf.RoundToInt(pair.Key.y));
            if (Vector2Int.Distance(tilePos, playerTilePos) > worldSize + 10)
            {
                Destroy(pair.Value);
                tilesToRemove.Add(pair.Key);
            }
        }
        foreach (KeyValuePair<Vector2, GameObject> pair in roadTiles)
        {
            Vector2Int roadTilePos = new Vector2Int(Mathf.RoundToInt(pair.Key.x), Mathf.RoundToInt(pair.Key.y));
            if (Vector2Int.Distance(roadTilePos, playerTilePos) > worldSize + 10)
            {
                Destroy(pair.Value);
                roadTilesToRemove.Add(pair.Key);
            }
        }
        foreach (Vector2 key in tilesToRemove)
        {
            tiles.Remove(key);
        }
        foreach (Vector2 key in roadTilesToRemove)
        {
            roadTiles.Remove(key);
        }

        // Generate new tiles within the world size around the player
        for (int x = -worldSize; x <= worldSize; x++)
        {
            for (int y = -worldSize; y <= worldSize; y++)
            {
                Vector2Int tilePos = playerTilePos + new Vector2Int(x, y);
                Vector2Int roadTilePos = playerTilePos + new Vector2Int(x, y);
                if (!tiles.ContainsKey(tilePos) && !roadTiles.ContainsKey(roadTilePos))
                {
                    probability = Random.Range(0, 1000);
                    tile = Instantiate(groundPrefab, new Vector3(tilePos.x * tileSize, 0, tilePos.y * tileSize), Quaternion.identity);
                    if (tile.transform.position.x >= -1 || tile.transform.position.x <= 1)
                    {
                        Destroy(tile);
                        roadTile = Instantiate(roadPrefab, new Vector3(roadTilePos.x * tileSize, 0, roadTilePos.y * tileSize), Quaternion.identity);
                    }
                    if (roadTile.transform.position.x < -1 || roadTile.transform.position.x > 1)
                    {
                        Destroy(roadTile);
                        tile = Instantiate(groundPrefab, new Vector3(tilePos.x * tileSize, 0, tilePos.y * tileSize), Quaternion.identity);
                    }
                    //GameObject thing = Instantiate(thingPrefab, new Vector3(tile.transform.position.x + Random.Range(-tileSize/2, tileSize/2), tile.transform.position.y + Random.Range(1, 10), tile.transform.position.z + Random.Range(-tileSize/2, tileSize/2)), Quaternion.identity);
                    if (probability == Random.Range(0, 1000))
                    {
                        if (probability > 730)
                        {
                            GameObject simpleStructure = PhotonNetwork.Instantiate("SimpleStructure", new Vector3(tile.transform.position.x + Random.Range(-tileSize/2, tileSize/2), 0, tile.transform.position.z + Random.Range(-tileSize/2, tileSize/2)), Quaternion.identity);
                            simpleStructure.transform.parent = new GameObject().transform.parent = tile.transform;
                        }
                        else if (probability < 730)
                        {
                            GameObject rock = Instantiate(rockPrefab, new Vector3(tile.transform.position.x + Random.Range(-tileSize/2, tileSize/2), 0, tile.transform.position.z + Random.Range(-tileSize/2, tileSize/2)), Quaternion.identity);
                            rock.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                            rock.transform.parent = new GameObject().transform.parent = tile.transform;
                        }
                    }
                    else if (probability >= 900)
                    {
                        GameObject smallRock = Instantiate(rockPrefab, new Vector3(tile.transform.position.x + Random.Range(-tileSize/2, tileSize/2), Random.Range(-2.3f, -0.7f), tile.transform.position.z + Random.Range(-tileSize/2, tileSize/2)), Quaternion.identity);
                        smallRock.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        smallRock.transform.parent = new GameObject().transform.parent = tile.transform;
                    }
                    //thing.transform.parent = new GameObject().transform.parent = tile.transform;
                    tiles.Add(tilePos, tile);
                    roadTiles.Add(roadTilePos, roadTile);
                }
            }
        }
    }
}
