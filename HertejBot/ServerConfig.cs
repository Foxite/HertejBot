using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HertejBot; 

public class ServerConfig {
	public ulong[] GuildIds { get; set; }
	public ImageSourceDescription[] ImageSources { get; set; }
}

public class ImageSourceDescription {
	private string m_Regex;

	public string Regex {
		get => m_Regex;
		set {
			m_Regex = value;
			ActualRegex = new Regex($@"\b{value}\b", RegexOptions.IgnoreCase);
		}
	}

	public string Reply { get; set; }
	public ImageSourceType Type { get; set; }
	public JToken Data { get; set; }
	
	[JsonIgnore]
	public Regex ActualRegex { get; private set; }
}

public enum ImageSourceType {
	Http,
	Tinyfox,
}
