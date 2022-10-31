using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace Features
{
    public class TestHelper : IDisposable
    {
        private readonly string _outputPath;

        private readonly string _schemaFilePath;

        private readonly string _testId;

        private Type _configurationType;

        private Type _apiClientType;

        private Assembly _generatedAssembly;

        public TestHelper(string testId)
        {
            _testId = testId;
            _outputPath = $"./generated-tests/{testId}";
            _schemaFilePath = $"{_outputPath}/schema.json";
        }

        public void GenerateApi(string schema)
        {
            WriteToJsonFile(schema);
            ForgeApi();
            GetApiClientTypes();
        }

        private void WriteToJsonFile(string fileContent)
        {
            if (Directory.Exists(Path.GetDirectoryName(_schemaFilePath)))
            {
                Directory.Delete(Path.GetDirectoryName(_schemaFilePath), true);
            }

            ;

            Directory.CreateDirectory(Path.GetDirectoryName(_schemaFilePath));
            using var sw = new StreamWriter(_schemaFilePath, false, new System.Text.UTF8Encoding());
            sw.Write(fileContent);
        }

        private void ForgeApi()
        {
            RunCmdPrompt($"openapi-forge forge {_schemaFilePath} {Constants.TemplateProjectPath} -o {_outputPath}");
        }

        private void GetApiClientTypes()
        {
            CreateProjectFile();
            CompileCode();
            StoreApiClientType();
        }

        private void StoreApiClientType()
        {
            _generatedAssembly = Assembly.LoadFrom(Path.GetFullPath($"{_outputPath}/bin/Api{_testId}.dll"));
            _configurationType = _generatedAssembly.GetType("OpenApiForge.Configuration");
            _apiClientType = _generatedAssembly.GetType("OpenApiForge.ApiClient");
        }

        private void CreateProjectFile()
        {
            using var sw = new StreamWriter($"{_outputPath}/Api{_testId}.csproj", false,
                new System.Text.UTF8Encoding());
            sw.Write(@"
            <Project Sdk=""Microsoft.NET.Sdk"">
                <PropertyGroup>
                    <TargetFramework>net6.0</TargetFramework>
                    <Nullable>disable</Nullable>
                    <IsPackable>true</IsPackable>
                </PropertyGroup>
                <ItemGroup>
                    <PackageReference Include=""Microsoft.Extensions.Configuration"" Version=""6.0.1"" />
                    <PackageReference Include=""Microsoft.Extensions.DependencyInjection"" Version=""6.0.0"" />
                    <PackageReference Include=""Microsoft.Extensions.Http"" Version=""6.0.0"" />
                </ItemGroup>
            </Project>");
        }

        private void CompileCode()
        {
            RunCmdPrompt($"dotnet build {_outputPath}/Api{_testId}.csproj -o {_outputPath}/bin");
        }

        public object CreateApiClient(HttpClient client, int? serverIndex = null)
        {
            var configuration = Activator.CreateInstance(_configurationType);
            var serversPropInfo = configuration.GetType().GetProperty("Servers");
            var servers = (string[])serversPropInfo.GetValue(configuration);

            if (servers.Length == 0)
            {
                serversPropInfo.SetValue(configuration, new string[] { Constants.BaseAddress });
            }

            if (serverIndex.HasValue)
            {
                configuration.GetType()
                    .GetProperty("SelectedServerIndex")
                    .SetValue(configuration, serverIndex.Value);
            }

            return Activator.CreateInstance(_apiClientType, new object[] { client, configuration });
        }

        public Type TryGetType(string typeName)
        {
            return _generatedAssembly.GetType($"OpenApiForge.{typeName}");
        }

        public object JsonToTypeInstance(string typeName, string json)
        {
            var objType = TryGetType(typeName);
            return System.Text.Json.JsonSerializer.Deserialize(json.Replace("'", "\""), objType);
        }

        public void Dispose()
        {
            _apiClientType = null;
            _configurationType = null;
            _generatedAssembly = null;
        }

        private static void RunCmdPrompt(string commandText)
        {

            var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "cmd.exe"
            : commandText.Substring(0, commandText.IndexOf(' '));

            var commandParams = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"/C {commandText}"
            : commandText.Substring(commandText.IndexOf(' ') + 1);

            using var cmd = new Process();
            cmd.StartInfo.FileName = command;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardError = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Arguments = commandParams;
            cmd.Start();
            cmd.WaitForExit(1000);

            using var errorReader = cmd.StandardError;
            var errorOutput = errorReader.ReadToEnd();

            Assert.Equal(string.Empty, errorOutput);
        }
    }
}
