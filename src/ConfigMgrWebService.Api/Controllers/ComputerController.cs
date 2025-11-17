using ConfigMgrWebService.Shared.Constants;
using ConfigMgrWebService.Shared.DTOs;
using ConfigMgrWebService.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigMgrWebService.Api.Controllers;

/// <summary>
/// Computer management operations (ConfigMgr)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = ApiConstants.Policies.CombinedPolicy)]
[Produces("application/json")]
public class ComputerController : ControllerBase
{
    private readonly ILogger<ComputerController> _logger;
    // TODO: Inject services
    // private readonly IComputerService _computerService;

    public ComputerController(ILogger<ComputerController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Add a new computer by BIOS GUID to ConfigMgr
    /// </summary>
    /// <param name="request">Computer details</param>
    /// <returns>Success or failure response</returns>
    /// <response code="201">Computer added successfully</response>
    /// <response code="400">Invalid request or computer already exists</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("add-by-bios-guid")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddComputerByBiosGuid([FromBody] AddComputerByBiosGuidRequest request)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

        try
        {
            _logger.LogInformation(
                "Adding computer {ComputerName} with BIOS GUID {BiosGuid}. CorrelationId: {CorrelationId}",
                request.ComputerName, request.BiosGuid, correlationId);

            // TODO: Implement actual logic
            // var resourceId = await _computerService.AddComputerByBiosGuidAsync(request.ComputerName, request.BiosGuid);

            // For now, return success
            var response = ApiResponse.SuccessResponse(ApiConstants.ResponseMessages.ComputerAdded);
            response.CorrelationId = correlationId;

            return CreatedAtAction(
                nameof(CheckComputerExists),
                new { computerName = request.ComputerName },
                response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Computer already exists: {ComputerName}", request.ComputerName);
            var response = ApiResponse.FailureResponse(
                ex.Message,
                ApiConstants.ResponseMessages.ComputerAlreadyExists);
            response.CorrelationId = correlationId;
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding computer {ComputerName}", request.ComputerName);
            var response = ApiResponse.FailureResponse(
                ex.Message,
                ApiConstants.ResponseMessages.OperationFailed);
            response.CorrelationId = correlationId;
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }

    /// <summary>
    /// Delete a computer by name from ConfigMgr
    /// </summary>
    /// <param name="computerName">Computer name to delete</param>
    /// <returns>Success or failure response</returns>
    /// <response code="200">Computer deleted successfully</response>
    /// <response code="404">Computer not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{computerName}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteComputer(string computerName)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

        try
        {
            _logger.LogInformation(
                "Deleting computer {ComputerName}. CorrelationId: {CorrelationId}",
                computerName, correlationId);

            // TODO: Implement actual logic
            // await _computerService.DeleteComputerAsync(computerName);

            var response = ApiResponse.SuccessResponse(ApiConstants.ResponseMessages.ComputerDeleted);
            response.CorrelationId = correlationId;

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Computer not found: {ComputerName}", computerName);
            var response = ApiResponse.FailureResponse(
                ex.Message,
                ApiConstants.ResponseMessages.ComputerNotFound);
            response.CorrelationId = correlationId;
            return NotFound(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting computer {ComputerName}", computerName);
            var response = ApiResponse.FailureResponse(
                ex.Message,
                ApiConstants.ResponseMessages.OperationFailed);
            response.CorrelationId = correlationId;
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }

    /// <summary>
    /// Check if a computer exists in ConfigMgr
    /// </summary>
    /// <param name="computerName">Computer name to check</param>
    /// <returns>Computer existence status</returns>
    /// <response code="200">Check completed successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{computerName}/exists")]
    [ProducesResponseType(typeof(ApiResponse<ComputerExistsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckComputerExists(string computerName)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

        try
        {
            _logger.LogInformation(
                "Checking if computer exists: {ComputerName}. CorrelationId: {CorrelationId}",
                computerName, correlationId);

            // TODO: Implement actual logic
            // var exists = await _computerService.CheckComputerExistsAsync(computerName);

            var exists = false; // Placeholder

            var data = new ComputerExistsResponse
            {
                Exists = exists,
                ComputerName = computerName
            };

            var response = ApiResponse<ComputerExistsResponse>.SuccessResponse(
                data,
                exists ? "Computer exists" : "Computer not found");
            response.CorrelationId = correlationId;

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking computer existence: {ComputerName}", computerName);
            var response = ApiResponse<ComputerExistsResponse>.FailureResponse(
                ex.Message,
                ApiConstants.ResponseMessages.OperationFailed);
            response.CorrelationId = correlationId;
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }

    /// <summary>
    /// Clear the PXE flag for a computer
    /// </summary>
    /// <param name="computerName">Computer name</param>
    /// <returns>Success or failure response</returns>
    /// <response code="200">PXE flag cleared successfully</response>
    /// <response code="404">Computer not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{computerName}/clear-pxe-flag")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearPxeFlag(string computerName)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

        try
        {
            _logger.LogInformation(
                "Clearing PXE flag for computer {ComputerName}. CorrelationId: {CorrelationId}",
                computerName, correlationId);

            // TODO: Implement actual logic
            // await _computerService.ClearPxeFlagAsync(computerName);

            var response = ApiResponse.SuccessResponse(ApiConstants.ResponseMessages.PxeFlagCleared);
            response.CorrelationId = correlationId;

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Computer not found: {ComputerName}", computerName);
            var response = ApiResponse.FailureResponse(
                ex.Message,
                ApiConstants.ResponseMessages.ComputerNotFound);
            response.CorrelationId = correlationId;
            return NotFound(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing PXE flag for {ComputerName}", computerName);
            var response = ApiResponse.FailureResponse(
                ex.Message,
                ApiConstants.ResponseMessages.OperationFailed);
            response.CorrelationId = correlationId;
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
}
