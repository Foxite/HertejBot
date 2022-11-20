namespace HertejBot;

public abstract class ImageSource {
	public abstract Task<Image> GetImage();
}