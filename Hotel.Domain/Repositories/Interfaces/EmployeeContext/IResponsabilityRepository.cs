using Hotel.Domain.DTOs.EmployeeContext.ResponsabilityDTOs;
using Hotel.Domain.Entities.EmployeeContext.ResponsabilityEntity;

namespace Hotel.Domain.Repositories.Interfaces.EmployeeContext;

public interface IResponsabilityRepository : IRepository<Responsability>, IRepositoryQuery<GetReponsability, ResponsabilityQueryParameters>
{
}