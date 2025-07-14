// see: https://github.com/google/sentencepiece/blob/master/doc/api.md
// gcc  -Wall -Wextra -g -std=c++17 -fPIC -shared -Ofast -g -DNDEBUG -pthread -o SentencePieceWrapper.so SentencePieceWrapper.cpp

#include "sentencepiece_processor.h"

#if defined _MSC_VER
#pragma comment(lib, "sentencepiece")
#define EXPORT extern "C" __declspec(dllexport)
#else
    #define EXPORT __attribute__((visibility("default")))
#endif

EXPORT void* CreateProcessor()
{
    return new sentencepiece::SentencePieceProcessor();
}

EXPORT int LoadModel(void* processorHandle, const char* filename, int* bos, int* eos, int* pad, int* unk, int* size)
{
    const auto processor = static_cast<sentencepiece::SentencePieceProcessor*>(processorHandle);
    sentencepiece::util::Status status = processor->Load(filename);
    if (status.code() == sentencepiece::util::StatusCode::kOk)
    {
        *bos = processor->bos_id();
        *eos = processor->eos_id();
        *pad = processor->pad_id();
        *unk = processor->unk_id();
        *size = processor->GetPieceSize();
    }

    return static_cast<int>(status.code());
}


EXPORT int EncodeAsSpans(void* processorHandle, const char* input, const int inputLength, int* outputIds, uint64_t* outputSpans, int capacity, int* length)
{
    const auto processor = static_cast<sentencepiece::SentencePieceProcessor*>(processorHandle);

    sentencepiece::ImmutableSentencePieceText spt;
    std::string_view inputView(input, inputLength);
    const sentencepiece::util::Status status = processor->Encode(inputView, spt.mutable_proto());

    // nothing we can do on error
    if (status.code() != sentencepiece::util::StatusCode::kOk)
    {
        *length = 0;
        return static_cast<int>(status.code());
    }


    const int requiredCapacity = static_cast<int>(spt.pieces_size());
    *length = requiredCapacity;
    if (requiredCapacity <= capacity)
    {
        int idx = 0;
        for (const auto& sp : spt.pieces())
        {
            outputIds[idx] = static_cast<int>(sp.id());
            outputSpans[idx] = static_cast<uint64_t>(sp.end()) << 32 | (sp.begin() & 0xFFFFFFFFL);
            ++idx;
        }
    }

    return 0; // sentencepiece::util::StatusCode::kOk
}

EXPORT int EncodeAsIds(void* processorHandle, const char* input, const int inputLength, int* output, int capacity, int* length)
{
    const auto processor = static_cast<sentencepiece::SentencePieceProcessor*>(processorHandle);

    std::vector<int> pieces;
    std::string_view inputView(input, inputLength);
    const sentencepiece::util::Status status = processor->Encode(inputView, &pieces);

    // nothing we can do on error
    if (status.code() != sentencepiece::util::StatusCode::kOk)
    {
        *length = 0;
        return static_cast<int>(status.code());
    }

    const int requiredCapacity = static_cast<int>(pieces.size());
    *length = requiredCapacity;
    if (requiredCapacity <= capacity)
        std::copy(pieces.begin(), pieces.end(), output);

    return 0; // sentencepiece::util::StatusCode::kOk
}

EXPORT int EncodeAsPieces(void* processorHandle, const char* input, const int inputLength, char* output, int capacity, int* length, int* numberOfTokens)
{
    const auto processor = static_cast<sentencepiece::SentencePieceProcessor*>(processorHandle);
    std::vector<std::string> pieces;
    std::string_view inputView(input, inputLength);
    const sentencepiece::util::Status status = processor->Encode(inputView, &pieces);
    const size_t lcapacity = capacity;

    // nothing we can do on error
    if (status.code() != sentencepiece::util::StatusCode::kOk)
    {
        *length = 0;
        *numberOfTokens = 0;
        return static_cast<int>(status.code());
    }

    size_t totalLength = 0;
    bool everythingCopied = true;
    for (const auto& piece : pieces)
    {
        const size_t size = piece.size();
        const size_t requiredLength = totalLength + size + 1;
        if (requiredLength <= lcapacity)
        {
            piece.copy(output, size);
            output += size;
            *output = ' ';
            output++;
        }
        else
        {
            everythingCopied = false;
        }
        totalLength = requiredLength;
    }

    if (!everythingCopied)
    {
        *length = static_cast<int>(totalLength);
        *numberOfTokens = 0;
    }
    else
    {
        *length = static_cast<int>(totalLength > 0 ? totalLength - 1 : 0);
        *numberOfTokens = static_cast<int>(pieces.size());
    }
    return 0; // sentencepiece::util::StatusCode::kOk
}

EXPORT int DecodeIds(void* processorHandle, const int* ids, const int numIds, char* output, int capacity, int* length)
{
    const auto processor = static_cast<sentencepiece::SentencePieceProcessor*>(processorHandle);

    const size_t lcapacity = capacity;
    std::vector<int> idInput(ids, ids + numIds);
    std::string result;
    const sentencepiece::util::Status status = processor->Decode(idInput, &result);

    // nothing we can do on error
    if (status.code() != sentencepiece::util::StatusCode::kOk)
    {
        *length = 0;
        return static_cast<int>(status.code());
    }

    *length = static_cast<int>(result.length());
    if (result.length() > lcapacity)
    {
        return 0; // sentencepiece::util::StatusCode::kOk
    }

    result.copy(output, result.length());
    return 0; // sentencepiece::util::StatusCode::kOk
}

EXPORT void DisposeProcessor(void* processorHandle)
{
    const auto processor = static_cast<sentencepiece::SentencePieceProcessor*>(processorHandle);
    delete processor;
}
