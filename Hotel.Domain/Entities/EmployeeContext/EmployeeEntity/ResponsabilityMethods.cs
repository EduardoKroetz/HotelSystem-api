using Hotel.Domain.Entities.EmployeeContext.EmployeeEntity.Interfaces;
using Hotel.Domain.Entities.EmployeeContext.ResponsabilityEntity;

namespace Hotel.Domain.Entities.EmployeeContext.EmployeeEntity;

public partial class Employee : IResponsabilitiesMethods
{
  public void AddResponsability(Responsability responsability)
  => Responsabilities.Add(responsability);
  
  public void RemoveResponsability(Responsability responsability)
  => Responsabilities.Remove(responsability);
  
}