using SharpWebview;
using SharpWebview.Content;

namespace Neutron.Scripts;

public class Application
{
    public bool Resizable { get; }

    private WebContent webContent;
    private Webview webview;

    public string Title { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public int MinWidth { get; private set; }
    public int MinHeight { get; private set; }

    public int MaxWidth { get; private set; }
    public int MaxHeight { get; private set; }

    public Application(string title, int width, int height, string webContentPath, bool debug = false, bool interceptExternalLinks = false)
    {
        Title = title;
        Width = width;
        Height = height;

        webview = new Webview(debug, interceptExternalLinks);
        webContent = new WebContent(webContentPath);

        webview.SetTitle(Title);
        webview.SetSize(width, height, WebviewHint.None);
    }

    public Webview Bind(string name, Action<string, string> callback)
    {
        return webview.Bind(name, callback);
    }

    public void SetTitle(string title)
    {
        webview.SetTitle(title);
    }

    public void SetMinSize(int width, int height)
    {
        MinWidth = width;
        MinHeight = height;
        webview.SetSize(MinWidth, MinHeight, WebviewHint.Min);
    }

    public void SetMaxSize(int width, int height)
    {
        MaxWidth = width;
        MaxHeight = height;
        webview.SetSize(MaxWidth, MaxHeight, WebviewHint.Max);
    }

    public void Run()
    {
        webview.Navigate(webContent);
        webview.Run();
        webview.Dispose();
    }
}
