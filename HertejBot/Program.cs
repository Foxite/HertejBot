using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DSharpPlus;
using Newtonsoft.Json.Linq;

var discord = new DiscordClient(new DiscordConfiguration() {
	Intents = DiscordIntents.GuildMessages,
	Token = Environment.GetEnvironmentVariable("BOT_TOKEN")
});

var http = new HttpClient() {
	DefaultRequestHeaders = {
		UserAgent = {
			new ProductInfoHeaderValue("HertejBot", "0.2"),
			new ProductInfoHeaderValue("(https://github.com/Foxite/HertejBot)")
		}
	}
};

var filters = new Dictionary<Regex, (string AnimalKey, string Reply)> {
	{ new Regex(@"\bhert(je|ej)?\b", RegexOptions.IgnoreCase), ( "bleat", "Hertej :)" ) },
	{ new Regex(@"\bvos(je|ej)?\b", RegexOptions.IgnoreCase), ( "fox", "Vosej :)" ) },
	{ new Regex(@"\bmart(en)?\b", RegexOptions.IgnoreCase), ( "marten", "Marten :)" ) },
	{ new Regex(@"\bhond(je|ej)?\b", RegexOptions.IgnoreCase), ( "dog", "Hondej :)" ) },
};
discord.MessageCreated += (c, args) => {
	if (!args.Author.IsBot) {
		foreach ((Regex? key, (string? animalKey, string? reply)) in filters) {
			if (key.IsMatch(args.Message.Content)) {
				_ = Task.Run(async () => {
					try {
						string url = JObject.Parse(await http.GetStringAsync($"https://api.tinyfox.dev/img?animal={animalKey}&json"))["loc"]!.ToObject<string>()!;
						using Stream download = await http.GetStreamAsync($"https://api.tinyfox.dev{url}");
						await args.Message.RespondAsync(dmb => dmb
							.WithContent(reply)
							.WithFile(Path.GetFileName(url), download)
						);
					} catch (Exception e) {
						Console.WriteLine(e);
					}
				});
				break;
			}
		}
	}
	return Task.CompletedTask;
};

await discord.ConnectAsync();
await Task.Delay(-1);
