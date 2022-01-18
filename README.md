# Actuatree
Actuatree is a plugin that breaks trees when the block(usually a grass block) below the root tile is actuated

![GitHub license](https://img.shields.io/github/license/Arthri/Actuatree?style=flat-square) ![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/Arthri/Actuatree?sort=semver&style=flat-square) ![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/Arthri/Actuatree?include_prereleases&sort=semver&style=flat-square)

## Status
(As Arthri) Actuatree is an old and abandoned project of mine that I updated and pushed onto GitHub for safekeeping

## Screenshots
![Example 1](../HEAD/docs/assets/Usage-1.gif)

## Installation
1. Grab the [latest](https://github.com/Arthri/Actuatree/relases/latest) release
2. Put the zip in `ServerPlugins` folder
3. Unzip the zip

## Usage
1. Place an actuator on the block below the tree's root tile
2. Activate the actuator

The entire tree will be broken and drop wood when the tile is actuated

## Development

### Prequisites
- .NET CLI
- .NET Framework 4.7.2 targetting pack

### Setup Dependencies
1. Restore dotnet tools(run `dotnet tool restore`)
2. Restore dependencies(run `dotnet paket restore`)

### Compile w/Visual Studio
1. Open `Actuatree.sln`
2. Build solution

### Compile w/dotnet CLI
1. Navigate to project root directory
2. Run `dotnet build`

### Get Compiled Files
1. Navigate to `src/Actuatree/bin/{BUILD_CONFIGURATION}/` where `{BUILD_CONFIGURATION}` is either Debug or Release
2. Do stuff with files
