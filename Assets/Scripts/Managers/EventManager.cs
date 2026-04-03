using DiceGame.Events;

namespace DiceGame.Managers
{
    public static class EventManager
    {
        public static UIEvents UIEvents { get; } = new UIEvents();
        public static DiceEvents DiceEvents { get; } = new DiceEvents();
    }
}

