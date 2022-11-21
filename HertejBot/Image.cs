namespace HertejBot;

public record Image(
	Stream Stream,
	string Filename,
	ImageAttribution? Attribution
) : IDisposable {
	public void Dispose() => Stream.Dispose();
}

public record ImageAttribution(
	string AuthorName,
	string InfoUrl,
	DateTime DateTaken
);
