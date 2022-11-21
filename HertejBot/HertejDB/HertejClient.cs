using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using HertejDB.Common;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HertejBot.HertejDB;

public class HertejClient {
	private readonly HttpClient m_Http;
	private readonly IOptions<Options> m_Options;
	private DiscoveryDocumentResponse? m_DiscoveryDocument;
	private TokenResponse? m_Token;
	private DateTimeOffset m_TokenAcquired;

	public HertejClient(HttpClient http, IOptions<Options> options) {
		m_Http = http;
		m_Options = options;
		m_TokenAcquired = DateTimeOffset.MinValue;
	}

	private async Task<TokenResponse> GetTokenAsync() {
		if (m_DiscoveryDocument == null) {
			m_DiscoveryDocument = await m_Http.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest {
				Address = m_Options.Value.DiscoveryDocument,
				Policy = {
					ValidateEndpoints = false,
					ValidateIssuerName = false
				}
			});

			if (m_DiscoveryDocument.IsError) {
				throw new InvalidOperationException("Failed to get discovery document: " + m_DiscoveryDocument.Error);
			}
		}
		
		if (m_Token != null && m_TokenAcquired.AddSeconds(m_Token.ExpiresIn) > DateTimeOffset.UtcNow.AddSeconds(15)) {
			return m_Token;
		}

		var clientCredentialsTokenRequest = new ClientCredentialsTokenRequest {
			Address = m_DiscoveryDocument!.TokenEndpoint,
			ClientId = m_Options.Value.ClientId,
			ClientSecret = m_Options.Value.ClientSecret,
			Scope = m_Options.Value.Scopes,
			GrantType = "client_credentials",
			Parameters = {
				{ "username", m_Options.Value.Username },
				{ "password", m_Options.Value.Password }
			}
		};
		TokenResponse tokenResponse = await m_Http.RequestClientCredentialsTokenAsync(clientCredentialsTokenRequest);

		if (tokenResponse.IsError) {
			throw new InvalidOperationException("Token response error " + tokenResponse.Error);
		}

		m_Token = tokenResponse;
		m_TokenAcquired = DateTimeOffset.UtcNow;

		return m_Token;
	}

	private async Task<T> Request<T>(string endpoint, T? anonymousType = default, bool authorize = false, HttpMethod? method = null, HttpContent? requestBody = null) {
		var hrm = new HttpRequestMessage(method ?? HttpMethod.Get, m_Options.Value.BaseUrl + "/" + endpoint) {
			Content = requestBody
		};
		
		if (authorize) {
			TokenResponse token = await GetTokenAsync();
			hrm.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
		}

		using HttpResponseMessage response = await m_Http.SendAsync(hrm);
		response.EnsureSuccessStatusCode();
		string content = await response.Content.ReadAsStringAsync();
		if (anonymousType != null) {
			return JsonConvert.DeserializeAnonymousType(content, anonymousType)!;
		} else {
			return JsonConvert.DeserializeObject<T>(content)!;
		}
	}

	private string GetQueryUrl(string baseUrl, params KeyValuePair<string, string>?[] parameters) {
		bool first = true;
		foreach (KeyValuePair<string, string>? param in parameters) {
			if (param == null) {
				continue;
			}
			
			if (first) {
				baseUrl += "?";
			} else {
				baseUrl += "&";
			}

			first = false;

			baseUrl += Uri.EscapeDataString(param.Value.Key) + "=" + Uri.EscapeDataString(param.Value.Value);
		}

		return baseUrl;
	}
	
	public Task<IDictionary<string, int>> GetCategories() {
		return Request<IDictionary<string, int>>("Image/categories");
	}
	
	public Task<IDictionary<string, int>> GetUnratedCategories() {
		return Request<IDictionary<string, int>>("ImageRating/categories", authorize: true);
	}
	
	public Task<GetImageDto> GetImage(string id) {
		return Request<GetImageDto>($"Image/{id}");
	}
	
	public Task<GetImageDto?> GetRandomImage(string category) {
		return Request<GetImageDto?>(GetQueryUrl("Image/random", new KeyValuePair<string, string>("category", category)));
	}

	public async Task<Image> DownloadImage(string id) {
		// Do not dispose, we return the content stream and that gets disposed elsewhere.
		HttpResponseMessage response = await m_Http.GetAsync(m_Options.Value.BaseUrl + $"/Image/{id}/download");
		response.EnsureSuccessStatusCode();
		return new Image(await response.Content.ReadAsStreamAsync(), "image" + response.Content.Headers.ContentType!.MediaType switch {
			"image/jpeg" => ".jpg",
			"image/png" => ".png",
			"image/gif" => ".gif",
			"image/webp" => ".webp",
			_ => ""
		});
	}
	
	public Task<GetImageDto?> GetUnratedImage(string userId, string? category = null) {
		return Request<GetImageDto?>(GetQueryUrl("ImageRating/unrated", new KeyValuePair<string, string>("userId", userId), category == null ? null : new KeyValuePair<string, string>("category", category)), authorize: true);
	}
	
	public Task SubmitRating(string imageId, string userId, bool isSuitable) {
		return Request<string?>($"ImageRating/{imageId}", method: HttpMethod.Put, authorize: true, requestBody: JsonContent.Create(new SubmitRatingDto() {
			UserId = userId,
			IsSuitable = isSuitable
		}));
	}

	public class Options {
		public string BaseUrl { get; set; }
		public string Authority { get; set; }
		public string DiscoveryDocument { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Scopes { get; set; }
	}
}
