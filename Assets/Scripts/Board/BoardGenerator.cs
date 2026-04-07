using UnityEngine;
using System.Collections.Generic;
using DiceGame.Data;
using DiceGame.Managers;
using DiceGame.Fruit;

namespace DiceGame.Board
{
    public class BoardGenerator : MonoBehaviour
    {
        public static BoardGenerator Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform boardParent;
        [SerializeField] private float spawnOffset = 5f;

        [Header("Board Settings")]
        [SerializeField] private int levelIndex;

        private readonly List<Tile> tiles = new List<Tile>();
        private readonly List<FruitController> spawnedFruits = new List<FruitController>();
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
            if (DatabaseManager.Instance == null)
            {
                Debug.LogError("DatabaseManager instance not found");
                return;
            }

            if (SimplePoolManager.Instance == null)
            {
                Debug.LogError("SimplePoolManager instance not found");
                return;
            }

            Tile spawnedTile = SimplePoolManager.Instance.Spawn<Tile>(PoolKey.Tile);
            if (spawnedTile != null)
            {
                Vector3 position = new Vector3(0, 0, index * spawnOffset);
                spawnedTile.name = $"Tile_{index}";
                spawnedTile.Initialize(tileData);
                spawnedTile.transform.position = position;
                spawnedTile.transform.rotation = Quaternion.identity;
                spawnedTile.transform.SetParent(boardParent, false);
                tiles.Add(spawnedTile);

                TrySpawnRewardFruit(tileData, spawnedTile);
            }
            else
            {
                Debug.LogError("Failed to spawn tile");
            }
        }

        private void ClearExistingTiles()
        {
            ClearSpawnedFruits();

            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    if (SimplePoolManager.Instance == null)
                    {
                        Debug.LogError("SimplePoolManager instance not found");
                        Destroy(tile.gameObject);
                        return;
                    }

                    SimplePoolManager.Instance.Despawn(PoolKey.Tile, tile);
                }
            }
            tiles.Clear();
        }

        private void TrySpawnRewardFruit(TileData tileData, Tile spawnedTile)
        {
            if (!tileData.HasReward || DatabaseManager.Instance.FruitDatabase == null)
            {
                return;
            }

            PoolKey fruitPoolKey = DatabaseManager.Instance.FruitDatabase.GetPoolKeyForFruit(tileData.FruitTypeEnum);
            if (fruitPoolKey == PoolKey.None)
            {
                Debug.LogWarning($"No pool key found for reward fruit type '{tileData.fruitType}' on tile {tileData.index}.");
                return;
            }

            FruitController spawnedFruit = SimplePoolManager.Instance.Spawn<FruitController>(fruitPoolKey);
            if (spawnedFruit == null)
            {
                return;
            }

            spawnedFruit.transform.SetParent(spawnedTile.transform, false);
            spawnedFruit.transform.localPosition = new Vector3(0f, 0.5f, 1f);
            spawnedFruit.transform.localRotation = Quaternion.identity;
            spawnedFruits.Add(spawnedFruit);
        }

        private void ClearSpawnedFruits()
        {
            if (DatabaseManager.Instance == null || DatabaseManager.Instance.FruitDatabase == null || SimplePoolManager.Instance == null)
            {
                return;
            }

            foreach (var fruit in spawnedFruits)
            {
                if (fruit == null)
                {
                    continue;
                }

                PoolKey fruitPoolKey = DatabaseManager.Instance.FruitDatabase.GetPoolKeyForFruit(fruit.FruitType);
                if (fruitPoolKey == PoolKey.None)
                {
                    Debug.LogWarning($"No pool key found while despawning reward fruit '{fruit.name}'.");
                    continue;
                }

                SimplePoolManager.Instance.Despawn(fruitPoolKey, fruit);
            }

            spawnedFruits.Clear();
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
