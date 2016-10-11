var pushSettings = new NuGetPushSettings {
	ApiKey = "f429acbd-e994-4f16-a345-aea7f3d7a58b",
	Source = "https://www.nuget.org/api/v2/package"
};

// Arguments
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var sln = GetFiles("./*.sln").First ().FullPath;
var versionPath = "./version.txt";

var csprojPath = "./RubyStack/RubyStack.csproj";

string Version (string path)
{
	return System.IO.File.ReadAllText(path).Trim ();
}

FilePath Pack(string csprojPath, string version, string configuration)
{
	var csproj = File(csprojPath);
	var name = System.IO.Path.GetFileNameWithoutExtension(csproj);
	var nuspec = File(System.IO.Path.ChangeExtension(csproj, ".nuspec"));

	var projectDir = Directory(System.IO.Path.GetDirectoryName(csproj));
	var bin = projectDir + Directory("bin") + Directory(configuration);

	var nupkgFileName = string.Format("{0}.{1}.nupkg", name, version);
	var nupkgDir = Directory("./nupkg");
	var nupkg = nupkgDir + File(nupkgFileName);

	Information(nupkg);

	NuGetPack(nuspec, new NuGetPackSettings {
		Verbosity = NuGetVerbosity.Detailed,
		Version = version,
		OutputDirectory = nupkgDir,
		BasePath = bin,
	});

	return nupkg;
}

Task("Restore")
.Does(() => {
	NuGetRestore(sln);
});

Task("Build")
.IsDependentOn("Restore")
.Does(() => {
	if(IsRunningOnWindows())
	{
		MSBuild(sln, settings =>
			settings.SetConfiguration(configuration)
					.SetVerbosity(Verbosity.Minimal));
	} else {
		XBuild(sln, settings => settings.SetConfiguration(configuration));
	}
});

Task("IncrementVersion")
.Does(() => {
	var separators = new char [] { '.' };
	var items = System.IO.File.ReadAllText(versionPath).Trim ().Split (separators);
	System.IO.File.WriteAllText (versionPath, string.Format ("{0}.{1}.{2}", items[0], items[1], int.Parse(items[2]) + 1));
});

Task("Push")
.IsDependentOn("Build")
.IsDependentOn("IncrementVersion")
.Does (() => {
	CreateDirectory ("./nupkg/");
	var version = Version(versionPath);

	var coreNupkg = Pack(csprojPath, version, configuration);
	NuGetPush(coreNupkg, pushSettings);
});

Task("Default")
	.IsDependentOn("Build");

RunTarget(target);