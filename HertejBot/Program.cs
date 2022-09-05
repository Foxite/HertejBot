using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using HertejBot;

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

ImageSource Tinyfox(string animal) => new HttpImageSource(http, $"https://api.tinyfox.dev/img?animal={animal}");

List<(Regex Regex, ImageSource Source, string Reply)> filters = new[] {
	("hert(je|ej)?",              Tinyfox("bleat"),  "Hertej"   ),
	("vos(je|ej)?",               Tinyfox("fox"),    "Vosej"    ),
	("fox(ie|es|ies)?",           Tinyfox("fox"),    "Vosej"    ),
	("mart(en)?(t?(ej|je))?",     Tinyfox("marten"), "Martej"   ),
	("hond(je|ej)?",              Tinyfox("dog"),    "Hondej"   ),
	("pum(a?)[tp]?(ej|je)?",      Tinyfox("puma"),   "Pumaj"    ),
	("snek(je|ej)?",              Tinyfox("snek"),   "Slangej"  ),
	("slang(e?t?(je|ej))?",       Tinyfox("snek"),   "Slangej"  ),
	("(hyeen|hyena)(t?(ej|je))?", Tinyfox("yeen"),   "Hyenaj"   ),
	("man(u|oe)l((et?)(ej|je))?", Tinyfox("manul"),  "Manulej"  ),
	("o?poss(um[tp]?)?(je|ej)?",  Tinyfox("poss"),   "Opossumej"),
	("leeuw(t?(ej|je))?",         Tinyfox("leo"),    "Leeuwej"  ),
	("serval(t?(ej|je))?",        Tinyfox("serval"), "Servalej" ),
	("shiba(t?(ej|je))?",         Tinyfox("shiba"),  "Shibaj"   ),
	("wasbeer(t?(ej|je))?",       Tinyfox("racc"),   "Wasbeerej"),
	("fret(t?(ej|je))?",          Tinyfox("dook"),   "Fretej"   ),
	("ott(je|ej|erej|ertje)?",    Tinyfox("ott"),    "Ottej"    ),
	("w(ol|ø)f(je|ej)?",          Tinyfox("woof"),   "Wolfej"   ),
}.Select(tuple => (new Regex(@$"\b{tuple.Item1}s?\b", RegexOptions.IgnoreCase), tuple.Item2, tuple.Item3)).ToList();

discord.MessageCreated += (c, args) => {
	if (!args.Author.IsBot) {
		foreach ((Regex regex, ImageSource source, string reply) in filters) {
			if (regex.IsMatch(args.Message.Content)) {
				_ = Task.Run(async () => {
					try {
						using Image image = await source.GetImage();
						await args.Message.RespondAsync(
							new DiscordMessageBuilder()
								.WithContent(reply + " :)")
								.WithFile(Path.GetFileName(image.Filename.Replace("\"", "")), image.Stream)
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
