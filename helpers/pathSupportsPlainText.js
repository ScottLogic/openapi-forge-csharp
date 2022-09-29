const contentType = "text/plain";

const pathSupportsPlainText = (path) => {
    if (path.requestBody) {
        if (path.requestBody.content) {
            return path.requestBody.content[contentType]
                ? true
                : false;
        }
        return false;
    }

    if (path.responses) {
        if (Object.entries(path.responses).some(entry => entry[1].content !== undefined && entry[1].content[contentType])) {
            return true;
        }

        if (Object.entries(path.responses).every(entry => entry[1].content === undefined)) {
            return true;
        }

        if (Object.entries(path.responses).length === 1 && Object.entries(path.responses)[0][1].default !== undefined) {
            return true;
        }

        return false;
    }

    return true;
};

module.exports = pathSupportsPlainText;