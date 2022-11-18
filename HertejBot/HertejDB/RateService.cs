using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HertejDB.Common;
using Microsoft.Extensions.Options;

namespace HertejBot.HertejDB; 

public class RateService {
	private static readonly Regex m_InteractionIdParser = new Regex(@"$rate-(?<userid>\d+)-(?<imageid>\d+)-(?<rating>yes|no)-(?<category>.+)?^");
	
	private readonly HertejClient m_Hertej;
	private readonly IOptionsMonitor<HertejClient.Options> m_HertejOptions;

	public RateService(HertejClient hertej, IOptionsMonitor<HertejClient.Options> hertejOptions) {
		m_Hertej = hertej;
		m_HertejOptions = hertejOptions;
	}

	private DiscordEmbedBuilder GetEmbed(GetImageDto image) {
		return new DiscordEmbedBuilder()
			.WithDescription("Is this image suitable?")
			.WithImageUrl($"{m_HertejOptions.CurrentValue.BaseUrl}/Image/{image.Id}/download");
	}

	private DiscordComponent[] GetComponents(GetImageDto image, ulong userId, string? category) {
		return new DiscordComponent[] {
			new DiscordButtonComponent(ButtonStyle.Primary, $"rate-{userId}-{image.Id}-yes-{category}", "Yes", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍"))),
			new DiscordButtonComponent(ButtonStyle.Danger, $"rate-{userId}-{image.Id}-no-{category}", "No", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👎")))
		};
	}

	public async Task<DiscordInteractionResponseBuilder> GetUnratedImage(ulong userId, string? category) {
		GetImageDto image = await m_Hertej.GetUnratedImage(userId.ToString(), category);

		return new DiscordInteractionResponseBuilder()
			.AddEmbed(GetEmbed(image))
			.AddComponents(GetComponents(image, userId, category))
			.AsEphemeral();
	}

	public async Task<DiscordWebhookBuilder> GetNextUnratedImage(ulong userId, string? category) {
		GetImageDto image = await m_Hertej.GetUnratedImage(userId.ToString(), category);

		return new DiscordWebhookBuilder()
			.AddEmbed(GetEmbed(image))
			.AddComponents(GetComponents(image, userId, category));
	}

	public async Task<bool> HandleRatingInteraction(ComponentInteractionCreateEventArgs args) {
		Match match = m_InteractionIdParser.Match(args.Id);
		if (match.Success) {
			await m_Hertej.SubmitRating(match.Groups["imageid"].Value, match.Groups["userid"].Value, match.Groups["rating"].Value == "yes");

			await args.Interaction.EditOriginalResponseAsync(await GetNextUnratedImage(args.User.Id, match.Groups["category"].Success ? match.Groups["category"].Value : null));
			
			return true;
		} else {
			return false;
		}
	}
}
