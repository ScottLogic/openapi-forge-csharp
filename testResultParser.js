function parse(resultLine) {
  // Extract the results of the testing from stdout. In stdout is a count of tests and their outcomes. Also included is the test duration.
  const resultMatch = resultLine.match(
    /Failed:\s+(\d+),\sPassed:\s+(\d+),\sSkipped:\s+(\d+),\sTotal:\s+(\d+),\sDuration:\s+(.*)\s-\sFeaturesTests.dll\s\(net6\.0\)/
  );

  let result;
  if (resultMatch) {
    result = {
      scenarios: parseInt(resultMatch[4]),
      passed: parseInt(resultMatch[2]),
      skipped: parseInt(resultMatch[3]),
      undef: 0,
      failed: parseInt(resultMatch[1]),
      time: resultMatch[5].replace(" ", ""),
    };
  } else {
    throw new Error(`Could not parse the results of the CSharp testing.`);
  }
  return result;
}

module.exports = {
  parse,
};
