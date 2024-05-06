using Hotel.Domain.DTOs;
using Hotel.Domain.DTOs.AdminContext.AdminDTOs;

namespace Hotel.Domain.Handlers.AdminContext.AdminHandlers;

public partial class AdminHandler
{
  public async Task<Response<GetAdmin>> HandleGetByIdAsync(Guid adminId)
  {
    var admin = await _repository.GetByIdAsync(adminId);
    if (admin == null)
      throw new ArgumentException("Admin não encontrado.");
    
    return new Response<GetAdmin>(200,"Admin encontrado com sucesso!", admin);
  }
}