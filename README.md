# Neutron
Build apps with c# and web technologies using webview

# Prerequisite Windows
- Node js, install it from https://nodejs.org/en or use NVM (Node Version Manager) to easily install and manage multiple nodejs installations https://github.com/coreybutler/nvm-windows
- Dotnet SDK, use the version you want to target, this framework support .Net 5, 6, 7, and 8 https://dotnet.microsoft.com/en-us/
- Enable Loopback, you need to enable the Loopback to make webview works on windows, run `CheckNetIsolation LoopbackExempt -a -n="Microsoft.win32webviewhost_cw5n1h2txyewy"` with elevated permission powershell, your user of your application also need to do that, it's recommended to use an installer when distributing your application so you can enable Loopback in the installion script 

# Prerequisite Linux
- Node js, install it from https://nodejs.org/en or use NVM (Node Version Manager) to easily install and manage multiple nodejs installations https://github.com/nvm-sh/nvm
- Dotnet SDK, use the version you want to target the framework support .Net 5, 6, 7, and 8 https://dotnet.microsoft.com/en-us/
- libwebkit2gtk, install using your distro package manager for debian use `sudo apt install libwebkit2gtk-4.0-37` and fedora use `sudo dnf install webkit2gtk4.0` if you distribute the application on linux the user of your application also need to install it

# Initializing The Project
Download the [neutroncli](https://github.com/annasajkh/Neutron/releases)<br/>
or for windows you can also use chocolatey<br/>
```choco install neutroncli```<br/>
for flatpak you need to install it using sudo for example<br/>
```sudo flatpak install neutroncli_0.2.0_x86_64.flatpak```<br/>
also for flatpak after the installation you need to run<br/>
```sudo ln -s /var/lib/flatpak/app/com.annasvirtual.neutroncli/current/active/files/bin/neutroncli /usr/local/bin/neutroncli```<br/>
this will create a symbolic link to make neutroncli command accessible system wide<br/>
because flatpak uses a sandboxed environment<br/>

open the terminal and type<br/>
```neutroncli init```<br/>

fill out the option or you can pass it directly<br/>
```neutroncli init --name ProjectName --dotnet-version net8 --framework react_ts```<br/>

# Running The Project
cd to the project directory and type `neutroncli run`<br/>
or you can go to the c# project directory part and type `dotnet run`<br/>

# Building The Project
cd to the project directory and type <br/>
```neutroncli build```<br/>

or pass it directly <br/>
```neutroncli build --platform win_x64 --build-mode release --self-contained```<br/>

or you can go to the c# project directory part and type<br/>
```dotnet publish --configuration Release --runtime win-x64 --self-contained```<br/>

it's decoupled from the neutroncli at this point so either way works

i recommend you distribute your application using an installer or a package manager<br/>
so that it can enable loopback on windows or install webkit2gtk on linux during app installation<br/>
so the user wouldn't need do anything extra<br/>
i might add `neutrocli package` that package the app automatically and do everything that it need to run in the installer so you don't have to