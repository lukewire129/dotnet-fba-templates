#!/usr/bin/dotnet run

#:sdk Microsoft.NET.Sdk.Web

#:package CommunityToolkit.Mvvm@8.*
#:package Avalonia.Desktop@11.*
#:package Avalonia.Fonts.Inter@11.*
#:package Avalonia.Themes.Fluent@11.*
#:package Avalonia.Markup.Declarative@11.*
#:package Lemon.Hosting.AvaloniauiDesktop@1.*

#:property PublishAot=True
#:property OutputType=WinExe

using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Themes.Fluent;
using Avalonia.Layout;
using AvaloniaFba.Template;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lemon.Hosting.AvaloniauiDesktop;

[assembly: SupportedOSPlatform("windows")]
[assembly: SupportedOSPlatform("linux")]
[assembly: SupportedOSPlatform("macos")]

Thread.CurrentThread.SetApartmentStateAsSTA();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAvaloniauiDesktopApplication<App>(
	b => b.UsePlatformDetect().WithInterFont().LogToTrace());

builder.Services.AddMainWindow<MainWindow, MainWindowViewModel>();

var app = builder.Build();
app.RunAvaloniauiApplication<MainWindow>(args).GetAwaiter().GetResult();

public sealed class App : Application
{
	public App(IServiceProvider sp) => Styles.Add(new FluentTheme(sp));
}

public sealed partial class MainWindowViewModel : ObservableObject
{
	[ObservableProperty]
	private int _count;

	[ObservableProperty]
	private string _buttonCaption = "Hello";

	[RelayCommand]
	private void ButtonAction() => ButtonCaption = $"Clicked {++Count} time(s)!";
}

public sealed class MainWindow : Window
{
	public MainWindow(MainWindowViewModel vm)
	{
		DataContext = vm;
		Title = "No XAML UI";
		Width = 400;
		Height = 300;
		Content = new Grid()
			.Children([
				new Button()
					.HorizontalAlignment(HorizontalAlignment.Center)
					.VerticalAlignment(VerticalAlignment.Center)
					.BindContent(vm, x => x.ButtonCaption)
					.BindCommand(vm, x => x.ButtonActionCommand),
			]);
	}
}
