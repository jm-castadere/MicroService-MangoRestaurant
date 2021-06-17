using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Messages;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartAPIController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponRepository _couponRepository;
        
        private readonly IMessageBus _messageBus;
        
        protected ResponseDto _responseRetDto;

        public CartAPIController(ICartRepository cartRepository, IMessageBus messageBus, ICouponRepository couponRepository)
        {
            _cartRepository = cartRepository;
            _couponRepository = couponRepository;
            _messageBus = messageBus;
            this._responseRetDto = new ResponseDto();
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<object> GetCart(string userId)
        {
            try
            {
                CartDto cartDto = await _cartRepository.GetCartByUserId(userId);
                _responseRetDto.Result = cartDto;
            }
            catch (Exception ex)
            {
                _responseRetDto.IsSuccess = false;
                _responseRetDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseRetDto;
        }

        [HttpPost("AddCart")]
        public async Task<object> AddCart(CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _responseRetDto.Result = cartDt;
            }
            catch (Exception ex)
            {
                _responseRetDto.IsSuccess = false;
                _responseRetDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseRetDto;
        }

        [HttpPost("UpdateCart")]
        public async Task<object> UpdateCart(CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _responseRetDto.Result = cartDt;
            }
            catch (Exception ex)
            {
                _responseRetDto.IsSuccess = false;
                _responseRetDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseRetDto;
        }

        [HttpPost("RemoveCart")]
        public async Task<object> RemoveCart([FromBody] int cartId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveFromCart(cartId);
                _responseRetDto.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _responseRetDto.IsSuccess = false;
                _responseRetDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseRetDto;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                bool isSuccess = await _cartRepository.ApplyCoupon(cartDto.CartHeader.UserId,
                    cartDto.CartHeader.CouponCode);
                _responseRetDto.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _responseRetDto.IsSuccess = false;
                _responseRetDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseRetDto;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] string userId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveCoupon(userId);
                _responseRetDto.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _responseRetDto.IsSuccess = false;
                _responseRetDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseRetDto;
        }

        /// <summary>
        /// Checkout paiement
        /// </summary>
        /// <param name="checkoutHeader"></param>
        /// <returns></returns>
        [HttpPost("Checkout")]
        public async Task<object> Checkout(CheckoutHeaderDto checkoutHeader)
        {
            try
            {
                //Get cart value of user
                CartDto cartDto = await _cartRepository.GetCartByUserId(checkoutHeader.UserId);
                if (cartDto == null)
                {
                    return BadRequest();
                }

                if (!string.IsNullOrEmpty(checkoutHeader.CouponCode))
                {
                    //Get current coupon
                    CouponDto couponVal = await _couponRepository.GetCoupon(checkoutHeader.CouponCode);
                    
                    //If coupon is channged
                    if (checkoutHeader.DiscountTotal != couponVal.DiscountAmount)
                    {
                        _responseRetDto.IsSuccess = false;
                        _responseRetDto.ErrorMessages = new List<string>() { "Coupon Price has changed, please confirm" };
                        _responseRetDto.DisplayMessage = "Coupon Price has changed, please confirm";
                        return _responseRetDto;
                    }
                }

                //Set cart detail
                checkoutHeader.CartDetails = cartDto.CartDetails;
                
                //logic to add message to process order in AZURE BUS-> azure topic name
                await _messageBus.PublishMessage(checkoutHeader, "checkoutqueue");

                //Clean cart
                await _cartRepository.ClearCart(checkoutHeader.UserId);
            }
            catch (Exception ex)
            {
                _responseRetDto.IsSuccess = false;
                _responseRetDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseRetDto;
        }
    }
}
