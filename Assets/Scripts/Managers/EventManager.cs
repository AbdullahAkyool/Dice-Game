using DiceGame.Events;

namespace DiceGame.Managers
{
    public static class EventManager
    {
        public static UIEvents UIEvents { get; } = new UIEvents();
        public static DiceEvents DiceEvents { get; } = new DiceEvents();
        public static InventoryEvents InventoryEvents { get; } = new InventoryEvents();
        public static CameraEvents CameraEvents { get; } = new CameraEvents();
    }
}

