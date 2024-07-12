﻿using Hotel.Domain.DTOs;
using Hotel.Domain.Exceptions;
using Hotel.Domain.ValueObjects;
using Stripe;

namespace Hotel.Domain.Handlers.CustomerHandlers;

public partial class CustomerHandler
{
    public new async Task<Response> HandleUpdateNameAsync(Guid id, Name name)
    {
        var transaction = await _repository.BeginTransactionAsync();

        try
        {
            var customer = await _repository.GetEntityByIdAsync(id)
                ?? throw new NotFoundException("Usuário não encontrado");

            customer.ChangeName(name);

            await _repository.SaveChangesAsync();

            try
            {
                await _stripeService.UpdateCustomerAsync(customer.StripeCustomerId, customer.Name, customer.Email, customer.Phone, customer.Address);
            }
            catch (StripeException)
            {
                _logger.LogError("Erro ao atualizar cliente no stripe");
                throw new StripeException("Ocorreu um erro ao atualizar o cliente no Stripe");
            }

            await transaction.CommitAsync();

            return new Response("Nome atualizado com sucesso!");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }


    }
}