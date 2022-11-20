using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace HertejBot.HertejDB; 

public class RateModule : ApplicationCommandModule {
	private readonly RateService m_Service;

	public RateModule(RateService service) {
		m_Service = service;
	}

	[SlashCommand("rate", "Start rating")]
	public async Task StartRating(InteractionContext context, [Option("category", "The category of images to rate"), ChoiceProvider(typeof(UnratedCategoryChoiceProvider))] string? category = null) {
		await context.DeferAsync(true);
		DiscordFollowupMessageBuilder dirb = await m_Service.GetUnratedImage(context.User.Id, category);
		await context.FollowUpAsync(dirb);
	}
}
