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
		UserAgent = { new ProductInfoHeaderValue("HertejBot", "0.1") }
	}
};

var filters = new Dictionary<Regex, (string AnimalKey, string Reply)> {
	{ new Regex(@"\bhert(je|ej)?\b", RegexOptions.IgnoreCase), ( "bleat", "Hertej :)" ) },
	{ new Regex(@"\bvos(je|ej)?\b", RegexOptions.IgnoreCase), ( "fox", "Vosej :)" ) },
};
discord.MessageCreated += async (_, args) => {
	if (!args.Author.IsBot) {
		foreach ((Regex? key, (string? animalKey, string? reply)) in filters) {
			if (key.IsMatch(args.Message.Content)) {
				string url = JObject.Parse(await http.GetStringAsync($"https://api.tinyfox.dev/img?animal={animalKey}&json"))["loc"]!.ToObject<string>()!;
				await args.Message.RespondAsync($"{reply} https://api.tinyfox.dev{url}");
				return;
			}
		}
	}
};

await discord.ConnectAsync();
await Task.Delay(-1);
