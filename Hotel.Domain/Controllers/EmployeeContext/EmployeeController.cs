using Hotel.Domain.DTOs.Base.User;
using Hotel.Domain.DTOs.EmployeeContext.EmployeeDTOs;
using Hotel.Domain.Handlers.EmployeeContexty.EmployeeHandlers;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Domain.Controllers.EmployeeContext;

public class EmployeeController : ControllerBase
{
  private readonly EmployeeHandler _handler;

  public EmployeeController(EmployeeHandler handler)
  {
    _handler = handler;
  }

  [HttpGet("v1/employees")]
  public async Task<IActionResult> GetAsync(
  [FromBody] EmployeeQueryParameters queryParameters)
  => Ok(await _handler.HandleGetAsync(queryParameters));

  [HttpGet("v1/employees/{Id:guid}")]
  public async Task<IActionResult> GetByIdAsync(
    [FromRoute] Guid id
  )
  => Ok(await _handler.HandleGetByIdAsync(id));


  [HttpPost("v1/employees")]
  public async Task<IActionResult> PostAsync(
    [FromBody] CreateUser model
  )
  => Ok(await _handler.HandleCreateAsync(model));


  [HttpPut("v1/employees/{Id:guid}")]
  public async Task<IActionResult> PutAsync(
    [FromBody] UpdateUser model,
    [FromRoute] Guid id
  )
  => Ok(await _handler.HandleUpdateAsync(model, id));

  [HttpDelete("v1/employees/{Id:guid}")]
  public async Task<IActionResult> DeleteAsync(
    [FromRoute] Guid id
  )
  => Ok(await _handler.HandleDeleteAsync(id));

}