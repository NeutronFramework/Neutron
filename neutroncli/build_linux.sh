#!/bin/bash

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

version="0.3.7"

dotnet publish ../neutroncli.csproj --configuration Release --runtime linux-x64 --self-contained true --output ./publish

sudo apt-get update
sudo apt-get install -y rpm dpkg zlib1g-dev clang
sudo apt-get install -y file

# Debian build

mkdir --parents neutroncli_debian/DEBIAN neutroncli_debian/usr/local/bin

echo "Package: neutroncli
Version: $version
Section: utils
Priority: optional
Architecture: amd64
Maintainer: AnnasVirtual
Description: Build apps with C# and web technologies using webview" > neutroncli_debian/DEBIAN/control

cp --recursive --verbose publish/* neutroncli_debian/usr/local/bin

chmod +x neutroncli_debian/usr/local/bin/neutroncli
chmod 755 neutroncli_debian/DEBIAN

dpkg --build neutroncli_debian artifacts/neutroncli_${version}_x86_64.deb

# Rpm build

mkdir --parents neutroncli_rpm/{BUILD,RPMS,SOURCES,SPECS}

echo "Name: neutroncli
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
" > ./neutroncli_rpm/SPECS/neutroncli.spec

cp --recursive --verbose publish/* neutroncli_rpm/SOURCES

rpmbuild -bb ./neutroncli_rpm/SPECS/neutroncli.spec --define "_topdir `pwd`/neutroncli_rpm"

mv ./neutroncli_rpm/RPMS/x86_64/neutroncli-${version}-1.x86_64.rpm ./neutroncli_rpm/RPMS/x86_64/neutroncli_${version}_x86_64.rpm
mv ./neutroncli_rpm/RPMS/x86_64/neutroncli_${version}_x86_64.rpm ./artifacts