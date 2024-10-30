using FluentAssertions;
using NSubstitute;
using sessionSix.App.ObservableBehavior.Domain;
using sessionSix.App.ObservableBehavior.Services;
using Xunit;

namespace sessionSix.App.Tests.ObservableBehavior.Services;

// The following unit tests are not qualified enough
// There are several points that can be refactored
public class OrderServiceTests
{
    private readonly OrderService _sut;

    private readonly IStoreRepository _storeRepository;
    private readonly IDiscountRepository _discountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    private Order _expectedOrder;

    public OrderServiceTests()
    {
        _storeRepository = Substitute.For<IStoreRepository>();
        _discountRepository = Substitute.For<IDiscountRepository>();
        _customerRepository = Substitute.For<ICustomerRepository>();
        _productRepository = Substitute.For<IProductRepository>();
        _orderRepository = Substitute.For<IOrderRepository>();

        _sut = new OrderService(_storeRepository, _discountRepository,
            _customerRepository, _productRepository, _orderRepository);
    }

    [Fact]
    public void Order_is_created_successfully()
    {
        //Arrange
        var request = new CreateOrderRequest
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            StoreId = Guid.NewGuid().ToString(),
            DiscountCode = Guid.NewGuid().ToString(),
            Products =
            [
                new ProductRequestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 10
                },

                new ProductRequestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 10
                }
            ]
        };
        var store = new Store
        {
            Id = request.Id,
            IsActive = true
        };
        _storeRepository.GetBy(request.StoreId).Returns(store);

        var discount = new Discount
        {
            Code = request.DiscountCode,
            IsActive = true
        };
        _discountRepository.GetBy(discount.Code).Returns(discount);

        var customer = new Customer
        {
            Id = request.Id,
            IsActive = true
        };
        _customerRepository.GetBy(request.CustomerId).Returns(customer);

        var products = new List<Product>();
        foreach (var productRequestItem in request.Products)
        {
            var product = new Product
            {
                Id = productRequestItem.Id,
                Price = 100
            };
            products.Add(product);
            _productRepository.GetBy(productRequestItem.Id).Returns(product);
        }

        _expectedOrder = new Order
        {
            Id = request.Id,
            Store = store,
            Discount = discount,
            Customer = customer,
            Products = products
        };

        //Act
        var actual = _sut.CreateOrder(request);

        //Assert
        actual.Should().BeEquivalentTo(_expectedOrder);
        _orderRepository.Received(1).Add(Arg.Any<Order>());
    }

    [Fact]
    public void Order_is_modified_successfully()
    {
        //Arrange
        var request = new ModifyOrderRequest
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            StoreId = Guid.NewGuid().ToString(),
            DiscountCode = Guid.NewGuid().ToString(),
            Products =
            [
                new ProductRequestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 10
                },

                new ProductRequestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 10
                }
            ]
        };

        _expectedOrder = new Order
        {
            Id = request.Id,
        };
        _orderRepository.GetBy(request.Id).Returns(_expectedOrder);

        var store = new Store
        {
            Id = request.Id,
            IsActive = true
        };
        _storeRepository.GetBy(request.StoreId).Returns(store);

        var discount = new Discount
        {
            Code = request.DiscountCode,
            IsActive = true
        };
        _discountRepository.GetBy(discount.Code).Returns(discount);

        var customer = new Customer
        {
            Id = request.Id,
            IsActive = true
        };
        _customerRepository.GetBy(request.CustomerId).Returns(customer);

        var products = new List<Product>();
        foreach (var productRequestItem in request.Products)
        {
            var product = new Product
            {
                Id = productRequestItem.Id,
                Price = 100
            };
            products.Add(product);
            _productRepository.GetBy(productRequestItem.Id).Returns(product);
        }

        _expectedOrder = new Order
        {
            Id = request.Id,
            Store = store,
            Discount = discount,
            Customer = customer,
            Products = products
        };

        //Act
        var actual = _sut.UpdateOrder(request);

        //Assert
        actual.Should().BeEquivalentTo(_expectedOrder);
        _orderRepository.Received(1).Add(Arg.Any<Order>());
    }

    [Fact]
    public void Order_is_created_only_for_active_store()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Id = Guid.NewGuid().ToString(),
            StoreId = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            Products =
            [
                new ProductRequestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            ]
        };

        var store = new Store
        {
            Id = request.StoreId, 
            IsActive = false
        };
        _storeRepository.GetBy(request.StoreId).Returns(store);

        var customer = new Customer
        {
            Id = request.CustomerId, 
            IsActive = true
        };
        _customerRepository.GetBy(request.CustomerId).Returns(customer);

        // Act & Assert
        Assert.Throws<Exception>(() => _sut.CreateOrder(request))
            .Message.Should().Be("Store is deActivated");
    }

    [Fact]
    public void Order_is_created_only_for_active_customer()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Id = Guid.NewGuid().ToString(),
            StoreId = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            Products =
            [
                new ProductRequestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            ]
        };

        var store = new Store
        {
            Id = request.StoreId, 
            IsActive = true
        };
        _storeRepository.GetBy(request.StoreId).Returns(store);

        var customer = new Customer
        {
            Id = request.CustomerId, 
            IsActive = false
        };
        _customerRepository.GetBy(request.CustomerId).Returns(customer);

        // Act & Assert
        Assert.Throws<Exception>(() => _sut.CreateOrder(request))
            .Message.Should().Be("Customer is deActivated");
    }

    [Fact]
    public void Order_is_created_with_atLeast_one_product()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Id = Guid.NewGuid().ToString(),
            StoreId = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            Products = new List<ProductRequestItem>()
        };

        var store = new Store
        {
            Id = request.StoreId, 
            IsActive = true
        };
        _storeRepository.GetBy(request.StoreId).Returns(store);

        var customer = new Customer
        {
            Id = request.CustomerId, 
            IsActive = true
        };
        _customerRepository.GetBy(request.CustomerId).Returns(customer);

        // Act & Assert
        Assert.Throws<Exception>(() => _sut.CreateOrder(request))
            .Message.Should().Be("AtLeast one product is required.");
    }

    [Fact]
    public void Order_is_created_having_only_active_discountCode()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            Id = Guid.NewGuid().ToString(),
            StoreId = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString(),
            DiscountCode = "DISCOUNT123",
            Products =
            [
                new ProductRequestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            ]
        };

        var store = new Store
        {
            Id = request.StoreId, 
            IsActive = true
        };
        _storeRepository.GetBy(request.StoreId).Returns(store);

        var customer = new Customer
        {
            Id = request.CustomerId, 
            IsActive = true
        };
        _customerRepository.GetBy(request.CustomerId).Returns(customer);

        var discount = new Discount
        {
            Code = request.DiscountCode, 
            IsActive = false
        };
        _discountRepository.GetBy(request.DiscountCode).Returns(discount);

        // Act & Assert
        Assert.Throws<Exception>(() => _sut.CreateOrder(request))
            .Message.Should().Be("Invalid discount code");
    }
}