using HertejDB.Common;
using Microsoft.Extensions.Options;

namespace HertejBot.HertejDB;

public class HertejImageSource : ImageSource {
	private readonly HertejClient m_Hertej;
	private readonly string m_Category;

	public HertejImageSource(HertejClient hertej, string category) {
		m_Hertej = hertej;
		m_Category = category;
	}
	
	public async override Task<Image> GetImage() {
		GetImageDto hertejImage = (await m_Hertej.GetRandomImage(m_Category))!;
		return await m_Hertej.DownloadImage(hertejImage.Id);
	}
}
