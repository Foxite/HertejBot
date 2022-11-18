using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HertejDB.Common;
using Microsoft.Extensions.Options;

namespace HertejBot.HertejDB; 

public class RateService {
	private static readonly Regex InteractionIdParser = new Regex(@"^rate-(?<userid>\d+)-(?<imageid>\d+)-(?<rating>yes|no)-(?<category>.+)?$");
	
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

	private DiscordComponent[] GetComponents(string userId, string? category, GetImageDto image) {
		return new DiscordComponent[] {
			new DiscordButtonComponent(ButtonStyle.Success, $"rate-{userId}-{image.Id}-yes-{category}", "Yes", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍"))),
			new DiscordButtonComponent(ButtonStyle.Danger, $"rate-{userId}-{image.Id}-no-{category}", "No", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👎")))
		};
	}

	public async Task<DiscordFollowupMessageBuilder> GetUnratedImage(ulong userId, string? category) {
		GetImageDto? image = await m_Hertej.GetUnratedImage(userId.ToString(), category);

		var builder = new DiscordFollowupMessageBuilder();

		if (image == null) {
			return builder.WithContent("No more images to rate. Thank you!");
		} else {
			return builder
				.AddEmbed(GetEmbed(image))
				.AddComponents(GetComponents(userId.ToString(), category, image))
				.AsEphemeral();
		}
	}

	private async Task<DiscordWebhookBuilder> GetNextUnratedImage(string userId, string? category) {
		GetImageDto? image = await m_Hertej.GetUnratedImage(userId, category);

		var builder = new DiscordWebhookBuilder();
		
		if (image == null) {
			return builder.WithContent("No more images to rate. Thank you!");
		} else {
			return builder
				.AddEmbed(GetEmbed(image))
				.AddComponents(GetComponents(userId, category, image));
		}
	}

	public async Task<bool> HandleRatingInteraction(ComponentInteractionCreateEventArgs args) {
		Match match = InteractionIdParser.Match(args.Id);
		if (match.Success) {
			await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			
			await m_Hertej.SubmitRating(match.Groups["imageid"].Value, match.Groups["userid"].Value, match.Groups["rating"].Value == "yes");

			await args.Interaction.EditFollowupMessageAsync(args.Message.Id, await GetNextUnratedImage(match.Groups["userid"].Value, match.Groups["category"].Success ? match.Groups["category"].Value : null));
			
			return true;
		} else {
			return false;
		}
	}
}
