namespace Cirreum.AuthenticationProvider.Tests.Coordination;

using Cirreum.Authentication;
using Cirreum.AuthenticationProvider;
using Cirreum.AuthenticationProvider.Coordination;
using Microsoft.Extensions.DependencyInjection;

public sealed class CoordinationRegistrationTests {

	private static IAuthenticationBuilder BuilderOver(IServiceCollection services) {
		var builder = Substitute.For<IAuthenticationBuilder>();
		builder.Services.Returns(services);
		return builder;
	}

	[Fact]
	public void AddCoordination_UseInMemory_registers_both_primitives_as_singletons() {
		var services = new ServiceCollection();

		BuilderOver(services).AddCoordination(c => c.UseInMemory());

		using var provider = services.BuildServiceProvider();
		provider.GetRequiredService<IReplayGuard>().Should().BeOfType<InMemoryReplayGuard>();
		provider.GetRequiredService<IRequestThrottle>().Should().BeOfType<InMemoryRequestThrottle>();

		services.Single(d => d.ServiceType == typeof(IReplayGuard)).Lifetime.Should().Be(ServiceLifetime.Singleton);
		services.Single(d => d.ServiceType == typeof(IRequestThrottle)).Lifetime.Should().Be(ServiceLifetime.Singleton);
	}

	[Fact]
	public void UseInMemory_is_idempotent_leaving_a_single_registration_each() {
		var services = new ServiceCollection();

		BuilderOver(services).AddCoordination(c => c.UseInMemory().UseInMemory());

		services.Count(d => d.ServiceType == typeof(IReplayGuard)).Should().Be(1);
		services.Count(d => d.ServiceType == typeof(IRequestThrottle)).Should().Be(1);
	}

	[Fact]
	public void AddCoordination_with_a_null_builder_throws() {
		var act = () => ((IAuthenticationBuilder)null!).AddCoordination(c => c.UseInMemory());

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void AddCoordination_with_a_null_configure_throws() {
		var services = new ServiceCollection();

		var act = () => BuilderOver(services).AddCoordination(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void CoordinationBuilder_with_null_services_throws() {
		var act = () => new CoordinationBuilder(null!);

		act.Should().Throw<ArgumentNullException>();
	}

}
