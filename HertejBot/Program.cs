using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using HertejBot;

var discord = new DiscordClient(new DiscordConfiguration() {
	Intents = DiscordIntents.GuildMessages | DiscordIntents.DirectMessages,
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

ImageSource Tinyfox(string animal) => new HttpImageSource(http, $"https://api.tinyfox.dev/img?animal={animal}");

List<(Regex Regex, ImageSource Source, string Reply)> filters = new[] {
	("hert(je|ej)?",                                  Tinyfox("bleat"),   "Hertej"          ),
	("vos(je|ej)?",                                   Tinyfox("fox"),     "Vosej"           ),
	("fox(ie|es|ies)?",                               Tinyfox("fox"),     "Vosej"           ),
	("mart(en)?(t?(ej|je))?",                         Tinyfox("marten"),  "Martej"          ),
	("pum(a?)[tp]?(ej|je|j)?",                        Tinyfox("puma"),    "Pumaj"           ),
	("snek(je|ej)?",                                  Tinyfox("snek"),    "Slangej"         ),
	("slang(e?t?(je|ej))?",                           Tinyfox("snek"),    "Slangej"         ),
	("(h?yeen|hyena)(t?(ej|je|j))?",                  Tinyfox("yeen"),    "Hyenaj"          ),
	("man(u|oe)l((e?t)?(ej|je))?",                    Tinyfox("manul"),   "Manulej"         ),
	("o?poss(um[tp]?)?(je|ej)?",                      Tinyfox("poss"),    "Opossumej"       ),
	("leeuw(t?(ej|je))?",                             Tinyfox("leo"),     "Leeuwej"         ),
	("serval(t?(ej|je))?",                            Tinyfox("serval"),  "Servalej"        ),
	("shiba(t?(ej|je|j))?",                           Tinyfox("shiba"),   "Shibaj"          ),
	("wasbeer(t?(ej|je))?",                           Tinyfox("racc"),    "Wasbeertej"      ),
	("fret(t?(ej|je))?",                              Tinyfox("dook"),    "Fretej"          ),
	("ott(je|ej|erej|ertje)?",                        Tinyfox("ott"),     "Ottej"           ),
	("w(ol|ø)f(je|ej)?",                              Tinyfox("woof"),    "Wolfej"          ),
	("snep(ej|je)?",                                  Tinyfox("snep"),    "Snepej"          ),
	("sneeuw ?luipaard(ej|je)?",                      Tinyfox("snep"),    "Sneeuwluipaardej"),
	("cap[iy](bara)?t?(ej|je)?",                      Tinyfox("capy"),    "Capybaratej"     ),
	("beert?(ej|je)?",                                Tinyfox("bear"),    "Beertej"         ),
	("ko?ni[ij]nt?(ej|je)?",                          Tinyfox("bun"),     "Knijntej"        ),
	("caracalt?(ej|je)?",                             Tinyfox("caracal"), "Caracalej"       ),
	("maa?n(en)? ?wolf(ej|je)?",                      Tinyfox("mane"),    "Manenwolfej"     ),
	("tijger?t?(ej|je)?",                             Tinyfox("tig"),     "Tijgetej"        ),
	("stink ?diert?(ej|je)?",                         Tinyfox("skunk"),   "Stinkdiertej"    ),
	("jaguart?(ej|je)?",                              Tinyfox("jaguar"),  "Jaguartej"       ),
	("(co)?yote(tje|tej|j|je)?",                      Tinyfox("yote"),    "Coyotej"         ),
	("waht?(ej|je)?",                                 Tinyfox("wah"),     "Rood pandatej"   ),
	("(red|rood|rode|rooie) ?pand(a|er)t?(ej|je)?",   Tinyfox("wah"),     "Rood pandatej"   ),
	("chi(tej|tje|ej|je|j)?",                         Tinyfox("chi"),     "Chitej"          ),
	("afri[kc]a(n|anse)?( wilde?)? ?hon[dt](ej|je)?", Tinyfox("chi"),     "Wild hondej"     ),
	("hond(je|ej)?",                                  Tinyfox("dog"),     "Hondej"          ), // this is a subset of the one above, so it must come after
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
