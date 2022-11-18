using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace HertejBot.HertejDB; 

public class RateModule : ApplicationCommandModule {
	private readonly RateService m_Service;

	public RateModule(RateService service) {
		m_Service = service;
	}

	[SlashCommand("startrating", "Start rating")]
	public async Task StartRating(InteractionContext context, [Option("category", "The category of images to rate"), ChoiceProvider(typeof(CategoryChoiceProvider))] string? category = null) {
		DiscordInteractionResponseBuilder dirb = await m_Service.GetUnratedImage(context.User.Id, category);
		await context.CreateResponseAsync(dirb);
	}
}
