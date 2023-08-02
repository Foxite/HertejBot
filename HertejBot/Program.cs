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
	icb.AddJsonFile($"appsettings.{hbc.HostingEnvironment.EnvironmentName}.json", true, true);
	icb.AddJsonFile("serverconfigs.json", false, true);
	icb.AddJsonFile($"serverconfigs.{hbc.HostingEnvironment.EnvironmentName}.json", true, true);
	icb.AddEnvironmentVariables();
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
		var discordOptions = isp.GetRequiredService<IOptions<DiscordOptions>>();
		
		var discord = new DiscordClient(new DiscordConfiguration() {
			Intents = DiscordIntents.GuildMessages | DiscordIntents.Guilds,
			Token = discordOptions.Value.Token,
			LoggerFactory = isp.GetRequiredService<ILoggerFactory>(),
		});

		var slashCommands = discord.UseSlashCommands(new SlashCommandsConfiguration() {
			Services = isp
		});
		
		slashCommands.RegisterCommands<RateModule>(discordOptions.Value.CommandsGuild);

		slashCommands.SlashCommandInvoked += (o, e) => {
			isp.GetRequiredService<ILogger<SlashCommandsExtension>>().LogDebug("Slash command invoked: {CommandName}", e.Context.CommandName);
			return Task.CompletedTask;
		};

		slashCommands.SlashCommandExecuted += (o, e) => {
			isp.GetRequiredService<ILogger<SlashCommandsExtension>>().LogDebug("Slash command executed: {CommandName}", e.Context.CommandName);
			return Task.CompletedTask;
		};

		slashCommands.SlashCommandErrored += (o, e) => {
			isp.GetRequiredService<ILogger<SlashCommandsExtension>>().LogError(e.Exception, "Slash command errored: {CommandName}", e.Context.CommandName);
			return Task.CompletedTask;
		};
		
		return discord;
	});

	isc.AddSingleton<RateService>();
	isc.AddSingleton<HertejClient>();
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
				app.Services.GetRequiredService<ILogger<Program>>().LogError(e, "Error handling message off-thread");
			}
		});
	}
	return Task.CompletedTask;
};

discord.ComponentInteractionCreated += (c, args) => {
	//app.Services.GetRequiredService<ILogger<Program>>().LogInformation(args.ToString());

	_ = Task.Run(async () => {
		try {
			var rateService = app.Services.GetRequiredService<RateService>();
			await rateService.HandleRatingInteraction(args);
		} catch (Exception e) {
			app.Services.GetRequiredService<ILogger<Program>>().LogError(e, "Error handling component interaction off-thread");
		}
	});

	return Task.CompletedTask;
};

await discord.ConnectAsync();

await app.RunAsync();
