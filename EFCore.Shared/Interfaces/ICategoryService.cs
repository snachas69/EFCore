using EFCore.Model;

namespace EFCore.Shared.Interfaces;
public interface ICategoryService
{
    Category? Add(string name);
    void Delete(string name);
    void Edit(Category? updatedData);
    bool Exists(string name);
    List<Category> GetCategoriesByName(string name);
    Category? GetCategoryById(int id);
    Category? GetCategoryByName(string name);
    int LoadProducts(Category category);
}