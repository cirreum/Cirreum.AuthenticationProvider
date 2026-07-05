namespace Cirreum.AuthenticationProvider.Tests.Coordination;

using Cirreum.Authentication;
using Cirreum.AuthenticationProvider;
using Cirreum.Coordination;
using Microsoft.Extensions.DependencyInjection;

public sealed class CoordinationRegistrationTests {

	private static IAuthenticationBuilder BuilderOver(IServiceCollection services) {
		var builder = Substitute.For<IAuthenticationBuilder>();
		builder.Services.Returns(services);
		return builder;
	}

	[Fact]
	public void Auth_ConfigureCoordination_forwards_backend_selection_to_the_service_collection() {
		var services = new ServiceCollection();

		BuilderOver(services).ConfigureCoordination(c => c.UseInMemory());

		using var provider = services.BuildServiceProvider();
		provider.GetRequiredService<IReplayGuard>().Should().NotBeNull();
		provider.GetRequiredService<IRequestThrottle>().Should().NotBeNull();
		services.Count(d => d.ServiceType == typeof(IReplayGuard)).Should().Be(1);
		services.Count(d => d.ServiceType == typeof(IRequestThrottle)).Should().Be(1);
	}

	[Fact]
	public void Auth_ConfigureCoordination_with_a_null_builder_throws() {
		var act = () => ((IAuthenticationBuilder)null!).ConfigureCoordination(c => c.UseInMemory());

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Auth_ConfigureCoordination_with_a_null_configure_throws() {
		var services = new ServiceCollection();

		var act = () => BuilderOver(services).ConfigureCoordination(null!);

		act.Should().Throw<ArgumentNullException>();
	}

}
