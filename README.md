# Compare Source
Command-line tool for comparing directories / files.
Attempts to match filnames, if they match, compares the file hash.

- usage: cmpsrc.exe {cmp_mode} {path1} {path2} {switches}
- {cmp_mode}      - The compare mode
- -d              - Compare directories
- -f              - Compare files
- {path1}         - Path 1 to compare
- {path2}         - Path 2 to compare

### Switches
- -c              - Display output in color. default: off
- -t              - Search top directory only. default: off
- -excl {ext} ... - Exclude file extension. Eg: -excl *.obj;*.pdb
- -incl {ext} ... - Include file extension. Eg: -inc *.h;*.cpp

## Output mode (multiple output modes can be combined) default: all
- -noverb - No verbose output
- -ok     - Show only matched
- -hash   - Show only hash mismatched");
- -file   - Show only missing");
- -diff   - Show only diff");
- -found  - Show only found");
