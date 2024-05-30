using Hotel.Domain.Attributes;
using Hotel.Domain.DTOs.Base.User;
using Hotel.Domain.Enums;
using Hotel.Domain.Handlers.CustomerContext.CustomerHandlers;
using Hotel.Domain.Services.Users;
using Hotel.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Domain.Controllers.CustomerContext;

[ApiController]
[Route("v1/customers")]
[Authorize(Roles = "RootAdmin,Admin,Employee,Customer")]
public class CustomerController : ControllerBase
{
  private readonly CustomerHandler _handler;

  public CustomerController(CustomerHandler handler)
    => _handler = handler;

  // Endpoint para buscar clientes
  [HttpGet]
  public async Task<IActionResult> GetAsync([FromBody] UserQueryParameters queryParameters)
    => Ok(await _handler.HandleGetAsync(queryParameters));

  // Endpoint para buscar cliente por ID
  [HttpGet("{Id:guid}")]
  public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    => Ok(await _handler.HandleGetByIdAsync(id));

  // Endpoint para editar cliente (somente para administradores)
  [HttpPut("{Id:guid}")]
  [AuthorizePermissions([EPermissions.EditCustomer, EPermissions.DefaultAdminPermission])]
  public async Task<IActionResult> EditCustomerAsync([FromBody] UpdateUser model, [FromRoute] Guid id)
    => Ok(await _handler.HandleUpdateAsync(model, id));

  // Endpoint para deletar cliente (somente para administradores)
  [HttpDelete("{Id:guid}")]
  [AuthorizePermissions([EPermissions.DeleteCustomer, EPermissions.DefaultAdminPermission])]
  public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    => Ok(await _handler.HandleDeleteAsync(id));

  // Endpoint para editar o pr�prio cliente (usando ID do token)
  [HttpPut]
  public async Task<IActionResult> EditAsync([FromBody] UpdateUser model)
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleUpdateAsync(model, customerId));
  }

  // Endpoint para deletar o pr�prio cliente (usando ID do token)
  [HttpDelete]
  public async Task<IActionResult> DeleteAsync()
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleDeleteAsync(customerId));
  }

  // Endpoint para editar o nome do cliente (usando ID do token)
  [HttpPatch("name")]
  public async Task<IActionResult> UpdateNameAsync([FromBody] Name name)
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleUpdateNameAsync(customerId, name));
  }

  // Endpoint para editar o e-mail do cliente (usando ID do token)
  [HttpPatch("email")]
  public async Task<IActionResult> UpdateEmailAsync([FromBody] Email email)
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleUpdateEmailAsync(customerId, email));
  }

  // Endpoint para editar o telefone do cliente (usando ID do token)
  [HttpPatch("phone")]
  public async Task<IActionResult> UpdatePhoneAsync([FromBody] Phone phone)
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleUpdatePhoneAsync(customerId, phone));
  }

  // Endpoint para editar o endere�o do cliente (usando ID do token)
  [HttpPatch("address")]
  public async Task<IActionResult> UpdateAddressAsync([FromBody] Address address)
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleUpdateAddressAsync(customerId, address));
  }

  // Endpoint para editar o g�nero do cliente (usando ID do token)
  [HttpPatch("gender/{gender:int}")]
  public async Task<IActionResult> UpdateGenderAsync([FromRoute] int gender)
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleUpdateGenderAsync(customerId, (EGender)gender));
  }

  // Endpoint para editar a data de nascimento do cliente (usando ID do token)
  [HttpPatch("date-of-birth")]
  public async Task<IActionResult> UpdateDateOfBirthAsync([FromBody] UpdateDateOfBirth newDateOfBirth)
  {
    var customerId = UserServices.GetIdFromClaim(User);
    return Ok(await _handler.HandleUpdateDateOfBirthAsync(customerId, newDateOfBirth.DateOfBirth));
  }
}
