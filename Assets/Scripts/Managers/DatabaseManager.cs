using DiceGame.Data;
using UnityEngine;

namespace DiceGame.Managers
{
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }

        [Header("Databases")]
        [SerializeField] private FruitDatabase fruitDatabase;
        public FruitDatabase FruitDatabase => fruitDatabase;

        [SerializeField] private LevelDatabase levelDatabase;
        public LevelDatabase LevelDatabase => levelDatabase;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
