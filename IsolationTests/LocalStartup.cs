using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using Moq;

namespace IsolationTests
{
	public class LocalStartup<TStartup> where TStartup : IStartup, IServiceScopeFactory
	{
		public IStartup Startup { get; }
		public IServiceProvider ApplicationServices { get; }
		private ApplicationBuilder ApplicationBuilder { get; }
		public RequestDelegate RequestDelegate { get; }

        public StartupFixtures(Action<IServiceCollection> mockServices, IConfiguration? config = null)
        {
            config ??= new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            var hostingEnv = new Mock<IWebHostEnvironment>();
            hostingEnv
                .Setup(m => m.EnvironmentName)
                .Returns("Development");
            Startup = (TStartup)Activator.CreateInstance(
                typeof(TStartup),
                config,
                hostingEnv.Object,
                new NullLoggerFactory()
            )!;
            if (typeof(TStartup).GetProperty("UnderTest",
                typeof(bool)) is { CanWrite: true } prop)
            {
                prop.SetValue(Startup, true);
            }
            var services = new ServiceCollection();
            services.MockSingleton(hostingEnv);
            services.AddSingleton(SystemClock.Instance.InUtc());
            services.AddSingleton(config);
            Startup.ConfigureServices(services);
            services.AddScoped<HttpContext>(svc =>
            {
                var context = new DefaultHttpContext();
                context.Request.Headers[HeaderNames.Authorization] = "foo";
                return context;
            });
            MockLoggerFactory.Add(services);
            mockServices(services);
            ApplicationServices = services.BuildAutofacServiceProvider();

            // Setup middleware
            ApplicationBuilder = new ApplicationBuilder(ApplicationServices);
            Startup.Configure(ApplicationBuilder);
            RequestDelegate = ApplicationBuilder.Build();
        }

        public IServiceScope CreateScope() =>
            ApplicationServices.CreateScope();
    }
}
