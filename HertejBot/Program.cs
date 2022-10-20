using System.Net.Http.Headers;
using DSharpPlus;
using DSharpPlus.Entities;
using HertejBot;

var discord = new DiscordClient(new DiscordConfiguration() {
	Intents = DiscordIntents.GuildMessages | DiscordIntents.Guilds,
	Token = Environment.GetEnvironmentVariable("BOT_TOKEN")
});

var http = new HttpClient() {
	DefaultRequestHeaders = {
		UserAgent = {
			new ProductInfoHeaderValue("HertejBot", "0.3"),
			new ProductInfoHeaderValue("(https://github.com/Foxite/HertejBot)")
		}
	}
};

var serverConfigManager = ServerConfigManager.Create(Environment.GetEnvironmentVariable("CONFIG_PATH") ?? "serverconfigs.json", new ImageSourceFactory(http));

discord.MessageCreated += (c, args) => {
	if (args.Guild != null) {
		Permissions perms = args.Channel.PermissionsFor(args.Guild.CurrentMember);
		if (!perms.HasFlag(args.Channel.IsThread ? Permissions.SendMessagesInThreads : Permissions.SendMessages)) {
			return Task.CompletedTask;
		}
	}

	if (args.Author.IsBot) {
		return Task.CompletedTask;
	}
	
	ServerConfigManager.RegisteredSource? source = serverConfigManager.GetImageSource(args.Message);
	if (source != null) {
		_ = Task.Run(async () => {
			try {
				using Image image = await source.ImageSource.GetImage();
				await args.Message.RespondAsync(
					new DiscordMessageBuilder()
						.WithContent(source.Reply + " :)")
						.WithFile(Path.GetFileName(image.Filename.Replace("\"", "")), image.Stream)
				);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		});
	}
	return Task.CompletedTask;
};

await discord.ConnectAsync();
await Task.Delay(-1);
