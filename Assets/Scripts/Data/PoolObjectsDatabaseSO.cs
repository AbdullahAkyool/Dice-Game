using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceGame.Data
{
    [CreateAssetMenu(fileName = "NewPoolObjectsDatabase", menuName = "Database/Pool Objects Database")]
    public class PoolObjectsDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<PoolObjectData> poolObjects;

        public PoolObjectData GetPoolObjectData(PoolKey type)
        {
            return poolObjects != null ? poolObjects.Find(po => po.Type == type) : null;
        }

        public List<PoolObjectData> GetPoolObjectDataList()
        {
            return poolObjects;
        }
    }

    [Serializable]
    public class PoolObjectData
    {
        public PoolKey Type;
        public bool IsUIObject;
        public GameObject Prefab;
        public int InitialSize = 5;
        public int MaxSize = 50;
    }

    [Serializable]
    public enum PoolKey
    {
        None = 0,
        Tile = 1,
        LevelUIItem = 2,
        Fruit_Apple = 100,
        Fruit_Pear = 101,
        Fruit_Strawberry = 102,

    }
}
