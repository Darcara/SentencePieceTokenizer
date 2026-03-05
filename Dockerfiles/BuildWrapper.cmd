podman build --file BuildWrapper.Dockerfile --tag wrapperbuild ../SentencePieceWrapper/

podman run -it --rmi --read-only --detach --name wrapperbuild --timeout 5 wrapperbuild

podman cp --overwrite wrapperbuild:/src/SentencePieceWrapper.so ../SentencePieceTokenizer/runtimes/linux-x64/native/

timeout /t 5

podman image rm wrapperbuild