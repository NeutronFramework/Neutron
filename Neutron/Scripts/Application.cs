using SharpWebview;
using SharpWebview.Content;
using System.Text.Json;

namespace Neutron.Scripts;

public class Application
{
    bool resizable = true;
    public bool Resizable 
    { 
        get
        {
            return resizable;
        }

        set
        {
            resizable = value;
            
            if (resizable)
            {
                webview.SetSize(Width, Height, WebviewHint.None);
            }
            else
            {
                webview.SetSize(Width, Height, WebviewHint.Fixed);
            }
        }
    }

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

    /// <summary>
    /// Posts a function to be executed on the main thread of the Application
    /// </summary>
    /// <param name="dispatchFunc">The function to call on the main thread</param>
    public void Dispatch(Action dispatchFunc)
    {
        webview.Dispatch(dispatchFunc);
    }

    /// <summary>
    /// Injects JavaScript code at the initialization of the new page.Every time the
    /// webview will open a new page. this initialization code will be executed. It is
    /// guaranteed that code is executed before window.onload.
    /// Execute this method before Application.Navigate()
    /// </summary>
    /// <param name="javascriptCode">The javascript code to execute</param>
    public void InitScript(string javascriptCode)
    {
        webview.InitScript(javascriptCode);
    }

    /// <summary>
    ///  Evaluates arbitrary JavaScript code. Evaluation happens asynchronously, also the result of the expression is ignored. Use bindings if you want to receive notifications about the results of the evaluation.
    /// </summary>
    /// <param name="javascriptCode">The javascript code to execute</param>
    public void Evaluate(string javascriptCode)
    {
        webview.Evaluate(javascriptCode);
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function
    /// </summary>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with two parameters. id -> The id of the call, parameters -> The parameters of the call as List of objects</param>
    /// <exception cref="Exception">Exception is thrown if the parameters is null</exception>
    public void Bind(string name, Action<string, List<object>> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            List<object>? parameters = JsonSerializer.Deserialize<List<object>>(parametersString);

            if (parameters is null)
            {
                throw new Exception("Parameters is null");
            }

            callback(id, parameters);
        });
    }

    /// <summary>
    /// Allows to return a value to the caller of a bound callback of Application.Bind()
    /// </summary>
    /// <param name="id">The id of the call</param>
    /// <param name="rpcResult">The result of the call</param>
    /// <param name="returnObject">The return of the function as a Dictionary of objects</param>
    public void Return(string id, RPCResult rpcResult, Dictionary<string, object> returnObject)
    {
        webview.Return(id, rpcResult, JsonSerializer.Serialize(returnObject));
    }

    /// <summary>
    /// Set the title of the window
    /// </summary>
    /// <param name="title">The title of the window</param>
    public void SetTitle(string title)
    {
        webview.SetTitle(title);
    }

    /// <summary>
    /// Set the minimum size of the window
    /// </summary>
    /// <param name="width">The minimum width of the window</param>
    /// <param name="height">The minimum height of the window</param>
    public void SetMinSize(int width, int height)
    {
        MinWidth = width;
        MinHeight = height;

        webview.SetSize(MinWidth, MinHeight, WebviewHint.Min);
    }

    /// <summary>
    /// Set the maximum size of the window
    /// </summary>
    /// <param name="width">The maximum width of the window</param>
    /// <param name="height">The maximum height of the window</param>
    public void SetMaxSize(int width, int height)
    {
        MaxWidth = width;
        MaxHeight = height;

        webview.SetSize(MaxWidth, MaxHeight, WebviewHint.Max);
    }

    /// <summary>
    /// Set the size of the window
    /// </summary>
    /// <param name="width">The width of the window</param>
    /// <param name="height">The height of the window</param>
    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;

        webview.SetSize(Width, Height, WebviewHint.None);
    }

    /// <summary>
    /// Run the application
    /// </summary>
    public void Run()
    {
        webview.Navigate(webContent);
        webview.Run();
        webview.Dispose();
    }
}
