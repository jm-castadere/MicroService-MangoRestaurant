using AutoMapper;
using Mango.Services.ProductAPI.DbContexts;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mango.Services.ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _dbAplDbContext;
        private IMapper _mapper;

        public ProductRepository(ApplicationDbContext db, IMapper mapper)
        {
            _dbAplDbContext = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetProducts()
        {
            List<Product> productList = await _dbAplDbContext.Products.ToListAsync();

            return _mapper.Map<List<ProductDto>>(productList);

        }


        public async Task<ProductDto> GetProductById(int productId)
        {
            Product product = await _dbAplDbContext.Products.Where(x => x.ProductId == productId).FirstOrDefaultAsync();
            return _mapper.Map<ProductDto>(product);
        }

        
        public async Task<ProductDto> CreateUpdateProduct(ProductDto productDto)
        {
            Product newProduct = _mapper.Map<ProductDto, Product>(productDto);
            if (newProduct.ProductId > 0)
            {
                _dbAplDbContext.Products.Update(newProduct);
            }
            else
            {
                _dbAplDbContext.Products.Add(newProduct);
            }
            await _dbAplDbContext.SaveChangesAsync();

            return _mapper.Map<Product, ProductDto>(newProduct);
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            try{
                Product delProduct = await _dbAplDbContext.Products.FirstOrDefaultAsync(u => u.ProductId == productId);
                if (delProduct == null)
                {
                    return false;
                }
                _dbAplDbContext.Products.Remove(delProduct);
                await _dbAplDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

       
    }
}
