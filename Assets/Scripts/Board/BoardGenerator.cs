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
        [SerializeField] private Transform boardParent;
        [SerializeField] private float spawnOffset = 5f;

        [Header("Board Settings")]
        [SerializeField] private int levelIndex;

        private readonly List<Tile> tiles = new List<Tile>();
        private MapData mapData;

        public int TileCount => tiles.Count;
        public int CurrentLevelIndex => levelIndex;

        public const int StartTileBoardIndex = 0;
        public const int FirstPlayableTileBoardIndex = 1;

        public static bool IsStartTileIndex(int boardIndex)
        {
            if (Instance == null)
            {
                return boardIndex == StartTileBoardIndex;
            }

            Tile tile = Instance.GetTile(boardIndex);
            return tile != null
                ? tile.TileData != null && tile.TileData.TileTypeEnum == TileType.Start
                : boardIndex == StartTileBoardIndex;
        }

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
                spawnedTile.Initialize(tileData, index);
                spawnedTile.transform.position = position;
                spawnedTile.transform.rotation = Quaternion.identity;
                spawnedTile.transform.SetParent(boardParent, false);
                tiles.Add(spawnedTile);
            }
            else
            {
                Debug.LogError("Failed to spawn tile");
            }
        }

        private void ClearExistingTiles()
        {
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

        public int GetWrappedPlayableIndex(int currentTileIndex, int steps)
        {
            if (tiles.Count <= 1)
            {
                return StartTileBoardIndex;
            }

            if (steps <= 0)
            {
                return Mathf.Clamp(currentTileIndex, StartTileBoardIndex, tiles.Count - 1);
            }

            int playableTileCount = tiles.Count - 1;
            int clampedCurrentIndex = Mathf.Clamp(currentTileIndex, StartTileBoardIndex, tiles.Count - 1);
            int currentProgressIndex = IsStartTileIndex(clampedCurrentIndex)
                ? 0
                : clampedCurrentIndex;

            return ((currentProgressIndex + steps - 1) % playableTileCount) + FirstPlayableTileBoardIndex;
        }
    }
}
