using Hotel.Domain.Entities.Base;
using Hotel.Domain.Entities.EmployeeContext.ResponsabilityEntity;
using Hotel.Domain.Enums;

namespace Hotel.Domain.Entities.RoomContext.ServiceEntity;

public partial class Service : Entity
{
  public Service(string name, decimal price, bool isActive, EPriority priority, int timeInMinutes, Responsability responsability)
  {
    Name = name;
    Price = price;
    IsActive = isActive;
    Priority = priority;
    TimeInMinutes = timeInMinutes;
    Responsabilities = [];

    Validate();
    AddResponsability(responsability);
  }

  public string Name { get; private set; }
  public decimal Price { get; private set; }
  public bool IsActive { get; private set; }
  public EPriority Priority { get; private set; }
  public int TimeInMinutes { get; private set; }
  public List<Responsability> Responsabilities { get; private set; } 
}