using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.Models;
using System.Security.Claims;
using BusinessObjects.DTOs;
using SunStore.APIServices;
using BusinessObjects.RequestModel;

namespace SunStore.Controllers
{
    public class CartController : Controller
    {
        private readonly CartAPIService _cartAPIService;

        public CartController(CartAPIService cartAPIService)
        {
            _cartAPIService = cartAPIService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
            var uid = 0;

            if(userId != null)
            {
                uid = int.Parse(userId);
            }

            var listCart = await _cartAPIService.GetCartAsync(uid);
            return View(listCart);
        }

        public async Task<JsonResult> AddToCart(int id, int quantity)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToString();
            int? uid = null;

            if (userId != null)
            {
                uid = int.Parse(userId);
            }

            var request = new CartActionRequest
            {
                CustomerId = uid,
                ProductOptionId = id,
                Quantity = quantity
            };

            var response = await _cartAPIService.AddToCartAsync(request);
            bool exist = response.Message.Equals("True");

            return Json(new
            {
                exist
            });
        }

        public async Task<JsonResult> UpdateQuantity(int id, int quantity)
        {
            var request = new UpdateQuantityRequest
            {
                CartItemId = id,
                Quantity = quantity
            };

            var response = await _cartAPIService.UpdateQuantityAsync(request);

            return Json(new
            {
                quantity = response.Quantity,
                price = response.Price,
                unitprice = response.UnitPrice,
                total = response.Total
            });
        }

        public async Task<JsonResult> DeleteItem(int id)
        {
            var result = await _cartAPIService.DeleteItemAsync(id);

            //// update Cart Quantity
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //int uid = userId != null ? int.Parse(userId) : 0;
            //var remainingCount = (await _cartAPIService.GetCartAsync(uid)).Count;
            //HttpContext.Session.SetString("CartQuantity", remainingCount.ToString());

            return Json(new
            {
                total = result.Total,
            });
        }
    }
}
