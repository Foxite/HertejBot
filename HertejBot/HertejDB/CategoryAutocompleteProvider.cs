using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Options;

namespace HertejBot.HertejDB;

public class CategoryAutocompleteProvider : IAutocompleteProvider {
	private readonly HertejClient m_Hertej;
	private readonly IOptionsMonitor<Options> m_Options;

	private string[]? m_CachedCategories = null;
	private DateTimeOffset m_LastRefreshed = DateTimeOffset.MinValue;

	public CategoryAutocompleteProvider(HertejClient hertej, IOptionsMonitor<Options> options) {
		m_Hertej = hertej;
		m_Options = options;
	}
	
	public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
		if (m_CachedCategories == null || m_LastRefreshed.AddMinutes(m_Options.CurrentValue.MaxCacheAgeMinutes) < DateTimeOffset.UtcNow) {
			m_LastRefreshed = DateTimeOffset.UtcNow;
			m_CachedCategories = await m_Hertej.GetCategories();
		}

		return m_CachedCategories.Select(category => new DiscordAutoCompleteChoice(category, category)).Append(new DiscordAutoCompleteChoice("", null));
	}

	public class Options {
		public int MaxCacheAgeMinutes { get; set; } = 1;
	}
}
