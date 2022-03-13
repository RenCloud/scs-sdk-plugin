#ifndef SCS_TELEMETRY_INC_TCP_SERVER_HPP_
#define SCS_TELEMETRY_INC_TCP_SERVER_HPP_

#include <scssdk.h>

#include <cstdio>
#include <iostream>
#include <set>
#include <string>
#include <thread>

#include "log.hpp"

#if WIN32

#include <winsock.h>

#pragma comment(lib, "Ws2_32.lib")

#else

#include <sys/socket.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <unistd.h>
#include <cerrno>

#endif

#define PORT 45454

class TcpServer {
 public:
#ifdef WIN32
    std::set<SOCKET> *clients = nullptr;
    SOCKET srv = INVALID_SOCKET;
#else
    std::set<int> *clients = nullptr;
    int srv = -1;
#endif
    bool finish = false;
    bool finished = false;

 public:
    explicit TcpServer(Log *loggerImpl);
    ~TcpServer();

    bool init();
    void broadcast(const char *data, int dataSize) const;

 private:
    void acceptLoop();
    Log *logger;
};

#endif  // SCS_TELEMETRY_INC_TCP_SERVER_HPP_
