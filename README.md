# Neutron
Build apps with c# and web technologies using webview

# Prerequisite Windows
- Node js, install it from https://nodejs.org/en or use NVM (Node Version Manager) to easily install and manage multiple nodejs installations https://github.com/coreybutler/nvm-windows
- Dotnet SDK, use the version you want to target, this framework support .Net 7, and 8 https://dotnet.microsoft.com/en-us/

# Prerequisite Linux
- Node js, install it from https://nodejs.org/en or use NVM (Node Version Manager) to easily install and manage multiple nodejs installations https://github.com/nvm-sh/nvm
- Dotnet SDK, use the version you want to target the framework support .Net 7, and 8 https://dotnet.microsoft.com/en-us/
- libwebkit2gtk and libgtk-3, install using your distro package manager for debian use `sudo apt install -y libwebkit2gtk-4.0-dev libgtk-3-dev` and fedora use `sudo dnf install webkit2gtk4.0-devel gtk3-devel` if you distribute the application on linux the user of your application also need to install it

# Initializing The Project
Download the [neutroncli](https://github.com/NeutronFramework/Neutron/releases)<br/><br/>
or for windows you can also use chocolatey, launch an elevated powershell and type<br/>
```choco install neutroncli --version=0.2.6```<br/><br/>
or download the chocolatey package from [neutroncli](https://github.com/NeutronFramework/Neutron/releases) make a folder, put the package inside, launch an elevated powershell, and type<br/>
```choco install neutroncli --source "path/to/folder/containing/chocolatey"```<br/><br/>
for flatpak you need to install it using sudo for example<br/>
```sudo flatpak install neutroncli_0.2.0_x86_64.flatpak```<br/><br/>
also for flatpak after the installation you need to run<br/>
```sudo ln -s /var/lib/flatpak/app/com.annasvirtual.neutroncli/current/active/files/bin/neutroncli /usr/local/bin/neutroncli```<br/><br/>
this will create a symbolic link to make neutroncli command accessible system wide<br/>
this is because flatpak uses a sandboxed environment<br/>

open the terminal and type<br/>
```neutroncli init```<br/>

fill out the option or you can pass it directly<br/>
```neutroncli init --name ProjectName --dotnet-version net8 --framework react_ts```<br/>

# Running The Project
cd to the project directory and type `neutroncli run`<br/>
or you can go to the c# project directory and type `dotnet run`<br/>
or open the c# project on your favorite IDEs and press run<br/><br/>
you can also run the frontend independently for fast ui iteration and hot reload<br/>
go to the project-name-frontend folder and type `npm run dev`<br/>

# Building The Project
cd to the project directory and type <br/>
```neutroncli build```<br/><br/>
or pass it directly <br/>
```neutroncli build --platform win_x64 --build-mode release --self-contained```<br/><br/>
or you can go to the c# project directory part and type<br/>
```dotnet publish --configuration Release --runtime win-x64 --self-contained```<br/><br/>
i recommend you distribute your application using an installer or a package manager<br/>
so that it can install webkit2gtk and libgtk3 on linux during app installation<br/>
so the user wouldn't need do anything extra<br/>
my suggestion is to use https://velopack.io/
it's crossplatform and really easy to setup