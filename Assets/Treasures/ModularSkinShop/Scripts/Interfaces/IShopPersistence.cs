namespace ModularSkinShop.Interfaces
{
    public interface IShopPersistence
    {
        bool IsUnlocked(string id);
        void Unlock(string id);
        string GetActiveId();
        void SetActive(string id);
    }
}
