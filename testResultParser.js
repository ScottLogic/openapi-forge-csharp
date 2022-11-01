function parse(results) {
  const length = results.length;
  const failLineRegex = /\s+Failed.*::\s(.*)\s\[\d+\s\w+\].*/;
  let failures = [];
  let rr;
  for (let xx = 0; xx < length; xx++) {
    if ((rr = results[xx].match(failLineRegex))) failures.push(rr[1]);
  }

  let result = {};
  result.failures = failures;

  // Extract the results of the testing from stdout. In stdout is a count of tests and their outcomes. Also included is the test duration.
  const resultMatch = results[length - 2].match(
    /Failed:\s+(\d+),\sPassed:\s+(\d+),\sSkipped:\s+(\d+),\sTotal:\s+(\d+),\sDuration:\s+(.*)\s-\sFeaturesTests.dll\s\(net6\.0\)/
  );

  if (resultMatch) {
    result.scenarios = parseInt(resultMatch[4]);
    result.passed = parseInt(resultMatch[2]);
    result.skipped = parseInt(resultMatch[3]);
    result.undef = 0;
    result.failed = parseInt(resultMatch[1]);
    result.time = resultMatch[5].replace(" ", "");
  } else {
    throw new Error(`Could not parse the results of the CSharp testing.`);
  }
  return result;
}

module.exports = {
  parse,
};
