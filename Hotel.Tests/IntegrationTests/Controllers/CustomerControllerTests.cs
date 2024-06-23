﻿using Hotel.Domain.Data;
using Hotel.Domain.DTOs.Base.User;
using Hotel.Domain.Entities.VerificationCodeEntity;
using Hotel.Domain.Enums;
using Hotel.Domain.ValueObjects;
using Hotel.Tests.IntegrationTests.Factories;
using Hotel.Tests.IntegrationTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Stripe;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Hotel.Tests.IntegrationTests.Controllers;

[TestClass]
public class CustomerControllerTests
{
    private static HotelWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;
    private static HotelDbContext _dbContext = null!;
    private static string _rootAdminToken = null!;
    private const string _baseUrl = "v1/customers";
    private static Domain.Services.TokenServices.TokenService _tokenService = null!;
    private static CustomerService _stripeCustomerService = new CustomerService();

    [ClassInitialize]
    public static void ClassInitialize(TestContext? context)
    {
        _factory = new HotelWebApplicationFactory();
        _client = _factory.CreateClient();
        _dbContext = _factory.Services.GetRequiredService<HotelDbContext>();
        _tokenService = _factory.Services.GetRequiredService<Domain.Services.TokenServices.TokenService>();


        _rootAdminToken = _factory.LoginFullAccess().Result;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _rootAdminToken);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _factory.Dispose();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _rootAdminToken);
    }

    [TestMethod]
    public async Task GetCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Jennifer", "Lawrence"),
          new Email("jenniferLawrenceOfficial@gmail.com"),
          new Phone("+44 (20) 97890-1234"),
          "789",
          EGender.Feminine,
          DateTime.Now.AddYears(-30),
          new Domain.ValueObjects.Address("United States", "Los Angeles", "US-456", 789)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        //Act
        var response = await _client.GetAsync($"{_baseUrl}?take=1");

        //Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetCustomerById_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Ana", "Souza"),
          new Email("anaSouzaOfficial@gmail.com"),
          new Phone("+55 (31) 91234-5678"),
          "789",
          EGender.Feminine,
          DateTime.Now.AddYears(-28),
          new Domain.ValueObjects.Address("Brazil", "Belo Horizonte", "BR-123", 789)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        //Act
        var response = await _client.GetAsync($"{_baseUrl}/{customer.Id}");

        //Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteCustomer_ShouldReturn_OK()
    {
        //Arrange
        var newCustomer = new CreateUser
        (
            "Maria", "Silva",
            "mariaSilva@gmail.com",
            "+55 (11) 98765-1234",
            "password123",
            EGender.Feminine,
            DateTime.Now.AddYears(-25),
            "Brazil", "São Paulo", "SP-101", 101
        );

        var verificationCode = new VerificationCode(new Email(newCustomer.Email));
        await _dbContext.VerificationCodes.AddAsync(verificationCode);
        await _dbContext.SaveChangesAsync();

        var createCustomerResponse = await _client.PostAsJsonAsync($"v1/register/customers?code={verificationCode.Code}", newCustomer);
        var createCustomerContent = JsonConvert.DeserializeObject<Response<DataStripeCustomerId>>(await createCustomerResponse.Content.ReadAsStringAsync())!;
        var customer = await _dbContext.Customers.FirstAsync(x => x.Id == createCustomerContent.Data.Id);

        //Act
        var response = await _client.DeleteAsync($"{_baseUrl}/{customer.Id}");

        //Assert
        var exists = await _dbContext.Customers.AnyAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsFalse(exists);

        var content = JsonConvert.DeserializeObject<Response<DataStripeCustomerId>>(await response.Content.ReadAsStringAsync())!;
        Assert.AreEqual("Usuário deletado com sucesso!", content.Message);

        var stripeCustomer = await _stripeCustomerService.GetAsync(content.Data.StripeCustomerId);
        Assert.IsTrue(stripeCustomer.Deleted);
    }

    [TestMethod]
    public async Task DeleteLoggedCustomer_ShouldReturn_OK()
    {
        //Arrange
        var newCustomer = new CreateUser
        (
            "Emma", "Watson",
            "emmaWatson@gmail.com",
            "+44 (20) 99346-1912",
            "123",
            EGender.Feminine,
            DateTime.Now.AddYears(-31),
            "United Kingdom", "London", "UK-123", 456
        );

        var verificationCode = new VerificationCode(new Email(newCustomer.Email));
        await _dbContext.VerificationCodes.AddAsync(verificationCode);
        await _dbContext.SaveChangesAsync();

        var createCustomerResponse = await _client.PostAsJsonAsync($"v1/register/customers?code={verificationCode.Code}", newCustomer);
        var createCustomerContent = JsonConvert.DeserializeObject<Response<DataStripeCustomerId>>(await createCustomerResponse.Content.ReadAsStringAsync())!;
        var customer = await _dbContext.Customers.FirstAsync(x => x.Id == createCustomerContent.Data.Id);

        _factory.Login(_client, customer);

        //Act
        var response = await _client.DeleteAsync(_baseUrl);

        //Assert
        var exists = await _dbContext.Customers.AnyAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsFalse(exists);

        var content = JsonConvert.DeserializeObject<Response<DataStripeCustomerId>>(await response.Content.ReadAsStringAsync())!;

        var stripeCustomer = await _stripeCustomerService.GetAsync(content.Data.StripeCustomerId);
        Assert.IsTrue(stripeCustomer.Deleted);
    }

    [TestMethod]
    public async Task DeleteCustomer_WithStripeServiceError_ShouldReturn_BAD_REQUEST_AND_MAKE_ROLLBACK()
    {
        //Arrange
        var factory = new HotelWebApplicationFactory();
        var dbContext = factory.Services.GetRequiredService<HotelDbContext>();
        var client = factory.CreateClient();

        var newCustomer = new CreateUser
        (
            "Pedro", "Souza",
            "pedroSouza@gmail.com",
            "+55 (31) 98765-5678",
            "password456",
            EGender.Masculine,
            DateTime.Now.AddYears(-28),
            "Brazil", "Belo Horizonte", "MG-303", 303
        );

        var verificationCode = new VerificationCode(new Email(newCustomer.Email));
        await dbContext.VerificationCodes.AddAsync(verificationCode);
        await dbContext.SaveChangesAsync();

        var createCustomerResponse = await client.PostAsJsonAsync($"v1/register/customers?code={verificationCode.Code}", newCustomer);
        var createCustomerContent = JsonConvert.DeserializeObject<Response<DataStripeCustomerId>>(await createCustomerResponse.Content.ReadAsStringAsync())!;
        var customer = await dbContext.Customers.FirstAsync(x => x.Id == createCustomerContent.Data.Id);

        factory.Login(client, _rootAdminToken);

        var apiKey = StripeConfiguration.ApiKey.ToString();
        StripeConfiguration.ApiKey = "";
        //Act
        var response = await client.DeleteAsync($"{_baseUrl}/{customer.Id}");

        //Assert
        var exists = await dbContext.Customers.AnyAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsTrue(exists);

        var content = JsonConvert.DeserializeObject<Response<object>>(await response.Content.ReadAsStringAsync())!;
        Assert.AreEqual("Ocorreu um erro ao deletar o cliente no Stripe", content.Errors[0]);

        StripeConfiguration.ApiKey = apiKey;
        var stripeCustomer = await _stripeCustomerService.GetAsync(createCustomerContent.Data.StripeCustomerId);
        Assert.IsNull(stripeCustomer.Deleted);
    }

    [TestMethod]
    public async Task UpdateCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Rafael", "Oliveira"),
          new Email("rafaelOliveira@gmail.com"),
          new Phone("+55 (41) 97654-3210"),
          "password4",
          EGender.Masculine,
          DateTime.Now.AddYears(-32),
          new Domain.ValueObjects.Address("Brazil", "Curitiba", "PR-404", 404)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var body = new UpdateUser("Jão", "Pedro", "+55 (41) 97654-3210", EGender.Feminine, DateTime.Now.AddYears(-20), "Brazil", "Curitiba", "PR-404", 404);

        //Act
        var response = await _client.PutAsJsonAsync($"{_baseUrl}/{customer.Id}", body);

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(updatedCustomer!.Id, customer.Id);
        Assert.AreEqual(updatedCustomer.Name.FirstName, body.FirstName);
        Assert.AreEqual(updatedCustomer.Name.LastName, body.LastName);
        Assert.AreEqual(updatedCustomer.Phone.Number, body.Phone);
        Assert.AreEqual(updatedCustomer.Gender, body.Gender);
        Assert.AreEqual(updatedCustomer.DateOfBirth, body.DateOfBirth);
        Assert.AreEqual(updatedCustomer!.Address!.Country, body.Country);
        Assert.AreEqual(updatedCustomer!.Address.City, body.City);
        Assert.AreEqual(updatedCustomer!.Address!.Number, body.Number);
        Assert.AreEqual(updatedCustomer!.Address.Street, body.Street);
    }


    [TestMethod]
    public async Task UpdateLoggedCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Camila", "Costa"),
          new Email("camilaCosta@gmail.com"),
          new Phone("+55 (71) 93456-7890"),
          "password5",
          EGender.Feminine,
          DateTime.Now.AddYears(-29),
          new Domain.ValueObjects.Address("Brazil", "Salvador", "BA-505", 505)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new UpdateUser("Jão", "Pedro", "+55 (41) 93651-3210", EGender.Feminine, DateTime.Now.AddYears(-20), "Brazil", "Curitiba", "PR-404", 404);

        //Act
        var response = await _client.PutAsJsonAsync(_baseUrl, body);

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(updatedCustomer!.Id, customer.Id);
        Assert.AreEqual(updatedCustomer.Name.FirstName, body.FirstName);
        Assert.AreEqual(updatedCustomer.Name.LastName, body.LastName);
        Assert.AreEqual(updatedCustomer.Phone.Number, body.Phone);
        Assert.AreEqual(updatedCustomer.Gender, body.Gender);
        Assert.AreEqual(updatedCustomer.DateOfBirth, body.DateOfBirth);
        Assert.AreEqual(updatedCustomer!.Address!.Country, body.Country);
        Assert.AreEqual(updatedCustomer!.Address.City, body.City);
        Assert.AreEqual(updatedCustomer!.Address!.Number, body.Number);
        Assert.AreEqual(updatedCustomer!.Address.Street, body.Street);
    }

    [TestMethod]
    public async Task UpdateCustomerName_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Lucas", "Ferreira"),
          new Email("lucasFerreira@gmail.com"),
          new Phone("+55 (61) 92345-6789"),
          "password6",
          EGender.Masculine,
          DateTime.Now.AddYears(-28),
          new Domain.ValueObjects.Address("Brazil", "Brasília", "DF-606", 606)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new Name("John", "Wick");

        //Act
        var response = await _client.PatchAsJsonAsync($"{_baseUrl}/name", body);

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(updatedCustomer!.Name.FirstName, body.FirstName);
        Assert.AreEqual(updatedCustomer.Name.LastName, body.LastName);
    }

    [TestMethod]
    public async Task UpdateEmailCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Fernanda", "River"),
          new Email("fernandaRiver@gmail.com"),
          new Phone("+55 (51) 91219-5678"),
          "password7",
          EGender.Feminine,
          DateTime.Now.AddYears(-26),
          new Domain.ValueObjects.Address("Brazil", "Porto Alegre", "RS-707", 707)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new Email("feeRriber@gmail.com");

        //Act
        var response = await _client.PatchAsJsonAsync($"{_baseUrl}/email", body);

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(updatedCustomer!.Email.Address, body.Address);
    }

    [TestMethod]
    public async Task UpdatePhoneCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Michele", "Silva"),
          new Email("micheleSilvaa100@gmail.com"),
          new Phone("+55 (62) 99846-1432"),
          "password8",
          EGender.Masculine,
          DateTime.Now.AddYears(-31),
          new Domain.ValueObjects.Address("Brazil", "Goiânia", "GO-808", 808)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new Phone("+55 (62) 99156-3449");

        //Act
        var response = await _client.PatchAsJsonAsync($"{_baseUrl}/phone", body);

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(updatedCustomer!.Phone.Number, body.Number);
    }

    [TestMethod]
    public async Task UpdateAddressCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Vinicius", "Silva"),
          new Email("viniSilva@gmail.com"),
          new Phone("+55 (62) 91876-3432"),
          "password8",
          EGender.Masculine,
          DateTime.Now.AddYears(-31),
          new Domain.ValueObjects.Address("Brazil", "Goiânia", "GO-808", 808)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new Domain.ValueObjects.Address("Brazil", "Florianópolis", "SC-909", 909);

        //Act
        var response = await _client.PatchAsJsonAsync($"{_baseUrl}/address", body);

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(updatedCustomer!.Address!.Country, body.Country);
        Assert.AreEqual(updatedCustomer!.Address.City, body.City);
        Assert.AreEqual(updatedCustomer!.Address!.Number, body.Number);
        Assert.AreEqual(updatedCustomer!.Address.Street, body.Street);
    }

    [TestMethod]
    public async Task UpdateGenderCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Gustavo", "Souza"),
          new Email("gustavoSouza@gmail.com"),
          new Phone("+55 (27) 93456-7890"),
          "password10",
          EGender.Masculine,
          DateTime.Now.AddYears(-33),
          new Domain.ValueObjects.Address("Brazil", "Vitória", "ES-1010", 1010)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        //Act
        var response = await _client.PatchAsJsonAsync($"{_baseUrl}/gender/2", new { });

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(EGender.Feminine, updatedCustomer!.Gender);
    }

    [TestMethod]
    public async Task UpdateDateOfBirthCustomer_ShouldReturn_OK()
    {
        //Arrange
        var customer = new Domain.Entities.CustomerEntity.Customer
        (
          new Name("Geovane", "Silva"),
          new Email("geoSilv@gmail.com"),
          new Phone("+55 (27) 93113-7859"),
          "password10",
          EGender.Masculine,
          DateTime.Now.AddYears(-33),
          new Domain.ValueObjects.Address("Brazil", "Vitória", "ES-1011", 1011)
        );

        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var token = _tokenService.GenerateToken(customer);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new UpdateDateOfBirth(DateTime.Now.AddYears(-35));

        //Act
        var response = await _client.PatchAsJsonAsync($"{_baseUrl}/date-of-birth", body);

        //Assert
        var updatedCustomer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);

        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(body.DateOfBirth, updatedCustomer!.DateOfBirth);
    }
}
