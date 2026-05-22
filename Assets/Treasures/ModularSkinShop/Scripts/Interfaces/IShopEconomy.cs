namespace ModularSkinShop.Interfaces
{
    public interface IShopEconomy
    {
        int GetBalance();
        bool CanAfford(int amount);
        void Spend(int amount);
    }
}
