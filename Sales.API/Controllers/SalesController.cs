using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sales.API.Helpers;
using Sales.Shared.DTOs;

namespace Sales.API.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("/api/sales")]
    public class SalesController : ControllerBase
    {
        private readonly IOrdersHelper _ordersHelper;

        public SalesController(IOrdersHelper ordersHelper)
        {
            _ordersHelper = ordersHelper;
        }

        [HttpPost]
        public async Task<ActionResult> Post(SaleDTO saleDTO)
        {
            var response = await _ordersHelper.ProcessOrderAsync(User.Identity!.Name!, saleDTO.Remarks);
            if (response.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(response.Message);
        }
    }
}
