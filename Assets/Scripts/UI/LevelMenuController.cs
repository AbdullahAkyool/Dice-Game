using DiceGame.Data;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.UI
{
    public class LevelMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform itemContainer;
        [SerializeField] private LevelUIItem itemPrefab;

        private void Start()
        {
            if (itemContainer == null || itemPrefab == null)
            {
                Debug.LogError("LevelMenuController: assign item container and item prefab.");
                return;
            }

            if (DatabaseManager.Instance == null || DatabaseManager.Instance.LevelDatabase == null)
            {
                Debug.LogError("LevelMenuController: DatabaseManager or LevelDatabase missing.");
                return;
            }

            ClearContainer();

            var levels = DatabaseManager.Instance.LevelDatabase.levels;
            if (levels == null)
            {
                return;
            }

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData data = levels[i];
                if (data == null || data.mapJsonFile == null)
                {
                    continue;
                }

                LevelUIItem item = Instantiate(itemPrefab, itemContainer);
                item.Initialize(data, BuildTitle(data));
            }
        }

        private static string BuildTitle(LevelData data)
        {
            if (data.mapJsonFile != null && !string.IsNullOrEmpty(data.mapJsonFile.name))
            {
                return data.mapJsonFile.name.Replace('-', ' ');
            }

            return $"Level {data.levelIndex + 1}";
        }

        private void ClearContainer()
        {
            for (int i = itemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(itemContainer.GetChild(i).gameObject);
            }
        }
    }
}
