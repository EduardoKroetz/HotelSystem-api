﻿using Hotel.Domain.DTOs;
using Hotel.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace Hotel.Domain.Handlers.ReservationHandlers;

public partial class ReservationHandler
{
    public async Task<Response> HandleUpdateExpectedCheckInAsync(Guid id, DateTime expectedCheckIn)
    {
        var transaction = await _repository.BeginTransactionAsync();

        try
        {
            var reservation = await _repository.GetEntityByIdAsync(id)
                ?? throw new NotFoundException("Reserva não encontrada.");

            reservation.UpdateExpectedCheckIn(expectedCheckIn);

            try
            {
                await _repository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException("Ocorreu um erro ao atualizar a reserva no banco de dados");
            }

            try
            {
                await _stripeService.UpdatePaymentIntentAsync(reservation.StripePaymentIntentId, reservation.ExpectedTotalAmount());
            }
            catch (StripeException)
            {
                throw new StripeException("Ocorreu um erro ao atualizar o PaymentIntent no Stripe");
            }

            await transaction.CommitAsync();

            return new Response("CheckIn esperado atualizado com sucesso!");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}