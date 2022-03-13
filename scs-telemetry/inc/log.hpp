#ifndef SCS_TELEMETRY_INC_LOG_HPP_
#define SCS_TELEMETRY_INC_LOG_HPP_

#include <scssdk.h>

#include <string>
#include <system_error>
#include <cstdarg>

class Log {
 public:
    explicit Log(scs_log_t gameLog) : logger(gameLog), isDebug(false) { }

    void info(const std::string &msg) {
        logInternal(SCS_LOG_TYPE_message, msg);
    }

    void info(const std::string &msg, const int errorCode) {
        info(getCodeDescriptor(msg, errorCode));
    }

    void debug(const std::string &msg) {
        if (isDebug) {
            info(msg);
        }
    }

    void debug(const std::string &msg, const int errorCode) {
        if (isDebug) {
            info(msg, errorCode);
        }
    }

    void warn(const std::string &msg) {
        logInternal(SCS_LOG_TYPE_warning, msg);
    }

    void warn(const std::string &msg, const int errorCode) {
        warn(getCodeDescriptor(msg, errorCode));
    }

    void error(const std::string &msg) {
        logInternal(SCS_LOG_TYPE_error, msg);
    }

    void error(const std::string &msg, const int errorCode) {
        error(getCodeDescriptor(msg, errorCode));
    }

    void setDebug(bool debugFlag) {
        isDebug = debugFlag;
    }

    static std::string format(const char *const text, ...) {
        char formatted[1000];

        std::va_list args;
        va_start(args, text);

#ifdef WIN32
        vsnprintf_s(formatted, sizeof(formatted), _TRUNCATE, text, args);
#else
        vsnprintf(formatted, sizeof(formatted), text, args);
#endif

        formatted[sizeof(formatted) - 1] = 0;
        va_end(args);

        return {formatted};
    }

 private:
    const char *tag = "TelemetryPlugin";
    bool isDebug;
    scs_log_t logger{};

    static std::string getCodeDescriptor(
        const std::string &prefix,
        const int errorCode
    ) {
        auto result = prefix;
        result.append(" - (");
        result.append(std::to_string(errorCode));
        result.append(") ");
        result.append(std::system_category().message(errorCode));

        return result;
    }

    void logInternal(const scs_log_type_t logType, const std::string &msg) {
        std::string result = "[";
        result.append(tag);
        result.append("] ");
        result.append(msg);

        logger(logType, result.c_str());
    }
};

#endif  // SCS_TELEMETRY_INC_LOG_HPP_
