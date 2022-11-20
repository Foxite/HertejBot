using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HertejBot.HertejDB;

public class CategoryChoiceProvider : ChoiceProvider {
	private HertejClient? m_Hertej;
	private IOptionsMonitor<Options>? m_Options;

	private string[]? m_CachedCategories = null;
	private DateTimeOffset m_LastRefreshed = DateTimeOffset.MinValue;
	
	public async override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider() {
		m_Hertej ??= Services.GetRequiredService<HertejClient>();
		m_Options ??= Services.GetRequiredService<IOptionsMonitor<Options>>();
		
		if (m_CachedCategories == null || m_LastRefreshed.AddMinutes(m_Options.CurrentValue.MaxCacheAgeMinutes) < DateTimeOffset.UtcNow) {
			m_LastRefreshed = DateTimeOffset.UtcNow;
			m_CachedCategories = await m_Hertej.GetCategories();
		}

		return m_CachedCategories.Select(category => new DiscordApplicationCommandOptionChoice(category, category));
	}

	public class Options {
		public int MaxCacheAgeMinutes { get; set; } = 1;
	}
}
