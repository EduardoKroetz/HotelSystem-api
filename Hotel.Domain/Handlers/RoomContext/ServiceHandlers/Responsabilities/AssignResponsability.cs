﻿using Hotel.Domain.DTOs;

namespace Hotel.Domain.Handlers.RoomContext.ServiceHandler;

public partial class ServiceHandler
{
  public async Task<Response> HandleAssignResponsibilityAsync(Guid id, Guid responsibilityId)
  {
    var service = await _repository.GetServiceIncludeResponsabilities(id);
    if (service == null)
      throw new ArgumentException("Serviço não encontrado.");

    var responsibility = await _responsibilityRepository.GetEntityByIdAsync(responsibilityId);
    if (responsibility == null)
      throw new ArgumentException("Responsabilidade não encontrada.");

    service.AddResponsibility(responsibility);

    await _repository.SaveChangesAsync();

    return new Response(200, "Responsabilidade atribuida com sucesso!");
  }
}