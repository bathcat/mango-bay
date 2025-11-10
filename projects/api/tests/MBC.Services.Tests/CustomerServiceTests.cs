using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MBC.Core;
using MBC.Core.Entities;
using MBC.Core.Persistence;
using MBC.Services.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MBC.Services.Tests;

public class CustomerServiceTests
{
    private static CustomerService CreateService(
        ICustomerStore? customerStore = null,
        ICurrentUser? currentUser = null)
    {
        return new CustomerService(
            customerStore ?? Mock.Of<ICustomerStore>(),
            currentUser ?? Mock.Of<ICurrentUser>(),
            NullLogger<CustomerService>.Instance);
    }

    private static ICurrentUser CreateMockCurrentUser(string role, Guid userId, Guid? customerId = null)
    {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, role)],
            "mock"));

        var mockCurrentUser = new Mock<ICurrentUser>();
        mockCurrentUser.Setup(x => x.User).Returns(claimsPrincipal);
        mockCurrentUser.Setup(x => x.UserId).Returns(userId);
        mockCurrentUser.Setup(x => x.CustomerId).Returns(customerId);
        mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
        mockCurrentUser.Setup(x => x.Role).Returns(role);

        return mockCurrentUser.Object;
    }

    [Fact]
    public async Task UpdateCustomer_AsAdmin_UpdatesAnyCustomer()
    {
        var customerId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Nickname = "OldNickname",
            UserId = Guid.NewGuid()
        };

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetById(customerId)).ReturnsAsync(customer);
        mockStore.Setup(x => x.Update(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);

        var currentUser = CreateMockCurrentUser(UserRoles.Administrator, adminUserId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.UpdateCustomer(customerId, "NewNickname");

        Assert.NotNull(result);
        Assert.Equal("NewNickname", result.Nickname);
        mockStore.Verify(x => x.Update(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_AsCustomer_UpdatesSelf()
    {
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Nickname = "OldNickname",
            UserId = userId
        };

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetById(customerId)).ReturnsAsync(customer);
        mockStore.Setup(x => x.Update(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) => c);

        var currentUser = CreateMockCurrentUser(UserRoles.Customer, userId, customerId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.UpdateCustomer(customerId, "NewNickname");

        Assert.NotNull(result);
        Assert.Equal("NewNickname", result.Nickname);
        mockStore.Verify(x => x.Update(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_AsCustomer_CannotUpdateOther()
    {
        var targetCustomerId = Guid.NewGuid();
        var currentCustomerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var currentUser = CreateMockCurrentUser(UserRoles.Customer, userId, currentCustomerId);
        var service = CreateService(currentUser: currentUser);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateCustomer(targetCustomerId, "NewNickname"));

        Assert.Contains("not authorized", exception.Message);
    }

    [Fact]
    public async Task UpdateCustomer_CustomerNotFound_ThrowsInvalidOperationException()
    {
        var customerId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetById(customerId)).ReturnsAsync((Customer?)null);

        var currentUser = CreateMockCurrentUser(UserRoles.Administrator, adminUserId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateCustomer(customerId, "NewNickname"));

        Assert.Contains(customerId.ToString(), exception.Message);
    }

    [Fact]
    public async Task GetCustomerById_AsAdmin_GetsAnyCustomer()
    {
        var customerId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Nickname = "TestCustomer",
            UserId = Guid.NewGuid()
        };

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetById(customerId)).ReturnsAsync(customer);

        var currentUser = CreateMockCurrentUser(UserRoles.Administrator, adminUserId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.GetCustomerById(customerId);

        Assert.NotNull(result);
        Assert.Equal(customerId, result!.Id);
    }

    [Fact]
    public async Task GetCustomerById_AsCustomer_GetsSelf()
    {
        var customerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Nickname = "TestCustomer",
            UserId = userId
        };

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetById(customerId)).ReturnsAsync(customer);

        var currentUser = CreateMockCurrentUser(UserRoles.Customer, userId, customerId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.GetCustomerById(customerId);

        Assert.NotNull(result);
        Assert.Equal(customerId, result!.Id);
    }

    [Fact]
    public async Task GetCustomerById_AsCustomer_CannotGetOther()
    {
        var targetCustomerId = Guid.NewGuid();
        var currentCustomerId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var currentUser = CreateMockCurrentUser(UserRoles.Customer, userId, currentCustomerId);
        var service = CreateService(currentUser: currentUser);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetCustomerById(targetCustomerId));

        Assert.Contains("not authorized", exception.Message);
    }

    [Fact]
    public async Task GetCustomerById_NotFound_ReturnsNull()
    {
        var customerId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetById(customerId)).ReturnsAsync((Customer?)null);

        var currentUser = CreateMockCurrentUser(UserRoles.Administrator, adminUserId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.GetCustomerById(customerId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerByUserId_AsAdmin_GetsAnyCustomer()
    {
        var userId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Nickname = "TestCustomer",
            UserId = userId
        };

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetByUserId(userId)).ReturnsAsync(customer);

        var currentUser = CreateMockCurrentUser(UserRoles.Administrator, adminUserId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.GetCustomerByUserId(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result!.UserId);
    }

    [Fact]
    public async Task GetCustomerByUserId_AsCustomer_GetsSelf()
    {
        var userId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            Nickname = "TestCustomer",
            UserId = userId
        };

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetByUserId(userId)).ReturnsAsync(customer);

        var currentUser = CreateMockCurrentUser(UserRoles.Customer, userId, customerId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.GetCustomerByUserId(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result!.UserId);
    }

    [Fact]
    public async Task GetCustomerByUserId_AsCustomer_CannotGetOther()
    {
        var targetUserId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var currentUser = CreateMockCurrentUser(UserRoles.Customer, currentUserId, customerId);
        var service = CreateService(currentUser: currentUser);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetCustomerByUserId(targetUserId));

        Assert.Contains("not authorized", exception.Message);
    }

    [Fact]
    public async Task GetCustomerByUserId_NotFound_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();

        var mockStore = new Mock<ICustomerStore>();
        mockStore.Setup(x => x.GetByUserId(userId)).ReturnsAsync((Customer?)null);

        var currentUser = CreateMockCurrentUser(UserRoles.Administrator, adminUserId);
        var service = CreateService(customerStore: mockStore.Object, currentUser: currentUser);

        var result = await service.GetCustomerByUserId(userId);

        Assert.Null(result);
    }
}


