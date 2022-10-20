using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace HertejBot; 

public class ServerConfigManager {
	private readonly Dictionary<ulong, List<RegisteredSource>> m_SourcesByGuildId;

	private ServerConfigManager() {
		m_SourcesByGuildId = new Dictionary<ulong, List<RegisteredSource>>();
	}
	
	public static ServerConfigManager Create(string filePath, ImageSourceFactory imageSourceFactory) {
		ServerConfig[] configs = JsonConvert.DeserializeObject<ServerConfig[]>(File.ReadAllText(filePath))!;

		var ret = new ServerConfigManager();

		foreach (ServerConfig config in configs) {
			foreach (ImageSourceDescription isd in config.ImageSources) {
				ImageSource imageSource = imageSourceFactory.GetImageSource(isd);
				foreach (ulong guildId in config.GuildIds) {
					ret.Register(guildId, isd.ActualRegex, imageSource, isd.Reply);
				}
			}
		}

		return ret;
	}

	private void Register(ulong guildId, Regex regex, ImageSource imageSource, string reply) {
		List<RegisteredSource> sources = m_SourcesByGuildId.GetOrAdd(guildId, _ => new List<RegisteredSource>());
		sources.Add(new RegisteredSource(regex, imageSource, reply));
	}

	public RegisteredSource? GetImageSource(DiscordMessage message) {
		if (!message.Channel.GuildId.HasValue) {
			return null;
		}

		if (!m_SourcesByGuildId.TryGetValue(message.Channel.GuildId.Value, out List<RegisteredSource>? sources)) {
			return null;
		}

		foreach (RegisteredSource source in sources) {
			if (source.IsMatch(message.Content)) {
				return source;
			}
		}

		return null;
	}

	public record RegisteredSource(Regex Regex, ImageSource ImageSource, string Reply) {
		public bool IsMatch(string message) => Regex.IsMatch(message);
	};
}
