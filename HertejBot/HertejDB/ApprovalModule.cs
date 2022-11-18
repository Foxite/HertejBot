using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HertejDB.Common;
using Microsoft.Extensions.Options;

namespace HertejBot.HertejDB; 

public class ApprovalModule : ApplicationCommandModule {
	private readonly HertejClient m_Hertej;
	private readonly IOptionsMonitor<HertejClient.Options> m_HertejOptions;

	public ApprovalModule(HertejClient hertej, IOptionsMonitor<HertejClient.Options> hertejOptions) {
		m_Hertej = hertej;
		m_HertejOptions = hertejOptions;
	}

	[SlashCommand("start", "Start rating")]
	public async Task StartRating(InteractionContext context, [Option("category", "The category of images to rate", true), Autocomplete(typeof(CategoryAutocompleteProvider))] string? category = null) {
		GetImageDto image = await m_Hertej.GetUnratedImage(context.User.Id.ToString(), category);

		var dirb = new DiscordInteractionResponseBuilder()
			.AddEmbed(
				new DiscordEmbedBuilder()
					.WithDescription("Is this image suitable?")
					.WithImageUrl($"{m_HertejOptions.CurrentValue.BaseUrl}/Image/{image.Id}/download")
					.Build()
			)
			.AddComponents(
				// TODO handle component interactions
				new DiscordButtonComponent(ButtonStyle.Primary, $"{context.User.Id}-rate-{image.Id}-yes", "Yes", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍"))),
				new DiscordButtonComponent(ButtonStyle.Danger, $"{context.User.Id}-rate-{image.Id}-no", "No", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👎")))
			)
			.AsEphemeral();

		await context.CreateResponseAsync(dirb);
	}
}