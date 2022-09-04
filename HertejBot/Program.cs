using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;

var discord = new DiscordClient(new DiscordConfiguration() {
	Intents = DiscordIntents.GuildMessages,
	Token = Environment.GetEnvironmentVariable("BOT_TOKEN")
});

var http = new HttpClient() {
	DefaultRequestHeaders = {
		UserAgent = {
			new ProductInfoHeaderValue("HertejBot", "0.2.1"),
			new ProductInfoHeaderValue("(https://github.com/Foxite/HertejBot)")
		}
	}
};

List<(Regex Regex, string ApiName, string Reply)> filters = new[] {
	("hert(je|ej)?",              "bleat",  "Hertej"   ),
	("vos(je|ej)?",               "fox",    "Vosej"    ),
	("fox(ie|es|ies)?",           "fox",    "Vosej"    ),
	("mart(en)?(t?(ej|je))?",     "marten", "Martej"   ),
	("hond(je|ej)?",              "dog",    "Hondej"   ),
	("pum(a?)[tp]?(ej|je)?",      "puma",   "Pumaj"    ),
	("snek(je|ej)?",              "snek",   "Slangej"  ),
	("slang(e?t?(je|ej))?",       "snek",   "Slangej"  ),
	("(hyeen|hyena)(t?(ej|je))?", "yeen",   "Hyenaj"   ),
	("man(u|oe)l((et?)(ej|je))?", "manul",  "Manulej"  ),
	("o?poss(um[tp]?)?(je|ej)?",  "poss",   "Opossumej"),
	("leeuw(t?(ej|je))?",         "leo",    "Leeuwej"  ),
	("serval(t?(ej|je))?",        "serval", "Servalej" ),
	("shiba(t?(ej|je))?",         "shiba",  "Shibaj"   ),
	("wasbeer(t?(ej|je))?",       "racc",   "Wasbeerej"),
	("fret(t?(ej|je))?",          "dook",   "Fretej"   ),
	("ott(je|ej|erej|ertje)?",    "ott",    "Ottej"    ),
	("wolf(je|ej)?",              "wolf",   "Wolfej"   ),
}.Select(tuple => (new Regex(@$"\b{tuple.Item1}s?\b", RegexOptions.IgnoreCase), tuple.Item2, tuple.Item3)).ToList();

discord.MessageCreated += (c, args) => {
	if (!args.Author.IsBot) {
		foreach ((Regex regex, string apiName, string reply) in filters) {
			if (regex.IsMatch(args.Message.Content)) {
				_ = Task.Run(async () => {
					try {
						using HttpResponseMessage image = await http.GetAsync($"https://api.tinyfox.dev/img?animal={apiName}");
						await using Stream download = await image.Content.ReadAsStreamAsync();
						await args.Message.RespondAsync(
							new DiscordMessageBuilder()
								.WithContent(reply + " :)")
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
