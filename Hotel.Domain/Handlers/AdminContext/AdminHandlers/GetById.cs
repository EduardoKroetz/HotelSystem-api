using Hotel.Domain.DTOs;
using Hotel.Domain.DTOs.Base.User;

namespace Hotel.Domain.Handlers.AdminContext.AdminHandlers;

public partial class AdminHandler
{
  public async Task<Response<GetUser>> HandleGetByIdAsync(Guid adminId)
  {
    var admin = await _repository.GetByIdAsync(adminId);
    if (admin == null)
      throw new ArgumentException("Administrador não encontrado.");
    
    return new Response<GetUser>(200,"Administrador encontrado.", admin);
  }
}