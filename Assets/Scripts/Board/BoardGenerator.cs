using UnityEngine;
using System.Collections.Generic;
using DiceGame.Data;
using DiceGame.Managers;

namespace DiceGame.Board
{
    public class BoardGenerator : MonoBehaviour
    {
        public static BoardGenerator Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform boardParent;

        [Header("Board Settings")]
        [SerializeField] private int levelIndex;

        private List<Tile> tiles = new List<Tile>();
        private MapData mapData;

        public int TileCount => tiles.Count;
        public int CurrentLevelIndex => levelIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void BuildLevel(int index) // Loads JSON from LevelDatabase for this index
        {
            levelIndex = index;
            LoadMapData();
            GenerateBoard();
        }

        private void LoadMapData()
        {
            if (DatabaseManager.Instance == null)
            {
                Debug.LogError("DatabaseManager instance not found");
                return;
            }

            LevelData levelData = DatabaseManager.Instance.LevelDatabase.GetLevel(levelIndex);
            if (levelData == null)
            {
                Debug.LogError("Level data is not assigned");
                return;
            }

            string jsonContent = levelData.mapJsonFile.text;
            mapData = JsonUtility.FromJson<MapData>(jsonContent);

            if (mapData == null || mapData.tiles == null)
            {
                Debug.LogError("Failed to load map data from JSON");
            }
        }

        private void GenerateBoard()
        {
            if (mapData == null || mapData.tiles == null)
            {
                Debug.LogError("Cannot generate board: map data is null");
                return;
            }

            ClearExistingTiles();

            for (int i = 0; i < mapData.tiles.Count; i++)
            {
                CreateTile(mapData.tiles[i], i);
            }
        }

        private void CreateTile(TileData tileData, int index)
        {
            if (tilePrefab == null)
            {
                Debug.LogError("Tile prefab is not assigned!");
                return;
            }

            Vector3 position = CalculateTilePosition(index);
            GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity, boardParent);
            tileObject.name = $"Tile_{index}";

            Tile tile = tileObject.GetComponent<Tile>();
            if (tile != null)
            {
                tile.Initialize(tileData);
                tiles.Add(tile);
            }
            else
            {
                Debug.LogError($"Tile prefab is missing Tile component!");
                Destroy(tileObject);
            }
        }

        private Vector3 CalculateTilePosition(int index)
        {
            float spacing = tilePrefab.transform.localScale.z;
            return new Vector3(0, 0, index * spacing);
        }

        private void ClearExistingTiles()
        {
            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }
            tiles.Clear();
        }

        public Tile GetTile(int index)
        {
            if (index >= 0 && index < tiles.Count)
            {
                return tiles[index];
            }
            return null;
        }

        public int GetWrappedIndex(int index)
        {
            if (tiles.Count == 0) return 0;
            return index % tiles.Count;
        }
    }
}
