namespace HertejBot;

public record Image(
	Stream Stream,
	string Filename
) : IDisposable {
	public void Dispose() => Stream.Dispose();
}

public abstract class ImageSource {
	public abstract Task<Image> GetImage();
}

public class HttpImageSource : ImageSource {
	private readonly HttpClient m_Http;
	private readonly string m_Url;

	public HttpImageSource(HttpClient http, string url) {
		m_Http = http;
		m_Url = url;
	}
	
	public async override Task<Image> GetImage() {
		HttpResponseMessage image = await m_Http.GetAsync(m_Url);
		Stream download = await image.Content.ReadAsStreamAsync();
		return new Image(download, image.Content.Headers.ContentDisposition?.FileName ?? "image");
	}
}
