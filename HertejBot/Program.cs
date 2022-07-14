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
	{ new Regex(@"\bvos(je|ej)?s?\b", RegexOptions.IgnoreCase), ( "fox", "Vosej :)" ) },
	{ new Regex(@"\bfox(ie|es|ies)\b", RegexOptions.IgnoreCase), ( "fox", "Vosej :)" ) },
	{ new Regex(@"\bmart(en)?\b", RegexOptions.IgnoreCase), ( "marten", "Marten :)" ) },
	{ new Regex(@"\bhond(je|ej)?\b", RegexOptions.IgnoreCase), ( "dog", "Hondej :)" ) },
	{ new Regex(@"\bpum(aj|ej|aej)?\b", RegexOptions.IgnoreCase), ( "puma", "Pumaj :)" ) },
	{ new Regex(@"\bsnek(je|ej)?\b", RegexOptions.IgnoreCase), ( "snek", "Snek :)" ) },
	{ new Regex(@"\bhyeen(tje|ej|tej)?\b", RegexOptions.IgnoreCase), ( "yeen", "Hyeen :)" ) },
	{ new Regex(@"\bmanul(tje|ej|tej)?\b", RegexOptions.IgnoreCase), ( "manul", "Manul :)" ) },
	{ new Regex(@"\bposs(je|ej)?\b", RegexOptions.IgnoreCase), ( "poss", "Possej :)" ) },
	{ new Regex(@"\bleo(tje|j|tej)?\b", RegexOptions.IgnoreCase), ( "leo", "Leoj :)" ) },
	{ new Regex(@"\bserval(tje|ej|tej)?\b", RegexOptions.IgnoreCase), ( "serval", "Servalej :)" ) },
	{ new Regex(@"\bshiba(tje|j|tej)?\b", RegexOptions.IgnoreCase), ( "shiba", "Shibaj :)" ) },
	{ new Regex(@"\braccoon(tje|ej|tej)?\b", RegexOptions.IgnoreCase), ( "racc", "raccoontej :)" ) },
	{ new Regex(@"\bdook(tje|ej|tej)?\b", RegexOptions.IgnoreCase), ( "dook", "dookej :)" ) },
	{ new Regex(@"\bott(je|ej|erej\ertje)?\b", RegexOptions.IgnoreCase), ( "ott", "ottej :)" ) },
};
discord.MessageCreated += (c, args) => {
	if (!args.Author.IsBot) {
		foreach ((Regex? key, (string? animalKey, string? reply)) in filters) {
			if (key.IsMatch(args.Message.Content)) {
				_ = Task.Run(async () => {
					try {
						using HttpResponseMessage image = await http.GetAsync($"https://api.tinyfox.dev/img?animal={animalKey}");
						await using Stream download = await image.Content.ReadAsStreamAsync();
						await args.Message.RespondAsync(dmb => dmb
							.WithContent(reply)
							.WithFile(Path.GetFileName(image.Content.Headers.ContentDisposition.FileName.Replace("\"", "")), download)
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
