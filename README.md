# xsltp

Small command-line XSLT processor for .NET.

Features:

* XSLT 1.0 transformations via XslCompiledTransform
* XML output
* Text output
* Optional pretty-print formatting
* Console output mode
* File output mode
* No Visual Studio dependency

## Usage

```bash
xsltp.exe --xml input.xml --xslt transform.xslt
```

Console output:

```bash
xsltp.exe --xml input.xml --xslt transform.xslt --console-only
```

Pretty-print output:

```bash
xsltp.exe --xml input.xml --xslt transform.xslt --pretty
```

Help:

```bash
xsltp.exe --help
```

## Requirements

None.

Published as a self-contained executable.

## License

MIT
