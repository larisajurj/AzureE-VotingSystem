using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace VotingFn.Models;

public static class HttpRequestDataExtensions
{
	/// <summary>
	/// Creates an HTTP response with the specified data and status code.
	/// Supports both single-item responses and lists containing multiple child classes.
	/// </summary>
	/// <typeparam name="T">The type of data to include in the response.</typeparam>
	/// <param name="req">The incoming HTTP request.</param>
	/// <param name="data">The response data, which can be a single item or a list of items.</param>
	/// <param name="statusCode">The HTTP status code to return.</param>
	/// <returns>An <see cref="HttpResponseData"/> containing the serialized response data.</returns>
	public static async Task<HttpResponseData> CreateResponse<T>(this HttpRequestData req, T data,
		HttpStatusCode statusCode)
	{
		if (Equals(data, default(T))) return req.CreateResponse(statusCode);
		var response = req.CreateResponse();
		response.StatusCode = statusCode;

		var jsonResponse = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.None
		});

		await response.WriteStringAsync(jsonResponse, Encoding.UTF8);
		return response;
	}
}