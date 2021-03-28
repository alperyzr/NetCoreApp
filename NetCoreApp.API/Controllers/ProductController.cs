using AuthServer.Core.DTOs;
using AuthServer.Core.Model;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreApp.API.Controllers
{
    [Authorize]//Product Ekleecek kişinin bir tokenı olmak zorundadır.
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : CustomBaseController
    {
        //<> içerisindeki ilk class hangi class ile işlem yapacağımızı, virgülden sonraki class ise ne dönüceğimizi belirtir.
        private readonly IGenericService<Product, ProductDto> _productService;

        #region ctor
        public ProductController(IGenericService<Product, ProductDto> productService)
        {
            _productService = productService;
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> GetProduct()
        {
            return ActionResultInstance(await _productService.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct(ProductDto productDto)
        {
            return ActionResultInstance(await _productService.AddAsync(productDto));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(ProductDto productDto)
        {
            return ActionResultInstance(await _productService.Update(productDto, productDto.Id));
        }


        //validationda belirtmezsek api/product?id=2 şeklinde url den alıcaktı.
        //api/product/2
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            return ActionResultInstance(await _productService.Remove(id));
        }
    }
}
