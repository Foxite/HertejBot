using System.Net.Http.Headers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HertejBot;
using HertejBot.HertejDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((hbc, icb) => {
	icb.AddJsonFile("appsettings.json", false, true);
	icb.AddJsonFile($"appsettings.{hbc.HostingEnvironment.EnvironmentName}.json", false, true);
	icb.AddJsonFile("serverconfigs.json", false, true);
});

builder.ConfigureServices((hbc, isc) => {
	isc.Configure<DiscordOptions>(hbc.Configuration.GetSection("Discord"));
	isc.Configure<HertejClient.Options>(hbc.Configuration.GetSection("HertejDB"));
	isc.Configure<List<ServerConfig>>(hbc.Configuration.GetSection("ServerConfigs"));

	isc.AddSingleton(_ => new HttpClient() {
		DefaultRequestHeaders = {
			UserAgent = {
				new ProductInfoHeaderValue("HertejBot", "0.3"),
				new ProductInfoHeaderValue("(https://github.com/Foxite/HertejBot)")
			}
		}
	});

	isc.AddSingleton(isp => {
		var discord = new DiscordClient(new DiscordConfiguration() {
			Intents = DiscordIntents.GuildMessages | DiscordIntents.Guilds,
			Token = isp.GetRequiredService<IOptions<DiscordOptions>>().Value.Token,
			LoggerFactory = isp.GetRequiredService<ILoggerFactory>()
		});

		var slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration() {
			Services = isp
		});
		
		slashCommands.RegisterCommands<ApprovalModule>();

		return discord;
	});

	isc.AddSingleton<ImageSourceFactory>();
	isc.AddSingleton<ServerConfigManager>();
});

IHost app = builder.Build();

var discord = app.Services.GetRequiredService<DiscordClient>();
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
	
	ServerConfigManager.RegisteredSource? source = app.Services.GetRequiredService<ServerConfigManager>().GetImageSource(args.Message);
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

discord.ComponentInteractionCreated += (c, args) => {
	app.Services.GetRequiredService<ILogger<Program>>().LogInformation(args.ToString());
	
	return Task.CompletedTask;
};

await discord.ConnectAsync();

await app.RunAsync();
