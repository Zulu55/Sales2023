﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sales.API.Data;
using Sales.API.Helpers;
using Sales.Shared.DTOs;
using Sales.Shared.Entities;

namespace Sales.API.Controllers
{
    namespace Sales.API.Controllers
    {
        [ApiController]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("/api/products")]
        public class ProductsController : ControllerBase
        {
            private readonly DataContext _context;
            private readonly IFileStorage _fileStorage;

            public ProductsController(DataContext context, IFileStorage fileStorage)
            {
                _context = context;
                _fileStorage = fileStorage;
            }

            [HttpGet]
            public async Task<ActionResult> Get([FromQuery] PaginationDTO pagination)
            {
                var queryable = _context.Products
                    .Include(x => x.ProductImages)
                    .Include(x => x.ProductCategories)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
                }

                return Ok(await queryable
                    .OrderBy(x => x.Name)
                    .Paginate(pagination)
                    .ToListAsync());
            }


            [HttpGet("totalPages")]
            public async Task<ActionResult> GetPages([FromQuery] PaginationDTO pagination)
            {
                var queryable = _context.Products
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
                }

                double count = await queryable.CountAsync();
                double totalPages = Math.Ceiling(count / pagination.RecordsNumber);
                return Ok(totalPages);
            }

            [HttpGet("{id:int}")]
            public async Task<IActionResult> GetAsync(int id)
            {
                var product = await _context.Products
                    .Include(x => x.ProductImages)
                    .Include(x => x.ProductCategories!)
                    .ThenInclude(x => x.Category)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }

            [HttpPost]
            public async Task<ActionResult> PostAsync(ProductDTO productDTO)
            {
                try
                {
                    Product newProduct = new()
                    {
                        Name = productDTO.Name,
                        Description = productDTO.Description,
                        Price = productDTO.Price,
                        Stock = productDTO.Stock,
                        ProductCategories = new List<ProductCategory>(),
                        ProductImages = new List<ProductImage>()
                    };

                    foreach (var productImage in productDTO.ProductImages!)
                    {
                        var photoProduct = Convert.FromBase64String(productImage);
                        newProduct.ProductImages.Add(new ProductImage { Image = await _fileStorage.SaveFileAsync(photoProduct, ".jpg", "products") });
                    }

                    foreach (var productCategoryId in productDTO.ProductCategoryIds!)
                    {
                        var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == productCategoryId);
                        newProduct.ProductCategories.Add(new ProductCategory { Category = category! });
                    }

                    _context.Add(newProduct);
                    await _context.SaveChangesAsync();
                    return Ok(productDTO);
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                    {
                        return BadRequest("Ya existe una ciudad con el mismo nombre.");
                    }

                    return BadRequest(dbUpdateException.Message);
                }
                catch (Exception exception)
                {
                    return BadRequest(exception.Message);
                }
            }

            [HttpPut]
            public async Task<ActionResult> PutAsync(ProductDTO productDTO)
            {
                try
                {
                    var product = await _context.Products
                        .Include(x => x.ProductCategories)
                        .FirstOrDefaultAsync(x => x.Id == productDTO.Id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    product.Name = productDTO.Name;
                    product.Description = productDTO.Description;
                    product.Price = productDTO.Price;
                    product.Stock = productDTO.Stock;
                    product.ProductCategories = productDTO.ProductCategoryIds!.Select(x => new ProductCategory { CategoryId = x }).ToList();

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return Ok(productDTO);
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                    {
                        return BadRequest("Ya existe una ciudad con el mismo nombre.");
                    }

                    return BadRequest(dbUpdateException.Message);
                }
                catch (Exception exception)
                {
                    return BadRequest(exception.Message);
                }
            }

            [HttpDelete("{id:int}")]
            public async Task<IActionResult> DeleteAsync(int id)
            {
                var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
                if (product == null)
                {
                    return NotFound();
                }

                _context.Remove(product);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }
    }
}