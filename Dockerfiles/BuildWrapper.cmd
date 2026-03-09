podman build --file BuildWrapper.Dockerfile --no-cache --tag wrapperbuild ../SentencePieceWrapper/

podman run --read-only --name wrapperbuild --timeout 1 wrapperbuild

podman cp --overwrite wrapperbuild:/src/SentencePieceWrapper.so ../SentencePieceTokenizer/runtimes/linux-x64/native/

timeout /t 5

podman container rm --volumes --force wrapperbuild
podman image rm --force wrapperbuild