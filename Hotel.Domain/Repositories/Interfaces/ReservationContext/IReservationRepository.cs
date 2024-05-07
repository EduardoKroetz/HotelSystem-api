using Hotel.Domain.DTOs.PaymentContext.RoomInvoiceDTOs;
using Hotel.Domain.Entities.ReservationContext.ReservationEntity;

namespace Hotel.Domain.Repositories.Interfaces;

public interface IReservationRepository : IRepository<Reservation>,IRepositoryQuery<GetReservation,GetReservationCollection>
{
}