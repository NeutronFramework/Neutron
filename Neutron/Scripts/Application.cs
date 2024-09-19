using Neutron.Scripts.Helpers;
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

    public delegate T BindingFunction<T>();
    public delegate T1 BindingFunction<T, T1>(T? param);
    public delegate T3 BindingFunction<T1, T2, T3>(T1? param1, T2? param2);
    public delegate T4 BindingFunction<T1, T2, T3, T4>(T1? param1, T2? param2, T3? param3);
    public delegate T5 BindingFunction<T1, T2, T3, T4, T5>(T1? param1, T2? param2, T3? param3, T4? param4);
    public delegate T6 BindingFunction<T1, T2, T3, T4, T5, T6>(T1? param1, T2? param2, T3? param3, T4? param4, T5? param5);
    public delegate T7 BindingFunction<T1, T2, T3, T4, T5, T6, T7>(T1? param1, T2? param2, T3? param3, T4? param4, T5? param5, T6? param6);
    public delegate T8 BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8>(T1? param1, T2? param2, T3? param3, T4? param4, T5? param5, T6? param6, T7? param7);
    public delegate T9 BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1? param1, T2? param2, T3? param3, T4? param4, T5? param5, T6? param6, T7? param7, T8? param8);
    public delegate T10 BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1? param1, T2? param2, T3? param3, T4? param4, T5? param5, T6? param6, T7? param7, T8? param8, T9? param9);
    public delegate T11 BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1? param1, T2? param2, T3? param3, T4? param4, T5? param5, T6? param6, T7? param7, T8? param8, T9? param9, T10? param10);


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
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 0 parameters and return object of type T
    /// </summary>
    /// <param name="name">Global name of the javascript function</param>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="callback">Callback with 0 parameter and returning an object to be sent back to javascript</param>
    public void Bind<T>(string name, BindingFunction<T> callback)
    {
        webview.Bind(name, (id, _) => {
            Return(id, RPCResult.Success, callback());
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 1 parameter and return object of type T1
    /// </summary>
    /// <typeparam name="T">The first parameter type</typeparam>
    /// <typeparam name="T1">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 1 parameter that is the javascript function parameter and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T, T1>(string name, BindingFunction<T, T1> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 1;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param = JsonHelper.DeserializeToCSharp<T>(parameters[0]);

            Return(id, RPCResult.Success, callback((T?)param));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 2 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 2 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3>(string name, BindingFunction<T1, T2, T3> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 2;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 3 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The third parameter type</typeparam>
    /// <typeparam name="T4">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 3 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4>(string name, BindingFunction<T1, T2, T3, T4> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 3;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 4 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The second parameter type</typeparam>
    /// <typeparam name="T4">The fourth parameter type</typeparam>
    /// <typeparam name="T5">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 4 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4, T5>(string name, BindingFunction<T1, T2, T3, T4, T5> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 4;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);
            var param4 = JsonHelper.DeserializeToCSharp<T4>(parameters[3]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3, (T4?)param4));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 5 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The second parameter type</typeparam>
    /// <typeparam name="T4">The fourth parameter type</typeparam>
    /// <typeparam name="T5">The fifth parameter type</typeparam>
    /// <typeparam name="T6">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 5 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4, T5, T6>(string name, BindingFunction<T1, T2, T3, T4, T5, T6> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 5;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);
            var param4 = JsonHelper.DeserializeToCSharp<T4>(parameters[3]);
            var param5 = JsonHelper.DeserializeToCSharp<T5>(parameters[4]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3, (T4?)param4, (T5?)param5));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 6 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The third parameter type</typeparam>
    /// <typeparam name="T4">The fourth parameter type</typeparam>
    /// <typeparam name="T5">The fifth parameter type</typeparam>
    /// <typeparam name="T6">The sixth parameter type</typeparam>
    /// <typeparam name="T7">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 6 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4, T5, T6, T7>(string name, BindingFunction<T1, T2, T3, T4, T5, T6, T7> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 6;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);
            var param4 = JsonHelper.DeserializeToCSharp<T4>(parameters[3]);
            var param5 = JsonHelper.DeserializeToCSharp<T5>(parameters[4]);
            var param6 = JsonHelper.DeserializeToCSharp<T6>(parameters[5]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3, (T4?)param4, (T5?)param5, (T6?)param6));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 7 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The third parameter type</typeparam>
    /// <typeparam name="T4">The fourth parameter type</typeparam>
    /// <typeparam name="T5">The fifth parameter type</typeparam>
    /// <typeparam name="T6">The sixth parameter type</typeparam>
    /// <typeparam name="T7">The seventh parameter type</typeparam>
    /// <typeparam name="T8">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 7 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4, T5, T6, T7, T8>(string name, BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 7;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);
            var param4 = JsonHelper.DeserializeToCSharp<T4>(parameters[3]);
            var param5 = JsonHelper.DeserializeToCSharp<T5>(parameters[4]);
            var param6 = JsonHelper.DeserializeToCSharp<T6>(parameters[5]);
            var param7 = JsonHelper.DeserializeToCSharp<T7>(parameters[6]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3, (T4?)param4, (T5?)param5, (T6?)param6, (T7?)param7));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 8 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The third parameter type</typeparam>
    /// <typeparam name="T4">The fourth parameter type</typeparam>
    /// <typeparam name="T5">The fifth parameter type</typeparam>
    /// <typeparam name="T6">The sixth parameter type</typeparam>
    /// <typeparam name="T7">The seventh parameter type</typeparam>
    /// <typeparam name="T8">The eighth parameter type</typeparam>
    /// <typeparam name="T9">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 8 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string name, BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 8;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);
            var param4 = JsonHelper.DeserializeToCSharp<T4>(parameters[3]);
            var param5 = JsonHelper.DeserializeToCSharp<T5>(parameters[4]);
            var param6 = JsonHelper.DeserializeToCSharp<T6>(parameters[5]);
            var param7 = JsonHelper.DeserializeToCSharp<T7>(parameters[6]);
            var param8 = JsonHelper.DeserializeToCSharp<T8>(parameters[7]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3, (T4?)param4, (T5?)param5, (T6?)param6, (T7?)param7, (T8?)param8));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 9 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The third parameter type</typeparam>
    /// <typeparam name="T4">The fourth parameter type</typeparam>
    /// <typeparam name="T5">The fifth parameter type</typeparam>
    /// <typeparam name="T6">The sixth parameter type</typeparam>
    /// <typeparam name="T7">The seventh parameter type</typeparam>
    /// <typeparam name="T8">The eighth parameter type</typeparam>
    /// <typeparam name="T9">The ninth parameter type</typeparam>
    /// <typeparam name="T10">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 9 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string name, BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 9;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);
            var param4 = JsonHelper.DeserializeToCSharp<T4>(parameters[3]);
            var param5 = JsonHelper.DeserializeToCSharp<T5>(parameters[4]);
            var param6 = JsonHelper.DeserializeToCSharp<T6>(parameters[5]);
            var param7 = JsonHelper.DeserializeToCSharp<T7>(parameters[6]);
            var param8 = JsonHelper.DeserializeToCSharp<T8>(parameters[7]);
            var param9 = JsonHelper.DeserializeToCSharp<T9>(parameters[8]);

            Return(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3, (T4?)param4, (T5?)param5, (T6?)param6, (T7?)param7, (T8?)param8, (T9?)param9));
        });
    }

    /// <summary>
    /// Binds a callback so that it will appear under the given name as a global JavaScript function with 10 parameters
    /// </summary>
    /// <typeparam name="T1">The first parameter type</typeparam>
    /// <typeparam name="T2">The second parameter type</typeparam>
    /// <typeparam name="T3">The third parameter type</typeparam>
    /// <typeparam name="T4">The fourth parameter type</typeparam>
    /// <typeparam name="T5">The fifth parameter type</typeparam>
    /// <typeparam name="T6">The sixth parameter type</typeparam>
    /// <typeparam name="T7">The seventh parameter type</typeparam>
    /// <typeparam name="T8">The eighth parameter type</typeparam>
    /// <typeparam name="T9">The ninth parameter type</typeparam>
    /// <typeparam name="T10">The tenth parameter type</typeparam>
    /// <typeparam name="T11">The return type</typeparam>
    /// <param name="name">Global name of the javascript function</param>
    /// <param name="callback">Callback with 10 parameters that is the javascript function parameters and returning an object to be sent back to javascript</param>
    /// <exception cref="Exception">Exception is thrown when it failed to deserialize</exception>
    public void Bind<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(string name, BindingFunction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> callback)
    {
        webview.Bind(name, (id, parametersString) =>
        {
            JsonElement[]? parameters = JsonSerializer.Deserialize<JsonElement[]>(parametersString);

            byte expectedParamLength = 10;

            if (parameters == null || parameters.Length != expectedParamLength)
            {
                throw new Exception($"Failed to deserialize parameters for {name}, expected {expectedParamLength} but got {parameters?.Length ?? 0}");
            }

            var param1 = JsonHelper.DeserializeToCSharp<T1>(parameters[0]);
            var param2 = JsonHelper.DeserializeToCSharp<T2>(parameters[1]);
            var param3 = JsonHelper.DeserializeToCSharp<T3>(parameters[2]);
            var param4 = JsonHelper.DeserializeToCSharp<T4>(parameters[3]);
            var param5 = JsonHelper.DeserializeToCSharp<T5>(parameters[4]);
            var param6 = JsonHelper.DeserializeToCSharp<T6>(parameters[5]);
            var param7 = JsonHelper.DeserializeToCSharp<T7>(parameters[6]);
            var param8 = JsonHelper.DeserializeToCSharp<T8>(parameters[7]);
            var param9 = JsonHelper.DeserializeToCSharp<T9>(parameters[8]);
            var param10 = JsonHelper.DeserializeToCSharp<T10>(parameters[10]);

            Return<T11>(id, RPCResult.Success, callback((T1?)param1, (T2?)param2, (T3?)param3, (T4?)param4, (T5?)param5, (T6?)param6, (T7?)param7, (T8?)param8, (T9?)param9, (T10?)param10));
        });
    }

    /// <summary>
    /// Allows to return a value to the caller of a bound callback of Application.Bind()
    /// </summary>
    /// <param name="id">The id of the call</param>
    /// <param name="rpcResult">The result of the call</param>
    /// <param name="returnObject">The return of the function as a Dictionary of object</param>
    public void Return<T>(string id, RPCResult rpcResult, T returnObject)
    {
        webview.Return(id, rpcResult, JsonSerializer.Serialize<T>(returnObject));
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
