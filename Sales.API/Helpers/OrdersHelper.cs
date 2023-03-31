using Microsoft.EntityFrameworkCore;
using Sales.API.Data;
using Sales.Shared.Entities;
using Sales.Shared.Enums;
using Sales.Shared.Responses;

namespace Sales.API.Helpers
{
    public class OrdersHelper : IOrdersHelper
    {
        private readonly DataContext _context;

        public OrdersHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<Response> ProcessOrderAsync(string email, string remarks)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Usuario no válido"
                };
            }

            var temporalSales = await _context.TemporalSales
                .Include(x => x.Product)
                .Where(x => x.User!.Email == email)
                .ToListAsync();
            Response response = await CheckInventoryAsync(temporalSales);
            if (!response.IsSuccess)
            {
                return response;
            }

            Sale sale = new()
            {
                Date = DateTime.UtcNow,
                User = user,
                Remarks = remarks,
                SaleDetails = new List<SaleDetail>(),
                OrderStatus = OrderStatus.Nuevo
            };

            foreach (var temporalSale in temporalSales)
            {
                sale.SaleDetails.Add(new SaleDetail
                {
                    Product = temporalSale.Product,
                    Quantity = temporalSale.Quantity,
                    Remarks = temporalSale.Remarks,
                });

                Product? product = await _context.Products.FindAsync(temporalSale.Product!.Id);
                if (product != null)
                {
                    product.Stock -= temporalSale.Quantity;
                    _context.Products.Update(product);
                }

                _context.TemporalSales.Remove(temporalSale);
            }

            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return response;
        }

        private async Task<Response> CheckInventoryAsync(List<TemporalSale> temporalSales)
        {
            Response response = new() 
            { 
                IsSuccess = true 
            };

            foreach (var temporalSale in temporalSales)
            {
                Product? product = await _context.Products.FirstOrDefaultAsync(x => x.Id == temporalSale.Product!.Id);
                if (product == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"El producto {temporalSale.Product!.Name}, ya no está disponible";
                    return response;
                }
                if (product.Stock < temporalSale.Quantity)
                {
                    response.IsSuccess = false;
                    response.Message = $"Lo sentimos no tenemos existencias suficientes del producto {temporalSale.Product!.Name}, para tomar su pedido. Por favor disminuir la cantidad o sustituirlo por otro.";
                    return response;
                }
            }
            return response;
        }
    }
}
