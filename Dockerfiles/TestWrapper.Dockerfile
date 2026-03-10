FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

COPY . .

RUN --mount=type=cache,id=nugetpackages,target=/root/.nuget/packages \
    dotnet restore --nologo --disable-build-servers --runtime linux-x64

RUN --mount=type=cache,id=nugetpackages,target=/root/.nuget/packages \
    dotnet build SentencePieceTokenizer.Test --no-restore --nologo --disable-build-servers --runtime linux-x64 

COPY SentencePieceTokenizer/runtimes/linux-x64/native/ SentencePieceTokenizer.Test/bin/Debug/net10.0/linux-x64/

RUN ls -alH SentencePieceTokenizer.Test/bin/Debug/net10.0/linux-x64/

CMD ["dotnet", "test", "--nologo", "--no-restore", "--no-build", "--tl:off", "--runtime:linux-x64", "--filter:Category!~Benchmark"]
