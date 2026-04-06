using DiceGame.Events;

namespace DiceGame.Managers
{
    public static class EventManager
    {
        public static GameFlowEvents GameFlowEvents { get; } = new GameFlowEvents();
        public static UIEvents UIEvents { get; } = new UIEvents();
        public static DiceEvents DiceEvents { get; } = new DiceEvents();
        public static PlayerEvents PlayerEvents { get; } = new PlayerEvents();
        public static InventoryEvents InventoryEvents { get; } = new InventoryEvents();
        public static CameraEvents CameraEvents { get; } = new CameraEvents();
    }
}

