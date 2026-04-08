using System;
using DiceGame.Data;
using UnityEngine;

namespace DiceGame.Events
{
    [Serializable]
    public class InventoryEvents
    {
        public Action OnCheckEarnableRewards; // player hareketi tamamlandiginda kazanilabilecek odulleri kontrol et
        public Action<FruitType, int> OnRewardTileReached; // odullu tile'a gelindiginde, earn efekti baslatmak icin
        public Action<FruitType, int> OnItemAdded; // inventorye item eklendiginde, fruit type ve yeni miktar bilgisi ile birlikte
        public Action<FruitType, int> OnUpdateItemUIElement; // inventorydeki item miktari degistiginde, UI elementlerini guncellemek icin, fruit type ve yeni miktar bilgisi ile birlikte
        public Action OnInventoryReset; // inventory sifirlanmak istendiginde, genellikle yeni levela gecilirken veya level resetlenirken
        public Action OnInventoryForceRefresh; // inventorydeki item miktarlari degismese bile UI elementlerini guncellemek istedigimiz durumlarda, ornegin save dosyasindan gelen degerler uygulandiktan sonra UIin guncellenmesi icin
    }
}
