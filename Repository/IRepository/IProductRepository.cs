using ApiEcommerce.Models;

namespace ApiEcommerce.Repository.IRepository
{
    public interface IProductRepository
    {
        public ICollection<Product> GetAllProducts();
        public ICollection<Product> GetProductsInPages(int pageNumber, int pageSize);
        public int GetTotalProducts();

        public ICollection<Product> GetProductsByCategory(int categoryId);

        public ICollection<Product> SearchProducts(string searchTerm);

        public Product? GetProduct(int productId);

        public bool BuyProduct(string name, int quantity);

        public bool ProductExists(int productId);

        public bool ProductExists(string name);

        public bool CreateProduct(Product product);

        public bool UpdateProduct(Product product); 

        public bool DeleteProduct(Product product);

        public bool Save();

    }
}