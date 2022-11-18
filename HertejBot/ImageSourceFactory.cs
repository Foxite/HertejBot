using HertejBot.HertejDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HertejBot; 

public class ImageSourceFactory {
	private readonly HttpClient m_HttpClient;
	private readonly IOptions<HertejClient.Options> m_HertejOptions;

	public ImageSourceFactory(HttpClient httpClient, IOptions<HertejClient.Options> hertejOptions) {
		m_HttpClient = httpClient;
		m_HertejOptions = hertejOptions;
	}
	
	public ImageSource GetImageSource(ImageSourceDescription isd) {
		return isd.Type switch {
			ImageSourceType.Http => new HttpImageSource(m_HttpClient, isd.Data.Get<string>()),
			ImageSourceType.Tinyfox => new HttpImageSource(m_HttpClient, $"https://api.tinyfox.dev/img?animal={isd.Data.Get<string>()}"),
			ImageSourceType.HertejDb => new HttpImageSource(m_HttpClient, $"{m_HertejOptions.Value.BaseUrl}/Image/random?category={isd.Data.Get<string>()}"),
		};
	}
}
