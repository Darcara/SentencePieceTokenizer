FROM gcc:latest AS build
WORKDIR /src

COPY sentencepiece_processor.h .
COPY SentencePieceWrapper.cpp .

RUN  gcc  -Wall -Wextra -g -std=c++17 -fPIC -shared -Ofast -g -DNDEBUG -pthread --output=SentencePieceWrapper.so SentencePieceWrapper.cpp
    
RUN ls -alH
