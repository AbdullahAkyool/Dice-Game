using DiceGame.Data;
using DiceGame.Pooling;
using UnityEngine;

namespace DiceGame.Fruit
{
    public class FruitController : MonoBehaviour, IPoolable
    {
        [SerializeField] private FruitType fruitType;
        public FruitType FruitType => fruitType;

        private Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            if (animator == null)
            {
                Debug.LogError("FruitController: Animator not found.");
            }

            animator.SetFloat("Speed", Random.Range(0.8f, 1.2f));
        }

        public void OnDespawn()
        {
            gameObject.SetActive(false);
        }

        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }
    }
}
