using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using MBC.Endpoints.Endpoints;
using MBC.Endpoints.Mapping;
using Moq;
using Xunit;

namespace MBC.Endpoints.Tests;

public class CustomerEndpointsTests
{
    private static Customer CreateCustomer()
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            Nickname = "Test Customer",
            UserId = Guid.NewGuid()
        };
    }

    private static CustomerDto CreateCustomerDto()
    {
        return new CustomerDto
        {
            Id = Guid.NewGuid(),
            Nickname = "Test Customer"
        };
    }

    private static Mock<ICurrentUser> CreateMockCurrentUser(string role, Guid? customerId = null)
    {
        var claims = new[] { new Claim(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(x => x.User).Returns(principal);
        mockCurrentUser.Setup(x => x.CustomerId).Returns(customerId);
        return mockCurrentUser;
    }

    [Fact]
    public async Task GetCustomer_WhenNotFound_ReturnsNotFound()
    {
        var customerId = Guid.NewGuid();
        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Administrator);

        var mockCustomerService = new Mock<ICustomerService>();
        mockCustomerService.Setup(x => x.GetCustomerById(customerId)).ReturnsAsync((Customer?)null);

        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();

        var result = await CustomerEndpoints.GetCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId);

        var notFoundResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_AsAdmin_ReturnsOkWithDto()
    {
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer();
        customer.Id = customerId;
        var customerDto = CreateCustomerDto();
        customerDto = customerDto with { Id = customerId };

        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Administrator);

        var mockCustomerService = new Mock<ICustomerService>();
        mockCustomerService.Setup(x => x.GetCustomerById(customerId)).ReturnsAsync(customer);

        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();
        mockMapper.Setup(x => x.Map(customer)).Returns(customerDto);

        var result = await CustomerEndpoints.GetCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<CustomerDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(customerDto, okResult.Value);
    }

    [Fact]
    public async Task GetCustomer_AsPilot_ReturnsOkWithDto()
    {
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer();
        customer.Id = customerId;
        var customerDto = CreateCustomerDto();
        customerDto = customerDto with { Id = customerId };

        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Pilot);

        var mockCustomerService = new Mock<ICustomerService>();
        mockCustomerService.Setup(x => x.GetCustomerById(customerId)).ReturnsAsync(customer);

        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();
        mockMapper.Setup(x => x.Map(customer)).Returns(customerDto);

        var result = await CustomerEndpoints.GetCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<CustomerDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(customerDto, okResult.Value);
    }

    [Fact]
    public async Task GetCustomer_AsCustomerViewingSelf_ReturnsOkWithDto()
    {
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer();
        customer.Id = customerId;
        var customerDto = CreateCustomerDto();
        customerDto = customerDto with { Id = customerId };

        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, customerId);

        var mockCustomerService = new Mock<ICustomerService>();
        mockCustomerService.Setup(x => x.GetCustomerById(customerId)).ReturnsAsync(customer);

        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();
        mockMapper.Setup(x => x.Map(customer)).Returns(customerDto);

        var result = await CustomerEndpoints.GetCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<CustomerDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(customerDto, okResult.Value);
    }

    [Fact]
    public async Task GetCustomer_AsCustomerViewingOther_ReturnsNotFound()
    {
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();

        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, otherCustomerId);

        var mockCustomerService = new Mock<ICustomerService>();
        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();

        var result = await CustomerEndpoints.GetCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId);

        var notFoundResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_AsAdmin_ReturnsOkWithDto()
    {
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer();
        customer.Id = customerId;
        var customerDto = CreateCustomerDto();
        customerDto = customerDto with { Id = customerId };
        var request = new UpdateCustomerRequest { Nickname = "Updated Nickname" };

        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Administrator);

        var mockCustomerService = new Mock<ICustomerService>();
        mockCustomerService.Setup(x => x.UpdateCustomer(customerId, request.Nickname)).ReturnsAsync(customer);

        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();
        mockMapper.Setup(x => x.Map(customer)).Returns(customerDto);

        var result = await CustomerEndpoints.UpdateCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId,
            request);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<CustomerDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(customerDto, okResult.Value);
    }

    [Fact]
    public async Task UpdateCustomer_AsCustomerUpdatingSelf_ReturnsOkWithDto()
    {
        var customerId = Guid.NewGuid();
        var customer = CreateCustomer();
        customer.Id = customerId;
        var customerDto = CreateCustomerDto();
        customerDto = customerDto with { Id = customerId };
        var request = new UpdateCustomerRequest { Nickname = "Updated Nickname" };

        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, customerId);

        var mockCustomerService = new Mock<ICustomerService>();
        mockCustomerService.Setup(x => x.UpdateCustomer(customerId, request.Nickname)).ReturnsAsync(customer);

        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();
        mockMapper.Setup(x => x.Map(customer)).Returns(customerDto);

        var result = await CustomerEndpoints.UpdateCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId,
            request);

        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<CustomerDto>>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(customerDto, okResult.Value);
    }

    [Fact]
    public async Task UpdateCustomer_AsCustomerUpdatingOther_ReturnsNotFound()
    {
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var request = new UpdateCustomerRequest { Nickname = "Updated Nickname" };

        var mockCurrentUser = CreateMockCurrentUser(UserRoles.Customer, otherCustomerId);

        var mockCustomerService = new Mock<ICustomerService>();
        var mockMapper = new Mock<IMapper<Customer, CustomerDto>>();

        var result = await CustomerEndpoints.UpdateCustomer(
            mockCustomerService.Object,
            mockCurrentUser.Object,
            mockMapper.Object,
            customerId,
            request);

        var notFoundResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}

