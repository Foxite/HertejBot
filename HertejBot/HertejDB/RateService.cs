using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using HertejDB.Common;
using Microsoft.Extensions.Options;

namespace HertejBot.HertejDB; 

public class RateService {
	private static readonly Regex InteractionIdParser_Submit = new Regex(@"^rate-(?<imageid>\d+)-(?<rating>yes|no)-(?<category>.+)?$");
	private static readonly Regex InteractionIdParser_Back = new Regex(@"^back-(?<imageid>\d+)-(?<category>.+)?$");
	
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

	private IEnumerable<DiscordComponent> GetComponents(ulong userId, string? category, GetImageDto image, string? previousId) {
		IEnumerable<DiscordComponent> ret = new DiscordComponent[] {
			new DiscordButtonComponent(ButtonStyle.Success, $"rate-{userId}-{image.Id}-yes-{category}", "Yes", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👍"))),
			new DiscordButtonComponent(ButtonStyle.Danger, $"rate-{userId}-{image.Id}-no-{category}", "No", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👎"))),
		};

		if (previousId != null) {
			ret = ret.Append(new DiscordButtonComponent(ButtonStyle.Danger, $"back-{previousId}-{category}", "No", emoji: new DiscordComponentEmoji(DiscordEmoji.FromUnicode("👎"))));
		}

		return ret;
	}

	public async Task<DiscordFollowupMessageBuilder> GetUnratedImage(ulong userId, string? category) {
		GetImageDto? image = await m_Hertej.GetUnratedImage(userId.ToString(), category);

		var builder = new DiscordFollowupMessageBuilder();

		if (image == null) {
			return builder.WithContent("No more images to rate. Thank you!");
		} else {
			return builder
				.AddEmbed(GetEmbed(image))
				.AddComponents(GetComponents(userId, category, image, null))
				.AsEphemeral();
		}
	}

	private async Task<DiscordWebhookBuilder> GetNextUnratedImage(ulong userId, string? category, string? previousId) {
		GetImageDto? image = await m_Hertej.GetUnratedImage(userId.ToString(), category);

		var builder = new DiscordWebhookBuilder();
		
		if (image == null) {
			return builder.WithContent("No more images to rate. Thank you!");
		} else {
			return builder
				.AddEmbed(GetEmbed(image))
				.AddComponents(GetComponents(userId, category, image, previousId));
		}
	}

	private async Task<DiscordWebhookBuilder> GetPreviousUnratedImage(ulong userId, string? category, string previousId) {
		GetImageDto image = await m_Hertej.GetImage(previousId);

		return new DiscordWebhookBuilder()
			.AddEmbed(GetEmbed(image))
			.AddComponents(GetComponents(userId, category, image, null));
	}

	public async Task<bool> HandleRatingInteraction(ComponentInteractionCreateEventArgs args) {
		Match match = InteractionIdParser_Submit.Match(args.Id);
		if (match.Success) {
			await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			
			await m_Hertej.SubmitRating(match.Groups["imageid"].Value, args.User.Id.ToString(), match.Groups["rating"].Value == "yes");

			DiscordWebhookBuilder messageBuilder = await GetNextUnratedImage(args.User.Id, match.Groups["category"].Success ? match.Groups["category"].Value : null, match.Groups["imageid"].Value);
			await args.Interaction.EditFollowupMessageAsync(args.Message.Id, messageBuilder);
			
			return true;
		}
		
		match = InteractionIdParser_Back.Match(args.Id);
		if (match.Success) {
			await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

			DiscordWebhookBuilder messageBuilder = await GetPreviousUnratedImage(args.User.Id, match.Groups["category"].Success ? match.Groups["category"].Value : null, match.Groups["imageid"].Value);
			await args.Interaction.EditFollowupMessageAsync(args.Message.Id, messageBuilder);
			
			return true;
		}
		
		return false;
	}
}
