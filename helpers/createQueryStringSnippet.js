const Handlebars = require("handlebars");
const toParamName = require("./toParamName");
const getParametersByType = require("./getParametersByType");
const newLine = "\n";

const isStringType = (typeDef) =>
  typeDef.type === "string" &&
  (typeDef.format === undefined || typeDef.format === "string");

const isStringArrayParam = (param) =>
  param.schema.type === "array" &&
  param.schema.items &&
  isStringType(param.schema.items);

const serialiseArrayParam = (param) => {
  const safeParamName = toParamName(param.name);
  const serialisedParam = `{string.Join("&", ${safeParamName}.Select(p => $"${
    param.name
  }={${isStringArrayParam(param) ? "HttpUtility.UrlEncode(p)" : "p"}}"))}`;

  return `if(${safeParamName} != null && ${safeParamName}.Length > 0)
{ ${prefixSerialisedQueryParam(serialisedParam)} }`;
};

const serialiseObjectParam = (param) => {
  const safeParamName = toParamName(param.name);
  let serialisedObject = "";
  for (const [propName, objProp] of Object.entries(param.schema.properties)) {
    let serialisedParam = isStringType(objProp)
      ? `{(${safeParamName}.${propName} == null ? string.Empty : "${propName}=" + HttpUtility.UrlEncode(${safeParamName}.${propName}))}`
      : `${propName}={${safeParamName}.${propName}}`;

    serialisedObject += serialisedParam + "&";
  }

  return `if(${safeParamName} != null)
{ ${prefixSerialisedQueryParam(serialisedObject.slice(0, -1))} }`;
};

const serialisePrimitive = (param) => {
  const safeParamName = toParamName(param.name);
  const escaped = isStringType(param.schema)
    ? `HttpUtility.UrlEncode(${safeParamName})`
    : safeParamName;

  const serialisedParam = prefixSerialisedQueryParam(
    `${param.name}={${escaped}}`
  );
  return param._optional
    ? `if(${safeParamName} != null)
{ ${serialisedParam} }`
    : serialisedParam;
};

const prefixSerialisedQueryParam = (serialisedQueryParam) =>
  `queryString.Append($"{(queryString.Length == 0 ? "?" : "&")}${serialisedQueryParam}");`;

const createQueryStringSnippet = (params) => {
  const queryParams = getParametersByType(params, "query");

  if (queryParams.length === 0) {
    return "";
  }

  let queryStringSnippet = `var queryString = new StringBuilder();`;

  for (const queryParam of queryParams) {
    let serialisedQueryParam;
    switch (queryParam.schema.type) {
      case "array":
        serialisedQueryParam = serialiseArrayParam(queryParam);
        break;
      case "object":
        serialisedQueryParam = serialiseObjectParam(queryParam);
        break;
      default:
        serialisedQueryParam = serialisePrimitive(queryParam);
        break;
    }

    queryStringSnippet += newLine + serialisedQueryParam;
  }

  return new Handlebars.SafeString(queryStringSnippet);
};

module.exports = createQueryStringSnippet;
