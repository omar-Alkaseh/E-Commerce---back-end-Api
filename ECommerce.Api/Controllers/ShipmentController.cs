using Application.Common.Constants;
using Application.Features.Shipments.DTOs;
using Application.Interfaces.Services;
using Contracts.Requests;
using Contracts.Responses;
using ECommerce.Api.Mappings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Controllers
{
    [Authorize]
    [Route("api/shipments")]
    [ApiController]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [HttpGet("{shipmentId:int}")]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipmentResponse>> GetById(int shipmentId)
        {
            var shipment = await _shipmentService.GetByShipmentIdWithOrderDetailsAsync(shipmentId);

            return Ok(shipment.ToResponse());
        }


        [HttpGet]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(List<ShipmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ShipmentResponse>>> GetAllShipments()
        {
            var shipments = await _shipmentService.GetAllAsync();

            return Ok(shipments.ToResponseList());
        }


        [HttpGet("my-shipments")]
        [EnableRateLimiting("UserLimiter")]
        [ProducesResponseType(typeof(List<ShipmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ShipmentResponse>>> GetMyShipments()
        {
            var shipments = await _shipmentService.GetMyShipmentsAsync();

            return Ok(shipments.ToResponseList());
        }



        [HttpGet("status")]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(List<ShipmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ShipmentResponse>>> GetByStatus([FromQuery]GetShipmentRequest request)
        {
            var status = new ShipmentStatusDto
            {
                Status = request.Status
            };

            var shipments = await _shipmentService.GetByShipmentStatusWithOrderDetails(status);

            return Ok(shipments.ToResponseList());
        }



        [HttpPost("orders/{orderId:int}")]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipmentResponse>> CreateShipment([FromRoute] int orderId,[FromBody] CreateShipmentRequest request)
        {
            var shipment = await _shipmentService.CreateShipmentAsync(orderId, new CreateShipmentDto
            {
                CarrierName = request.CarrierName
            });

            return CreatedAtAction(
                nameof(GetById),
                new { shipmentId = shipment.ShipmentId },
                shipment.ToResponse());
        }



        [HttpPut("{shipmentId:int}")]
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        [EnableRateLimiting("AdminLimiter")]
        [ProducesResponseType(typeof(ShipmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShipmentResponse>> UpdateShipment([FromRoute] int shipmentId, [FromQuery] GetShipmentRequest statusRequest, [FromBody] UpdateShipmentStatusRequest request)
        {
            var status = new ShipmentStatusDto
            {
                Status = statusRequest.Status
            };

            var UpdateShipmentDto = new UpdateShipmentDto
            {
                TrackingNumber = request.TrackingNumber,
                CarrierName = request.CarrierName
            };

            var shipment = await _shipmentService.UpdateShipmentAsync(shipmentId, status, UpdateShipmentDto);

            return Ok(shipment.ToResponse());
        }
    }
}
