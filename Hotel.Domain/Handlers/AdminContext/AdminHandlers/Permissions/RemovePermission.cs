﻿
using Hotel.Domain.DTOs;
using Hotel.Domain.Services.Permissions;

namespace Hotel.Domain.Handlers.AdminContext.AdminHandlers;
partial class AdminHandler
{
  public async Task<Response> HandleRemovePermission(Guid adminId, Guid permissionId)
  {
    //Buscar admin
    var admin = await _repository.GetAdminIncludePermissions(adminId);
    if (admin == null)
      throw new ArgumentException("Administrador não encontrado.");

    //Buscar permissão
    var permission = await _permissionRepository.GetEntityByIdAsync(permissionId);
    if (permission == null)
      throw new ArgumentException("Permissão não encontrada.");

    //Faz verificação se a permissão a ser removida é uma permissão padrão. Se for, vai remover 'DefaultAdminPermissions'
    //e adicionar todas as permissões padrões menos a removida
    await DefaultAdminPermissions.HandleDefaultPermission(permission ,admin, _repository);

    admin.RemovePermission(permission);

    await _repository.SaveChangesAsync();

    return new Response(200, "Permissão removida! Faça login novamente para aplicar as alterações.",null!);
  }
}
