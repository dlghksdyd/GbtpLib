import os
from datetime import datetime


def merge_files_to_txt(source_dir: str, output_file: str) -> None:
    """Merge all files under source_dir (recursively) into a single text file.

    Each file's content is preceded by a header that includes the relative path.
    """

    if not os.path.isdir(source_dir):
        raise FileNotFoundError(f"Source directory not found: {source_dir}")

    os.makedirs(os.path.dirname(output_file), exist_ok=True)

    merged = 0

    # Directory names to exclude (case-insensitive)
    excluded_dir_names = {"bin", "obj", ".vs"}

    with open(output_file, "w", encoding="utf-8") as out:
        out.write(f"# Merged from: {source_dir}\n")
        out.write(f"# Generated: {datetime.now().isoformat()}\n\n")

        for root, dirs, files in os.walk(source_dir):
            # Prune excluded directories so os.walk won't descend into them.
            dirs[:] = [
                d for d in dirs
                if d.lower() not in excluded_dir_names
            ]

            files.sort()
            for name in files:
                path = os.path.join(root, name)

                # Skip the output file if it's placed under the same tree.
                if os.path.abspath(path) == os.path.abspath(output_file):
                    continue

                rel = os.path.relpath(path, source_dir)

                out.write("=" * 100 + "\n")
                out.write(f"FILE: {rel}\n")
                out.write("=" * 100 + "\n")

                try:
                    with open(path, "r", encoding="utf-8", errors="replace") as f:
                        out.write(f.read())
                except Exception as ex:
                    out.write(f"[ERROR reading file: {ex}]\n")

                out.write("\n\n")
                merged += 1

    print(f"Merged {merged} file(s) into: {output_file}")


if __name__ == "__main__":
    # Fixed paths per request
    source = r"C:\workspace\mdbc2generator\CanProtocolEditor"
    output = r"C:\workspace\mdbc2generator\CanProtocolEditor\CanProtocolEditor.txt"

    merge_files_to_txt(source, output)
