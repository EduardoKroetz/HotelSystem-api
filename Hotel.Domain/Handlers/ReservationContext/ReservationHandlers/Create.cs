using System.Collections.ObjectModel;
using Hotel.Domain.DTOs;
using Hotel.Domain.DTOs.ReservationContext.ReservationDTOs;
using Hotel.Domain.Entities.CustomerContext;
using Hotel.Domain.Entities.ReservationContext.ReservationEntity;
using Hotel.Domain.Handlers.Interfaces;
using Hotel.Domain.Repositories.Interfaces.CustomerContext;
using Hotel.Domain.Repositories.Interfaces.ReservationContext;
using Hotel.Domain.Repositories.Interfaces.RoomContext;

namespace Hotel.Domain.Handlers.ReservationContext.ReservationHandlers;

public partial class ReservationHandler : IHandler
{
  private readonly IReservationRepository  _repository;
  private readonly IRoomRepository  _roomRepository;
  private readonly ICustomerRepository  _customerRepository;
  private readonly IServiceRepository _serviceRepository;
  public ReservationHandler(IReservationRepository repository, IRoomRepository roomRepository,ICustomerRepository customerRepository, IServiceRepository serviceRepository)
  {
    _repository = repository;
    _roomRepository = roomRepository;
    _customerRepository = customerRepository;
    _serviceRepository = serviceRepository;
  }


  public async Task<Response> HandleCreateAsync(CreateReservation model)
  {
    var room = await _roomRepository.GetEntityByIdAsync(model.RoomId);
    if (room == null)
      throw new ArgumentException("Hospedagem não encontrada.");

    //Buscar todos os Customers através dos IDs passados
    var customers = new List<Customer>(
      await _customerRepository.GetCustomersByListId(model.Customers)
    );

    var reservation = new Reservation(room,model.CheckIn,customers,model.CheckOut);

    await _repository.CreateAsync(reservation);
    await _repository.SaveChangesAsync();

    return new Response(200,"Reserva criada com sucesso!",new { reservation.Id });
  }
}