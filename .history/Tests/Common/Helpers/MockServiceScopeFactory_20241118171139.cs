using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Tests.Common.Helpers;

public static class MockServiceScopeFactory
{
    public static IServiceScopeFactory Create()
    {
        var mockFactory = new Mock<IServiceScopeFactory>();
        var mockScope = new Mock<IServiceScope>();
        mockFactory.Setup(f => f.CreateScope()).Returns(mockScope.Object);
        return mockFactory.Object;
    }
}
