#include "tcp_server.hpp"
#include "log.hpp"

TcpServer::TcpServer(Log *loggerImpl) : logger(loggerImpl) {
#ifdef WIN32
    clients = new std::set<SOCKET>;
#else
    clients = new std::set<int>;
#endif
}

TcpServer::~TcpServer() {
    finish = true;

#ifdef WIN32
    WSACancelBlockingCall();

    shutdown(srv, 2);

    for (auto sock: *clients) {
        closesocket(sock);
    }

    closesocket(srv);
#else
    shutdown(srv, SHUT_RDWR);

    for (auto sock: *clients) {
        close(sock);
    }

    close(srv);
#endif

    delete clients;

    while (!finished){};
}

bool TcpServer::init() {
    logger->info("Starting TCP server");

    sockaddr_in service{};
    service.sin_family = AF_INET;
    service.sin_addr.s_addr = INADDR_ANY;
    service.sin_port = htons(PORT);

#ifdef WIN32
    WSADATA wsaData{};

    auto res = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (res != NO_ERROR) {
        logger->error("WSAStartup error", res);
        return false;
    }

    srv = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (srv == INVALID_SOCKET) {
        logger->error("error creating socket", WSAGetLastError());
        WSACleanup();
        return false;
    }

    if (bind(srv, reinterpret_cast<SOCKADDR*>(&service), sizeof(service)) == SOCKET_ERROR) {
        logger->error("error binding socket", WSAGetLastError());
        closesocket(srv);
        WSACleanup();
        return false;
    }

    if (listen(srv, 5) == SOCKET_ERROR) {
        logger->error("error listening", WSAGetLastError());
        closesocket(srv);
        WSACleanup();
        return false;
    }
#else
    srv = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (srv <= 0) {
        logger->error("Error creating socket", errno);
        return false;
    }

    int opt = 1;
#ifdef __APPLE__
    int ret = setsockopt(srv, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));
#else
    int ret = setsockopt(
        srv, SOL_SOCKET,
        SO_REUSEADDR | SO_REUSEPORT,
        &opt,
        sizeof(opt));
#endif

    if (ret) {
        logger->error("Error setting socket options", errno);
        close(srv);
        return false;
    }

    if (bind(srv, (struct sockaddr*)&service, sizeof(service)) < 0) {
        logger->error("Error binding socket", errno);
        close(srv);
        return false;
    }

    if (listen(srv, 5) < 0) {
        logger->error("Error listening", errno);
        close(srv);
        return false;
    }
#endif

    std::thread t(&TcpServer::acceptLoop, this);
    t.detach();

    return true;
}

void TcpServer::broadcast(const char *data, int dataSize) const {
#ifdef WIN32
    std::set<SOCKET> toRemove;
#else
    std::set<int> toRemove;
#endif

    for (auto sock: *clients) {
#ifdef WIN32
        if (send(sock, reinterpret_cast<char*>(&dataSize), sizeof(dataSize), 0) == SOCKET_ERROR) {
            logger->warn("error sending", WSAGetLastError());
            toRemove.insert(sock);
            continue;
        }

        if (send(sock, data, dataSize, 0) == SOCKET_ERROR) {
            logger->warn("error sending", WSAGetLastError());
            toRemove.insert(sock);
        }
#else
        if (send(sock, reinterpret_cast<char*>(&dataSize), sizeof(dataSize), MSG_NOSIGNAL) < 0) {
            logger->warn("Error sending", errno);
            toRemove.insert(sock);
            continue;
        }

        if (send(sock, data, dataSize, MSG_NOSIGNAL) < 0) {
            logger->warn("Error sending", errno);
            toRemove.insert(sock);
        }
#endif
    }

    if (!toRemove.empty()) {
        for (auto sock: toRemove) {
#ifdef WIN32
            closesocket(sock);
#else
            close(sock);
#endif

            clients->erase(sock);
        }
    }
}

void TcpServer::acceptLoop() {
    while (!finish) {
#if defined(__APPLE__) || defined(WIN32)
        auto s = accept(srv, nullptr, nullptr);
#elif defined(__linux__)
        auto s = accept4(srv, nullptr, nullptr, SOCK_NONBLOCK);
#else
#error "Unknown architecture."
#endif

#ifdef WIN32
        if (s == INVALID_SOCKET) {
            auto code = WSAGetLastError();
            if (code == WSAEINTR) {
                logger->info("Finishing accepting thread");
            } else {
                logger->error("Accept failed", code);
            }
            break;
        }
#else
        if (s < 0 && (errno == EAGAIN || errno == EWOULDBLOCK)) {
            std::this_thread::sleep_for(std::chrono::milliseconds(100));
            continue;
        }

        if (s < 0) {
            logger->error("Accept failed", errno);
            break;
        }
#endif

        logger->info("Connection accepted");
        clients->insert(s);
    }

    finished = true;
}
