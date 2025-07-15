# SentencePiece Tokenizer

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/SentencePieceTokenizer)](https://www.nuget.org/packages/SentencePieceTokenizer/)
![GitHub License](https://img.shields.io/github/license/darcara/SentencePieceTokenizer)

## Usage
The tokenizers should be thread-safe, as the underlying sentencepiece processor is thread-safe. This has not been extensively tested however!


## Marian Tokenizer
Uses the tokenizer of a sentencepiece model, but a different vocabulary for the ids.

## XlmRoberta Tokenizer

Use the facebook [xlm-roberta-base](https://huggingface.co/FacebookAI/xlm-roberta-base/tree/main) ([Alt1](https://s3.amazonaws.com/models.huggingface.co/bert/xlm-roberta-base-sentencepiece.bpe.model), [Alt2](https://github.com/microsoft/BlingFire/raw/refs/heads/master/ldbsrc/xlm_roberta_base/spiece.model)) model

## References
* Using [SentencePiece v0.2.0](https://github.com/google/sentencepiece/releases/tag/v0.2.0) from 2024-02-19
* For BERT-style embeddings it is recommended to use [FastBertTokenizer](https://github.com/georg-jung/FastBertTokenizer)
* Inspired by [SIL.Machine.Tokenization.SentencePiece](https://github.com/sillsdev/machine/tree/master/src/SIL.Machine.Tokenization.SentencePiece)