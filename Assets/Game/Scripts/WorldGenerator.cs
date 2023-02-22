using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject groundPrefab; // the prefab for the ground tiles
    public GameObject thingPrefab;
    public int worldSize = 10; // the size of the world (in tiles) around the player
    public int tileSize = 1; // the size of each tile in world units

    private Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>(); // stores the generated ground tiles
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
        foreach (KeyValuePair<Vector2, GameObject> pair in tiles)
        {
            Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(pair.Key.x), Mathf.RoundToInt(pair.Key.y));
            if (Vector2Int.Distance(tilePos, playerTilePos) > worldSize)
            {
                Destroy(pair.Value);
                tilesToRemove.Add(pair.Key);
            }
        }
        foreach (Vector2 key in tilesToRemove)
        {
            tiles.Remove(key);
        }

        // Generate new tiles within the world size around the player
        for (int x = -worldSize; x <= worldSize; x++)
        {
            for (int y = -worldSize; y <= worldSize; y++)
            {
                Vector2Int tilePos = playerTilePos + new Vector2Int(x, y);
                if (!tiles.ContainsKey(tilePos))
                {
                    GameObject tile = Instantiate(groundPrefab, new Vector3(tilePos.x * tileSize, 0, tilePos.y * tileSize), Quaternion.identity);
                    GameObject thing = Instantiate(thingPrefab, new Vector3(tile.transform.position.x + Random.Range(-tileSize/2, tileSize/2), tile.transform.position.y + Random.Range(1, 10), tile.transform.position.z + Random.Range(-tileSize/2, tileSize/2)), Quaternion.identity);
                    thing.transform.parent = new GameObject().transform.parent = tile.transform;
                    tiles.Add(tilePos, tile);
                }
            }
        }
    }
}
