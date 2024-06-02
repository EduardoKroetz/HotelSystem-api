﻿using Hotel.Domain.DTOs;
using Hotel.Domain.ValueObjects;

namespace Hotel.Domain.Handlers.Base.GenericUserHandler;

public partial class GenericUserHandler<TRepository, TUser>
{
  public async Task<Response> HandleUpdateDateOfBirthAsync(Guid userId, DateTime? dateOfBirth)
  {
    var user = await _repository.GetEntityByIdAsync(userId);
    if (user == null)
      throw new ArgumentException("Não foi possível encontrar o usuário.");

    user.ChangeDateOfBirth(dateOfBirth);

    await _repository.SaveChangesAsync();

    return new Response(200, "Data de nascimento atualizada.");
  }
}