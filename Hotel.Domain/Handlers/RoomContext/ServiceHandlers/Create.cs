using Hotel.Domain.DTOs;
using Hotel.Domain.DTOs.RoomContext.ServiceDTOs;
using Hotel.Domain.Entities.RoomContext.ServiceEntity;
using Hotel.Domain.Handlers.Interfaces;
using Hotel.Domain.Repositories.Interfaces.EmployeeContext;
using Hotel.Domain.Repositories.Interfaces.RoomContext;

namespace Hotel.Domain.Handlers.RoomContext.ServiceHandler;

public partial class ServiceHandler : IHandler
{
  private readonly IServiceRepository  _repository;
  private readonly IResponsabilityRepository _responsabilityRepository;
  public ServiceHandler(IServiceRepository repository, IResponsabilityRepository responsabilityRepository)
  {
    _repository = repository;
    _responsabilityRepository = responsabilityRepository;
  }


  public async Task<Response> HandleCreateAsync(EditorService model)
  {
    var service = new Service(model.Name,model.Price,model.Priority,model.TimeInMinutes);  

    await _repository.CreateAsync(service);
    await _repository.SaveChangesAsync();

    return new Response(200,"Serviço criado.",new { service.Id });
  }
}