cmake_minimum_required(VERSION 3.19)
project(scs-telemetry VERSION 12.0.0)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_OSX_ARCHITECTURES "x86_64")

if(UNIX)
    set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -pthread")
endif()

include_directories("../scs_sdk/include")
include_directories("inc")

file(GLOB scs-telemetry-SRC
        "src/*.cpp"
        "inc/*.h"
        "inc/*.hpp")

add_library(scs-telemetry SHARED scs-telemetry.def
        ${scs-telemetry-SRC})