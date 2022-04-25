using System;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace IsolationTests
{
	public class AuthorizationTests
	{
		[Fact]
		public void Test1()
		{
			var bearerToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2Yjk2NjJmYi0zMTdiLTQ1M2UtODBlOS0zYWU0Y2I0Y2I2OTUiLCJuYW1lIjoiSm9obiBEb2UiLCJpYXQiOjE1MTYyMzkwMjIsImN1c3RvbTpyb2xlIjoiQWRtaW4ifQ.FHbZ9zC7FJv-yK-03Ev4lcGbgzbfGCCtHHChxn7M9VE"; //has "custom:role":"Admin"
			var httpContext = BuildHttpContext(bearerToken, "/WeatherForecast", "GET");
			httpContext.ServiceScopeFactory = _fixtures;

		}

		private DefaultHttpContext BuildHttpContext(string bearerToken, string path = "/", string method = "GET", Dictionary<object, object> data = null)
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers[HeaderNames.Authorization] = bearerToken;
			httpContext.Request.Headers[HeaderNames.Host] = "localhost:1234";
			httpContext.Request.Headers[HeaderNames.Accept] = "*/*";
			httpContext.Request.Protocol = "HTTP/1.1";
			httpContext.Request.Scheme = "http";
			httpContext.Request.Method = method;
			httpContext.Request.Path = path;
			httpContext.Items = data;
			return httpContext;
		}
	}
}
