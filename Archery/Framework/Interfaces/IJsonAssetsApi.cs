namespace Archery.Framework.Interfaces
{
    public interface IJsonAssetsApi
    {
        string GetObjectId(string name);
        string GetCropId(string name);
        string GetFruitTreeId(string name);
        string GetBigCraftableId(string name);
        string GetHatId(string name);
        string GetWeaponId(string name);
        string GetClothingId(string name);
        string GetPantsId(string name);
        string GetShirtId(string name);
    }
}
