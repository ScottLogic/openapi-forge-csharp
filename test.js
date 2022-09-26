const fs = require("fs");
const shell = require("shelljs");
const path = require('path');

// Extract cl arguments
const clArgs = process.argv.slice(2);

// Retrieve the path to feature paths from cl arguments of 'npm test', use default value if none given
let featurePath = clArgs[0] || "../../node_modules/openapi-forge/features/*.feature";

if(!path.isAbsolute(featurePath)) featurePath = "$(ProjectDir)" + featurePath;

const projectPath = "./tests/FeaturesTests/FeaturesTests.csproj";

const originalFile = fs.readFileSync(projectPath, "utf-8");

// Replace file path to .feature files in .csproj file, use handlebars style to help make the search value unique
fs.writeFileSync(projectPath, originalFile.replace("{{FEATURE_PATH}}", featurePath));

const result = shell.exec(`dotnet test ${projectPath}`);

// Revert .csproj file back to original
fs.writeFileSync(projectPath, originalFile);

process.exit(result.code);
