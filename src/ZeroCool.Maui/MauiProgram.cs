using Microsoft.Extensions.Logging;
using ZeroCool.UI.Services;

namespace ZeroCool.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<ZeroCool.Core.Storage.IFileStore>(
			new ZeroCool.Data.LocalFileStore(Path.Combine(FileSystem.AppDataDirectory, "data")));
		builder.Services.AddZeroCoolUI();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
