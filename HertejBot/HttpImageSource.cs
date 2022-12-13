namespace HertejBot;

public class HttpImageSource : ImageSource {
	private readonly HttpClient m_Http;
	private readonly string m_Url;

	public HttpImageSource(HttpClient http, string url) {
		m_Http = http;
		m_Url = url;
	}
	
	public async override Task<Image> GetImage() {
		// Do not dispose, we return the content stream and that gets disposed elsewhere.
		HttpResponseMessage response = await m_Http.GetAsync(m_Url);
		response.EnsureSuccessStatusCode();
		Stream download = await response.Content.ReadAsStreamAsync();
		return new Image(download, response.Content.Headers.ContentDisposition?.FileName ?? "image" + response.Content.Headers.ContentType?.MediaType switch {
			"image/jpeg" => ".jpg",
			"image/png" => ".png",
			"image/gif" => ".gif",
			"image/webp" => ".webp",
			_ => ""
		});
	}
}
