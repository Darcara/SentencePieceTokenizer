# SentencePieceTokenizer

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/SentencePieceTokenizer)](https://www.nuget.org/packages/SentencePieceTokenizer/)
![GitHub License](https://img.shields.io/github/license/darcara/SentencePieceTokenizer)

## Usage
The tokenizers should be thread-safe, as the underlying sentencepiece processor is thread-safe. This has not been extensively tested however!


## MarianTokenizer
Uses the tokenizer of a sentencepiece model, but a different vocabulary for the ids.


## References
* Using [SentencePiece v0.2.0](https://github.com/google/sentencepiece/releases/tag/v0.2.0) from 2024-02-19
* For BERT-style embeddings it is recommended to use [FastBertTokenizer](https://github.com/georg-jung/FastBertTokenizer) [![NuGet version (FastBertTokenizer)](https://img.shields.io/nuget/v/FastBertTokenizer.svg?style=flat)](https://www.nuget.org/packages/FastBertTokenizer/)
* Inspired by [SIL.Machine.Tokenization.SentencePiece](https://github.com/sillsdev/machine/tree/master/src/SIL.Machine.Tokenization.SentencePiece)