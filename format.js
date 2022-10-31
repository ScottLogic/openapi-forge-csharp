const shell = require("shelljs");

const checkArgs = {
  dotnet: "--verify-no-changes",
  prettier: "--check",
};

const writeArgs = {
  dotnet: "",
  prettier: "--write",
};

// Extract cl arguments
const clArgs = process.argv.slice(2);

let type = clArgs[0] || "check";

let args;

switch (type) {
  case "check":
    args = checkArgs;
    break;
  case "write":
    args = writeArgs;
    break;
  default:
    console.log("Unknown formatting type");
    process.exit(1);
}

let exitCode = 0;

let result = shell.exec(
  `dotnet format ./tests/FeaturesTests/FeaturesTests.csproj ${args.dotnet}`
);

exitCode = result.code;

result = shell.exec(`prettier  ${args.prettier} .`);

exitCode = exitCode | result.code;

process.exit(exitCode);
