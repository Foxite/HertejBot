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

var regex = new Regex("hert(je|ej)?");
discord.MessageCreated += async (_, args) => {
	if (!args.Author.IsBot && regex.IsMatch(args.Message.Content.ToLower())) {
		string url = JObject.Parse(await http.GetStringAsync("https://api.tinyfox.dev/img?animal=bleat&json"))["loc"].ToObject<string>();
		await args.Message.RespondAsync("Hertej :) https://api.tinyfox.dev" + url);
	}
};

await discord.ConnectAsync();
await Task.Delay(-1);
