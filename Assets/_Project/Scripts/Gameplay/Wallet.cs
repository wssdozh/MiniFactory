using System;

namespace MiniFactory.Gameplay
{
    public class Wallet
    {
        private int _coins;

        public int Coins => _coins;
        public event Action<int> CoinsChanged;

        public Wallet(int initialCoins = 0)
        {
            _coins = Math.Max(0, initialCoins);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || _coins < amount)
                return false;

            _coins -= amount;
            CoinsChanged?.Invoke(_coins);
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            _coins += amount;
            CoinsChanged?.Invoke(_coins);
        }
    }
}
