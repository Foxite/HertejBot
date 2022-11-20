namespace HertejBot;

public record Image(
	Stream Stream,
	string Filename
) : IDisposable {
	public void Dispose() => Stream.Dispose();
}
