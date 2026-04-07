using DiceGame.Data;
using DiceGame.Managers;
using UnityEngine;

namespace DiceGame.UI
{
    public class LevelMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform itemContainer;

        private void Start()
        {
            if (SimplePoolManager.Instance == null)
            {
                Debug.LogError("LevelMenuController: SimplePoolManager instance not found.");
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

                LevelUIItem item = SimplePoolManager.Instance.Spawn<LevelUIItem>(PoolKey.LevelUIItem);
                item.transform.SetParent(itemContainer, false);
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
                LevelUIItem item = itemContainer.GetChild(i).GetComponent<LevelUIItem>();
                if (item != null)
                {
                    SimplePoolManager.Instance.Despawn(PoolKey.LevelUIItem, item);
                }
            }
        }
    }
}
