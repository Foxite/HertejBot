using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HertejBot; 

public class ServerConfigManager : IDisposable {
	private readonly ImageSourceFactory m_ImageSourceFactory;
	private readonly Dictionary<ulong, List<RegisteredSource>> m_SourcesByGuildId;
	private readonly IDisposable m_OnSourcesChangeListener;

	public ServerConfigManager(ImageSourceFactory imageSourceFactory, IOptionsMonitor<List<ServerConfig>> sourcesMonitor) {
		m_SourcesByGuildId = new Dictionary<ulong, List<RegisteredSource>>();
		m_ImageSourceFactory = imageSourceFactory;
		m_OnSourcesChangeListener = sourcesMonitor.OnChange(OnSourcesChange);
		
		OnSourcesChange(sourcesMonitor.CurrentValue);
	}
	
	private void OnSourcesChange(ICollection<ServerConfig> configs) {
		m_SourcesByGuildId.Clear();

		foreach (ServerConfig config in configs) {
			foreach (ImageSourceDescription isd in config.ImageSources) {
				ImageSource imageSource = m_ImageSourceFactory.GetImageSource(isd);
				foreach (ulong guildId in config.GuildIds) {
					Register(guildId, isd.ActualRegex, imageSource, isd.Reply);
				}
			}
		}
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
	}

	public void Dispose() {
		m_OnSourcesChangeListener.Dispose();
	}
}
