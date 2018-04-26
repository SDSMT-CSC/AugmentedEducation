#!/bin/bash

for f in $(find ./ -name "*.tex"); do
	aspell -t -c "$f"
done
