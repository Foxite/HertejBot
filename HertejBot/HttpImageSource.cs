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
		HttpResponseMessage image = await m_Http.GetAsync(m_Url);
		Stream download = await image.Content.ReadAsStreamAsync();
		return new Image(download, image.Content.Headers.ContentDisposition?.FileName ?? "image" + image.Content.Headers.ContentType?.MediaType switch {
			"image/jpeg" => ".jpg",
			"image/png" => ".png",
			"image/gif" => ".gif",
			"image/webp" => ".webp",
			_ => ""
		}, null);
	}
}
