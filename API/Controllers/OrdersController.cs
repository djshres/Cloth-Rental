using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using GSF.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        public OrdersController(IOrderService orderService, IMapper mapper)
        {
            _mapper = mapper;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
        {
            var email = HttpContext.User.RetrieveEmailFromPrincipal();

            var address = _mapper.Map<AddressDto, Address>(orderDto.ShipToAddress);

            var order = await _orderService.CreateOrderAsync(email, orderDto.DeliveryMethodId, orderDto.BasketId, address);

            SendMail(order);

            if (order == null) return BadRequest(new ApiResponse(400, "Problem creating order"));

            return Ok(order);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrdersForUser()
        {
            var email = User.RetrieveEmailFromPrincipal();

            var orders = await _orderService.GetOrdersForUserAsync(email);

            return Ok(_mapper.Map<IReadOnlyList<OrderToReturnDto>>(orders));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderByIdForUser(int id)
        {
            var email = User.RetrieveEmailFromPrincipal();

            var order = await _orderService.GetOrderByIdAsync(id, email);

            if (order == null) return NotFound(new ApiResponse(404));

            return _mapper.Map<OrderToReturnDto>(order);
        }

        [HttpGet("deliveryMethods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            return Ok(await _orderService.GetDeliveryMethodsAsync());
        }

        public void SendMail(Order order)
        {
            var message = new System.Text.StringBuilder();
            message.Append("<pre>");
            message.Append("<p>Dear Admin,</p>");
            message.Append("<p>New order has been placed </p>");
            message.Append("<b>Order Details</b>");
            message.Append("<table>");
            message.Append("<tr><td style=\"width:200px;\">Buyer Address:</td><td style=\"width:140px;\">" + order.ShipToAddress.FirstName +","+ 
                order.ShipToAddress.LastName +","+ order.ShipToAddress.State +" "+ order.ShipToAddress.Street +" "+ order.ShipToAddress.ZipCode + "</td></tr>");
            message.Append("<tr><td>Order Date:</td><td>" + order.OrderDate.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
            message.Append("<tr><td>Email:</td><td>" + order.BuyerEmail + "</td></tr>");
            message.Append("<tr><td>Delivery Method:</td><td>" + order.DeliveryMethod.ShortName + order.DeliveryMethod.DeliveryTime + order.DeliveryMethod.Description + order.DeliveryMethod.Price + "</td></tr>");
            message.Append("<tr><td>Subtotal:</td><td>" + order.Subtotal + "</td></tr>");
            message.Append("<tr><td>Order Status:</td><td>" + "Pending" + "</td></tr>");
            message.Append("<tr><td>Order Items:</td><td></td></tr>");
            foreach (var item in order.OrderItems)
            {
                message.Append("<tr><td>Product Name:</td><td>" + item.ItemOrdered.ProductName + "</td></tr>");
                message.Append("<tr><td>Price:</td><td>" + item.Price + "</td></tr>");
                message.Append("<tr><td>Quantity:</td><td>" + item.Quantity + "</td></tr>");
            }
            message.Append("</table>");
            message.Append("<p>Thank you for doing business with us.</p>");
            //message.Append("");
            //message.Append("<p>Sincerely,</p>");
            //message.Append("<div>Air Flight Express Courier Pvt. Ltd.</div>");
            //message.Append("<div>Thapagaun, New Baneshwor, Kathmandu-10, Nepal</div>");
            //message.Append("<div>Phone: +977-1-4487650</div>");
            //message.Append("<div>Email: info@airflight.com.np</div>");
            //message.Append($"<div>{MvcApplication.CompanyName}</div>");
            //message.Append($"<div>{MvcApplication.CorporateOffice}</div>");
            //message.Append($"<div>Email: {MvcApplication.Email}</div>");
            //message.Append($"<div>Website: {MvcApplication.Website}</div>");
            message.Append("</pre>");
            var email = "njshres7@gmail.com";

            try
            {
                new Mail().Send(email + "|" + email, "Order Confirmation", message.ToString());
            }
            catch (Exception ex)
            {
               // LogUtil.WriteLog($"Unable to send email: {email}, {ex.Message}");
            }
        }
    }
}