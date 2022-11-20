using HertejBot.HertejDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HertejBot; 

public class ImageSourceFactory {
	private readonly HttpClient m_HttpClient;
	private readonly HertejClient m_HertejClient;

	public ImageSourceFactory(HttpClient httpClient, HertejClient hertejClient) {
		m_HttpClient = httpClient;
		m_HertejClient = hertejClient;
	}
	
	public ImageSource GetImageSource(ImageSourceDescription isd) {
		return isd.Type switch {
			ImageSourceType.Http => new HttpImageSource(m_HttpClient, isd.Data.Get<string>()),
			ImageSourceType.Tinyfox => new HttpImageSource(m_HttpClient, $"https://api.tinyfox.dev/img?animal={isd.Data.Get<string>()}"),
			ImageSourceType.HertejDb => new HertejImageSource(m_HertejClient, isd.Data.Get<string>()),
		};
	}
}
