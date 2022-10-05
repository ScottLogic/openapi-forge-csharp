const supportedTypes = ["application/json", "text/plain"];
const arrayCount = supportedTypes.length - 1;

const pathContentTypeSupported = (path) => {
    for (let i = 0; i <= arrayCount; i++) {
        if (path.requestBody) {
            if (path.requestBody.content) {
                if (path.requestBody.content[supportedTypes[i]]) {
                    return true;
                } else if (i === arrayCount) {
                    return false;
                } else {
                    continue;
                }
            }
            return false;
        }

        if (path.responses) {
            if (Object.entries(path.responses).some(entry => entry[1].content !== undefined && entry[1].content[supportedTypes[i]])) {
                return true;
            }

            if (Object.entries(path.responses).every(entry => entry[1].content === undefined)) {
                return true;
            }

            if (Object.entries(path.responses).length === 1 && Object.entries(path.responses)[0][1].default !== undefined) {
                return true;
            }

            if (i === arrayCount) {
                return false;
            } else {
                continue;
            }
        }

        return true;
    };
};

module.exports = pathContentTypeSupported;