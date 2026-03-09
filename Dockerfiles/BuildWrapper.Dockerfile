FROM gcc:latest AS build
WORKDIR /src

COPY sentencepiece_processor.h SentencePieceWrapper.cpp libsentencepiece.so ./

RUN ls -alH
RUN gcc  --verbose -Wall -Wextra -g -std=c++17 -fPIC -shared -Ofast -DNDEBUG -pthread -I. -L. --output=SentencePieceWrapper.so SentencePieceWrapper.cpp -Wl,-rpath,'$ORIGIN' -lsentencepiece

RUN ls -alH

RUN nm -D SentencePieceWrapper.so
RUN nm -D --defined-only SentencePieceWrapper.so
RUN ldd SentencePieceWrapper.so
RUN readelf -d SentencePieceWrapper.so | grep library
RUN readelf -d SentencePieceWrapper.so | grep ORIGIN

CMD ["echo", "Hello World!"]