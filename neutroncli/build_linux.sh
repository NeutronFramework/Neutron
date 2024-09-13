#!/bin/bash

version="0.3.7"

if [ ! -d build ]; then
    mkdir build
fi

if [ -d build/publish ]; then
    rm -rf build/publish
fi

if [ ! -d build/artifacts ]; then
    mkdir build/artifacts
fi

cd build

if [ -d neutroncli_debian ]; then
    rm -rf neutroncli_debian
fi

if [ -d neutroncli_rpm ]; then
    rm -rf neutroncli_rpm
fi

if [ -d neutroncli_flatpak ]; then
    rm -rf neutroncli_flatpak
fi

if [ -d repo ]; then
    rm -rf repo
fi

if [ -d ".flatpak-builder" ]; then
    rm -rf ".flatpak-builder"
fi

dotnet publish ../neutroncli.csproj --configuration Release --runtime linux-x64 --self-contained true --output ./publish

sudo apt-get update
sudo apt-get install -y rpm dpkg zlib1g-dev clang file flatpak-builder

mkdir --parents neutroncli_debian/DEBIAN neutroncli_debian/usr/local/bin

cat <<EOF > neutroncli_debian/DEBIAN/control 
Package: neutroncli
Version: $version
Section: utils
Priority: optional
Architecture: amd64
Maintainer: AnnasVirtual
Description: Build apps with C# and web technologies using webview
EOF

cp --recursive --verbose publish/* neutroncli_debian/usr/local/bin
chmod +x neutroncli_debian/usr/local/bin/neutroncli
chmod 755 neutroncli_debian/DEBIAN
dpkg --build neutroncli_debian artifacts/neutroncli_${version}_x86_64.deb

mkdir --parents neutroncli_rpm/{BUILD,RPMS,SOURCES,SPECS}

cat <<EOF > ./neutroncli_rpm/SPECS/neutroncli.spec
Name: neutroncli
Version: $version
Release: 1
Summary: Build apps with C# and web technologies using webview
License: MIT

%description
neutroncli is a tool to build apps with C# and web technologies using webview.

%files
/usr/local/bin/*

%install
mkdir -p %{buildroot}/usr/local/bin
cp -r %{_sourcedir}/* %{buildroot}/usr/local/bin/
EOF

cp --recursive --verbose publish/* neutroncli_rpm/SOURCES
rpmbuild -bb ./neutroncli_rpm/SPECS/neutroncli.spec --define "_topdir `pwd`/neutroncli_rpm"
mv ./neutroncli_rpm/RPMS/x86_64/neutroncli-${version}-1.x86_64.rpm ./neutroncli_rpm/RPMS/x86_64/neutroncli_${version}_x86_64.rpm
mv ./neutroncli_rpm/RPMS/x86_64/neutroncli_${version}_x86_64.rpm ./artifacts

sudo flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo
sudo flatpak install -y flathub org.freedesktop.Platform//21.08 org.freedesktop.Sdk//21.08

mkdir -p neutroncli_flatpak/neutroncli/{files,export,export/share/applications}

cat <<EOF > neutroncli_flatpak/neutroncli/neutroncli.json
{
  "app-id": "com.annasvirtual.neutroncli",
  "runtime": "org.freedesktop.Platform",
  "runtime-version": "21.08",
  "sdk": "org.freedesktop.Sdk",
  "command": "neutroncli",
  "branch": "main",
  "modules": [
    {
      "name": "neutroncli",
      "buildsystem": "simple",
      "build-commands": [
        "mkdir -p /app/bin",
        "cp -r * /app/bin/"
      ],
      "sources": [
        {
          "type": "dir",
          "path": "../../publish"
        }
      ]
    }
  ]
}
EOF

cp -r publish/* neutroncli_flatpak/neutroncli/files

flatpak-builder --force-clean --repo=repo neutroncli_flatpak/build-dir neutroncli_flatpak/neutroncli/neutroncli.json
flatpak build-export repo neutroncli_flatpak/build-dir

if [ ! -d repo ]; then
    echo "Error: Flatpak repository creation failed."
    exit 1
fi

flatpak build-bundle repo artifacts/neutroncli_${version}_x86_64.flatpak com.annasvirtual.neutroncli main

if [ -f artifacts/neutroncli_${version}_x86_64.flatpak ]; then
    echo "Flatpak bundle created successfully!"
else
    echo "Error: Failed to create Flatpak bundle."
    exit 1
fi