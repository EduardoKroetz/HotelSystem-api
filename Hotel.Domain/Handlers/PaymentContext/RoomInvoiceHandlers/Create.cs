using Hotel.Domain.DTOs;
using Hotel.Domain.DTOs.PaymentContext.RoomInvoiceDTOs;
using Hotel.Domain.Entities.PaymentContext.InvoiceRoomEntity;
using Hotel.Domain.Handlers.Interfaces;
using Hotel.Domain.Repositories.Interfaces;


namespace Hotel.Domain.Handlers.PaymentContext.RoomInvoiceHandlers;

public partial class RoomInvoiceHandler : IHandler
{
  private readonly IRoomInvoiceRepository  _repository;
  private readonly IReservationRepository  _reservationRepository;
  public RoomInvoiceHandler(IRoomInvoiceRepository repository, IReservationRepository reservationRepository)
  {
    _repository = repository;
    _reservationRepository = reservationRepository;
  }
  

  public async Task<Response<object>> HandleCreateAsync(CreateRoomInvoice model)
  {
    var reservation = await _reservationRepository.GetEntityByIdAsync(model.ReservationId);
    if (reservation == null)
      throw new ArgumentException("Reserva não encontrada.");
    var roomInvoice = new RoomInvoice(model.PaymentMethod,reservation,model.TaxInformation);

    await _repository.CreateAsync(roomInvoice);
    await _repository.SaveChangesAsync();

    return new Response<object>(200,"Fatura de quarto gerada.",new { reservation.Id });
  }
}