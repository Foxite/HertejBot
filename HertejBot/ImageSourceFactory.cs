using Microsoft.Extensions.Configuration;

namespace HertejBot; 

public class ImageSourceFactory {
	private readonly HttpClient m_HttpClient;

	public ImageSourceFactory(HttpClient httpClient) {
		m_HttpClient = httpClient;
	}
	
	public ImageSource GetImageSource(ImageSourceDescription isd) {
		return isd.Type switch {
			ImageSourceType.Http => new HttpImageSource(m_HttpClient, isd.Data.Get<string>()),
			ImageSourceType.Tinyfox => new HttpImageSource(m_HttpClient, $"https://api.tinyfox.dev/img?animal={isd.Data.Get<string>()}")
		};
	}
}
