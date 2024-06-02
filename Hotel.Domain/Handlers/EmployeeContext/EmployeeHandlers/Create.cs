using Hotel.Domain.DTOs;
using Hotel.Domain.DTOs.EmployeeContext.EmployeeDTOs;
using Hotel.Domain.Entities.AdminContext.AdminEntity;
using Hotel.Domain.Entities.EmployeeContext.EmployeeEntity;
using Hotel.Domain.Exceptions;
using Hotel.Domain.Handlers.Base.GenericUserHandler;
using Hotel.Domain.Handlers.Interfaces;
using Hotel.Domain.Repositories.Interfaces.AdminContext;
using Hotel.Domain.Repositories.Interfaces.EmployeeContext;
using Hotel.Domain.Services.EmailServices;
using Hotel.Domain.Services.EmailServices.Interface;
using Hotel.Domain.Services.Permissions;
using Hotel.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Domain.Handlers.EmployeeContext.EmployeeHandlers;

public partial class EmployeeHandler : GenericUserHandler<IEmployeeRepository,Employee> ,IHandler
{
  private readonly IEmployeeRepository  _repository;
  private readonly IResponsabilityRepository _responsabilityRepository;
  private readonly IPermissionRepository _permissionRepository;
  private readonly IEmailService _emailService;

  public EmployeeHandler(IEmployeeRepository repository, IResponsabilityRepository responsabilityRepository, IPermissionRepository permissionRepository, IEmailService emailService) : base(repository)
  {
    _repository = repository;
    _responsabilityRepository = responsabilityRepository;
    _permissionRepository = permissionRepository;
    _emailService = emailService;
  }

  public async Task<Response> HandleCreateAsync(CreateEmployee model, string? code)
  {
    var email = new Email(model.Email);
    var response = await _emailService.VerifyEmailCodeAsync(email ,code);
    if (response.Status != 200)
      return response;

    DefaultEmployeePermissions.DefaultPermission = DefaultEmployeePermissions.DefaultPermission ?? await _repository.GetDefaultPermission() ?? throw new NotFoundException("Permissão padrão não encontrada.");

    var employee = new Employee(
      new Name(model.FirstName,model.LastName),
      email,
      new Phone(model.Phone),
      model.Password,
      model.Gender,
      model.DateOfBirth,
      new Address(model.Country,model.City,model.Street,model.Number),
      model.Salary,
      [DefaultEmployeePermissions.DefaultPermission!]
    );

    try
    {
      await _repository.CreateAsync(employee);
      await _repository.SaveChangesAsync();
    }
    catch (DbUpdateException e)
    {
      var innerException = e.InnerException?.Message;

      if (innerException != null)
      {

        if (innerException.Contains("Email"))
          return new Response(400, "Esse email já está cadastrado.");

        if (innerException.Contains("Phone"))
          return new Response(400, "Esse telefone já está cadastrado.");
      }
    }


    return new Response(200,"Funcionário cadastrado com sucesso!",new { employee.Id });
  }
}